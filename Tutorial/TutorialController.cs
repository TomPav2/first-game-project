using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SceneLoader;
using static GameValues;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private Transform mainCharTransform;
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private MainCharController mainCharController;
    [SerializeField] private GameObject blockage;
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private GameObject rmbAttack;
    [SerializeField] private SpawnerTutorial spawner;
    [SerializeField] private ThrowItem throwObject;
    [SerializeField] private LevelManager levelManager;
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

    private bool waitingForInput = false;
    private bool waitForEvent = false; // when this is true, hitting space won't trigger next tutorial step
    private byte waitingForEnemies = 0;

    private static readonly Vector2 TUTORIAL_START_LOCATION = new Vector2(-96, 33);

    private readonly List<SkellyTutorial> tutorialEnemies = new List<SkellyTutorial>();
    private readonly List<Action> steps = new List<Action>();

    private static List<String> messages = new List<string> {
        "Welcome to the tutorial! You are about to learn the controls and basic mechanics of this game. Escape pauses the game and opens the menu.",
        "You can move around with WASD keys. Head up to the cell to begin your combat lessons.",
        // On trigger enter
        "In this game, you will be fighting hordes of skeletons. They will go after you if they spot you, so you need to learn how to deal with them.",
        "Your main attack is a magical projectile, which can hit multiple enemies if they are close together.",
        "Click or hold the left mouse button to shoot.\nI'll give you a practice target to destroy. It won't move, but don't touch it.",
        // Spawn enemy, wait for its death
        "That's it! Your other attack is the\nDeath Enforcing Vicious Inferno Laser.",
        "It deals rapid damage to one target with unlimited range. Use it wisely as it recharges rather slowly.",
        "You can use your beam by holding the right mouse button. Destroy the next enemy with it, but be careful, this one moves.",
        // Spawn enemy, wait for its death
        "Great at finishing off enemies who are low on health. But it has another use, which you will learn soon.",
        "Skeletons come here from portals, which can appear in a few specific places. There are three types of portals.",
        "First, fast portals. They will quickly spawn a wave of enemies before vanishing.",
        "This can be challenging, especially if there are multiple portals active. But you can handle it, right? Just don't get cornered.",
        "Then there are, of course, slow portals. They spawn enemies annoyingly slowly, just enough to distract you from your objective.",
        "You can wait them out, but since enemies grow stronger over time, it's not advised. There is an another strategy, but before that...",
        // Portal appears
        "The last type of portal is the tutorial portal. Look, it's pretty harmless. I'll poke it to spawn a few enemies for your warmup.",
        // Spawn three enemies, wait for them to die
        "Now, let's get rid of the portal. While you can't exactly destroy it, you can use your beam to overload it. Try it now.",
        // Enable portal collider, wait for overload
        "When you overload a portal, it will quickly dump as many enemies as it can before it collapses. Better get out of the way.",
        // Spawn three enemies, wait for them to die
        "Why go through all this trouble? Because some of the skeletons will be lost to the void, giving you more free time.",
        "In short, if you can run or fight through the enemies to reach the portal, you should.",
        "That's it for the combat tutorial. Make your way to the main chamber - down the corridor and take the first turn.",
        // On trigger enter
        "In the centre of this chamber is an easel. Your goal is to paint four paintings. This will naturally attract the art-hating skeletons.",
        "They are real snobs, I tell you.",
        "You will inevitably make mistakes when painting. Clear them by simply clicking. They will slow you down until you do so.",
        "When you're next to the easel, press spacebar to start painting. That's all you need to know to start. Good luck!"
    };

    private void Awake()
    {
        if (!inTutorial) gameObject.SetActive(false);

        steps.Add(() => showMessage(messages[0], true));
        steps.Add(() => enablePlayerControls());
        steps.Add(() => showMessage(messages[1], false)); // On trigger enter
        steps.Add(() => showMessage(messages[2], true));
        steps.Add(() => showMessage(messages[3], true));
        steps.Add(() => showMessage(messages[4], true));
        steps.Add(() => spawnFirstEnemy()); // Spawn enemy, wait for its death
        steps.Add(() => showMessage(messages[5], true));
        steps.Add(() => showMessage(messages[6], true));
        steps.Add(() => enableRMB());
        steps.Add(() => showMessage(messages[7], true));
        steps.Add(() => spawnSecondEnemy()); // Spawn enemy, wait for its death
        steps.Add(() => showMessage(messages[8], true));
        steps.Add(() => showMessage(messages[9], true));
        steps.Add(() => showMessage(messages[10], true));
        steps.Add(() => showMessage(messages[11], true));
        steps.Add(() => showMessage(messages[12], true));
        steps.Add(() => showMessage(messages[13], true));
        steps.Add(() => showPortal()); // Portal appears
        steps.Add(() => showMessage(messages[14], true));
        steps.Add(() => pokePortal()); // Spawn three enemies, wait for them to die
        steps.Add(() => allowPortalOverload()); // Enable portal collider, wait for overload
        steps.Add(() => showMessage(messages[15], false));
        steps.Add(() => showMessage(messages[16], true));
        steps.Add(() => spawnEnemies(0.6f));
        steps.Add(() => hidePortal()); // Portal disappears when all enemies are dead
        steps.Add(() => showMessage(messages[17], true));
        steps.Add(() => showMessage(messages[18], true));
        steps.Add(() => hideBlockage());
        steps.Add(() => showMessage(messages[19], false)); // Wait for player to enter central area
        steps.Add(() => showMessage(messages[20], true));
        steps.Add(() => showMessage(messages[21], true));
        steps.Add(() => showMessage(messages[22], true));
        steps.Add(() => showMessage(messages[23], true));
        steps.Add(() => finishTutorial());
    }

    private void Start()
    {
        // add enemies
        foreach (SkellyTutorial enemy in GetComponentsInChildren<SkellyTutorial>(false))
        {
            tutorialEnemies.Add(enemy);
            enemy.init(mainChar, null, null, null, null);
        }

        // setup starting player controls
        lockControls = true;
        rmbAttack.SetActive(false);
        mainCharTransform.position = TUTORIAL_START_LOCATION;

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


        blockage.SetActive(true);
        nextStep();
    }

    private void Update()
    {
        if (!isPaused && waitingForInput && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForInput = false;
            if (waitForEvent) hideMessage();
            else nextStep();
        }
    }

    public void nextStep()
    {
        if (steps.Count == 0) return;

        Action currentStep = steps[0];
        steps.RemoveAt(0);
        currentStep();
    }

    public void showMessage(String message, bool triggerNext)
    {
        textPanel.SetActive(true);
        waitingForInput = true;
        waitForEvent = !triggerNext;
        textField.text = message;
    }

    public void hideMessage()
    {
        textPanel.SetActive(false);
    }

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

    public void notifyDeath()
    {
        waitingForEnemies--;
        if (waitingForEnemies == 0) nextStep();
    }

    private void enablePlayerControls()
    {
        lockControls = false;
        nextStep();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            GetComponent<BoxCollider2D>().enabled = false;
            waitForEvent = false;
            nextStep();
        }
    }

    private void spawnFirstEnemy()
    {
        hideMessage();
        waitingForEnemies++;
        tutorialEnemies[0].spawnInState(EnemyState.Idle, DamageType.RMB, this);
    }

    private void enableRMB()
    {
        rmbAttack.SetActive(true);
        nextStep();
    }

    private void spawnSecondEnemy()
    {
        hideMessage();
        waitingForEnemies++;
        tutorialEnemies[1].spawnInState(EnemyState.Following, DamageType.LMB, this);
    }

    private void showPortal()
    {
        spawner.engage(0, 0, 0);
        nextStep();
    }

    private void pokePortal()
    {
        hideMessage();
        throwObject.throwThing();
    }

    private void allowPortalOverload()
    {
        spawner.enableCollision(this);
        nextStep();
    }

    private void hidePortal()
    {
        spawner.tutorialDisengage();
        nextStep();
    }

    public void spawnEnemies(float delay)
    {
        hideMessage();
        waitingForEnemies = (byte)(tutorialEnemies.Count);
        StartCoroutine(tutorialSpawnProcess(delay));
    }

    private IEnumerator tutorialSpawnProcess(float delay)
    {
        foreach (SkellyTutorial enemy in tutorialEnemies)
        {
            enemy.spawnInState(EnemyState.Following, DamageType.None, this);
            yield return new WaitForSeconds(delay);
        }
        yield break;
    }

    private void hideBlockage()
    {
        blockage.SetActive(false);
        nextStep();
    }

    private void finishTutorial()
    {
        hideMessage();
        rmbAttack.GetComponent<RMBAttack>().addCharge(1000);
        mainChar.heal(12);
        inTutorial = false;
        levelManager.setupNextStage();
        this.gameObject.SetActive(false);
    }
}