using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItem : MonoBehaviour
{
    [SerializeField] private TutorialController tutorial;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private Sprite sprite3;
    [SerializeField] private Sprite sprite4;
    [SerializeField] private Sprite sprite5;

    private void Start()
    {
        List<Sprite> sprites = new List<Sprite>()
        {
            sprite1 , sprite2 , sprite3 , sprite4 , sprite5
        };
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count - 1)];
    }

    public void throwThing()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Animator>().enabled = true;
    }

    private void hitPortal()
    {
        tutorial.spawnEnemies(3);
    }

    private void finishThis()
    {
        gameObject.SetActive(false);
    }
}