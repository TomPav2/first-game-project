using System.Collections.Generic;
using UnityEngine;
using static ScenePersistence;

public class SpellPortalChannel : Spell, ISpawnerHandler
{
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private LevelManager manager;
    [SerializeField] private GameObject skellyPrefab;
    [SerializeField] private int enemyHealth;

    private List<SpawnerController> freeSpawners = new List<SpawnerController>();
    private List<SpawnerController> activeSpawners = new List<SpawnerController>();

    private void Awake()
    {
        channeled = true; //has both channeled and active components
        manacost = 90;
        foreach (SpawnerController spawner in GetComponentsInChildren<SpawnerController>(false))
        {
            freeSpawners.Add(spawner);
        }
    }

    // ------------ enemy lifecycle ------------
    public SkellyController getSkeleton(bool overloaded)
    {
        // get or create new enemy
        GameObject enemy = getEnemy();
        if (enemy == null)
        {
            enemy = Instantiate(skellyPrefab);
            DontDestroyOnLoad(enemy);
            registerPersistentEnemy(enemy);
        }

        enemy.SetActive(true);
        SkellyController enemyController = enemy.GetComponent<SkellyController>();
        enemyController.init(mainCharReference, manager, this, null, null);
        return enemyController;
    }

    public void removeFromLiving(GameObject enemy)
    {
        // not tracked by spell
    }

    // ------------ spawner lifecycle ------------
    public void stoppedSpawning(SpawnerController spawner)
    {
        freeSpawners.Add(spawner);
        activeSpawners.Remove(spawner);
    }


    // ------------ spell ------------
    public override byte channel()
    {
        return (byte)activeSpawners.Count;
    }

    public override void startChanneling()
    {
        // nothing to do
    }

    public override void stopChanneling()
    {
        foreach (SpawnerController spawner in activeSpawners)
        {
            spawner.stopSpawning();
        }
    }

    public override void cast(Vector2 startPos)
    {
        if (freeSpawners.Count == 0) return;
        SpawnerController spawner = freeSpawners[Random.Range(0, freeSpawners.Count)];
        activeSpawners.Add(spawner);
        freeSpawners.Remove(spawner);
        spawner.engageIndefinitely(5, enemyHealth);
    }

    public void fakeSpawnedEnemies(byte amount)
    {
        // nothing to do
    }
}
