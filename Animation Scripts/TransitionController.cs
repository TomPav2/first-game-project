using UnityEngine;
using static ScenePersistence;

public class TransitionController : MonoBehaviour
{
    [SerializeField] private GameObject screenCover;
    [SerializeField] private GameObject gameTitle;

    private Animator animator;

    private Scene targetScene;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void transitionToScene(Scene toScene)
    {
        targetScene = toScene;

        animator.enabled = true;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        screenCover.SetActive(true);
        gameTitle.SetActive(true);

        animator.SetTrigger("startTransition");
    }

    // called by animation
    private void afterFirstTransition()
    {
        animator.enabled = false;
        screenCover.SetActive(false);
        gameTitle.SetActive(false);
    }

    // called by animation
    private void afterSecondTransition()
    {
        ScenePersistence.load(targetScene);
    }
}
