using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArenaController : MonoBehaviour
{
    public bool hasWaypoints {  get; protected set; }


    public abstract Vector2 getWaypoint(Vector2 from);

    public abstract void enemyDied(BossEnemyController enemy);
}
