using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private LevelManager manager;
    [SerializeField] private GameObject skellyPrefab;
    [SerializeField] private RavenController raven;

    private List<SkellyController> availableEnemies = new List<SkellyController>(); // pool of enemies to reuse
    private List<SkellyController> livingEnemies = new List<SkellyController>(); // keep track of enemies to kill at end of level
    private List<SpawnerController> spawners = new List<SpawnerController>();

    private Coroutine spawnerRoutine;
    private byte activeSpawners = 0;
    private int enemiesSpawned = 0;

    private void Awake()
    {
        foreach (SpawnerController spawner in GetComponentsInChildren<SpawnerController>(false))
        {
            spawners.Add(spawner);
        }
    }

    public void testSpawn() // TODO remove
    {
        Vector2 spawnPos = new Vector2(-107, 87);
        getSkeleton(false).spawn(spawnPos, 100);
    }

    // ------------ enemy lifecycle ------------
    public SkellyController getSkeleton(bool overloaded)
    {
        SkellyController skelly = null;
        int freeEnemyCount = availableEnemies.Count;
        if (freeEnemyCount > 0)
        {
            skelly = availableEnemies[freeEnemyCount - 1];
            availableEnemies.RemoveAt(freeEnemyCount - 1);
        }
        else skelly = makeNewSkeleton();
        livingEnemies.Add(skelly);
        enemiesSpawned++;
        if (overloaded) enemiesSpawned++;
        return skelly;
    }

    public void removeFromLiving(SkellyController enemy)
    {
        livingEnemies.Remove(enemy);
    }

    public void makeAvailable(SkellyController enemy)
    {
        availableEnemies.Add(enemy);
    }

    private void destroyAllEnemies()
    {
        foreach (SkellyController enemy in livingEnemies)
        {
            enemy.damageMax(DamageType.None);
        }
    }

    private SkellyController makeNewSkeleton()
    {
        GameObject newEnemy = Instantiate(skellyPrefab);
        SkellyController newController = newEnemy.GetComponent<SkellyController>();
        newController.init(mainCharReference, manager, this, raven);
        return newController;
    }

    // ------------ spawning ------------
    public void startStage(LevelManager.Stage stage)
    {
        spawnerRoutine = StartCoroutine(mainSpawningProcess(stage.spawnInterval, stage.spawnerLifetime, stage.maxSpawners, stage.intensity));
    }

    public void endStage()
    {
        foreach (SpawnerController spawner in spawners)
        {
            spawner.stopSpawning();
        }
        destroyAllEnemies();
        enemiesSpawned = 0;
    }

    public void stoppedSpawning()
    {
        activeSpawners--;
    }

    // this is a bad way of getting a random available element, but I like the idea of the player getting a break by chance
    private void delegateSpawning(int avgTime, float avgInterval, int enemyHealth)
    {
        bool success = false;
        byte attmepts = 5;
        while (!success && attmepts > 0)
        {
            int spawnerId = Random.Range(0, spawners.Count);
            success = spawners[spawnerId].engage(avgTime, avgInterval, enemyHealth);
            attmepts -= 1;
        }
        if (success) activeSpawners++;
    }

    private IEnumerator mainSpawningProcess(float interval, int lifetime, byte spawnerLimit, float desiredIntensity)
    {
        int currentInterval = 0;
        int enemyHealth = Difficulty.baseHealth;

        while (true)
        {
            // start raising enemy health after grace period
            if ((currentInterval >= Difficulty.raiseHealthGracePeriod) && (currentInterval % Difficulty.raiseHealthInterval == 0))
                enemyHealth += enemyHealth / 10;

            // if time taken by combat is below treshold, and a spawner is available, start a spawner
            if (currentInterval == 0) delegateSpawning(lifetime, interval, enemyHealth);
            else
            {
                if (activeSpawners < spawnerLimit)
                {
                    int timeBlocked = enemiesSpawned * Difficulty.baseHealth / Difficulty.estimatedDPS;
                    float timeBlockedProportion = timeBlocked / (currentInterval * 30);
                    Debug.Log("Intensity: " + timeBlockedProportion + " / " + desiredIntensity);
                    if (timeBlockedProportion < desiredIntensity) delegateSpawning(lifetime, interval, enemyHealth);
                }
            }

            // increase interval counter and wait for next
            currentInterval++;
            yield return new WaitForSeconds(30);
        }
    }
}