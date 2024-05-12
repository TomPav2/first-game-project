using UnityEngine;
using static GameValues;

public class ShieldBearer : SkellyController
{
    [SerializeField] protected PolygonCollider2D shieldCollider;
    [SerializeField] protected Rigidbody2D shieldBody;
    [SerializeField] protected Transform shieldTransform;

    protected static readonly Vector3 SCALE_RIGHT = new Vector3(0.1f, 0.1f, 0.1f);
    protected static readonly Vector3 SCALE_LEFT = new Vector3(-0.1f, 0.1f, 0.1f);

    public override void afterFadeIn()
    {
        shieldCollider.enabled = true;
        shieldBody.simulated = true;
        base.afterFadeIn();
    }

    protected override void rotate()
    {
        shieldTransform.localScale = ((transform.position.x - target.x) > 0) ? SCALE_LEFT : SCALE_RIGHT;
        base.rotate();
    }

    protected override void die(DamageType damageType)
    {
        shieldCollider.enabled = false;
        shieldBody.simulated = false;
        base.die(damageType);
    }

    public override void afterFadeOut()
    {
        switchState(EnemyState.Dead);
        ((Fight1Controller)handler).makeAvailable(gameObject);
    }
}
