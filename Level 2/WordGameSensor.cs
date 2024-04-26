using UnityEngine;
using static GameValues;

public class WordGameSensor : MonoBehaviour
{
    [SerializeField] private WordGame game;
    [SerializeField] private GameObject prompt;

    // this is needed because player has two colliders
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!playerInRange && collision.CompareTag(Tag.PLAYER))
        {
            playerInRange = true;
            game.playerApproached();
            prompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (playerInRange && collision.CompareTag(Tag.PLAYER))
        {
            playerInRange = false;
            game.playerLeft();
            prompt.SetActive(false);
        }
    }
}