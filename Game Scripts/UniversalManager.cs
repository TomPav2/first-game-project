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

    // text panel
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private RectTransform continuePrompt;
    [SerializeField] private TMP_FontAsset fontRegular;
    [SerializeField] private TMP_FontAsset fontAlternative;
    private Vector2 panelSizeOriginal;
    private Vector2 panelSizeAlternative;
    private Vector2 textFieldSizeOriginal;
    private Vector2 textFieldSizeAlternative;
    private Vector3 textFieldPosOriginal;
    private Vector3 textFieldPosAlternative;
    private Vector3 tooltipPosOriginal;
    private Vector3 tooltipPosAlternative;

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
        // text settings
        panelSizeOriginal = textPanel.GetComponent<RectTransform>().sizeDelta;
        panelSizeAlternative = new Vector2(-880, -890);
        textFieldSizeOriginal = textField.gameObject.GetComponent<RectTransform>().sizeDelta;
        textFieldSizeAlternative = new Vector2(1040, 150);
        textFieldPosOriginal = textField.gameObject.GetComponent<RectTransform>().localPosition;
        textFieldPosAlternative = new Vector3(0, 35, 0);
        tooltipPosOriginal = continuePrompt.localPosition;
        tooltipPosAlternative = new Vector3(360, -75, 0);
        
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
    }

    public void exit()
    {
        transitionController.transitionToScene(Scene.MenuScene);
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
            textField.gameObject.GetComponent<RectTransform>().sizeDelta = textFieldSizeAlternative;
            textField.gameObject.GetComponent<RectTransform>().localPosition = textFieldPosAlternative;
            continuePrompt.localPosition = tooltipPosAlternative;
        }
        else
        {
            textField.font = fontRegular;
            textField.lineSpacing = 0;
            textField.characterSpacing = 1;
            textField.wordSpacing = 0;
            textPanel.GetComponent<RectTransform>().sizeDelta = panelSizeOriginal;
            textField.gameObject.GetComponent<RectTransform>().sizeDelta = textFieldSizeOriginal;
            textField.gameObject.GetComponent<RectTransform>().localPosition = textFieldPosOriginal;
            continuePrompt.localPosition = tooltipPosOriginal;
        }
    }
}