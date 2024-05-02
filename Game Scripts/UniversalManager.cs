using System;
using UnityEngine;
using static ScenePersistence;

public class UniversalManager : MonoBehaviour
{
    [SerializeField] private GameObject easyLevel;
    [SerializeField] private GameObject hardLevel;
    [SerializeField] private GameObject mainCharacter;
    [SerializeField] private TextHudController hudController;
    [SerializeField] private TransitionController transitionController;

    private LevelManager activeManager;

    private Action escapeAction = null;

    private void Awake()
    {
        GameObject currentLevel = levelDifficulty == LevelDifficulty.Easy ? easyLevel : hardLevel;
        Destroy(levelDifficulty == LevelDifficulty.Easy ? hardLevel : easyLevel);
        activeManager = currentLevel.GetComponent<LevelManager>();

        currentLevel.SetActive(true);
        mainCharacter.GetComponent<MainCharacterSheet>().init(activeManager);
        mainCharacter.GetComponent<MainCharController>().init(activeManager);
    }

    private void Start()
    {
        isPaused = false;
    }

    // ------------ game control ------------
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escapeAction != null) escapeAction();
            else if (isPaused) resume();
            else pause();
        }
    }

    private void pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        hudController.pauseMenu(true);
    }

    public void resume()
    {
        hudController.pauseMenu(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void restart()
    {
        transitionController.transitionToScene(Scene.GameScene);
    }

    public void exit()
    {
        transitionController.transitionToScene(Scene.MenuScene);
    }

    public void waitingForEscape(Action action)
    { escapeAction = action; }

    public void clearEscape()
    { escapeAction = null; }
}