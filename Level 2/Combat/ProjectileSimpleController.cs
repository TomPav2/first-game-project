using System.Collections;
using UnityEngine;
using static GameValues;

public class ProjectileSimpleController : MonoBehaviour
{
    private static readonly Vector3 MOVEMENT = new Vector3(0, -25, 0);
    private static readonly float SPEED = 20;
    private static readonly float START_OFFSET = 0.2f;
    private Vector3 move;
    private Coroutine timer;
    private Animator animator;

    public ShooterController holder { set; private get; }
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private bool isMoving = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        transform.position += move * Time.deltaTime;
    }

    public void shootStraight(Vector3 from)
    {
        move = MOVEMENT;
        transform.position = randomOffsetHorizontal(from);
        shoot();
    }

    public void shootAtTarget(Vector3 from, Vector3 target, float duration)
    {
        move = (target - from).normalized * SPEED;
        move = spreadOut(move);
        transform.right = move;
        transform.position = from + (move * START_OFFSET);

        shoot();
        timer = StartCoroutine(timeoutRoutine(duration));
    }

    private void shoot()
    {
        spriteRenderer.enabled = true;
        polygonCollider.enabled = true;
        isMoving = true;
    }

    private void end()
    {
        spriteRenderer.enabled = false;
        polygonCollider.enabled = false;
        isMoving = false;
        if (timer != null) StopCoroutine(timer);
        holder.reuseProjectile(this);
    }

    // called by animation
    private void afterFadeOut()
    {
        end();
    }

    private static Vector3 randomOffsetHorizontal(Vector3 position)
    {
        float offset = Random.Range(-4f, 2f);
        return new Vector3(position.x + offset, position.y, 0);
    }

    private static Vector3 spreadOut(Vector3 target)
    {
        Vector3 shift = new Vector3(target.y, -target.x, 0) * Random.Range(-0.3f, 0.3f);
        return target + shift;
    }

    private IEnumerator timeoutRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        if (isMoving) animator.SetTrigger(Trigger.FADE_OUT);
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.WALL_TOP) || collision.CompareTag(Tag.PLAYER))
        {
            end();
        }
    }
}
