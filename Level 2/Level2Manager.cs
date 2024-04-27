using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SceneLoader;
using static GameValues;

public class Level2Manager : LevelManager
{
    [SerializeField] private GameObject aCHeart;
    [SerializeField] private RitualController ritual;

    private bool playerHasHeart = false;

    private void Awake()
    {
        if (levelDifficulty != LevelDifficulty.Hard) gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Time.timeScale = 1;
    }

    public void givePlayerHeart()
    {
        playerHasHeart = true;
        aCHeart.SetActive(true);
    }

    public override void enteredPentagram()
    {
        if (playerHasHeart)
        {
            playerHasHeart = false;
            aCHeart.SetActive(false);
            ritual.submitHeart();
        }
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
