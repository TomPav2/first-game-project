using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

public class SpawnerManager : MonoBehaviour, ISpawnerHandler
{
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private LevelManager manager;
    [SerializeField] private GameObject skellyPrefab;
    [SerializeField] private RavenController raven;
    [SerializeField] private CrowController crow;
    [SerializeField] private TextHudController hud;

    private List<GameObject> livingEnemies = new List<GameObject>(); // keep track of enemies to kill at end of level
    private List<SpawnerController> freeSpawners = new List<SpawnerController>();
    private List<SpawnerController> activeSpawners = new List<SpawnerController>();

    private Coroutine spawnerRoutine;
    private int enemiesSpawned = 0;

    private void Awake()
    {
        foreach (SpawnerController spawner in GetComponentsInChildren<SpawnerController>(false))
        {
            freeSpawners.Add(spawner);
        }
    }

    // ------------ enemy lifecycle ------------
    public SkellyController getSkeleton(bool overloaded) //TODO reuse enemies
    {
        if (livingEnemies.Count >= (Difficulty.MAX_ENEMIES - 1))
        {
            manager.tooManyEnemies(); // this ends the game and calls to disable all spawning
            return null; // not a good idea to spawn another enemy at this time
        }

        // get or create new enemy
        GameObject enemy = getEnemy();
        if (enemy == null) {
            enemy = Instantiate(skellyPrefab);
            DontDestroyOnLoad(enemy);
            registerPersistentEnemy(enemy);
        }
        livingEnemies.Add(enemy);

        // add to count for difficulty calculation
        enemiesSpawned++;
        if (overloaded) enemiesSpawned++;

        enemy.SetActive(true);
        SkellyController enemyController = enemy.GetComponent<SkellyController>();
        enemyController.init(mainCharReference, manager, this, raven, crow);
        return enemyController;
    }

    public void removeFromLiving(GameObject enemy)
    {
        livingEnemies.Remove(enemy);
    }

    // this is not to punish player for destroying the portal at certain times
    public void fakeSpawnedEnemies(byte amount)
    {
        enemiesSpawned += amount;
    }

    private void killAllEnemies()
    {
        List<GameObject> enemiesToKill = livingEnemies.ToList();
        foreach (GameObject enemy in enemiesToKill)
        {
            enemy.GetComponent<SkellyController>().obliterate();
        }
    }

    // ------------ spawning ------------
    public void startStage(Level1Manager.Stage stage)
    {
        spawnerRoutine = StartCoroutine(mainSpawningProcess(stage));
    }

    public void endStage()
    {
        if (spawnerRoutine != null) StopCoroutine(spawnerRoutine);
        foreach (SpawnerController spawner in activeSpawners)
        {
            spawner.stopSpawning();
        }
        killAllEnemies();
        enemiesSpawned = 0;
        hud.stopQueuedText();
    }

    public void stoppedSpawning(SpawnerController spawner)
    {
        freeSpawners.Add(spawner);
        activeSpawners.Remove(spawner);
    }

    private void delegateSpawning(Level1Manager.Stage stage, int enemyHealth)
    {

        bool fast = flipACoin();
        int avgTime = fast ? stage.fastSpawnerLifetime : stage.slowSpawnerLifetime;
        float avgInterval = fast ? stage.fastSpawnerInterval : stage.slowSpawnerInterval;

        SpawnerController spawner = freeSpawners[Random.Range(0, freeSpawners.Count)];
        spawner.engage(avgTime, avgInterval, enemyHealth);
        activeSpawners.Add(spawner);
        freeSpawners.Remove(spawner);
        hud.popUp(null, null, "You can hear " + (fast ? "a lot of" : "some") + " clacking from the " + spawner.getLocationDescription(), 5);
    }

    private IEnumerator mainSpawningProcess(Level1Manager.Stage stage)
    {
        int currentInterval = 0;
        int enemyHealth = Difficulty.BASE_HEALTH;

        while (true)
        {
            // start raising enemy health after grace period
            if ((currentInterval >= Difficulty.RAISE_HEALTH_GRACE_PERIOD) && (currentInterval % Difficulty.RAISE_HEALTH_INTERVAL == 0))
                enemyHealth += enemyHealth / 10;

            // if time taken by combat is below treshold, and a spawner is available, start a spawner
            if (currentInterval == 0) delegateSpawning(stage, enemyHealth);
            else
            {
                if (activeSpawners.Count < stage.maxSpawners)
                {
                    int timeBlocked = enemiesSpawned * Difficulty.BASE_HEALTH / Difficulty.ESTIMATED_DPS;
                    float timeBlockedProportion = timeBlocked / (float)(currentInterval * Difficulty.INTERVAL);
                    //Debug.Log("Intensity: " + Math.Round(timeBlockedProportion, 3) + " / " + stage.intensity);
                    if (timeBlockedProportion < stage.intensity) delegateSpawning(stage, enemyHealth);
                }
            }

            // increase interval counter and wait for next
            currentInterval++;
            yield return new WaitForSeconds(Difficulty.INTERVAL);
        }
    }
}