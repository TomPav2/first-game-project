using System.Collections.Generic;
using UnityEngine;
using static GameValues;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform mainCharTransform;
    [SerializeField] private LayerMask collideLayers;

    private List<Waypoint> waypoints = new List<Waypoint>();

    public void Awake()
    {
    }

    public bool charLoS(Vector2 from)
    {
        Vector2 charPos = mainCharTransform.position;
        Vector2 to = charPos - from;
        if (Vector2.Distance(from, to) > 80) return false;
        RaycastHit2D hit = Physics2D.Raycast(from, to, 80f, collideLayers);
        if (hit.collider != null && hit.collider.CompareTag(Tag.player)) return true;
        return false;
    }

    public Vector2 getCharPos()
    { return mainCharTransform.position; }

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