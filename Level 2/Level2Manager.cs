using System.Collections.Generic;
using UnityEngine;
using static ScenePersistence;
using static GameValues;
using System.Collections;
using System;
using TMPro;

public class Level2Manager : LevelManager
{
    [SerializeField] private GameObject aCHeart;
    [SerializeField] private RitualController ritual;
    [SerializeField] private GameObject barrierContainer;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private UniversalManager universalManager;

    [SerializeField] private AudioController audioController;
    [SerializeField] private AudioClip defaultClip;

    private HashSet<GameObject> barriers = new HashSet<GameObject>();
    private bool playerHasHeart = false;
    private float time = 0;

    private bool waitingForInput = false;
    private bool continueRoutine = false;

    private static readonly string[] MESSAGES = new string[] {
        "You don't know where you are...",
        "You don't know why you're here...",
        "You're not entirely sure who you are...",
        "But somehow you know this: you must face the skeletons head-on.",
        "Their leader split the essence of life into three parts and locked them away.",
        "You must find them and bring them here to begin the ritual.",
        "Good luck!"
    };

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Hard) gameObject.SetActive(false);
        else foreach (Transform barrier in barrierContainer.transform)
        {
            barriers.Add(barrier.gameObject);
        }
    }

    private void Start()
    {
        player.GetComponent<Rigidbody2D>().position = ritual.transform.position;
        lockControls = true;
        StartCoroutine(introRoutine());
        audioController.playTrack(defaultClip);
    }

    private void Update()
    {
        if (!isPaused && waitingForInput && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForInput = false;
            continueRoutine = true;
        }
    }

    public void givePlayerHeart(GameObject openBarrier)
    {
        hudController.popUp("You got a heart!", "It's quite heavy...", "Hand it in before continuing.");
        playerHasHeart = true;
        aCHeart.SetActive(true);
        updateBarriers(openBarrier);
    }

    public override void enteredPentagram()
    {
        if (playerHasHeart)
        {
            playerHasHeart = false;
            aCHeart.SetActive(false);
            ritual.submitHeart();
            updateBarriers(null);
        }
    }

    public override void restrictedAccess()
    {
        if (playerHasHeart)
        {
            hudController.popUp("", "Put away the heart at the pentagram before continuing", "");
        }
    }

    // close the path to other challenges, but open it for the current one
    private void updateBarriers(GameObject openBarrier)
    {
        foreach (GameObject bar in barriers)
        {
            bar.SetActive(playerHasHeart);
        }
        if (openBarrier != null) openBarrier.SetActive(false);
    }

    public override void endScreen(CauseOfLoss cause)
    {
        StopAllCoroutines();
        hudController.lostGameMenu();
    }

    public void winScreen()
    {
        StopAllCoroutines();
        lockControls = true;
        pauseGame();
        int seconds = Mathf.RoundToInt(time);
        int minutes = seconds / 60;
        seconds %= 60;

        hudController.wonGameMenu("Time: " + minutes + ":" + seconds);
    }

    private IEnumerator timerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }
    }

    private IEnumerator introRoutine()
    {
        universalManager.waitingForEscape(() => endIntro());
        for (int i = 0; i < MESSAGES.Length; i++)
        {
            showMessage(MESSAGES[i]);
            yield return waitForInput();
        }
        endIntro();
        yield break;
    }

    private void showMessage(String message)
    {
        textPanel.SetActive(true);
        textField.text = message;
    }

    private void hideMessage()
    {
        textPanel.SetActive(false);
    }

    private IEnumerator waitForInput()
    {
        waitingForInput = true;
        while (!continueRoutine)
        {
            yield return null;
        }
        hideMessage();
        continueRoutine = false;
        yield break;
    }

    public void endIntro()
    {
        hideMessage();
        StopAllCoroutines();
        lockControls = false;
        universalManager.clearEscape();
        StartCoroutine(timerRoutine());
    }

    public override bool charLoS(Vector2 from)
    {
        return true;
    }

    public override (Vector2 pos, bool doNotWait) getWaypoint(Transform from, bool inCentralArea)
    {
        // no vision obstacles on hard mode, enemy can always see player
        return (Vector2.zero, false);
    }

    public override void addScore(DamageType type)
    {
        // no score on hard mode
    }

    public override void tooManyEnemies()
    {
        // not in hard mode
    }
}
