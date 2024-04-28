using UnityEngine;
using static GameValues;

public abstract class EnemyBase : MonoBehaviour
{
    public abstract void damage(byte amount, DamageType type);
}
