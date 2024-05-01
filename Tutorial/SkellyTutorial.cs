using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static UnityEngine.Rendering.DebugUI.Table;

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
    public override void afterFadeIn()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        switchState(startingState);
        StartCoroutine(navigator());
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
        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;
        healthBar.fade(); // TODO check

        causeOfDeath = damageType;
        if (damageType == DamageType.Contact) tutorial.showMessage("Do not touch the skeletons. Try again.", false);

        switchState(EnemyState.Dying);
    }

    public override void afterFadeOut()
    {
        switchState(EnemyState.Dead);
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
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
