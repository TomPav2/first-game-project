using System.Collections;
using UnityEngine;
using static GameValues;

public class BossExplosionController : MonoBehaviour
{
    [SerializeField] private GameObject worldHoles;
    [SerializeField] private GameObject worldHoleTargets;
    [SerializeField] private GameObject entranceBlock;
    [SerializeField] private MainCharacterSheet mainChar;
    private Animator animator;

    private bool bossAlive = true;
    private int index = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void explode()
    {
        transform.position = worldHoleTargets.transform.GetChild(index).position;
        animator.SetTrigger(Trigger.ANIMATION_START);
    }

    public void startExtraExplosions()
    {
        StartCoroutine(explosionProcess());
    }

    public void bossDied()
    {
        bossAlive = false;
    }

    // called by animation
    private void explosionApex()
    {
        worldHoles.transform.GetChild(index).gameObject.SetActive(true);
        if (index == 0)
        {
            entranceBlock.SetActive(true);
        }
        index++;
    }

    private IEnumerator explosionProcess()
    {
        while (bossAlive && index < worldHoles.transform.childCount)
        {
            explode();
            yield return new WaitForSeconds(30);
        }
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER)) mainChar.damage(12);
    }
}
