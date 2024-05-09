using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FIght2Controller : FightController
{
    [SerializeField] GameObject assassin;
    [SerializeField] GameObject waypointContainer;

    public override void begin()
    {
        List<Vector2> waypoints = new List<Vector2>();
        foreach (Transform waypoint in waypointContainer.transform)
        {
            waypoints.Add(waypoint.position);
        }
        assassin.transform.position = waypoints[Random.Range(0, waypoints.Count)];

        enemiesToKill = 1;
        StartCoroutine(introRoutine());
    }

    protected override IEnumerator introRoutine()
    {
        textHud.popUp("Well done!", "", "");
        yield return new WaitForSeconds(3);
        textHud.popUp("Now for the real challenge...", "", "");
        yield return new WaitForSeconds(3);

        textHud.popUp("The Assassin", "", "");
        assassin.SetActive(true);
        yield break;
    }
}
