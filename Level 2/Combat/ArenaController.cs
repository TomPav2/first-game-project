using UnityEngine;

public abstract class ArenaController : MonoBehaviour
{
    [SerializeField] protected TextHudController hudController;
    public bool hasWaypoints { get; protected set; }

    public virtual Vector2 getWaypoint(Vector2 from)
    {
        return Vector2.zero;
    }
    public TextHudController getHudController() { return hudController; }

    public abstract void nextFight();
}
