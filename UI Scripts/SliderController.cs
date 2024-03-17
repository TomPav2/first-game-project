using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image background;
    [SerializeField] private Image fill;

    private Coroutine fading;
    private float alpha = 0f;
    private readonly float fadeAmount = 1f / 60f;

    private void Start()
    {
        updateVisibility();
    }

    public void updateValue(float max, float current)
    {
        if (max == 0)
        {
            Debug.LogError("Trying to set slider with max value of 0 on slider" + gameObject.name); return;
        }
        if (alpha < 1f)
        {
            if (fading != null && current > 0) StopCoroutine(fading);
            alpha = 1f;
            updateVisibility();
        }
        if (current > 0)
        {
            float newValue = Mathf.Clamp01(current / max);
            slider.value = newValue;
            fill.enabled = true;
        }
        else
        {
            fill.enabled = false;
        }
    }

    public void fade()
    {
        fading = StartCoroutine(fadeRoutine());
    }

    private void updateVisibility()
    {
        fill.color = fill.color.WithAlpha(alpha);
        background.color = background.color.WithAlpha(alpha);
    }

    private IEnumerator fadeRoutine()
    {
        while (alpha > 0)
        {
            alpha -= fadeAmount;
            if (alpha < 0) alpha = 0;
            updateVisibility();
            yield return null;
        }
        yield break;
    }
}