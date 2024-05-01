using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class RavenController : MonoBehaviour
{
    [SerializeField] private Sprite spriteDefault;
    [SerializeField] private Sprite spriteAttacking;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LineRenderer laserLine;

    private HashSet<SkellyController> skeletonsInArea = new HashSet<SkellyController>();

    private static readonly float COOLDOWN_NO_TARGET = 5f;
    private static readonly byte COOLDOWN_FIXED = 2;
    private static readonly byte COOLDOWN_FIXED_UPGRADED = 1;

    private bool upgraded = false;

    public void summon()
    {
        if (spriteRenderer.enabled) upgraded = true;
        else
        {
            spriteRenderer.enabled = true;
            StartCoroutine(attackProcess());
        }
    }

    public void upgrade()
    {
        upgraded = true;
    }

    public bool isUpgraded() { return upgraded; }

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
                lowestHp = enemy.Health;
            }
            else if (enemy.Health < lowestHp)
            {
                weakestEnemy = enemy;
                lowestHp = enemy.Health;
            }
        }
        return weakestEnemy;
    }

    private IEnumerator attackVisual(Vector3 enemyPos)
    {
        spriteRenderer.sprite = spriteAttacking;
        laserLine.startWidth = 0.5f;
        laserLine.endWidth = 0.5f;
        Vector3 targetPosition = new Vector3(enemyPos.x, enemyPos.y + 2, 0);
        laserLine.SetPosition(1, targetPosition);
        laserLine.enabled = true;
        while (laserLine.startWidth > 0)
        {
            yield return new WaitForSeconds(0.1f);
            laserLine.startWidth -= 0.1f;
            laserLine.endWidth -= 0.1f;
        }
        laserLine.enabled = false;
        spriteRenderer.sprite = spriteDefault;
        yield break;
    }

    private int calculateCooldown(int damage)
    {
        if (damage < 10) return ( upgraded ? COOLDOWN_FIXED_UPGRADED : COOLDOWN_FIXED) ;
        int damageDealt = upgraded ? damage - 5 : damage;
        return ((damageDealt / 10) + (upgraded ? COOLDOWN_FIXED_UPGRADED : COOLDOWN_FIXED));
    }

    private IEnumerator attackProcess()
    {
        while (true)
        {
            if (skeletonsInArea.Count > 0)
            {
                SkellyController enemy = selectWeakestEnemy();
                int damageDealt = enemy.damageMax(DamageType.Raven);
                StartCoroutine(attackVisual(enemy.transform.position));
                yield return new WaitForSeconds(calculateCooldown(damageDealt));
            }
            else
            {
                yield return new WaitForSeconds(COOLDOWN_NO_TARGET);
            }
        }
    }
}