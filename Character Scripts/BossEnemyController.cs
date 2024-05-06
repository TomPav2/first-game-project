using System.Collections;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

public class BossEnemyController : EnemyBase
{
    // pointers
    [SerializeField] private BattleArenaController arena;
    [SerializeField] private SliderController healthBar;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Spell primarySpell;
    [SerializeField] private Spell secondarySpell;
    [SerializeField] private GameObject introEffect;

    private Animator animator;
    private Rigidbody2D body;
    // stats
    private short maxHealth = Difficulty.BOSS_ENEMY_HEALTH;
    private short health = Difficulty.BOSS_ENEMY_HEALTH;
    private int mana = 50;
    private bool hasAltSpell = true;

    // movement
    private Vector2 target;
    private static readonly float SPEED = 4;
    private static readonly Vector3 LEFT = new Vector3(-1, 1, 1);
    private static readonly Vector3 RIGHT = new Vector3(1, 1, 1);

    private State state = State.Waiting;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    public short getHealth() { return health; }

    // ---------------- MOVEMENT ----------------
    private void FixedUpdate()
    {
        if (state == State.Walking)
        {
            body.MovePosition(Vector2.MoveTowards(body.position, target, SPEED * Time.deltaTime));
        }
    }

    protected void rotate()
    {
        bool rotateLeft = (transform.position.x - target.x) > 0;
        transform.localScale = rotateLeft ? LEFT : RIGHT;
        healthBar.transform.localScale = new Vector3(rotateLeft ? -0.25f : 0.25f, 1, 1);
    }

    private IEnumerator movementRoutine()
    {
        switchState(State.Walking);
        target = arena.getWaypoint(transform.position);
        rotate();
        while (state != State.Death)
        {
            float distanceToTarget = Vector2.Distance(target, transform.position);
            if (distanceToTarget < 2)
            {
                target = arena.getWaypoint(transform.position);
                rotate();
                distanceToTarget = Vector2.Distance(target, transform.position);
            }
            yield return new WaitForSeconds(distanceToTarget / SPEED);
        }
        yield break;
    }

    // ---------------- LIFECYCLE ----------------
    public void showOffEffect()
    {
        introEffect.SetActive(true);
        Animator introAnim = introEffect.GetComponent<Animator>();
        if (introAnim != null) introAnim.SetTrigger(Trigger.ANIMATION_START);
    }

    public void beginFight()
    {
        if (introEffect != null) Destroy(introEffect);
        StartCoroutine(castingRoutine());
        if (arena.hasWaypoints) StartCoroutine(movementRoutine());
    }

    private void die()
    {
        switchState(State.Death);
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
        GetComponentInParent<FightController>().registerTakedown();
    }

    // called by animation
    private void stopParticles()
    {
        particles.Stop();
        healthBar.fade();
    }

    // called by animation
    private void deathAnimationFinished()
    {
        Destroy(gameObject);
    }

    private void switchState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.Waiting:
                animator.SetTrigger(Trigger.IDLE);
                break;

            case State.Walking:
                animator.SetTrigger(Trigger.WALK);
                break;

            case State.Casting:
                animator.SetTrigger(Trigger.IDLE);
                break;

            case State.Death:
                animator.SetTrigger(Trigger.DIE);
                break;
        }
    }

    private enum State
    {
        Waiting,
        Walking,
        Casting,
        Death
    }

    // ---------------- COMBAT OFFENSIFE ----------------

    public void unlockAltSpell()
    {
        hasAltSpell = secondarySpell != null;
    }

    private void attemptCast(Spell spell)
    {
        if (state != State.Casting && spell != null && spell.manacost <= mana)
        {
            StartCoroutine(castSpell(spell));
            mana -= spell.manacost;
        }
    }

    private IEnumerator castSpell(Spell spell)
    {
        State startingState = state;
        switchState(State.Casting);

        yield return new WaitForSeconds(1);
        spell.cast(particles.transform.position);
        yield return new WaitForSeconds(1);

        switchState(startingState);
        yield break;
    }

    private IEnumerator castingRoutine()
    {
        if (primarySpell.channeled) primarySpell.startChanneling();

        while (state != State.Death)
        {
            if (primarySpell.channeled)
            {
                mana += (5 - primarySpell.channel());
                attemptCast(secondarySpell);
            }
            else
            {
                mana += 5;
                if (hasAltSpell) attemptCast(flipACoin() ? primarySpell : secondarySpell);
                else attemptCast(primarySpell);
            }

            yield return new WaitForSeconds(1);
        }

        if (primarySpell.channeled) primarySpell.stopChanneling();
        yield break;
    }

    // ---------------- COMBAT DEFENSIVE ----------------
    public override void damage(byte amount, DamageType type)
    {
        if (amount == 0) return;
        if (amount < health) health -= amount;
        else
        {
            health = 0;
            die();
        }
        healthBar.updateValue(maxHealth, health);
    }

    public override void heal(byte amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        healthBar.updateValue(maxHealth, health);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.ATTACK_LMB))
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            damage(projectile.Hit(true), DamageType.LMB);
        }
    }
}