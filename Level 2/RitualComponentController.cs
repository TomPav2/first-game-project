using UnityEngine;

public class RitualComponentController : MonoBehaviour
{
    [SerializeField] RitualController mainController;

    // called by animation
    private void finished()
    {
        if (mainController != null) mainController.ritualDone();
    }
}
