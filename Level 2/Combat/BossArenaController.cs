using UnityEngine;
using static GameValues;

public class BossArenaController : ArenaController
{
    [SerializeField] private BossFightController fightController;

    private void Awake()
    {
        hasWaypoints = false;
    }

    public override void nextFight()
    {
        GetComponentInParent<Level2Manager>().winScreen();
    }

    public void disableCollider()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Tag.PLAYER))
        {
            fightController.begin();
        }
    }
}
