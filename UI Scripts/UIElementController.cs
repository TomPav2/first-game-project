using TMPro;
using UnityEngine;
using static GameValues;

public class UIElementController : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Animator animator;
    private RectTransform rectTransform;

    private void Awake()
    {
        textField = GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();

        if (textField != null) textField.enabled = false;
    }

    public void showText(string text)
    {
        animator.SetTrigger(Trigger.ANIMATION_STOP);
        textField.enabled = true;
        textField.text = text;
    }

    public void hideText()
    {
        textField.enabled = false;
    }

    public void popUpText(string text)
    {
        showText(text);
        animator.ResetTrigger(Trigger.ANIMATION_STOP);
        animator.SetTrigger(Trigger.FADE_TEXT);
    }

    public void setSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }
}