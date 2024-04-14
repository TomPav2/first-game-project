using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using Random = UnityEngine.Random;

public class SkellyController : MonoBehaviour
{
    // pointers
    [SerializeField] protected SliderController healthBar;
    [SerializeField] protected Rigidbody2D body;
    [SerializeField] protected Sprite defaultSprite;
    [SerializeField] private Animator animator;
    protected MainCharacterSheet mainChar;
    private LevelManager manager;
    private SpawnerManager spawner;
    private RavenController raven;
    private CrowController crow;
    //private CrowController crow;

    // movement
    protected Vector2 target;
    private bool isInFinalArea = false;
    private bool noIdlingOnTarget = true;

    private float distanceTolerance = 0.2f;
    private float lastDistance = 10000f;
    private byte currentSpeed = 0;

    private readonly byte fast = 10;
    private readonly byte slow = 6;

    private bool slowed = false;

    // combat
    private int maxHealth;
    public int health {  get; private set; }

    // processing
    private EnemyState state = EnemyState.None;

    private Coroutine currentProcess;

    // ---------------- LIFECYCLE ----------------
    private void Awake()
    {
        healthBar = GetComponentInChildren<SliderController>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GetComponent<SpriteRenderer>().enabled = false;
        body.simulated = false;
        animator.enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
    }

    public void init(MainCharacterSheet character, LevelManager levelManager, SpawnerManager spawnerManager, RavenController ravenController, CrowController crowController)
    {
        manager = levelManager;
        mainChar = character;
        spawner = spawnerManager;
        raven = ravenController;
        crow = crowController;
    }

    public void spawn(Vector2 pos, int hp)
    {
        transform.position = pos;
        maxHealth = hp;
        health = hp;
        currentProcess = StartCoroutine(performSpawn());
    }

    public void obliterate()
    {
        if (state == EnemyState.Spawning)
        {
            StopAllCoroutines();
            deathSecondStage();
        } else if (state == EnemyState.Dead || state == EnemyState.Dying)
        {
            return;
        } else
        {
            damageMax(DamageType.None);
        }
    }

    protected virtual void die(DamageType damageType)
    {
        StopAllCoroutines();
        manager.addScore(damageType);
        if (raven != null) raven.deregister(this);
        if (crow != null) crow.deregister(this);
        slowed = false;

        deathFirstStage();
    }

    protected virtual IEnumerator performSpawn()
    {
        switchState(EnemyState.Spawning);
        GetComponent<SpriteRenderer>().enabled = true;

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 0.1f;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha); // TODO replace with animation
            yield return new WaitForSeconds(0.1f);
        }

        animator.enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        switchState(EnemyState.Idle);
        animator.ResetTrigger(Trigger.idle); // not needed after spawn
        StartCoroutine(navigator());
        yield break;
    }

    // disables collision and starts death animation
    protected virtual void deathFirstStage()
    {
        switchState(EnemyState.Dying);
        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;
        healthBar.fade();
        spawner.removeFromLiving(this);
    }

    // called by animation event, stops animation and performs fade
    private void deathSecondStage()
    {
        switchState(EnemyState.Dead);
        animator.enabled = false;
        currentProcess = StartCoroutine(deathFadeProcess());
    }

    protected virtual IEnumerator deathFadeProcess()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= 0.2f;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha); // TODO replace with animation
            yield return new WaitForSeconds(0.1f);
        }
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        GetComponent<SpriteRenderer>().enabled = false;
        spawner.makeAvailable(this);
    }

    // ---------------- MOVEMENT ----------------
    private void FixedUpdate()
    {
        if (!movingState())
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target);

        // if target is reached...
        if (distanceToTarget <= distanceTolerance)
        {
            if (state == EnemyState.InertiaRun)
            {
                // if state was inertia, stop and look around
                switchState(EnemyState.InertiaStand);
            }
            else if (state == EnemyState.Walking)
            {
                // if state was walking and not in target area, get a new target
                if (isInFinalArea && !noIdlingOnTarget)
                {
                    switchState(EnemyState.Idle);
                    currentProcess = StartCoroutine(idleTimer());
                }
                else targetWaypoint();
            }
        }

        if (state == EnemyState.Walking)
        {
            if (distanceToTarget > lastDistance) targetWaypoint(); // this is needed because some enemies would block each other
            else lastDistance = distanceToTarget;
        }

        Vector2 motion = slowed
            ? Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed * 0.8f)
            : Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed);
        body.MovePosition(motion);
    }

    protected void rotate()
    {
        Vector2 motion = Vector2.MoveTowards(transform.position, target, 1);
        GetComponent<SpriteRenderer>().flipX = ((transform.position.x - motion.x) > 0);
    }

    private IEnumerator inertiaRun()
    {
        yield return new WaitForSeconds(5);
        switchState(EnemyState.Walking);
        yield break;
    }

    private IEnumerator inertiaLook()
    {
        currentSpeed = 0;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.5f);
            GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
        }
        switchState(EnemyState.Walking);
        yield break;
    }

    // ---------------- STATE HANDLING ----------------
    protected void switchState(EnemyState state)
    {
        if (this.state == state) return;
        this.state = state;

        animator.ResetTrigger(Trigger.idle);
        animator.ResetTrigger(Trigger.walk);
        animator.ResetTrigger(Trigger.run);

        switch (state)
        {
            case EnemyState.Idle:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.idle);
                break;

            case EnemyState.Walking:
                currentSpeed = slow;
                animator.SetTrigger(Trigger.walk);
                break;

            case EnemyState.Following:
                currentSpeed = fast;
                if (currentProcess != null) StopCoroutine(currentProcess);
                animator.SetTrigger(Trigger.run);
                break;

            case EnemyState.InertiaRun:
                currentSpeed = fast;
                currentProcess = StartCoroutine(inertiaRun());
                break;

            case EnemyState.InertiaStand:
                currentSpeed = 0;
                if (currentProcess != null) StopCoroutine(currentProcess);
                currentProcess = StartCoroutine(inertiaLook());
                animator.SetTrigger(Trigger.idle);
                target = transform.position;
                break;

            case EnemyState.Dying:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.die);
                break;

            case EnemyState.Dead:
                animator.SetTrigger(Trigger.idle);
                break;
        }
    }

    private static class Trigger
    {
        public static readonly String walk = "walk";
        public static readonly String run = "run";
        public static readonly String idle = "idle";
        public static readonly String die = "die";
    }

    private bool movingState()
    {
        return (state == EnemyState.Walking || state == EnemyState.Following || state == EnemyState.InertiaRun);
    }

    // ---------------- TARGETTING ----------------

    protected IEnumerator navigator()
    {
        while (true)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    updateTarget();
                    yield return new WaitForSeconds(0.5f);
                    break;

                case EnemyState.Walking:
                    updateTarget();
                    yield return new WaitForSeconds(0.2f);
                    break;

                case EnemyState.Following:
                    updateTarget();
                    yield return new WaitForSeconds(1f);
                    break;

                case EnemyState.InertiaRun:
                    updateTarget();
                    yield return new WaitForSeconds(0.1f);
                    break;

                case EnemyState.InertiaStand:
                    updateTarget();
                    yield return new WaitForSeconds(0.1f);
                    break;

                default:
                    // if this occurs, navigator was running in a dead/transition state, which should not happen
                    Debug.Log("Unexpected state: " + state.ToString());
                    yield break;
            }
        }
    }

    protected virtual void updateTarget()
    {
        if (manager.charLoS(transform.position))
        {
            target = manager.getCharPos();
            rotate();
            switchState(EnemyState.Following);
        }
        else switch (state)
            {
                case EnemyState.Idle:
                    if (!isInFinalArea) targetWaypoint();
                    break;

                case EnemyState.Following:
                    switchState(EnemyState.InertiaRun);
                    break;
            }
    }

    private void targetWaypoint()
    {
        var result = manager.getWaypoint(transform, isInFinalArea);
        target = result.pos;
        noIdlingOnTarget = result.doNotWait;
        lastDistance = 10000f;
        switchState(EnemyState.Walking);
        rotate();
    }

    private IEnumerator idleTimer()
    {
        float randomTime = Random.Range(5f, 10f);
        yield return new WaitForSeconds(randomTime);
        targetWaypoint();
        yield break;
    }

    // ---------------- COMBAT ----------------
    public virtual void damage(byte amount, DamageType type)
    {
        if (amount == 0) return;
        if (amount < health) health -= amount;
        else
        {
            health = 0;
            if (type == DamageType.LMB || type == DamageType.RMB) mainChar.offerLife();
            die(type);
        }
        healthBar.updateValue(maxHealth, health);
    }

    public int damageMax(DamageType type)
    {
        healthBar.updateValue(maxHealth, 0);
        die(type);
        return health;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == Tag.player)
        {
            mainChar.damage(1);
            die(DamageType.Contact);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.attackLMB))
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            damage(projectile.Hit(true), DamageType.LMB);
        }
        else if (collision.gameObject.CompareTag(Tag.raven))
        {
            isInFinalArea = true;
            raven.register(this);
        }
        else if (collision.gameObject.CompareTag(Tag.crow))
        {
            slowed = crow.register(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.raven))
        {
            isInFinalArea = false;
            raven.deregister(this);
        }
        else if (collision.CompareTag(Tag.crow))
        {
            crow.deregister(this);
            slowed = false;
        }
    }
}