using System.Collections.Generic;
using UnityEngine;
using static ScenePersistence;
using static GameValues;
using System.Collections;

public class Level2Manager : LevelManager
{
    [SerializeField] private GameObject aCHeart;
    [SerializeField] private RitualController ritual;
    [SerializeField] private GameObject barrierContainer;

    private HashSet<GameObject> barriers = new HashSet<GameObject>();
    private bool playerHasHeart = false;
    private float time = 0;

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Hard) gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Time.timeScale = 1;

        foreach (Transform barrier in barrierContainer.transform)
        {
            barriers.Add(barrier.gameObject);
        }

        StartCoroutine(timerRoutine()); // TODO move this
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

    public override void setupNextStage()
    {
        // not in hard mode
    }

    public override void tooManyEnemies()
    {
        // not in hard mode
    }
}
