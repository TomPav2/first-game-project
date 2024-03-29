using UnityEngine;

public class MainCharController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private int speed;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private GameObject healthBar;

    private bool dirRight = true;
    private bool dirUp = true;
    private float speedX = 0;
    private float speedY = 0;
    private float speedUpgrade = 1;

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
            body.velocity = speed * speedUpgrade * Time.deltaTime * new Vector2(dirRight ? speedX : -speedX, dirUp ? speedY : -speedY);
        } else
        {
            body.velocity = Vector2.zero;
        }
    }

    public void setSpeedUpgrade(float speedUpgrade)
    {
        this.speedUpgrade = speedUpgrade;
    }

    public void setBonusSprite()
    {
        // TODO
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