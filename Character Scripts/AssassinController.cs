using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static UnityEngine.EventSystems.EventTrigger;

public class AssassinController : EnemyBase, IFading
{
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private SliderController healthBar;
    [SerializeField] private FightController fight;
    [SerializeField] private GameObject daggerRight;
    [SerializeField] private GameObject daggerLeft;
    [SerializeField] private GameObject bow;
    [SerializeField] private AssassinWeaponController arrow;
    [SerializeField] private GameObject smokeFX1;
    [SerializeField] private GameObject smokeFX2;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;

    private static readonly float OFFSET = 2;
    private Vector2 boundsBottomLeft;
    private Vector2 boundsTopRight;

    // combat values
    private static readonly float ATTACK_RANGE_SQUARED = 5 * 5;
    private static readonly float ATTACK_RANGE_UP_SQUARED = 7 * 7;
    private static readonly float ATTACK_RANGE_DOWN_SQUARED = 5 * 5;
    private static readonly float BLINK_RANGE_SQUARED = 15 * 15;
    private static readonly int MAX_HEALTH = 800;
    private int health = MAX_HEALTH;

    // movement
    private static readonly float SPEED_RUN = 20;
    private static readonly float SPEED_WALK = 10;
    private static readonly float SPEED_SNEAK = 3;
    private Vector2 moveTarget;
    private float currentSpeed = 0;

    // bow positions
    private static readonly Vector3 BOW_RIGHT = new Vector3(0.77f, 2.5f, 0);
    private static readonly Vector3 BOW_LEFT = new Vector3(-0.77f, 2.5f, 0);

    private State currentState;
    private Coroutine sneakProcess;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        boundsBottomLeft = smokeFX1.transform.position;
        boundsTopRight = smokeFX2.transform.position;
    }

    public void testPrint()
    {
        StartCoroutine(approach());
    }

    // ----------- DIRECTIONS -----------

    private Vector2 position2d()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }

    // for movement
    private Vector2 getDiffToPlayer()
    {
        Vector2 target = mainChar.transform.position;

        return target - position2d();
    }

    // for targetting
    private Vector2 getDiffToPlayerOffset()
    {
        Vector2 currentPos = new Vector2(transform.position.x, transform.position.y + OFFSET);
        Vector2 target = mainChar.transform.position;

        return target - currentPos;
    }

    private Vector2 normaliseVector(Vector2 heading)
    {
        float distance = heading.magnitude;
        return heading / distance * 5;
    }

    private Vector2 perpendicular(Vector2 heading)
    {
        return new Vector2(heading.y, -heading.x);
    }

    private Direction getCardinalDirection(Vector2 heading)
    {
        if (Mathf.Abs(heading.x) > Mathf.Abs(heading.y))
            return heading.x > 0 ? Direction.Right : Direction.Left;
        else
            return heading.y > 0 ? Direction.Up : Direction.Down;
    }

    private Vector2? getSidestepPosition(Vector2 position, Vector2 normalisedHeading)
    {
        bool dir = flipACoin();
        Vector2 movement = perpendicular(normalisedHeading);
        
        // try first direction
        Vector2 moveTarget = dir ? position + movement : position - movement;
        if (checkPositionBounds(moveTarget)) return moveTarget;

        // try second direction
        moveTarget = dir ? position - movement : position + movement;
        if (checkPositionBounds(moveTarget)) return moveTarget;

        return null;
    }

    private bool isInRange(float sqRange)
    {
        float dist = getDiffToPlayerOffset().sqrMagnitude;
        return dist < sqRange;
    }

    private bool isInAttackDistance(Direction dir)
    {
        float targetRange;
        switch (dir)
        {
            case Direction.Up:
                targetRange = ATTACK_RANGE_UP_SQUARED; break;
            case Direction.Down:
                targetRange = ATTACK_RANGE_DOWN_SQUARED; break;
            default:
                targetRange = ATTACK_RANGE_SQUARED; break;
        }
        return getDiffToPlayerOffset().sqrMagnitude < targetRange;
    }

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    // ----------- MOVEMENT -----------
    private void FixedUpdate()
    {
        if (currentSpeed == 0) return;
        
        body.MovePosition(Vector2.MoveTowards(body.position, moveTarget, Time.deltaTime * currentSpeed));
        rotateToPlayer();
    }

    private void LateUpdate()
    {
        if (currentState == State.Shooting)
        {
            // turns the bow which is being controlled by animator
            bow.transform.right = mainChar.transform.position - bow.transform.position;
            return;
        }
    }

    private float moveSideways(Vector2? reuseVector)
    {
        Vector2 primaryMovement = reuseVector.HasValue ?
            reuseVector.Value :
            normaliseVector(getDiffToPlayerOffset());

        Vector2? sideMoveTarget = getSidestepPosition(position2d(), primaryMovement);
        if (sideMoveTarget.HasValue)
        {
            moveTarget = sideMoveTarget.Value;
            return Vector2.Distance(position2d(), sideMoveTarget.Value) / currentSpeed;
        }

        StartCoroutine(blink(fallbackPosition()));
        return 0.2f;
    }

    private float kingMove(bool backward)
    {
        Vector2 primaryMovement = normaliseVector(getDiffToPlayerOffset());
        if (backward) primaryMovement *= -1;
        Vector2 primaryMoveTarget = position2d() + primaryMovement;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            // get side position if player is attacking
            Vector2? sideMoveTarget = getSidestepPosition(primaryMoveTarget, primaryMovement);
            if (sideMoveTarget.HasValue)
            {
                moveTarget = sideMoveTarget.Value;
                return Vector2.Distance(position2d(), sideMoveTarget.Value) / currentSpeed;
            }
        }
        else
        {
            // otherwise move forward/backward
            if (checkPositionBounds(primaryMoveTarget))
            {
                moveTarget = primaryMoveTarget;
                return primaryMovement.magnitude / currentSpeed;
            }
        }
        // if not viable, move simply sideways (handles fallback)
        return moveSideways(primaryMovement);
    }

    private void setTargetToPlayer()
    {
        moveTarget = mainChar.transform.position;
        moveTarget.y -= OFFSET;
    }

    private void rotateToPlayer()
    {
        if (mainChar.transform.position.x > transform.position.x)
        {
            if (spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
                daggerRight.SetActive(true);
                daggerLeft.SetActive(false);
            }
        }
        else if (!spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
            daggerRight.SetActive(false);
            daggerLeft.SetActive(true);
        }
    }

    private bool checkPositionBounds(Vector2 target)
    {
        if (target.x < boundsBottomLeft.x ||
            target.x > boundsTopRight.x ||
            target.y < boundsBottomLeft.y ||
            target.y > boundsTopRight.y) return false;
        return true;
    }

    private Vector2 fallbackPosition()
    {
        return transform.parent.position;
    }

    // ----------- STATES -----------
    private void switchState(State state)
    {
        if (state == currentState) return;
        currentState = state;

        switch (state)
        {
            case State.Still:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.IDLE);
                break;

            case State.Shooting:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.IDLE);
                break;

            case State.Sneaking:
                currentSpeed = SPEED_SNEAK;
                break;

            case State.Approaching:
                currentSpeed = SPEED_WALK;
                animator.SetTrigger(Trigger.WALK);
                break;

            case State.Retreating:
                currentSpeed = SPEED_WALK;
                animator.SetTrigger(Trigger.WALK);
                break;

            case State.Charging:
                currentSpeed = SPEED_RUN;
                animator.SetTrigger(Trigger.RUN);
                break;

            case State.Death:
                currentSpeed = 0;
                StopAllCoroutines();
                GetComponent<CapsuleCollider2D>().enabled = false;
                animator.SetTrigger(Trigger.DIE);
                break;
        }
    }

    public void afterFadeIn()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
        sneakProcess = StartCoroutine(sneakAttack());
    }

    public void afterFadeOut()
    {
        fight.registerTakedown();
    }

    private enum State
    {
        Spawning,
        Sneaking,
        Still,
        Approaching,
        Retreating,
        Charging,
        Shooting,
        Death
    }

    // ----------- COMBAT -----------

    private IEnumerator blink(Vector2 blinkTarget)
    {
        // setup positions and start animations
        Vector2 blinkStart = position2d();
        blinkStart.y += (OFFSET + 0.5f);
        smokeFX1.transform.position = blinkStart;
        smokeFX1.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        Vector2 smokeBlinkTarget = blinkTarget;
        smokeBlinkTarget.y += (OFFSET + 0.5f);
        smokeFX2.transform.position = smokeBlinkTarget;
        smokeFX2.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        switchState(State.Still);
        smokeFX1.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        smokeFX2.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);

        // move after a small delay
        yield return new WaitForSeconds(0.1f);
        transform.position = blinkTarget;
        yield return null;
        rotateToPlayer();
        yield break;
    }

    private IEnumerator specialAttack()
    {
        setTargetToPlayer();
        switchState(State.Charging);

        // run towards player
        while (!isInRange(BLINK_RANGE_SQUARED))
        {
            yield return new WaitForSeconds(0.1f);
            setTargetToPlayer();
        }

        // see if blink strike can be performed
        Vector2 heading = getDiffToPlayerOffset();

        Vector2 forwardTarget = heading * 2 + position2d();
        if (checkPositionBounds(forwardTarget))
        {
            StartCoroutine(blinkStrike(forwardTarget));
            yield break;
        }

        // blink back or to fallback and shoot
        Vector2 backTarget = position2d() - heading;
        if (checkPositionBounds(backTarget)) yield return blink(backTarget);
        else yield return blink(fallbackPosition());
        StartCoroutine(shootBow());
        yield break;
    }

    private IEnumerator blinkStrike(Vector2 blinkTarget)
    {
        yield return blink(blinkTarget);

        // continue running at the existing target
        switchState(State.Charging);
        yield return new WaitForSeconds(Vector2.Distance(position2d(), moveTarget) / currentSpeed * 0.8f);
        yield return stabAtPlayer();
        StartCoroutine(retreatThenApproach());
        yield break;
    }

    private IEnumerator stabAtPlayer()
    {
        animator.SetLayerWeight(1, 1);
        Direction dir = getCardinalDirection(getDiffToPlayerOffset());
        switch (dir)
        {
            case Direction.Up:
                animator.SetTrigger("stabUp");
                break;

            case Direction.Down:
                animator.SetTrigger("stabDown");
                break;

            default:
                animator.SetTrigger("stabSide");
                break;
        }
        yield return new WaitForSeconds(0.6f);
        animator.SetLayerWeight(1, 0);
        yield break;
    }

    private IEnumerator shootBow()
    {
        // set bow position and continuously rotation
        rotateToPlayer();
        bow.transform.position = transform.position + (spriteRenderer.flipX ? BOW_LEFT : BOW_RIGHT);
        switchState(State.Shooting);

        // hide daggers
        if (spriteRenderer.flipX) daggerLeft.SetActive(false);
        else daggerRight.SetActive(false);

        // draw bow
        bow.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        yield return new WaitForSeconds(0.5f);
        // shoot arrow
        Vector3 direction = mainChar.transform.position - bow.transform.position;
        switchState(State.Still);
        arrow.shoot(normaliseVector(direction));
        yield return new WaitForSeconds(0.5f);

        // show daggers
        if (spriteRenderer.flipX) daggerLeft.SetActive(true);
        else daggerRight.SetActive(true);
        StartCoroutine(approachThenRetreat());
        yield break;
    }

    private IEnumerator sneakAttack()
    {
        setTargetToPlayer();
        switchState(State.Sneaking);

        while (!isInAttackDistance(getCardinalDirection(getDiffToPlayerOffset())))
        {
            yield return new WaitForSeconds(0.1f);
            setTargetToPlayer();
        }
        switchState(State.Still);
        yield return stabAtPlayer();
        StartCoroutine(retreatThenApproach());
        yield break;
    }

    private IEnumerator retreat()
    {
        float timer = 0;
        while (timer < 9)
        {
            switchState(State.Retreating); // must be called every time in case of fallback blink
            float moveTime = (Random.Range(0, 5) > 0) ? kingMove(true) : moveSideways(null);
            timer += moveTime;
            yield return new WaitForSeconds(moveTime);
        }
        yield break;
    }
    private IEnumerator approach()
    {
        Coroutine aggression = StartCoroutine(aggressiveRoutine());
        float timer = 0;
        while (timer < 9)
        {
            switchState(State.Approaching); // must be called every time in case of fallback blink
            float moveTime = (Random.Range(0, 5) > 0) ? kingMove(false) : moveSideways(null);
            timer += moveTime;
            yield return new WaitForSeconds(moveTime);
        }
        StopCoroutine(aggression);
        yield break;
    }

    private IEnumerator aggressiveRoutine()
    {
        while (true)
        {
            if (isInAttackDistance(getCardinalDirection(getDiffToPlayerOffset())))
            {
                StartCoroutine(stabAtPlayer());
                yield return new WaitForSeconds(1);
            }
            else yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator retreatThenApproach()
    {
        yield return retreat();
        yield return approach();
        StartCoroutine(specialAttack());
        yield break;
    }

    private IEnumerator approachThenRetreat()
    {
        yield return approach();
        yield return retreat();
        StartCoroutine(specialAttack());
        yield break;
    }

    public override void damage(byte amount, DamageType type)
    {
        if (type == DamageType.RMB && flipACoin()) return;
        health -= amount;
        if (health < 0) health = 0;
        healthBar.updateValue(MAX_HEALTH, health);
        if (currentState == State.Sneaking)
        {
            if (sneakProcess != null) StopCoroutine(sneakProcess);
            StartCoroutine(shootBow());
        }
        else if (health == 0) switchState(State.Death);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.ATTACK_LMB))
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            damage(projectile.Hit(true), DamageType.LMB);
        }
    }

    public override void heal(byte amount)
    {
        // no healing
    }
}