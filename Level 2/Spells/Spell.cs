using UnityEngine;

public abstract class Spell : MonoBehaviour
{
    public bool channeled { get; protected set; }
    public byte manacost { get; protected set; }

    public abstract void cast(Vector2 startPos);

    public abstract byte channel();

    public abstract void startChanneling();

    public abstract void stopChanneling();
}
