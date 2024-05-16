using UnityEngine;
using static GameValues;

public class ArrowSimpleController : MonoBehaviour
{
    private static readonly Vector3 MOVEMENT = new Vector3(0, -25, 0);

    public ArrowPortalController portal { set; private get; }
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private bool isMoving = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        transform.position += MOVEMENT * Time.deltaTime;
    }

    public void shoot()
    {
        transform.position = randomOffset(portal.transform.position);
        spriteRenderer.enabled = true;
        polygonCollider.enabled = true;
        isMoving = true;
    }

    private void hit()
    {
        spriteRenderer.enabled = false;
        polygonCollider.enabled = false;
        isMoving = false;
        portal.reuseArrow(this);
    }

    private static Vector3 randomOffset(Vector3 position)
    {
        float offset = Random.Range(-4f, 2f);
        return new Vector3(position.x + offset, position.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.WALL_TOP) || collision.CompareTag(Tag.PLAYER))
        {
            hit();
        }
    }
}
