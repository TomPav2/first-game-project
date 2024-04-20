using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;

public class SkellyTutorial : SkellyController
{
    private Vector2 spawnPoint;
    private bool firstSpawn = true;

    private TutorialController tutorial;
    private DamageType ignoredType = DamageType.None;
    private DamageType causeOfDeath = DamageType.None;
    private EnemyState startingState = EnemyState.None;

    public void spawnInState(EnemyState state, DamageType ignoredDamage, TutorialController tutorial)
    {
        if (firstSpawn)
        {
            firstSpawn = false;
            spawnPoint = transform.position;
        }
        startingState = state;
        ignoredType = ignoredDamage;
        this.tutorial = tutorial;

        spawn(spawnPoint, Difficulty.BASE_HEALTH);
    }
    protected override IEnumerator performSpawn()
    {
        switchState(EnemyState.Spawning);
        GetComponent<SpriteRenderer>().enabled = true;

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 0.1f;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha); // TODO replace with animation
            yield return new WaitForSeconds(0.1f);
        }

        GetComponent<Animator>().enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        switchState(startingState);
        StartCoroutine(navigator());
        yield break;
    }

    public override void damage(byte amount, DamageType type)
    {
        if(type != ignoredType) base.damage(amount, type);
    }

    protected override void updateTarget()
    {
        target = mainChar.transform.position;
        rotate();
    }

    protected override void die(DamageType damageType)
    {
        StopAllCoroutines();
        causeOfDeath = damageType;
        if (damageType == DamageType.Contact) tutorial.showMessage("Do not touch the skeletons. Try again.", false);
        deathFirstStage();
    }

    protected override void deathFirstStage()
    {
        switchState(EnemyState.Dying);
        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;
        healthBar.fade();
    }

    protected override IEnumerator deathFadeProcess()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= 0.2f;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha); // TODO replace with animation
            yield return new WaitForSeconds(0.1f);
        }
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        GetComponent<SpriteRenderer>().enabled = false;
        tutorialDeathTrigger();
    }

    private void tutorialDeathTrigger()
    {
        if (causeOfDeath == DamageType.Contact)
        {
            tutorial.hideMessage();
            spawnInState(startingState, ignoredType, tutorial);
        }
        else
        {
            tutorial.notifyDeath();
        }
    }
}
