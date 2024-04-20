using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class CrowController : MonoBehaviour, IFading
{
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject areaOfEffect;
    [SerializeField] private Animator damageEffect;

    private readonly List<SkellyController> skeletonsInArea = new List<SkellyController>();
    private static readonly Vector3 UPGRADED_SCALE = new Vector3(60, 60, 0);

    private bool readyToAttack = true;
    private bool upgraded = false;

    public void summon()
    {
        if (spriteRenderer.enabled) upgrade();
        else
        {
            spriteRenderer.enabled = true;
            areaOfEffect.SetActive(true);
        }
    }

    private void upgrade() {
        upgraded = true;
        areaOfEffect.transform.localScale = UPGRADED_SCALE;
    }

    public bool isUpgraded() { return upgraded; }

    public bool register(SkellyController enemy)
    {
        skeletonsInArea.Add(enemy);
        if (readyToAttack) startFadeIn();
        return upgraded;
    }

    public void deregister(SkellyController enemy)
    {
        skeletonsInArea.Remove(enemy);
    }

    private void startFadeIn()
    {
        if (!readyToAttack) return; // in case this is triggered twice in one frame
        readyToAttack = false;
        spriteRenderer.sprite = attackSprite;
        damageEffect.SetTrigger(Trigger.FADE_IN);
    }

    public void afterFadeIn()
    {
        animator.enabled = true;
        StartCoroutine(attackProcess());
    }

    private void startFadeOut()
    {
        animator.enabled = false;
        spriteRenderer.sprite = defaultSprite;
        damageEffect.SetTrigger(Trigger.FADE_OUT);
    }

    public void afterFadeOut()
    {
        readyToAttack = true;
        if (skeletonsInArea.Count > 0) startFadeIn();
    }

    private IEnumerator attackProcess()
    {
        while (true)
        {
            // break condition
            if (skeletonsInArea.Count < 1)
            {
                startFadeOut();
                yield break;
            }

            // damage all enemies in area
            for (int i = skeletonsInArea.Count - 1; i >= 0; i--)
            {
                skeletonsInArea[i].damage(1, DamageType.Crow);
            }
            yield return new WaitForSeconds(1);
        }
    }
}