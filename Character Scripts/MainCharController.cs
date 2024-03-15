using UnityEngine;

public class MainCharController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private int speed = 5;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private GameObject healthBar;

    private bool dirRight = true;
    private bool dirUp = true;
    private float speedX = 0;
    private float speedY = 0;

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W) ^ Input.GetKey(KeyCode.S))
        {
            speedY = 1;
            dirUp = Input.GetKey(KeyCode.W);
        }
        else if (speedY > 0) speedY -= 0.1f;

        if (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D))
        {
            speedX = 1;
            if (Input.GetKey(KeyCode.A)) turnLeft(); else turnRight();
        }
        else if (speedX > 0) speedX -= 0.1f;

        if (speedX > 0 || speedY > 0)
        {
            body.velocity = new Vector2(dirRight ? speedX : -speedX, dirUp ? speedY : -speedY) * speed * Time.deltaTime;
        } else
        {
            body.velocity = Vector2.zero;
        }
    }

    private void turnRight()
    {
        if (!dirRight)
        {
            sprite.sprite = spriteRight;
            dirRight = true;
        }
    }

    private void turnLeft()
    {
        if (dirRight)
        {
            sprite.sprite = spriteLeft;
            dirRight = false;
        }
    }
}