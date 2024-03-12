using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HeartController : MonoBehaviour
{
    // hearts displayed on screen
    [SerializeField] private SpriteRenderer hp1;
    [SerializeField] private SpriteRenderer hp2;
    [SerializeField] private SpriteRenderer hp3;
    [SerializeField] private SpriteRenderer hp4;

    // heart sprites
    [SerializeField] private Sprite heart0;
    [SerializeField] private Sprite heart1;
    [SerializeField] private Sprite heart2;
    [SerializeField] private Sprite heart3;
    [SerializeField] private Sprite heart4;

    // background hearts for flash effect
    [SerializeField] private SpriteRenderer hp1Flash;
    [SerializeField] private SpriteRenderer hp2Flash;
    [SerializeField] private SpriteRenderer hp3Flash;
    [SerializeField] private SpriteRenderer hp4Flash;

    private readonly SpriteRenderer[] health = new SpriteRenderer[4];
    private readonly Sprite[] heart = new Sprite[5];
    private byte currentHp = 12;

    private void Awake()
    {
        // map hearts and sprites to arrays
        health[0] = hp1;
        health[1] = hp2;
        health[2] = hp3;
        health[3] = hp4;

        heart[0] = heart0;
        heart[1] = heart1;
        heart[2] = heart2;
        heart[3] = heart3;
        heart[4] = heart4;
    }

    public void displayHp(byte newHp)
    {
        int index1 = newHp / 4;
        int index2 = currentHp / 4;

        int value1 = (newHp - (index1 * 4));
        int value2 = (newHp - (index2 * 4));

        health[index1].sprite = heart[value1];
        if (index1 != index2 && value2 >= 0) health[index2].sprite = heart[value2];

        if (newHp > currentHp) StartCoroutine(healFlash());
        currentHp = newHp;
    }

    public void unlockBonus()
    {
        health[3].enabled = true;
    }

    private IEnumerator healFlash()
    {
        hp1Flash.enabled = true;
        hp2Flash.enabled = true;
        hp3Flash.enabled = true;
        if (hp4.enabled) hp4Flash.enabled = true;

        yield return new WaitForSeconds(0.2f);

        hp1Flash.enabled = false;
        hp2Flash.enabled = false;
        hp3Flash.enabled = false;
        hp4Flash.enabled = false;

        yield break;
    }
}
