using UnityEngine;
using static ScenePersistence;

public class SpellCurse : Spell
{
    private void Awake()
    {
        channeled = true;
        manacost = 3;
    }

    public override byte channel()
    {
        return manacost;
    }

    public override void startChanneling()
    {
        invertControls = true;
    }

    public override void stopChanneling()
    {
        invertControls = false;
    }
    public override void cast(Vector2 startPos)
    {
        // channeled spell
    }
}
