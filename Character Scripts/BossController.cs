using System.Collections;
using UnityEngine;
using static GameValues;

public class BossController : EnemyBase
{
    [SerializeField] private GameObject scepter;
    [SerializeField] private Spell greenDeath;
    [SerializeField] private Spell greenDeath2;
    [SerializeField] private ArrowPortalController arrowPortal;
    [SerializeField] private BossExplosionController explosions;
    [SerializeField] private SliderController healthBar;
    [SerializeField] private FightController fightController;

    private static readonly int MAX_HEALTH = 1600;
    private int health = MAX_HEALTH;

    private bool secondPhase = false;

    public override void damage(byte amount, GameValues.DamageType type)
    {
        if (amount > health) health = 0;
        else health -= amount;
        healthBar.updateValue(MAX_HEALTH, health);
        if (health < (MAX_HEALTH/2) && !secondPhase )
        {
            secondPhase = true;
            StartCoroutine(arrowPortalRoutine());
        } 
        if (health ==  0) die();
    }

    public override void heal(byte amount)
    {
        health += amount;
        if (health > MAX_HEALTH) health = MAX_HEALTH;
        healthBar.updateValue(MAX_HEALTH, health);
    }

    public void introAttack()
    {
        StartCoroutine(greenDeathIntroRoutine());
    }

    public void beginFight()
    {
        healthBar.gameObject.SetActive(true);
        healthBar.updateValue(MAX_HEALTH, health);
        GetComponent<CapsuleCollider2D>().enabled = true;
        StartCoroutine(greenDeathRoutine());
    }

    private void die()
    {
        explosions.bossDied();
        GetComponent<CapsuleCollider2D>().enabled = false;
        GetComponent<Animator>().SetTrigger(Trigger.DIE);
        fightController.registerTakedown();
    }

    private IEnumerator greenDeathRoutine()
    {
        yield return new WaitForSeconds(3);
        while (health > 0)
        {
            greenDeath.cast(scepter.transform.position);
            if (secondPhase)
            {
                yield return new WaitForSeconds(1);
                greenDeath2.cast(scepter.transform.position);
            }
            yield return new WaitForSeconds(8.6f);
        }
        yield break;
    }

    private IEnumerator greenDeathIntroRoutine()
    {
        greenDeath.cast(scepter.transform.position);
        yield return new WaitForSeconds(0.5f);
        greenDeath2.cast(scepter.transform.position);
        yield break;
    }

    private IEnumerator arrowPortalRoutine()
    {
        while (health > 0)
        {
            arrowPortal.startShooting();
            yield return new WaitForSeconds(15.3f);
        }
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tag.ATTACK_LMB))
        {
            LMBAttack projectile = collision.GetComponent<LMBAttack>();
            damage(projectile.Hit(true), DamageType.LMB);
        }
    }
}
