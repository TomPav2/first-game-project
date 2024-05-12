using System.Collections;
using UnityEngine;
using static GameValues;

public class SpellGreenDeath : Spell
{
    [SerializeField] private Transform targetter;
    [SerializeField] private MainCharacterSheet mainChar;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CircleCollider2D circleCollider;

    private static readonly float SPEED = 20;
    private static readonly Vector2 START_OFFSET = new Vector2(-1.5f, 1.5f);

    private bool active = false;

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

        targetter.position = new Vector3(mainChar.transform.position.x, targetter.position.y, 0);
        transform.position = Vector2.MoveTowards(transform.position, targetter.position, Time.deltaTime * SPEED);
    }

    public override void cast(Vector2 startPos)
    {
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            mainChar.damage(1);
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
