using System.Collections;
using UnityEngine;
using static GameValues;

public class RMBAttack : MonoBehaviour

{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask collideLayers;
    [SerializeField] private SliderController mainIndicator;
    [SerializeField] private SliderController bonusIndicator;

    private static readonly ushort chargeLimit = 1000;

    private float chargeDelay = 0.5f;
    private float chargeInterval = 0.1f;
    private byte chargeAmount = 1;

    private Battery mainBattery = null;
    private Battery bonusBattery = null;

    private Battery usedBattery = null;

    private void Start()
    {
        mainBattery = new Battery(mainIndicator);
    }

    private void Update()
    {
        // on RMB down, select and activate the battery with lower charge
        if (Input.GetMouseButtonDown(1))
        {
            if (bonusBattery != null && (bonusBattery.charge > mainBattery.charge))
            {
                usedBattery = bonusBattery;
            }
            else
            {
                usedBattery = mainBattery;
            }
            usedBattery.inUse = true;
        }

        // on RMB up, release the used battery
        if (Input.GetMouseButtonUp(1))
        {
            usedBattery.inUse = false;
            StartCoroutine(chargeRoutine(usedBattery));
            usedBattery = null;
        }

        // if there is a battery in use, try drain and perform attack
        if (usedBattery != null && usedBattery.drain())
        {
            Vector2 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 from = transform.position;
            Vector2 direction = clickPosition - from;
            RaycastHit2D hit = Physics2D.Raycast(from, direction, Mathf.Infinity, collideLayers);

            // this should only occur in development, but it is safer to keep it
            if (hit == false) return;

            if (hit.collider.CompareTag(Tag.enemy))
            {
                SkellyController enemyController = hit.collider.GetComponent<SkellyController>();
                enemyController.damage(1, DamageType.RMB);
            }

            Debug.DrawLine(from, hit.point, Color.red);
        }
    }

    public void resetAttack()
    {
        StopAllCoroutines();
        chargeDelay = 0.5f;
        chargeInterval = 0.1f;
        chargeAmount = 1;
        mainBattery = new Battery(mainIndicator);
        bonusBattery = null;
    }

    public void unlockDouble()
    {
        bonusBattery = new Battery(bonusIndicator);
    }

    public void unlockSpeed()
    {
        // TODO test this
        chargeDelay = 0.3f;
        chargeInterval = 0.15f;
        chargeAmount = 2;
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
            this.charge = chargeLimit;
        }

        public bool drain()
        {
            if (charge == 0) return false;
            else
            {
                charge--;
                indicator.updateValue(chargeLimit, charge);
                return true;
            }
        }

        public bool chargeTillFull(byte amount)
        {
            if (inUse) { return false; }
            charge += amount;
            indicator.updateValue(chargeLimit, charge);
            if (charge > chargeLimit)
            {
                charge = chargeLimit;
                indicator.fade();
                return false;
            }
            return true;
        }
    }
}