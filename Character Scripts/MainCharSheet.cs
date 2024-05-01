using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

public class MainCharacterSheet : MonoBehaviour
{
    // pointers
    [SerializeField] private HeartController healthBar;
    [SerializeField] private DamageFX damageFX;
    [SerializeField] private EaselController easel;
    [SerializeField] private MainCharController charController;
    [SerializeField] private CharExplosionController explosion;
    [SerializeField] private TextHudController hudController;

    // attacks
    [SerializeField] private RMBAttack laserAttack;
    [SerializeField] private GameObject attackBasic;
    [SerializeField] private GameObject attackFast;
    [SerializeField] private GameObject attackStrong;
    private GameObject currentAttack;

    // health stats
    private static readonly int REGEN_INTERVAL = 30;
    private byte maxHealth = 12;
    private byte health;
    private bool lifesteal = false;

    // active bonuses
    private Bonus.Health activeHealth;
    private Bonus.Attack activeAttack;
    private Bonus.Speed activeSpeed;
    private readonly List<int> emptyUpgrades = new List<int> { 0, 1, 2 };


    private LevelManager levelManager;

    private void Start()
    {
        currentAttack = attackBasic;
        health = maxHealth;
    }

    public void applyRandomBonus()
    {
        int index = UnityEngine.Random.Range(0, emptyUpgrades.Count);
        int bonusType = emptyUpgrades[index];
        emptyUpgrades.Remove(bonusType);

        Array bonusValues;
        switch (bonusType)
        {
            case 0:
                {
                    bonusValues = Enum.GetValues(typeof(Bonus.Health));
                    activeHealth = (Bonus.Health)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeHealth);
                    break;
                }
            case 1:
                {
                    bonusValues = Enum.GetValues(typeof(Bonus.Attack));
                    activeAttack = (Bonus.Attack)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeAttack);
                    break;
                }
            case 2:
                {
                    bonusValues = Enum.GetValues(typeof(Bonus.Speed));
                    activeSpeed = (Bonus.Speed)bonusValues.GetValue(UnityEngine.Random.Range(1, bonusValues.Length));
                    applyBonus(activeSpeed);
                    break;
                }
        }
    }

    public void init(LevelManager manager)
    {
        this.levelManager = manager;
    }

    public void applyLMBBonus()
    {
        applyBonus(flipACoin() ? Bonus.Attack.FastAttack : Bonus.Attack.StrongAttack);
    }

    public void applyRMBBonus()
    {
        applyBonus(flipACoin() ? Bonus.Attack.DoubleBeam : Bonus.Attack.FastBeam);
    }

    public void offerLife()
    {
        if (lifesteal)
        {
            int roll = UnityEngine.Random.Range(0, 10);
            if (roll == 0) heal(1);
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
        if (levelDifficulty == LevelDifficulty.Easy) easel.stopPainting();
        if (health <= amount)
        {
            explosion.explode();
            levelManager.playerDied();
            health = 0;
        }
        else
        {
            health -= amount;
            if (activeHealth == Bonus.Health.Maxhealth) laserAttack.addCharge(100);
        }
        healthBar.displayHp(health);
    }

    public void rechargeLaser()
    {
        laserAttack.addCharge(2000);
    }

    private void applyBonus(Enum bonusType)
    {
        hudController.popUp(Bonus.getBonusName(bonusType), Bonus.getBonusDesc(bonusType), null);
        levelManager.hideBonusItem();
        switch (bonusType)
        {
            case Bonus.Health.Regen:
                {
                    StartCoroutine(regenRoutine());
                    break;
                }
            case Bonus.Health.Lifesteal:
                {
                    lifesteal = true;
                    break;
                }
            case Bonus.Health.Maxhealth:
                {
                    maxHealth = 16;
                    healthBar.unlockBonus();
                    heal(4);
                    break;
                }

            case Bonus.Attack.FastAttack:
                {
                    currentAttack.SetActive(false);
                    currentAttack = attackFast;
                    currentAttack.SetActive(true);
                    break;
                }
            case Bonus.Attack.StrongAttack:
                {
                    currentAttack.SetActive(false);
                    currentAttack = attackStrong;
                    currentAttack.SetActive(true);
                    break;
                }
            case Bonus.Attack.FastBeam:
                {
                    laserAttack.unlockSpeed();
                    break;
                }
            case Bonus.Attack.DoubleBeam:
                {
                    laserAttack.unlockDouble();
                    break;
                }

            case Bonus.Speed.PaintSpeed:
                {
                    easel.speedUpgrade();
                    break;
                }
            case Bonus.Speed.MoveSpeed:
                {
                    charController.setSpeedUpgrade(1.2f);
                    break;
                }
            default: break;
        }
    }

    private IEnumerator regenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(REGEN_INTERVAL);
            heal(1);
        }
    }

    private static class Bonus
    {
        public enum Health
        {
            None,
            Regen,
            Lifesteal,
            Maxhealth
        }

        public enum Attack
        {
            None,
            FastAttack,
            StrongAttack,
            DoubleBeam,
            FastBeam
        }

        public enum Speed
        {
            None,
            MoveSpeed,
            PaintSpeed
        }

        private static readonly Dictionary<Enum, String> names = new Dictionary<Enum, string>()
        {
            {Health.Regen, "Regeneration"},
            {Health.Lifesteal, "Lifesteal"},
            {Health.Maxhealth, "Tank"},
            {Attack.FastAttack, "Fast Attack"},
            {Attack.StrongAttack, "Strong Attack"},
            {Attack.DoubleBeam, "Double Beam Charge"},
            {Attack.FastBeam, "Faster Beam Charge"},
            {Speed.MoveSpeed, "Faster Movement"},
            {Speed.PaintSpeed, "Faster Painting"}
        };

        private static readonly Dictionary<Enum, String> descriptions = new Dictionary<Enum, string>()
        {
            {Health.Regen, "Regenerate health over time"},
            {Health.Lifesteal, "Gain health by killing"},
            {Health.Maxhealth, "Taking damage recharges beam"},
            {Attack.FastAttack, "Main attack shoots faster"},
            {Attack.StrongAttack, "Main attack is larger and hits harder"},
            {Attack.DoubleBeam, "Beam has two charge meters"},
            {Attack.FastBeam, "Beam recharges faster"},
            {Speed.MoveSpeed, "You move faster"},
            {Speed.PaintSpeed, "You paint faster"}
        };

        public static String getBonusName(Enum bonus) { return names[bonus]; }

        public static String getBonusDesc(Enum bonus) { return descriptions[bonus]; }
    }
 }