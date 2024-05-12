using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class SpellTornado : Spell
{
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private GameObject navigationContainer;

    private List<Transform> navPoints = new List<Transform>();
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PolygonCollider2D polygonCollider;

    private static readonly float SPEED = 15;

    private bool active = true;
    private bool direction = true;
    private Vector3 movement;

    private void Awake()
    {
        channeled = true;
        manacost = 50;

        for (int i = 0; i < navigationContainer.transform.childCount; i++)
        {
            navPoints.Add(navigationContainer.transform.GetChild(i));
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!active) return;

        transform.position += (Time.deltaTime * SPEED * movement);
    }

    private void setMovement()
    {
        Vector2 target;
        Vector2 currentPos = transform.position;
        if (direction)
        {
            target = navPoints[Random.Range(4, 8)].position;
        }
        else
        {
            target = navPoints[Random.Range(0, 4)].position;
        }
        direction = !direction;

        Vector2 heading = target - currentPos;
        movement = heading / heading.magnitude;
    }

    public override void cast(Vector2 startPos)
    {
        transform.position = navPoints[0].position;
        spriteRenderer.enabled = true;
        animator.enabled = true;
        polygonCollider.enabled = true;
        direction = true;
        setMovement();
        active = true;
        manacost = 25;
    }

    private void hide()
    {
        spriteRenderer.enabled = false;
        animator.enabled = false;
        polygonCollider.enabled = false;
        active = false;
    }

    public override byte channel()
    {
        return (byte)(active ? 5 : 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            mainChar.damage(1);
            hide();
        }
        else if (collision.CompareTag(Tag.WALL))
        {
            setMovement();
        }
    }

    public override void startChanneling()
    {
        // nothing to do
    }

    public override void stopChanneling()
    {
        hide();
    }
}