using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;
using static UnityEngine.Rendering.DebugUI.Table;

public class Fight1Controller : FightController, ISpawnerHandler
{
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private LevelManager manager;
    [SerializeField] private GameObject skellyPrefab;

    private SpawnerController[] spawners;
    private List<GameObject> livingEnemies = new List<GameObject>();
    private List<GameObject> availableEnemies = new List<GameObject>();

    public override void begin()
    {
        spawners = GetComponentsInChildren<SpawnerController>(true);

        enemiesToKill = 40;
        StartCoroutine(introRoutine());
    }

    public SkellyController getSkeleton(bool overloaded)
    {
        // get or create new enemy
        GameObject enemy;
        if (availableEnemies.Count > 0)
        {
            enemy = availableEnemies[availableEnemies.Count - 1];
            availableEnemies.RemoveAt(availableEnemies.Count-1);
        }
        else enemy = Instantiate(skellyPrefab);
        livingEnemies.Add(enemy);

        enemy.SetActive(true);
        ShieldBearer enemyController = enemy.GetComponent<ShieldBearer>();
        enemyController.init(mainCharReference, manager, this, null, null);
        return enemyController;
    }

    public void removeFromLiving(GameObject enemy)
    {
        livingEnemies.Remove(enemy);
    }

    public void makeAvailable(GameObject enemy)
    {
        enemy.SetActive(false);
        availableEnemies.Add(enemy);
        registerTakedown();
    }

    protected override IEnumerator introRoutine()
    {
        textHud.popUp("The buggers found shields!", null, null);
        yield return new WaitForSeconds(2);

        // open four spawners with delay
        spawners[0].spawnSpecificAmount(0.2f, Difficulty.BASE_HEALTH, 10);
        yield return new WaitForSeconds(15);

        spawners[2].spawnSpecificAmount(0.2f, Difficulty.BASE_HEALTH, 10);
        yield return new WaitForSeconds(15);

        spawners[1].spawnSpecificAmount(0.2f, Difficulty.BASE_HEALTH, 10);
        yield return new WaitForSeconds(15);

        spawners[3].spawnSpecificAmount(0.2f, Difficulty.BASE_HEALTH, 10);

        yield break;
    }

    public void fakeSpawnedEnemies(byte amount)
    {
        // nothing to do
    }

    public void stoppedSpawning(SpawnerController spawner)
    {
        // nothing to do
    }
}