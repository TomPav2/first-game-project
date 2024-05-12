using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScenePersistence;

public class BossFightController : FightController
{

    public override void begin()
    {

        enemiesToKill = 5;
        StartCoroutine(introRoutine());
    }

    protected override IEnumerator introRoutine()
    {
        lockControls = true;

        yield break;
    }
}
