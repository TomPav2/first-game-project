using UnityEngine;

public class CharExplosionController : MonoBehaviour
{
    [SerializeField] GameObject character;

    private bool exploded = false;

    private void Awake()
    {
        hideExplosion();
    }

    public void explode()
    {
        if (exploded) return; exploded = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Animator>().enabled = true;
    }

    // called by animator
    private void hideCharacter()
    {
        character.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void hideExplosion()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Animator>().enabled = false;
    }
}
