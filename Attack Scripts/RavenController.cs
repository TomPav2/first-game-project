using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class RavenController : MonoBehaviour
{
    private HashSet<SkellyController> skeletonsInArea = new HashSet<SkellyController>();

    private bool upgraded = false;
    private readonly float emptyCooldown = 5f;
    private readonly byte fixedCooldown = 2;
    private readonly byte fixedCooldownUpgraded = 1;

    public void summon()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        StartCoroutine(attackProcess());
    }

    public void upgrade()
    {
        upgraded = true;
    }

    public void register(SkellyController enemy)
    {
        skeletonsInArea.Add(enemy);
    }

    public void deregister(SkellyController enemy)
    {
        skeletonsInArea.Remove(enemy);
    }

    private SkellyController selectWeakestEnemy()
    {
        SkellyController weakestEnemy = null;
        int lowestHp = 0;
        foreach (SkellyController enemy in skeletonsInArea)
        {
            if (weakestEnemy == null)
            {
                weakestEnemy = enemy;
                lowestHp = enemy.health;
            } else if (enemy.health < lowestHp)
            {
                weakestEnemy = enemy;
                lowestHp = enemy.health;
            }
        }
        return weakestEnemy;
    }

    private int calculateCooldown(int damage)
    {
        int damageDealt = upgraded ? damage - 5 : damage;
        return ((damageDealt / 10) + (upgraded ? fixedCooldownUpgraded : fixedCooldown));
    }

    private IEnumerator attackProcess()
    {
        while (true)
        {
            if (skeletonsInArea.Count > 0)
            {
                int damageDealt = selectWeakestEnemy().damageMax(DamageType.Raven);
                // TODO add laser
                yield return new WaitForSeconds(calculateCooldown(damageDealt));
            } else
            {
                yield return new WaitForSeconds(emptyCooldown);
            }
        }
    }
}
