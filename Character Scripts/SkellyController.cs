using System.Collections;
using UnityEngine;
using static GameValues;
using Random = UnityEngine.Random;
using static ScenePersistence;

public class SkellyController : EnemyBase, IFading
{
    // pointers
    [SerializeField] protected SliderController healthBar;
    [SerializeField] protected Rigidbody2D body;
    [SerializeField] private Animator animator;
    protected MainCharacterSheet mainChar;
    protected ISpawnerHandler handler;
    private LevelManager manager;
    private RavenController raven;
    private CrowController crow;

    // movement
    protected Vector2 target;
    private bool isInFinalArea = false;
    private bool noIdlingOnTarget = true;

    private static readonly float DISTANCE_TOLERANCE = 0.2f * 0.2f;
    protected byte currentSpeed = 0;
    private float lastDistance = 10000f;
    private byte distanceCountdown = 10;

    private static readonly byte SPEED_FAST = 10;
    private static readonly byte SPEED_SLOW = 6;

    private bool slowed = false;

    // combat
    private int maxHealth;
    public int Health {  get; private set; }

    // processing
    private EnemyState state = EnemyState.None;
    private Coroutine currentProcess;

    // ---------------- LIFECYCLE ----------------
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        body.simulated = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
    }

    public void init(MainCharacterSheet character, LevelManager levelManager, ISpawnerHandler handler, RavenController ravenController, CrowController crowController)
    {
        manager = levelManager;
        mainChar = character;
        this.handler = handler;
        raven = ravenController;
        crow = crowController;

        lastDistance = 10000f;
        isInFinalArea = false;
    }

    public void spawn(Vector2 pos, int hp)
    {
        transform.position = pos;
        maxHealth = hp;
        Health = hp;
        healthBar.hide();

        switchState(EnemyState.Spawning);
    }

    // called by spawn animation
    public virtual void afterFadeIn()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        switchState(EnemyState.Idle);
        StartCoroutine(navigator());
    }

    public void obliterate()
    {
        if (state == EnemyState.Spawning)
        {
            StopAllCoroutines();
            afterFadeOut();
        } else if (state == EnemyState.Dead || state == EnemyState.Dying)
        {
            return;
        } else
        {
            damageMax(DamageType.None);
        }
    }

    // disables collision and starts death animation
    protected virtual void die(DamageType damageType)
    {
        StopAllCoroutines();
        manager.addScore(damageType);
        if (raven != null) raven.deregister(this);
        if (crow != null) crow.deregister(this);
        slowed = false;

        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;
        handler.removeFromLiving(gameObject);

        switchState(EnemyState.Dying);
    }

    // called by animation event, followed by fade animation
    private void deathSecondStage()
    {
        healthBar.fade();
    }

    public virtual void afterFadeOut()
    {
        switchState(EnemyState.Dead);
        reuseEnemy(gameObject);
    }

    // ---------------- MOVEMENT ----------------
    protected virtual void FixedUpdate()
    {
        if (!inMovingState())
        {
            return;
        }

        float? distanceToTarget = getDistanceToTargetSquared();

        // if target is reached...
        if (distanceToTarget.HasValue && distanceToTarget.Value <= DISTANCE_TOLERANCE)
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

        if (distanceToTarget.HasValue && state == EnemyState.Walking)
        {
            if (distanceToTarget.Value > lastDistance) targetWaypoint(); // this is needed because some enemies would block each other
            else lastDistance = distanceToTarget.Value;
        }

        Vector2 motion = slowed
            ? Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed * 0.8f)
            : Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed);
        body.MovePosition(motion);
    }

    protected virtual void rotate()
    {
        GetComponent<SpriteRenderer>().flipX = ((transform.position.x - target.x) > 0);
    }

    // for efficiency, distance doesn't need to be checked every frame
    private float? getDistanceToTargetSquared()
    {
        if (state == EnemyState.Following) return 0;
        if (distanceCountdown == 0)
        {
            distanceCountdown = 10;
            Vector2 currentPos = transform.position;
            return (target - currentPos).sqrMagnitude;
        }
        distanceCountdown--;
        return null;
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
        if (state == EnemyState.Dying && !inLivingState()) return; // guarantee that there is no hanging die trigger
        this.state = state;

        switch (state)
        {
            case EnemyState.Spawning:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.FADE_IN);
                break;

            case EnemyState.Idle:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.IDLE);
                break;

            case EnemyState.Walking:
                currentSpeed = SPEED_SLOW;
                animator.SetTrigger(Trigger.WALK);
                break;

            case EnemyState.Following:
                currentSpeed = SPEED_FAST;
                if (currentProcess != null) StopCoroutine(currentProcess);
                animator.SetTrigger(Trigger.RUN);
                break;

            case EnemyState.InertiaRun:
                currentProcess = StartCoroutine(inertiaRun());
                break;

            case EnemyState.InertiaStand:
                currentSpeed = 0;
                if (currentProcess != null) StopCoroutine(currentProcess);
                currentProcess = StartCoroutine(inertiaLook());
                animator.SetTrigger(Trigger.IDLE);
                target = transform.position;
                break;

            case EnemyState.Dying:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.DIE);
                break;

            case EnemyState.Dead:
                break;
        }
    }

    protected bool inMovingState()
    {
        return (state == EnemyState.Walking || state == EnemyState.Following || state == EnemyState.InertiaRun);
    }

    protected bool inLivingState()
    {
        if (state == EnemyState.Spawning || state == EnemyState.Dying || state == EnemyState.Dead) return false;
        return true;
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
    public override void damage(byte amount, DamageType type)
    {
        if (amount == 0) return;
        if (amount < Health) Health -= amount;
        else
        {
            Health = 0;
            if (type == DamageType.LMB || type == DamageType.RMB && mainChar != null) mainChar.offerLife();
            die(type);
        }
        healthBar.updateValue(maxHealth, Health);
    }

    public int damageMax(DamageType type)
    {
        healthBar.updateValue(maxHealth, 0);
        die(type);
        return Health;
    }

    public override void heal(byte amount)
    {
        // not applicable
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == Tag.PLAYER)
        {
            mainChar.damage(1);
            die(DamageType.Contact);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.ATTACK_LMB))
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            damage(projectile.Hit(true), DamageType.LMB);
        }
        else if (collision.gameObject.CompareTag(Tag.RAVEN))
        {
            isInFinalArea = true;
            raven.register(this);
        }
        else if (collision.gameObject.CompareTag(Tag.CROW))
        {
            slowed = crow.register(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.RAVEN))
        {
            isInFinalArea = false;
            raven.deregister(this);
        }
        else if (collision.CompareTag(Tag.CROW))
        {
            crow.deregister(this);
            slowed = false;
        }
    }
}