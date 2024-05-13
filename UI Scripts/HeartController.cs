using System.Collections;
using UnityEngine;

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
    private byte oldHp = 12;

    private Coroutine healRoutine;

    private void Start()
    {
        health[0] = hp1;
        health[1] = hp2;
        health[2] = hp3;
        health[3] = hp4;
        health[3].enabled = false;

        heart[0] = heart0;
        heart[1] = heart1;
        heart[2] = heart2;
        heart[3] = heart3;
        heart[4] = heart4;
    }

    public void unlockBonus()
    {
        health[3].enabled = true;
    }

    public void displayHp(byte newHp)
    {
        if (newHp > oldHp && !hp1Flash.enabled)
        {
            if (healRoutine != null) StopCoroutine(healRoutine);
            healRoutine = StartCoroutine(healFlash());
        }

        for (int i = 0; i < health.Length; i++)
        {
            newHp = fillHeart(i, newHp);
        }
        oldHp = newHp;
    }

    private byte fillHeart(int index, byte amount)
    {
        if (amount >= 4)
        {
            health[index].sprite = heart[4];
            return (byte)(amount - 4);
        }
        else
        {
            health[index].sprite = heart[amount];
            return 0;
        }
    }

    private IEnumerator healFlash()
    {
        hp1Flash.enabled = true;
        hp2Flash.enabled = true;
        hp3Flash.enabled = true;
        if (hp4.enabled) hp4Flash.enabled = true;

        yield return new WaitForSeconds(0.15f);

        hp1Flash.enabled = false;
        hp2Flash.enabled = false;
        hp3Flash.enabled = false;
        hp4Flash.enabled = false;

        yield break;
    }
}