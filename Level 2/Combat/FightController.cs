using System.Collections;
using UnityEngine;
using static ScenePersistence;

public abstract class FightController : MonoBehaviour
{
    [SerializeField] private ArenaController arenaController;
    protected TextHudController textHud;
    protected byte enemiesToKill;

    private void Awake()
    {
        textHud = arenaController.getHudController();
    }
    public abstract void begin();

    public virtual void registerTakedown()
    {
        enemiesToKill--;
        if (enemiesToKill == 0) StartCoroutine(waitForRemainingEnemies());
    }

    private void finishFight()
    {
        arenaController.nextFight();
        Destroy(gameObject);
    }

    private IEnumerator waitForRemainingEnemies()
    {
        while (areEnemiesRemaining()) yield return new WaitForSeconds(1);
        finishFight();
        yield break;
    }

    protected abstract IEnumerator introRoutine();
}
