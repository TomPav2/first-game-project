using UnityEngine;
using static GameValues;

public class SpellFiresurge : Spell
{
    [SerializeField] private MainCharacterSheet maincChar;
    private Animator animator;

    private void Awake()
    {
        manacost = 30;
        channeled = false;
        animator = GetComponent<Animator>();
    }
    public override void cast(Vector2 startPos)
    {
        animator.SetTrigger(Trigger.ANIMATION_START);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER)) maincChar.damage(1);
    }
    // not a channeled spell
    public override byte channel()
    {
        throw new System.NotImplementedException();
    }

    public override void startChanneling()
    {
        throw new System.NotImplementedException();
    }

    public override void stopChanneling()
    {
        throw new System.NotImplementedException();
    }
}
