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
        if (objectToNotify != null) scriptToNotify = objectToNotify.GetComponent<IFading>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null) spriteRenderer.enabled = !startTransparent;
    }

    private void enableRenderer()
    {
        spriteRenderer.enabled = true;
    }

    public void startFadeIn()
    {
        animator.SetTrigger(Trigger.FADE_IN);
    }

    private void finishFadeIn()
    {
        scriptToNotify.afterFadeIn();
    }

    public void startFadeOut()
    {
        animator.SetTrigger(Trigger.FADE_OUT);
    }

    private void finishFadeOut()
    {
        spriteRenderer.enabled = false;
        scriptToNotify.afterFadeOut();
    }
}