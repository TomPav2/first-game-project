
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleArenaController : ArenaController
{
    [SerializeField] private GameObject waypointContainer;
    [SerializeField] private Level2Manager levelManager;
    [SerializeField] private MainCharacterSheet mainCharacterSheet;
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject gate;

    private List<Vector2> waypoints = new List<Vector2>();
    private void Awake()
    {
        hasWaypoints = true;
        foreach (Transform waypoint in waypointContainer.transform)
        {
            waypoints.Add(waypoint.position);
        }
    }


    // ---------------- INTRO ----------------

    // ---------------- FIRST FIGHT ----------------

    // ---------------- SECOND FIGHT ----------------

    // ---------------- THIRD FIGHT ----------------
    public override void enemyDied(BossEnemyController enemy)
    {
        throw new System.NotImplementedException();
    }

    public override Vector2 getWaypoint(Vector2 from)
    {
        Vector2 selectedWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        while (Vector2.Distance(selectedWaypoint, mainCharacterSheet.transform.position) < 10
            && Vector2.Distance(selectedWaypoint, from) < 2)
        {
            selectedWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        }
        return selectedWaypoint;
    }
}
