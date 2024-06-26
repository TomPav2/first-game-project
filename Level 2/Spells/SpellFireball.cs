using System.Collections;
using UnityEngine;
using static GameValues;
using static VectorUtil;

public class SpellFireball : Spell
{
    [SerializeField] private MainCharacterSheet mainChar;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D spellCollider;

    private static readonly byte SPEED = 30;
    private static readonly byte DURATION = 4;

    private bool active = false;
    private Vector3 movement;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spellCollider = GetComponent<PolygonCollider2D>();

        channeled = false;
        manacost = 60;
    }

    private void FixedUpdate()
    {
        if (!active) return;

        transform.position += movement * Time.deltaTime;
    }

    public override void cast(Vector2 startPos)
    {
        if (active) return; // should never happen, but just in case

        // set position and rotation
        transform.position = startPos;
        movement = calculateDirection(startPos, mainChar.transform.position) * SPEED;
        transform.right = movement;

        // enable sprite and collisions
        spriteRenderer.enabled = true;
        spellCollider.enabled = true;

        // set active and expire
        active = true;
        StartCoroutine(expirationRoutine());
    }

    private void hit()
    {
        active = false;
        spellCollider.enabled = false;
        animator.SetTrigger(Trigger.ANIMATION_START);
    }

    private void animationFinished()
    {
        spriteRenderer.enabled = false;
    }

    private IEnumerator expirationRoutine()
    {
        yield return new WaitForSeconds(DURATION);
        if (active) hit();
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            mainChar.damage(1);
            hit();
        } else if (collision.CompareTag(Tag.WALL_TOP))
        {
            hit();
        }
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
