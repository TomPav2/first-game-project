using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MainCharacterSheet : MonoBehaviour
{
    public HeartController healthBar;
    public DamageFX damageFX;

    // attacks
    [SerializeField] private RMBAttack laserAttack;

    [SerializeField] private GameObject attackBasic;
    [SerializeField] private GameObject attackFast;
    [SerializeField] private GameObject attackStrong;
    private GameObject currentAttack;

    // health stats
    private byte maxHealth;

    private byte health;
    private readonly int regenInterval = 30;
    private bool lifesteal;
    private Coroutine regenerator;

    // active bonuses
    private BonusHealth activeHealth;
    private BonusAttack activeAttack;
    private BonusSpeed activeSpeed;
    private List<int> emptyUpgrades;

    private void Start()
    {
        init();
    }

    public void test()
    {
        //TODO remove
        applyBonus(BonusAttack.FastAttack);
    }

    public void init()
    {
        // TODO laserAttack.reset
        currentAttack = attackBasic;
        currentAttack.SetActive(true);
        maxHealth = 12;
        health = 12;
        healthBar.displayHp(health);
        lifesteal = false;
        if (regenerator != null) StopCoroutine(regenerator);
        activeHealth = BonusHealth.None;
        activeAttack = BonusAttack.None;
        activeSpeed = BonusSpeed.None;
        emptyUpgrades = new List<int> { 0, 1, 2 };
    }

    public void selectBonus()
    {
        int index = UnityEngine.Random.Range(0, emptyUpgrades.Count);
        int bonusType = emptyUpgrades[index];
        emptyUpgrades.Remove(bonusType);

        Array bonusValues;
        switch (bonusType)
        {
            case 0:
                {
                    bonusValues = Enum.GetValues(typeof(BonusHealth));
                    activeHealth = (BonusHealth)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeHealth);
                    break;
                }
            case 1:
                {
                    bonusValues = Enum.GetValues(typeof(BonusAttack));
                    activeAttack = (BonusAttack)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeAttack);
                    break;
                }
            case 2:
                {
                    bonusValues = Enum.GetValues(typeof(BonusSpeed));
                    activeSpeed = (BonusSpeed)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeSpeed);
                    break;
                }
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
            // TODO death animation and screen
            health = 0;
        }
        else health -= amount;
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

            case BonusAttack.FastAttack:
                {
                    currentAttack.SetActive(false);
                    currentAttack = attackFast;
                    currentAttack.SetActive(true);
                    break;
                }
            case BonusAttack.StrongAttack:
                {
                    currentAttack.SetActive(false);
                    currentAttack = attackStrong;
                    currentAttack.SetActive(true);
                    break;
                }
            default: break;
        }
    }

    private IEnumerator regenRoutine()
    {
        yield return new WaitForSeconds(regenInterval);
        if (activeHealth == BonusHealth.Regen) heal(1);
        else yield break;
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