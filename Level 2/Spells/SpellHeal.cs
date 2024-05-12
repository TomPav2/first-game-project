using UnityEngine;
using static GameValues;

public class SpellHeal : Spell
{
    [SerializeField] private BossEnemyController enemy1;
    [SerializeField] private BossEnemyController enemy2;
    [SerializeField] private BossEnemyController enemy3;

    private Animator animator;

    private static readonly Vector3 OFFSET = new Vector3(0, 3.4f, 0);

    private void Awake()
    {
        animator = GetComponent<Animator>();

        channeled = false;
        manacost = 80;
    }

    public override void cast(Vector2 startPos)
    {
        BossEnemyController target = pickTarget();
        target.heal(200);
        transform.position = target.transform.position + OFFSET;
        animator.SetTrigger(Trigger.ANIMATION_START);
    }

    private BossEnemyController pickTarget()
    {
        BossEnemyController target = enemy1;

        if (enemy2 != null &&
            enemy2.isActiveAndEnabled &&
            enemy2.GetComponent<CapsuleCollider2D>().enabled &&
            enemy2.getHealth() < target.getHealth()) target = enemy2;
        else if (enemy3 != null &&
            enemy3.isActiveAndEnabled &&
            enemy3.GetComponent<CapsuleCollider2D>().enabled &&
            enemy3.getHealth() < target.getHealth()) target = enemy3;

        return target;
    }


    // not channeled
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
