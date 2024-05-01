using System.Collections;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

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

    private LevelManager levelManager;

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
            if (speedX > 0) speedX = 0;
            if (speedY > 0) speedY = 0;
            return;
        }

        if (Input.GetKey(KeyCode.W) ^ Input.GetKey(KeyCode.S))
        {
            speedY = 1;
            dirUp = Input.GetKey(KeyCode.W) ^ invertControls;
        }
        else if (speedY > 0) speedY -= 0.1f;

        if (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D))
        {
            speedX = 1;
            if (Input.GetKey(KeyCode.A) ^ invertControls) turnLeft(); else turnRight();
        }
        else if (speedX > 0) speedX -= 0.1f;

        if (speedX > 0 || speedY > 0)
        {
            float moveX = ( (dirRight ? speedX : -speedX) * speed * speedUpgrade * Time.deltaTime ) + body.position.x;
            float moveY = ( ( dirUp   ? speedY : -speedY) * speed * speedUpgrade * Time.deltaTime ) + body.position.y;
            body.MovePosition(new Vector3(moveX, moveY, 0));
        }
    }

    public void init(LevelManager manager)
    {
        this.levelManager = manager;
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
            if (levelDifficulty == LevelDifficulty.Easy) characterSheet.applyRandomBonus();
            else if (levelDifficulty == LevelDifficulty.Hard) characterSheet.applyRMBBonus();
        }
        else if (collision.gameObject.CompareTag(Tag.RAVEN) && inTutorial)
        {
            tutorial.nextStep();
        }
        else if (collision.CompareTag(Tag.PENTAGRAM)){
            levelManager.enteredPentagram();
        }        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Tag.BARRIER))
        {
            levelManager.restrictedAccess();
        }
    }
}