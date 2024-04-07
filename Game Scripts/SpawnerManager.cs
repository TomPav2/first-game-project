using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
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

    // ------------ enemy lifecycle ------------
    public SkellyController getSkeleton(bool overloaded)
    {
        if (livingEnemies.Count == (Difficulty.maxEnemies - 1))
        {
            manager.tooManyEnemies(); // this ends the game and calls to disable all spawning
            return null; // not a good idea to spawn another enemy at this time
        }

        SkellyController enemy = null;
        int freeEnemyCount = availableEnemies.Count;
        if (freeEnemyCount > 0)
        {
            enemy = availableEnemies[freeEnemyCount - 1];
            enemy.enabled = true;
            availableEnemies.RemoveAt(freeEnemyCount - 1);
        }
        else enemy = makeNewSkeleton();
        livingEnemies.Add(enemy);
        enemiesSpawned++;
        if (overloaded) enemiesSpawned++;
        return enemy;
    }

    public void removeFromLiving(SkellyController enemy)
    {
        livingEnemies.Remove(enemy);
    }

    public void makeAvailable(SkellyController enemy)
    {
        availableEnemies.Add(enemy);
        enemy.enabled = false;
    }

    // this is not to punish player for destroying the portal at certain times
    public void fakeSpawnedEnemies(byte amount)
    {
        enemiesSpawned += amount;
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
        spawnerRoutine = StartCoroutine(mainSpawningProcess(stage));
    }

    public void endStage()
    {
        if (spawnerRoutine != null)  StopCoroutine(spawnerRoutine);
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
    private void delegateSpawning(LevelManager.Stage stage, int enemyHealth)
    {
        bool fast = UnityEngine.Random.Range(0, 2) == 0;
        int avgTime = fast ? stage.fastSpawnerLifetime : stage.slowSpawnerLifetime;
        float avgInterval = fast ? stage.fastSpawnerInterval : stage.slowSpawnerInterval;

        bool success = false;
        byte attmepts = 5;
        while (!success && attmepts > 0)
        {
            int spawnerId = UnityEngine.Random.Range(0, spawners.Count);
            success = spawners[spawnerId].engage(avgTime, avgInterval, enemyHealth);
            attmepts -= 1;
        }
        if (success) activeSpawners++;
    }

    private IEnumerator mainSpawningProcess(LevelManager.Stage stage)
    {
        int currentInterval = 0;
        int enemyHealth = Difficulty.baseHealth;

        while (true)
        {
            // start raising enemy health after grace period
            if ((currentInterval >= Difficulty.raiseHealthGracePeriod) && (currentInterval % Difficulty.raiseHealthInterval == 0))
                enemyHealth += enemyHealth / 10;

            // if time taken by combat is below treshold, and a spawner is available, start a spawner
            if (currentInterval == 0) delegateSpawning(stage, enemyHealth);
            else
            {
                Debug.Log("Time blocked currently: " + enemiesSpawned * Difficulty.baseHealth / Difficulty.estimatedDPS);
                if (activeSpawners < stage.maxSpawners)
                {
                    int timeBlocked = enemiesSpawned * Difficulty.baseHealth / Difficulty.estimatedDPS;
                    float timeBlockedProportion = timeBlocked / (float)(currentInterval * Difficulty.interval);
                    Debug.Log("Intensity: " + Math.Round(timeBlockedProportion, 3) + " / " + stage.intensity);
                    if (timeBlockedProportion < stage.intensity) delegateSpawning(stage, enemyHealth);
                }
            }

            // increase interval counter and wait for next
            currentInterval++;
            yield return new WaitForSeconds(Difficulty.interval);
        }
    }
}