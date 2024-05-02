using System.Collections;
using UnityEngine;
using static ScenePersistence;

public abstract class FightController : MonoBehaviour
{
    [SerializeField] private BattleArenaController arenaController;
    protected TextHudController textHud;
    protected byte enemiesToKill;

    private void Awake()
    {
        textHud = arenaController.getHudController();
    }
    public abstract void begin();

    public void registerTakedown()
    {
        enemiesToKill--;
        if (enemiesToKill == 0) StartCoroutine(waitForRemainingEnemies());
    }

    private void finishFight()
    {
        // TODO
        Debug.Log("Well Done");
    }

    private IEnumerator waitForRemainingEnemies()
    {
        while (areEnemiesRemaining()) yield return new WaitForSeconds(1);
        finishFight();
        yield break;
    }

    protected abstract IEnumerator introRoutine();
}
