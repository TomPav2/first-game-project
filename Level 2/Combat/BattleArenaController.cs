using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class BattleArenaController : MonoBehaviour
{
    [SerializeField] private GameObject waypointContainer;
    [SerializeField] private Level2Manager levelManager;
    [SerializeField] private MainCharacterSheet mainCharacterSheet;
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject gate;
    [SerializeField] private TextHudController hudController;
    [SerializeField] private FightController fight1;
    [SerializeField] private FightController fight2;
    [SerializeField] private FightController fight3;

    private List<Vector2> waypoints = new List<Vector2>();
    private List<FightController> fights;
    private int fightIndex = 0;

    private void Awake()
    {
        hasWaypoints = true;
        foreach (Transform waypoint in waypointContainer.transform)
        {
            waypoints.Add(waypoint.position);
        }
        fights = new List<FightController> { fight1, fight2, fight3 };
    }

    public bool hasWaypoints { get; protected set; }

    public TextHudController getHudController() { return hudController; }

    public void nextFight()
    {
        if (fightIndex == fights.Count)
        {
            StartCoroutine(finishRoutine());
            return;
        } else
        {
            fights[fightIndex].gameObject.SetActive(true);
            fights[fightIndex].begin();
            fightIndex++;
        }
    }
    
    public Vector2 getWaypoint(Vector2 from)
    {
        Vector2 selectedWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        while (Vector2.Distance(selectedWaypoint, mainCharacterSheet.transform.position) < 10
            && Vector2.Distance(selectedWaypoint, from) < 2)
        {
            selectedWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        }
        return selectedWaypoint;
    }

    private IEnumerator finishRoutine()
    {
        hudController.popUp("Congratulations!", null, null);
        yield return new WaitForSeconds(3);
        levelManager.givePlayerHeart(barrier);
        gate.SetActive(false);
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            GetComponent<BoxCollider2D>().enabled = false;
            gate.SetActive(true);
            nextFight();
        }
    }
}