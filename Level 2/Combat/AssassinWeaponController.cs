using System.Collections;
using UnityEngine;
using static GameValues;

public class AssassinWeaponController : MonoBehaviour
{
    [SerializeField] MainCharacterSheet mainCharacterSheet;
    [SerializeField] MainCharController mainCharController;

    private static bool primed = true;

    // for arrows
    private bool inFlight = false;
    private Vector3 arrowMove = Vector3.zero;
    private Vector3 startingLocalPos;

    private void Awake()
    {
        startingLocalPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (inFlight)
        {
            transform.position += (arrowMove * Time.deltaTime * 10);
        }
    }

    public void shoot(Vector2 movement)
    {
        if (inFlight) return;
        transform.localPosition = startingLocalPos;
        arrowMove = movement;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<PolygonCollider2D>().enabled = true;
        inFlight = true;
    }

    private void stopShot()
    {
        inFlight = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER) && primed)
        {
            damagePlayer();
            if (inFlight) stopShot();
        }
        else if (collision.CompareTag(Tag.WALL_TOP))
        {
            stopShot();
        }
    }

    private void damagePlayer()
    {
        if (mainCharController.transform.position.x > transform.position.x)
        {
            // player is to the right
            mainCharacterSheet.damage( (byte)(mainCharController.isFacingRight() ? 2 : 1) );
        }
        else
        {
            // player is to the left
            mainCharacterSheet.damage((byte)(mainCharController.isFacingRight() ? 1 : 2));
        }
        StartCoroutine(cooldown());
    }

    private IEnumerator cooldown()
    {
        primed = false;
        yield return new WaitForSeconds(1);
        primed = true;
    }
}
