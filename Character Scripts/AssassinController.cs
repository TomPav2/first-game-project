using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static UnityEngine.EventSystems.EventTrigger;

public class AssassinController : EnemyBase, IFading
{
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private GameObject daggerRight;
    [SerializeField] private GameObject daggerLeft;
    [SerializeField] private GameObject bow;
    [SerializeField] private AssassinWeaponController arrow;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;

    private static readonly float OFFSET = 2;
    private Vector2 boundsBottomLeft;
    private Vector2 boundsTopRight;

    // combat values
    private static readonly float ATTACK_RANGE_SQUARED = 5 * 5;
    private static readonly float ATTACK_RANGE_UP_SQUARED = 8 * 8;
    private static readonly float ATTACK_RANGE_DOWN_SQUARED = 6 * 6;
    private static readonly float BLINK_RANGE_SQUARED = 15 * 15;
    private static readonly int MAX_HEALTH;
    private int health = MAX_HEALTH;

    // movement
    private static readonly float SPEED_RUN = 10;
    private static readonly float SPEED_WALK = 5;
    private static readonly float SPEED_SNEAK = 3;
    private Vector2 moveTarget;
    private float currentSpeed = 0;

    // bow positions
    private static readonly Vector3 BOW_RIGHT = new Vector3(0.77f, 2.5f, 0);
    private static readonly Vector3 BOW_LEFT = new Vector3(-0.77f, 2.5f, 0);

    private State currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
    }

    public void testPrint()
    {
        StartCoroutine(shootBow());
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
        return new Vector2(heading.y, - heading.x);
    }

    private Direction getCardinalDirection(Vector2 heading)
    {
        if (Mathf.Abs(heading.x) > Mathf.Abs(heading.y))
            return heading.x > 0 ? Direction.Right : Direction.Left;
        else
            return heading.y > 0 ? Direction.Up : Direction.Down;
    }

    private Vector2? getSidestepPosition()
    {
        // coinflip
        // check it
        // return or
        // do the other one
        // check it
        // return or
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
            bow.transform.right = mainChar.transform.position - bow.transform.position;
            return;
        }
    }

    private void moveForward()
    {
        if (Input.GetMouseButton(0))
        {
            // get side position
        }
    }

    private void stayStill()
    {
        if (Input.GetMouseButton(0))
        {
            // get side position
        }
    }
    private void moveBack()
    {
        // todo
    }

    private Vector2 calculatePrimaryDirection()
    {
        return Vector2.one; // TODO
    }

    private void setTargetToPlayer()
    {
        moveTarget = mainChar.transform.position;
        moveTarget.y -= OFFSET;
    }

    private void rotateToPlayer()
    {
        if ( mainChar.transform.position.x > transform.position.x)
        {
            if ( spriteRenderer.flipX )
            {
                spriteRenderer.flipX = false;
                daggerRight.SetActive(true);
                daggerLeft.SetActive(false);
            }
        } else if ( !spriteRenderer.flipX )
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

    // ----------- STATES -----------
    private void switchState(State state)
    {
        if (state == currentState) return;
        currentState = state;

        switch(state)
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
                break;
            case State.Retreating:
                currentSpeed = SPEED_WALK;
                break;
            case State.Charging:
                currentSpeed = SPEED_RUN;
                animator.SetTrigger(Trigger.RUN);
                break;

            case State.Death:
                currentSpeed = 0;
                animator.SetTrigger(Trigger.DIE);
                break;
        }
    }

    public void afterFadeIn()
    {
        //StartCoroutine(sneakAttack());
        // todo temporary for dev
        switchState(State.Still);
    }

    public void afterFadeOut()
    {
        throw new System.NotImplementedException();
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

    private IEnumerator chargeAttackProcess()
    {
        setTargetToPlayer();
        switchState(State.Charging);

        // run towards player
        while (!isInRange(BLINK_RANGE_SQUARED))
        {
            yield return new WaitForSeconds(0.1f);
            setTargetToPlayer();
        }

        // blink behind player
        currentSpeed = 0;
        Vector2 blinkTarget = getDiffToPlayerOffset() * 2 + position2d();
        blinkTarget.y -= OFFSET;
        body.MovePosition(blinkTarget);
        yield return null;
        currentSpeed = SPEED_RUN;

        // continue running at the existing target
        yield return new WaitForSeconds(Vector2.Distance(position2d(), moveTarget) / currentSpeed * 0.8f);
        StartCoroutine(stabAtPlayer());
        yield break;
    }

    private IEnumerator stabAtPlayer()
    {
        animator.SetLayerWeight(1, 1);
        Direction dir = getCardinalDirection(getDiffToPlayerOffset());
        switch(dir)
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
        animator.SetLayerWeight(1, 0); // TODO retreat
        switchState(State.Still);
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
        // TODO
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
        StartCoroutine(stabAtPlayer());
        yield break;
    }

    public override void damage(byte amount, DamageType type)
    {
        if (type == DamageType.RMB && flipACoin()) return;
        health -= amount;
        if (health < 0) health = 0;
        // todo healthbar and death
    }

    public override void heal(byte amount)
    {
        // no healing
    }
}