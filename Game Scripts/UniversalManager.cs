using System;
using TMPro;
using UnityEngine;
using static ScenePersistence;
using static GameValues;

public class UniversalManager : MonoBehaviour
{
    [SerializeField] private GameObject easyLevel;
    [SerializeField] private GameObject hardLevel;
    [SerializeField] private GameObject mainCharacter;
    [SerializeField] private TextHudController hudController;
    [SerializeField] private TransitionController transitionController;
    [SerializeField] private AudioController audioController;

    // text panel
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private RectTransform continuePrompt;
    [SerializeField] private TMP_FontAsset fontRegular;
    [SerializeField] private TMP_FontAsset fontAlternative;
    private Vector2 panelSizeOriginal;
    private Vector2 panelSizeAlternative;

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
        Application.targetFrameRate = 60;
        Time.timeScale = 1;
    }

    private void Start()
    {
        // text settings
        panelSizeOriginal = textPanel.GetComponent<RectTransform>().sizeDelta;
        panelSizeAlternative = new Vector2(-880, -890);
        
        updateFont();
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
        pauseGame();
        hudController.pauseMenu(true);
    }

    public void resume()
    {
        hudController.pauseMenu(false);
        unPauseGame();
    }

    public void restart()
    {
        transitionController.transitionToScene(Scene.GameScene);
        GetComponent<DamageFX>().stopDamageEffect();
        audioController.stopPlaying();
    }

    public void exit()
    {
        transitionController.transitionToScene(Scene.MenuScene);
        GetComponent<DamageFX>().stopDamageEffect();
        audioController.stopPlaying();
    }

    public void waitingForEscape(Action action)
    { escapeAction = action; }

    public void clearEscape()
    { escapeAction = null; }

    public void updateFont()
    {
        if (Settings.altFont)
        {
            textField.font = fontAlternative;
            textField.lineSpacing = -80;
            textField.characterSpacing = -5;
            textField.wordSpacing = -40;
            textPanel.GetComponent<RectTransform>().sizeDelta = panelSizeAlternative;
        }
        else
        {
            textField.font = fontRegular;
            textField.lineSpacing = 0;
            textField.characterSpacing = 1;
            textField.wordSpacing = 0;
            textPanel.GetComponent<RectTransform>().sizeDelta = panelSizeOriginal;
        }
    }
}