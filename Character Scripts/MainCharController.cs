using System.Collections;
using UnityEngine;
using static GameValues;
using static SceneLoader;

public class MainCharController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private int speed;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRightBonus;
    [SerializeField] private Sprite spriteLeftBonus;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private RMBAttack laserAttack;
    [SerializeField] private MainCharacterSheet characterSheet;
    [SerializeField] private TutorialController tutorial;

    private Sprite activeSpriteRight;
    private Sprite activeSpriteLeft;

    private bool dirRight = true;
    private bool dirUp = true;
    private float speedX = 0;
    private float speedY = 0;
    private float speedUpgrade = 1;

    private void Awake()
    {
        activeSpriteRight = spriteRight;
        activeSpriteLeft = spriteLeft;
    }

    private void FixedUpdate()
    {
        if (lockControls)
        {
            body.velocity = Vector2.zero;
            return;
        }

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
        }
        else
        {
            body.velocity = Vector2.zero;
        }
    }

    public void setSpeedUpgrade(float speedUpgrade)
    {
        this.speedUpgrade = speedUpgrade;
    }

    private void turnRight()
    {
        if (!dirRight)
        {
            sprite.sprite = spriteRight;
            dirRight = true;
            laserAttack.setOffset(!dirRight);
        }
    }

    private void turnLeft()
    {
        if (dirRight)
        {
            sprite.sprite = spriteLeft;
            dirRight = false;
            laserAttack.setOffset(!dirRight);
        }
    }

    private IEnumerator pickUpBonusProcess()
    {
        activeSpriteRight = spriteRightBonus;
        activeSpriteLeft = spriteLeftBonus;
        sprite.sprite = dirRight ? activeSpriteRight : activeSpriteLeft;
        yield return new WaitForSeconds(1);

        activeSpriteRight = spriteRight;
        activeSpriteLeft = spriteLeft;
        sprite.sprite = dirRight ? activeSpriteRight : activeSpriteLeft;
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.BONUS))
        {
            StartCoroutine(pickUpBonusProcess());
            characterSheet.applyRandomBonus();
        }
        else if (collision.gameObject.CompareTag(Tag.RAVEN) && inTutorial)
        {
            tutorial.nextStep();
        }
    }
}