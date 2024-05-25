using System.Collections;
using UnityEngine;
using static ScenePersistence;
using static GameValues;

public class JumpCharController : MonoBehaviour
{
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private LayerMask collideLayers;
    [SerializeField] private PortalController catchPortal;
    [SerializeField] private PortalController dropPortal;
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private TextHudController hudController;

    [SerializeField] private SpriteRenderer chestRenderer;
    [SerializeField] private Sprite chestOpen;
    [SerializeField] private Level2Manager manager;
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject exitPortal;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private BoxCollider2D collider2d;

    private static readonly float SPEED_Y = 50;
    private static readonly float SPEED_X = 20;
    private static readonly float ACCELERATION = 150;
    private static readonly float JUMPPAD_FORCE = 500;

    private static readonly float COYOTE_TIME = 0.1f;
    private static readonly float JUMP_BOOST_TIME = 0.2f;

    private static readonly float ABYSS_HEIGHT = -30;

    private float boost;
    private Coroutine coyoteTimer;

    private float boundsHeight;
    private Vector3 startPos;
    private Vector3 dropoff;

    private bool searchingGround = false;
    private bool grounded = false;
    private bool catchMe = false;

    private bool hasHeart = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        boundsHeight = collider2d.bounds.extents.y + 0.02f;
        dropoff = new Vector3(dropPortal.transform.position.x, dropPortal.transform.position.y + boundsHeight, 0);
        startPos = body.position;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = body.velocity;
        // checks when falling
        if (searchingGround && checkGround())
        {
            searchingGround = false;
            setGrounded();
        }

        // jumping control
        else if (Input.GetKey(KeyCode.Space) && boost > 0)
        {
            velocity.y = SPEED_Y;
            boost -= Time.deltaTime;
        }

        // horizontal control
        if (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
            {
                turnLeft();
                if (grounded) velocity.x = -SPEED_X;
                else velocity.x = Mathf.Clamp(velocity.x - (Time.deltaTime * ACCELERATION), -SPEED_X, SPEED_X);
            }
            else
            {
                turnRight();
                if (grounded) velocity.x = SPEED_X;
                else velocity.x = Mathf.Clamp(velocity.x + (Time.deltaTime * ACCELERATION), -SPEED_X, SPEED_X);
            }
        }

        if (!grounded && body.position.y < ABYSS_HEIGHT)
        {
            if (!catchMe)
            {
                catchMe = true;
                body.gravityScale = 50;
                portalCatchMe();
            }
            if (body.position.y < (catchPortal.transform.position.y - boundsHeight))
            {
                body.position = dropoff;
                velocity.x = 0;
            }
        }

        body.velocity = velocity;
    }

    private void Update()
    {
        if (isPaused) return;

        if (Input.GetKeyUp(KeyCode.Space) && !grounded)
        {
            boost = 0;
        }
        else if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            grounded = false;
            searchingGround = true;
        }
    }

    public void resetPos()
    {
        body.velocity = Vector3.zero;
        body.position = startPos;
    }

    private void turnRight()
    {
        spriteRenderer.sprite = spriteRight;
    }

    private void turnLeft()
    {
        spriteRenderer.sprite = spriteLeft;
    }

    private bool checkGround()
    {
        if (body.velocity.y > 1) return false;

        Vector2 raycastStart = collider2d.bounds.center;
        raycastStart.x -= 0.5f;
        if (Physics2D.Raycast(collider2d.bounds.center, Vector2.down, boundsHeight, collideLayers)) return true;
        raycastStart.x += 1;
        if (Physics2D.Raycast(collider2d.bounds.center, Vector2.down, boundsHeight, collideLayers)) return true;
        return false;
    }

    private void setGrounded()
    {
        grounded = true;
        boost = JUMP_BOOST_TIME;
        if (coyoteTimer != null) StopCoroutine(coyoteTimer);
        if (catchMe)
        {
            catchMe = false;
            if (!hasHeart) body.gravityScale = 30;
            hudController.popUp(null, "Ow! You take some fall damage.", null);
            mainChar.damage(1);
        }
    }

    private void startCoyoteTimer()
    {
        if (coyoteTimer != null) StopCoroutine(coyoteTimer);
        coyoteTimer = StartCoroutine(coyoteRoutine());
    }

    private IEnumerator coyoteRoutine()
    {
        yield return new WaitForSeconds(COYOTE_TIME);
        boost = 0;
    }

    private void portalCatchMe()
    {
        catchPortal.gameObject.SetActive(true);
        dropPortal.gameObject.SetActive(true);
        catchPortal.catchMe(true);
        dropPortal.catchMe(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!grounded && checkGround()) setGrounded();
        else searchingGround = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (body.velocity.y < 10)
        {
            grounded = false;
            searchingGround = false;
            startCoyoteTimer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.BONUS))
        {
            chestRenderer.sprite = chestOpen;
            manager.givePlayerHeart(barrier);
            hasHeart = true;
            exitPortal.SetActive(true);
            body.gravityScale = 50;
        }
        if (collision.CompareTag(Tag.BARRIER) && collision.transform.position.y < collider2d.bounds.min.y)
        {
            body.velocity = new Vector3(body.velocity.x, JUMPPAD_FORCE, 0);
            collision.gameObject.GetComponentInParent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        }
    }
}