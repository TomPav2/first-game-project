using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaController : ArenaController
{

    private void Awake()
    {
        hasWaypoints = false;
    }

    public override void nextFight()
    {
        throw new System.NotImplementedException();
    }


}
