using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneLoader;
using static GameValues;

public class Level2Manager : LevelManager
{
    [SerializeField] private GameObject aCHeart;
    [SerializeField] private RitualController ritual;
    [SerializeField] private GameObject barrierContainer;

    private HashSet<GameObject> barriers = new HashSet<GameObject>();
    private bool playerHasHeart = false;

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Hard) gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Time.timeScale = 1;

        foreach (Transform barrier in barrierContainer.transform)
        {
            barriers.Add(barrier.gameObject);
        }
    }

    public void givePlayerHeart(GameObject openBarrier)
    {
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

    public override void addScore(DamageType type)
    {
        // no score on hard mode
    }

    public override void endScreen(CauseOfLoss cause)
    {
        throw new System.NotImplementedException(); // display time
    }

    public override (Vector2 pos, bool doNotWait) getWaypoint(Transform from, bool inCentralArea)
    {
        // no vision obstacles on hard mode, enemy can always see player
        return (Vector2.zero, false);
    }

    public override void setupNextStage()
    {
        // not in hard mode
    }
}
