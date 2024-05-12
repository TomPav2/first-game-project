using System.Collections;
using UnityEngine;
using static GameValues;

public class LeverMaster : ShieldBearer
{
    [SerializeField] private GameObject positionList;
    [SerializeField] private GameObject spikeLevers;

    private int currentPosIndex;

    protected override void FixedUpdate()
    {
        if (!inMovingState()) return;

        body.MovePosition(Vector2.MoveTowards(transform.position, target, Time.deltaTime * currentSpeed));
    }

    public override void afterFadeIn()
    {
        GetComponent<CapsuleCollider2D>().enabled = true;
        body.simulated = true;
        shieldCollider.enabled = true;
        shieldBody.simulated = true;
        StartCoroutine(entranceRoutine());
    }

    public override void afterFadeOut()
    {
        switchState(EnemyState.Dead);
    }

    protected override void die(DamageType damageType)
    {
        StopAllCoroutines();
        shieldCollider.enabled = false;
        shieldBody.simulated = false;

        GetComponent<CapsuleCollider2D>().enabled = false;
        body.simulated = false;

        switchState(EnemyState.Dying);
    }

    protected override void rotate()
    {
        base.rotate();
        shieldTransform.localScale = SCALE_LEFT;
    }

    private void pickNewPos()
    {
        switch (currentPosIndex)
        {
            case 0:
                currentPosIndex = flipACoin() ? 1 : 2;
                break;
            case 1:
                currentPosIndex = flipACoin() ? 0 : 2;
                break;
            case 2:
                currentPosIndex = flipACoin() ? 0 : 1;
                break;
        }
    }

    private Vector2 getTarget()
    {
        return positionList.transform.GetChild(currentPosIndex).position;
    }

    private void moveLeverUpper(bool down)
    {
        int iTop = currentPosIndex * 3;
        int iMid = currentPosIndex * 3 + 1;
        spikeLevers.transform.GetChild(iTop).gameObject.SetActive(down ? false : true);
        spikeLevers.transform.GetChild(iMid).gameObject.SetActive(down ? true : false);
    }
    private void moveLeverLower(bool down)
    {
        int iMid = currentPosIndex * 3 + 1;
        int iBot = currentPosIndex * 3 + 2;
        spikeLevers.transform.GetChild(iMid).gameObject.SetActive(down ? false : true);
        spikeLevers.transform.GetChild(iBot).gameObject.SetActive(down ? true : false);
    }

    private IEnumerator moveRoutine(Vector2 toGo)
    {
        switchState(EnemyState.Walking);
        Vector2 pos = transform.position;
        target = toGo;
        rotate();
        yield return new WaitForSeconds(Vector2.Distance(pos, target) / currentSpeed);
        switchState(EnemyState.Idle);
        yield break;
    }

    private IEnumerator entranceRoutine()
    {
        yield return moveRoutine(positionList.transform.position);
        StartCoroutine(mainRoutine());
        yield break;
    }

    private IEnumerator mainRoutine()
    {
        while (inLivingState())
        {
            yield return moveRoutine(getTarget());
            yield return leverRoutine();
            pickNewPos();
        }
        yield break;
    }

    private IEnumerator leverRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        moveLeverUpper(true);
        yield return new WaitForSeconds(0.3f);
        moveLeverLower(true);

        yield return new WaitForSeconds(10);
        moveLeverLower(false);
        yield return new WaitForSeconds(0.3f);
        moveLeverUpper(false);
        yield return new WaitForSeconds(0.2f);
    }
}
