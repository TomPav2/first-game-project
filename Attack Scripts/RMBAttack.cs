using System.Collections;
using UnityEngine;
using static GameValues;
using static SceneLoader;

public class RMBAttack : MonoBehaviour

{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask collideLayers;
    [SerializeField] private SliderController mainIndicator;
    [SerializeField] private SliderController bonusIndicator;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private LineRenderer laserFork;
    [SerializeField] private ParticleSystem laserParticles;

    private static readonly byte DAMAGE_AMOUNT = 2;
    private static readonly byte DRAIN_AMOUNT = 2;

    private static readonly ushort CHARGE_LIMIT= 1000;
    private static readonly float WIDTH_MIN = 0.5f;
    private static readonly float WIDTH_MAX = 1f;
    private static readonly float WIDTH_INTERVAL = 0.08f;

    private float chargeDelay = 0.5f;
    private float chargeInterval = 0.1f;
    private byte chargeAmount = 1;

    private Battery mainBattery = null;
    private Battery bonusBattery = null;
    private Battery usedBattery = null;

    private bool visualsEnabled = false;
    private bool laserIncreasing = false;
    private bool isOffset = false; // true when character is facing left

    private static readonly Vector3 OFFSET_X = new Vector3(-0.7f, 0, 0);
    private static readonly Vector3 FORK_OFFSET_RIGHT_1 = new Vector3(-1.65f, -0.25f, 0);
    private static readonly Vector3 FORK_OFFSET_RIGHT_2 = new Vector3(1.55f, -0.25f, 0);
    private static readonly Vector3 FORK_OFFSET_LEFT_1 = new Vector3(-1.35f, -0.25f, 0);
    private static readonly Vector3 FORK_OFFSET_LEFT_2 = new Vector3(1.65f, -0.25f, 0);

    private void Start()
    {
        mainBattery = new Battery(mainIndicator);
    }

    private void Update()
    {
        // on RMB down, select and activate the battery with lower charge, unless paused
        if (!isPaused && !lockControls && Input.GetMouseButtonDown(1))
        {
            usedBattery = selectBattery(true);
            usedBattery.inUse = true;
            enableVisuals(true);
        }

        // on RMB up, release the used battery, or when game is paused
        if ((Input.GetMouseButtonUp(1) || isPaused) && usedBattery != null)
        {
            usedBattery.inUse = false;
            StartCoroutine(chargeRoutine(usedBattery));
            usedBattery = null;
            enableVisuals(false);
        }

        // if there is a battery in use, try drain and perform attack
        if (usedBattery != null)
        {
            if (usedBattery.drain())
            {
                // raycast from start position (above player) to mouse position
                Vector2 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 from = transform.position;
                Vector2 direction = clickPosition - from;
                RaycastHit2D hit = Physics2D.Raycast(from, direction, Mathf.Infinity, collideLayers);

                // this should only occur in development, but it is safer to keep it
                if (hit == false) return;

                // set line renderer posistions
                laserLine.SetPosition(0, transform.position);
                laserLine.SetPosition(1, hit.point);
                laserParticles.transform.position = hit.point;
                laserParticles.transform.right = laserParticles.transform.position - transform.position;

                // set positions for the fork
                laserFork.SetPosition(0, transform.position + (isOffset ? FORK_OFFSET_LEFT_1 : FORK_OFFSET_RIGHT_1));
                laserFork.SetPosition(1, transform.position);
                laserFork.SetPosition(2, transform.position + (isOffset ? FORK_OFFSET_LEFT_2 : FORK_OFFSET_RIGHT_2));

                // change laser intensity so that it doesn't look like a solid line
                laserLine.startWidth = laserIncreasing ? laserLine.startWidth + WIDTH_INTERVAL : laserLine.startWidth - WIDTH_INTERVAL;
                laserLine.endWidth = laserIncreasing ? laserLine.endWidth + WIDTH_INTERVAL : laserLine.endWidth - WIDTH_INTERVAL;
                if (laserLine.startWidth >= WIDTH_MAX) laserIncreasing = false;
                else if (laserLine.startWidth <= WIDTH_MIN) laserIncreasing = true;

                // deal damage
                if (hit.collider.CompareTag(Tag.ENEMY))
                {
                    SkellyController enemyController = hit.collider.GetComponent<SkellyController>();
                    enemyController.damage(DAMAGE_AMOUNT, DamageType.RMB);
                }
                else if (hit.collider.CompareTag(Tag.SPAWNER))
                {
                    SpawnerController spawner = hit.collider.GetComponent<SpawnerController>();
                    spawner.damage(DAMAGE_AMOUNT);
                }
            }
            else if (visualsEnabled) // if battery ran empty but user is still holding down the mouse button, disable laser effects (without starting recharging)
            {
                enableVisuals(false);
            }
        }
    }

    public void setOffset(bool left)
    {
        if (left)
        {
            if (!isOffset)
            {
                isOffset = true;
                transform.position += OFFSET_X;
            }
        }
        else if (isOffset)
        {
            isOffset = false;
            transform.position -= OFFSET_X;
        }
    }

    public void resetAttack()
    {
        StopAllCoroutines();
        enableVisuals(false);
        chargeDelay = 0.5f;
        chargeInterval = 0.1f;
        chargeAmount = 1;
        mainBattery = new Battery(mainIndicator);
        bonusBattery = null;
        laserIncreasing = false;
        laserLine.startWidth = WIDTH_MAX;
        laserLine.endWidth = WIDTH_MAX;
    }

    public void unlockDouble()
    {
        bonusBattery = new Battery(bonusIndicator);
    }

    public void unlockSpeed()
    {
        chargeDelay = 0.2f;
        chargeInterval = 0.15f;
        chargeAmount = 2;
    }

    public void addCharge(ushort amount)
    {
        ushort remainder = selectBattery(false).forceCharge(amount);
        if (remainder > 0 && bonusBattery != null) selectBattery(false).forceCharge(remainder);
    }

    private Battery selectBattery(bool selectHigherCharge)
    {
        if (bonusBattery == null) return mainBattery;
        if (bonusBattery.charge > mainBattery.charge)
        {
            return selectHigherCharge ? bonusBattery : mainBattery;
        }
        else
        {
            return selectHigherCharge ? mainBattery : bonusBattery;
        }
    }

    private void enableVisuals(bool enable)
    {
        visualsEnabled = enable;
        laserLine.enabled = enable;
        laserFork.enabled = enable;
        if (enable) laserParticles.Play();
        else laserParticles.Stop();
    }

    private IEnumerator chargeRoutine(Battery battery)
    {
        yield return new WaitForSeconds(chargeDelay);
        while (battery.chargeTillFull(chargeAmount))
        {
            yield return new WaitForSeconds(chargeInterval);
        }
        yield break;
    }

    private class Battery
    {
        public ushort charge { get; set; }
        public bool inUse { get; set; }
        private readonly SliderController indicator;

        public Battery(SliderController indicator)
        {
            this.indicator = indicator;
            this.charge = CHARGE_LIMIT;
        }

        public bool drain()
        {
            if (charge < DRAIN_AMOUNT)
            {
                indicator.updateValue(CHARGE_LIMIT, 0);
                return false;
            }
            else
            {
                charge -= DRAIN_AMOUNT;
                indicator.updateValue(CHARGE_LIMIT, charge);
                return true;
            }
        }

        public bool chargeTillFull(byte amount)
        {
            if (inUse) { return false; }
            charge += amount;
            indicator.updateValue(CHARGE_LIMIT, charge);
            if (charge > CHARGE_LIMIT)
            {
                charge = CHARGE_LIMIT;
                indicator.fade();
                return false;
            }
            return true;
        }

        public ushort forceCharge(ushort amount)
        {
            if (charge == CHARGE_LIMIT) return amount;

            if (charge > CHARGE_LIMIT)
            {
                charge = CHARGE_LIMIT;
                indicator.fade();
                indicator.updateValue(CHARGE_LIMIT, charge);
                return (ushort)(charge - CHARGE_LIMIT);
            }

            indicator.updateValue(CHARGE_LIMIT, charge);
            return 0;
        }
    }
}