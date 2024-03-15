using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterSheet : MonoBehaviour
{
    public HeartController healthBar;
    public DamageFX damageFX;

    // health stats
    private byte maxHealth = 12;
    private byte health = 12;
    private readonly int regenInterval = 30;
    private bool lifesteal = false;
    private Coroutine regenerator;

    // active bonuses
    private BonusHealth activeHealth = BonusHealth.None;
    private BonusAttack activeAttack = BonusAttack.None;
    private BonusSpeed activeSpeed = BonusSpeed.None;


    public void test()
    {
        heal(1);
    }

    private IEnumerator regenRoutine()
    {
        yield return new WaitForSeconds(regenInterval);
        if (activeHealth == BonusHealth.Regen) heal(1);
        else yield break;
    }

    public void selectBonus() {
        // TODO: test this
        int bonusType = UnityEngine.Random.Range(0, 3);
    
        Array bonusValues;
        if (bonusType == 0) {
            bonusValues = Enum.GetValues(typeof(BonusHealth));
            activeHealth = (BonusHealth) bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length-1));
            applyBonus(activeHealth);
        } else if (bonusType == 1)
        {
            bonusValues = Enum.GetValues(typeof(BonusAttack));
            activeAttack = (BonusAttack) bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length-1));
            applyBonus(activeHealth);
        } else if (bonusType == 2)
        {
            bonusValues = Enum.GetValues(typeof(BonusSpeed));
            activeSpeed = (BonusSpeed) bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length - 1));
            applyBonus(activeHealth);
        }
    }

    public void offerLife()
    {
        if (lifesteal)
        {
            int roll = UnityEngine.Random.Range(0, 10);
            if (roll == 0) heal(0);
        }
    }

    public void heal(byte amount)
    {
        health += amount;
        if (health > maxHealth) { health = maxHealth; }
        healthBar.displayHp(health);
    }

    public void damage(byte amount)
    {
        damageFX.playDamageEffect(amount);
        if (health <= amount)
        {
            // TODO death
            health = 0;
        } else health -= amount;
        healthBar.displayHp(health);
    }

    private void applyBonus(Enum bonusType)
    {
        switch (bonusType)
        {
            case BonusHealth.Regen:
                {
                    regenerator = StartCoroutine(regenRoutine());
                    break;
                }
            case BonusHealth.Lifesteal:
                {
                    lifesteal = true;
                    break;
                }
            case BonusHealth.Maxhealth:
                {
                    maxHealth = 16;
                    healthBar.unlockBonus();
                    heal(4);
                    //TODO recharging laser
                    break;
                }
            default: break;
        }
    }



    private enum BonusHealth
    {
        None,
        Regen,
        Lifesteal,
        Maxhealth
    }

    private enum BonusAttack
    {
        None,
        FastAttack,
        StrongAttack,
        DoubleBeam,
        FastBeam
    }

    private enum BonusSpeed
    {
        None,
        MoveSpeed,
        PaintSpeed
    }
}
