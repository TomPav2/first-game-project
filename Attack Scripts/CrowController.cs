using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class CrowController : MonoBehaviour
{
    [SerializeField] private GameObject damageEffect;

    private HashSet<SkellyController> skeletonsInArea = new HashSet<SkellyController>();

    //private bool upgraded = false; TODO

    public void register(SkellyController enemy)
    {
        skeletonsInArea.Add(enemy);
        if (!damageEffect.activeInHierarchy)
        {
            damageEffect.SetActive(true);
            StartCoroutine(attackProcess());
        }
    }

    public void deregister(SkellyController enemy)
    {
        skeletonsInArea.Remove(enemy);
    }

    private IEnumerator attackProcess()
    {
        while (true)
        {
            if (skeletonsInArea.Count < 1)
            {
                damageEffect.SetActive(false);
                yield break;
            }
            foreach (SkellyController enemy in skeletonsInArea)
            {
                enemy.damage(1, DamageType.Crow);
            }
            yield return new WaitForSeconds(1);
        }
    }
}