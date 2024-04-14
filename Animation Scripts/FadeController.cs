using UnityEngine;
using static GameValues;

public class FadeController : MonoBehaviour
{
    [SerializeField] private GameObject objectToNotify;
    [SerializeField] private bool startTransparent = false;


    private IFading scriptToNotify;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        scriptToNotify = objectToNotify.GetComponent<IFading>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer == null) Debug.LogWarning("No sprite renderer for fading object");

        spriteRenderer.enabled = !startTransparent;
    }

    private void enableRenderer()
    {
        spriteRenderer.enabled = true;
    }

    public void startFadeIn()
    {
        animator.SetTrigger(Trigger.fadeIn);
    }

    private void finishFadeIn()
    {
        scriptToNotify.afterFadeIn();
    }

    public void startFadeOut()
    {
        animator.SetTrigger(Trigger.fadeOut);
    }

    private void finishFadeOut()
    {
        spriteRenderer.enabled = false;
        scriptToNotify.afterFadeOut();
    }
}