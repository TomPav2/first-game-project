using UnityEngine;
using static GameValues;

public class SpellProjectileHeal : Spell
{
    [SerializeField] private MainCharacterSheet mainchar;
    [SerializeField] private GameObject bossTarget;
    [SerializeField] private LayerMask collideLayers;

    private static readonly float SPEED = 16;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PolygonCollider2D polygonCollider;

    private bool active = false;
    private bool healing = true;
    private Vector3 movement = Vector3.zero;

    private void Awake()
    {
        manacost = 50;
        channeled = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!active) return;

        transform.position += (SPEED * Time.deltaTime * movement);
    }

    public override void cast(Vector2 startPos)
    {
        transform.position = startPos;
        spriteRenderer.enabled = true;
        animator.enabled = true;
        polygonCollider.enabled = true;
        setMovement();
        active = true;
    }

    private void hide()
    {
        spriteRenderer.enabled = false;
        animator.enabled = false;
        polygonCollider.enabled = false;
        active = false;
    }

    private Vector3 getTarget()
    {
        Vector3 heading = bossTarget.transform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, heading, Mathf.Infinity, collideLayers);

        if (hit.collider.CompareTag(Tag.PLAYER))
        {
            healing = false;
            animator.SetTrigger("damage");
            return mainchar.transform.position;
        }
        healing = true;
        animator.SetTrigger("heal");
        return bossTarget.transform.position;
    }

    private void setMovement()
    {
        Vector3 heading = getTarget() - transform.position;
        movement = heading / heading.magnitude;
        transform.right = heading;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            if (healing) mainchar.heal(1);
            else mainchar.damage(1);
        }
        else if (collision.CompareTag(Tag.ENEMY)) {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                if (healing) enemy.heal(100);
                else enemy.damage(100, DamageType.None);
            }
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
