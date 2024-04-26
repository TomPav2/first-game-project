using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static SceneLoader;

public abstract class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform mainCharTransform;
    [SerializeField] private LayerMask pathfindingLayers; // need to see enemies to find waypoints
    [SerializeField] private LayerMask trackPlayerLayers; // see player, not enemies

    [SerializeField] protected MainCharacterSheet mainCharacter;
    [SerializeField] protected TextHudController hudController;
    [SerializeField] protected GameObject bonusItem;


    // ------------ game control ------------
   public virtual void playerDied()
    {
        gameLost(CauseOfLoss.Damage);
    }

    protected void gameLost(CauseOfLoss cause)
    {
        lockControls = true;
        endScreen(cause);
    }

    // --------- NAVIGATION ---------
    public abstract (Vector2 pos, bool doNotWait) getWaypoint(Transform from, bool inCentralArea);

    // Check if the enemy is visible from the waypoint
    protected bool checkLoS(Transform enemy, Vector2 waypoint)
    {
        Vector2 enemyPos = enemy.position;
        Vector2 direction = enemyPos - waypoint;
        float distance = Vector2.Distance(enemyPos, waypoint);
        RaycastHit2D[] hits = Physics2D.RaycastAll(waypoint, direction, distance, pathfindingLayers);

        if (hits.Length == 0) return false;

        bool foundTarget = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag(Tag.WALL)) return false;
            if (hit.collider.transform == enemy) foundTarget = true;
        }
        return foundTarget;
    }

    public bool charLoS(Vector2 from)
    {
        Vector2 charPos = mainCharTransform.position;
        Vector2 direction = charPos - from;
        if (Vector2.Distance(from, charPos) > 80) return false;
        RaycastHit2D hit = Physics2D.Raycast(from, direction, 80f, trackPlayerLayers);
        if (hit.collider != null && hit.collider.CompareTag(Tag.PLAYER)) return true;
        return false;
    }

    public Vector2 getCharPos()
    { return mainCharTransform.position; }

    public void hideBonusItem()
    {
        bonusItem.SetActive(false);
    }

    // ------------ STAGE MANAGEMENT ------------
    public abstract void addScore(DamageType type);


    // --------- LEVEL SPECIFIC --------
    
    public abstract void endScreen(CauseOfLoss cause);

    public abstract void setupNextStage();

    public abstract void showWinScreen();
}