using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void updateHealth(float max, float current)
    {
        slider.value = current / max;
    }

    public void show(bool enable)
    {
        canvas.enabled = enable;
    }
}
