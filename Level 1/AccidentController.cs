using UnityEngine;
using UnityEngine.UI;
using static GameValues;

public class AccidentController : MonoBehaviour
{
    private AccidentManager manager;
    private Image buttonImage;
    private Button button;
    private Animator animator;
    private RectTransform rectTransform;

    private bool onScreen = false;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();
        manager = GetComponentInParent<AccidentManager>();
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();

        buttonImage.enabled = false;
        button.enabled = false;
    }

    public void show()
    {
        randomise();

        animator.ResetTrigger(Trigger.ANIMATION_STOP);
        onScreen = true;
        buttonImage.enabled = true;
        button.enabled = true;
        button.interactable = false;

        animator.SetTrigger(Trigger.FADE_IN);
    }

    public void dismiss(bool byClick)
    {
        if (onScreen)
        {
            buttonImage.enabled = false;
            button.enabled = false;
            if (byClick) manager.accidentCleared(this);
            onScreen = false;
            animator.SetTrigger(Trigger.ANIMATION_STOP);
        }
    }

    private void afterFadeIn()
    {
        button.interactable = true;
        manager.accidentAppeared();
    }

    private void randomise()
    {
        float posX = Random.Range(-500, 500);
        float posY = Random.Range(-400, 400);
        float rotation = Random.Range(0, 360);

        rectTransform.localPosition = new Vector3(posX, posY, 0);
        rectTransform.Rotate(new Vector3(0, 0, rotation));
    }
}
