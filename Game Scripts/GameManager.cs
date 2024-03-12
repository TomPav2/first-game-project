using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private MainCharacterSheet mainCharReference;
    [SerializeField] private LayerMask seeThrough;

    private List<Waypoint> waypoints = new List<Waypoint>();

    public void Awake()
    {
    }

    public bool charLoS(Vector2 from)
    {
        Vector2 dwanPos = mainCharReference.transform.position;
        Vector2 to = dwanPos - from;
        if (Vector2.Distance(from, to) > 80) return false;
        RaycastHit2D hit = Physics2D.Raycast(from, to, 80f, seeThrough);
        if (hit.collider != null && hit.collider.CompareTag("Player")) return true;
        return false;
    }

    public Vector2 getCharPos()
    { return mainCharReference.transform.position; }

    private struct Waypoint
    {
        private readonly Vector2 position { get; }
        private readonly byte priority { get; }

        public Waypoint(Vector2 position, byte priority)
        {
            this.position = position;
            this.priority = priority;
        }
    }
}