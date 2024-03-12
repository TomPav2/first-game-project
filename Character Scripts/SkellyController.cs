using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static GameValues;
using Unity.VisualScripting.Antlr3.Runtime;

public class SkellyController : MonoBehaviour
{
    // pointers
    [SerializeField] private HealthBarController healthBar;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;
    [SerializeField] private GameManager manager;
    private MainCharacterSheet mainChar;

    // movement
    private Vector2 target;
    private float distanceTolerance = 0.2f;
    private byte currentSpeed = 0;

    private readonly byte fast = 7;
    private readonly byte slow = 4;

    // combat
    private float maxHealth;
    private float health;

    // processing
    private EnemyState state = EnemyState.None;

    private Coroutine currentProcess;
    private Coroutine navigatorProcess;

    // ---------------- LIFECYCLE ----------------
    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBarController>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GetComponent<SpriteRenderer>().enabled = false;
        body.simulated = false;
        animator.enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
    }

    public void init(MainCharacterSheet character, GameManager gameManager)
    {
        manager = gameManager;
        mainChar = character;
    }
    public void spawn(Vector2 pos, float hp)
    {
        transform.position = pos;
        maxHealth = hp;
        health = hp;
        currentProcess = StartCoroutine(performSpawn());
    }

    private void die()
    {
        if (currentProcess != null) StopCoroutine(currentProcess);
        if (navigatorProcess != null) StopCoroutine(navigatorProcess);

        StartCoroutine(performDeath());
    }

    private IEnumerator performSpawn()
    {
        switchState(EnemyState.Spawning);
        GetComponent<SpriteRenderer>().enabled = true;

        Color color = Color.white;
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 0.1f;
            color.a = alpha;
            GetComponent<SpriteRenderer>().color = color;
            yield return new WaitForSeconds(0.1f);
        }

        animator.enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        switchState(EnemyState.Idle);
        animator.ResetTrigger("idle"); // not needed after spawn
        navigatorProcess = StartCoroutine(navigator());
        yield break;
    }

    private IEnumerator performDeath()
    {
        switchState(EnemyState.Dying);
        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;
        yield return new WaitForSeconds(1);

        animator.enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        healthBar.show(false);
        switchState(EnemyState.Dead);
        //spawnManager.makeAvailable(this);
        yield break;
    }

    // ---------------- MOVEMENT ----------------
    private void FixedUpdate()
    {
        if (!movingState())
        {
            return;
        }

        // if target is reached...
        if (Vector2.Distance(transform.position, target) <= distanceTolerance)
        {
            if (state == EnemyState.InertiaWalk)
            {
                // if state was inertia, stop and look around
                switchState(EnemyState.InertiaStand);
            }
            else if (state == EnemyState.Walking)
            {
                // if state was walking, get a new target
                targetWaypoint();
            }
        }
        // TODO random location in target area

        Vector2 motion = Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed);
        body.MovePosition(motion);
    }

    private void rotate()
    {
        Vector2 motion = Vector2.MoveTowards(transform.position, target, 1);
        GetComponent<SpriteRenderer>().flipX = ((transform.position.x - motion.x) > 0);
    }

    private IEnumerator inertiaWalk()
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
    private void switchState(EnemyState state)
    {
        if (this.state == state) return;
        this.state = state;

        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("run");

        switch (state)
        {
            case EnemyState.Idle:
                currentSpeed = 0;
                animator.SetTrigger("idle");
                break;

            case EnemyState.Walking:
                currentSpeed = slow;
                animator.SetTrigger("walk");
                break;

            case EnemyState.Following:
                currentSpeed = fast;
                if (currentProcess != null) StopCoroutine(currentProcess);
                animator.SetTrigger("run");
                break;

            case EnemyState.InertiaWalk:
                currentSpeed = fast;
                currentProcess = StartCoroutine(inertiaWalk());
                animator.SetTrigger("run");
                break;

            case EnemyState.InertiaStand:
                currentSpeed = 0;
                if (currentProcess != null) StopCoroutine(currentProcess);
                currentProcess = StartCoroutine(inertiaLook());
                animator.SetTrigger("idle");
                target = transform.position;
                break;

            case EnemyState.Dying:
                currentSpeed = 0;
                animator.SetTrigger("die");
                break;

            case EnemyState.Dead:
                animator.SetTrigger("idle");
                break;
        }
    }

    private bool movingState()
    {
        return (state == EnemyState.Walking || state == EnemyState.Following || state == EnemyState.InertiaWalk);
    }

    // ---------------- TARGETTING ----------------


    private IEnumerator navigator()
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

                case EnemyState.InertiaWalk:
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
    
    private void updateTarget()
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
                    if (!isInFinalArea())
                    {
                        targetWaypoint();
                    }
                    break;

                case EnemyState.Following:
                    switchState(EnemyState.InertiaWalk);
                    break;
            }
    }

    private void targetWaypoint()
    {
        //TODO
        //rotate();
    }

    private bool isInFinalArea()
    {
        //TODO
        return false;
    }

    // ---------------- COMBAT ----------------
    private void takeDamage(byte amount, DamageType type)
    {
        if (amount == 0) return;
        if (amount < health) health -= amount;
        else
        {
            health = 0;
            if (type == DamageType.LMB || type == DamageType.RMB) mainChar.offerLife();
            die();
        }
        healthBar.show(true);
        healthBar.updateHealth(maxHealth, health);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == Tag.player)
        {
            mainChar.damage(1);
            die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == Tag.AttackLMB)
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            takeDamage(projectile.Hit(true), DamageType.LMB);
        }
    }
}