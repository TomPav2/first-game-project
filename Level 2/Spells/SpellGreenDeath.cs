using UnityEngine;
using static GameValues;

public class SpellGreenDeath : Spell
{
    [SerializeField] private Transform targetterCentre;
    [SerializeField] private Transform targetterLeft;
    [SerializeField] private Transform targetterRight;
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private BossFightController bossfight;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CircleCollider2D circleCollider;

    private static readonly float SPEED = 20;
    private static readonly Vector2 START_OFFSET = new Vector2(-1.5f, 1.5f);

    private bool active = false;
    private Transform targetter;
    private bool movementType;
    // true - targetterCentre, move only X axis
    // faslse - left and right, movy onyly Y axis

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();

        channeled = false;
        manacost = 0;
    }

    private void FixedUpdate()
    {
        if (!active) return;

        updateTargetter(targetter, movementType);
        //transform.position = Vector2.MoveTowards(transform.position, targetter.position, Time.deltaTime * SPEED);
        transform.position += curvedMove(transform.position, targetter.position);
    }

    public override void cast(Vector2 startPos)
    {
        // select target
        updateTargetter(targetterLeft, false);
        updateTargetter(targetterRight, false);
        setUpTarget();

        // set spell in motion
        transform.position = startPos + START_OFFSET;
        spriteRenderer.enabled = true;
        animator.enabled = true;
        circleCollider.enabled = true;
        active = true;
    }

    private void hide()
    {
        spriteRenderer.enabled = false;
        animator.enabled = false;
        circleCollider.enabled = false;
        active = false;
    }

    private void updateTargetter(Transform target, bool horizontal)
    {
        target.position = horizontal
            ? new Vector3(mainChar.transform.position.x, target.position.y, 0)
            : new Vector3(target.position.x, mainChar.transform.position.y, 0);
    }

    private void setUpTarget()
    {
        if (mainChar.transform.position.y < 200 ||
            (mainChar.transform.position.x > -15 &&
            mainChar.transform.position.x < 21))
        {
            targetter = targetterCentre;
            movementType = true;
            return;
        }
        
        float left = Vector3.Distance(targetterLeft.position, mainChar.transform.position);
        float right = Vector3.Distance(targetterRight.position, mainChar.transform.position);
        movementType = false;
        targetter = left < right ? targetterLeft : targetterRight;
    }

    private Vector3 curvedMove(Vector2 pos, Vector2 target)
    {
        Vector2 movement = target - pos;
        movement = Time.deltaTime * SPEED * movement.normalized;
        if (movementType) movement.x *= 3;
        else movement.y *= 3;
        return movement;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            mainChar.damage(1);
        }
        else if (collision.CompareTag(Tag.BONUS))
        {
            bossfight.hitIntroBox();
        }
        hide();
    }

    // not a channeled spell
    public override byte channel()
    {
        throw new System.NotImplementedException();
    }

    public override void startChanneling()
    {
        throw new System.NotImplementedException();
    }

    public override void stopChanneling()
    {
        throw new System.NotImplementedException();
    }
}
