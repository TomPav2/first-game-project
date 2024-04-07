using TMPro;
using UnityEngine;

public class UIElementController : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Animator animator;
    private RectTransform rectTransform;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        textField = GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (textField != null) textField.enabled = false;
        if (animator != null ) animator.enabled = false;
    }

    public void showText(string text)
    {
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
        animator.enabled = true;
    }

    public void setSize(Vector2 size)
    {
        rectTransform.sizeDelta = size;
    }

    // called by animation
    private void hideAnimationEnd()
    {
        animator.enabled = false;
        if (textField != null)
        {
            textField.enabled = false;
            textField.color = Color.white;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
}