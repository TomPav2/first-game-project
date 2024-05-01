using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ScenePersistence
{
    private static readonly List<GameObject> ALL_REUSABLE_ENEMIES = new List<GameObject>();
    private static readonly List<GameObject> AVAILABLE_ENEMIES = new List<GameObject>();

    public static bool inTutorial = false;
    public static bool isPaused = false;
    public static bool inMinigame = false;

    public static bool lockControls = false;
    public static bool invertControls = false;

    public static bool currentlyPainting = false;

    public static LevelDifficulty levelDifficulty = LevelDifficulty.Hard; //TODO temporary for development

    public enum Scene
    {
        MenuScene, GameScene
    }

    public enum LevelDifficulty
    {
        None, Easy, Hard
    }

    public static void load(Scene sceneName)
    {
        cullEnemies();
        currentlyPainting = false;
        inMinigame = false;
        invertControls = false;
        SceneManager.LoadScene(sceneName.ToString());
    }

    public static GameObject getEnemy() {
        if (AVAILABLE_ENEMIES.Count > 0)
        {
            GameObject enemy = AVAILABLE_ENEMIES[AVAILABLE_ENEMIES.Count - 1];
            AVAILABLE_ENEMIES.RemoveAt(AVAILABLE_ENEMIES.Count - 1);
            return enemy;
        }
        return null;
    }

    public static void reuseEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        AVAILABLE_ENEMIES.Add(enemy);
    }

    public static void registerPersistentEnemy(GameObject enemy)
    {
        ALL_REUSABLE_ENEMIES.Add(enemy);
    }

    private static void cullEnemies()
    {
        foreach (GameObject enemy in ALL_REUSABLE_ENEMIES)
        {
            if (enemy.activeInHierarchy)
            {
                reuseEnemy(enemy);
            }
        }
    }

    public static bool areEnemiesRemaining()
    {
        return AVAILABLE_ENEMIES.Count < ALL_REUSABLE_ENEMIES.Count;
    }
}
