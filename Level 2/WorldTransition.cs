using UnityEngine;
using static GameValues;

public class WorldTransition: MonoBehaviour
{
    [SerializeField] GameObject transitionFloor;
    [SerializeField] GameObject transitionWalls;
    [SerializeField] GameObject transitionWalltops;
    [SerializeField] RitualController ritual;

    private static readonly Vector3 ROTATION = new Vector3(0, 0, 180);
    private static readonly Vector3 POSITION = new Vector3(4, 88, 0);
    public void transformWorld()
    {
        transitionFloor.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        transitionWalls.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        transitionWalltops.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
    }

    private void afterAnimation()
    {
        ritual.secondPhaseDone();
    }
}
