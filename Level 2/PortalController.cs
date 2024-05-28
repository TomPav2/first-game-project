using Cinemachine;
using System.Collections;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

public class PortalController : MonoBehaviour
{
    [SerializeField] private bool teleportToJumpgame;
    [SerializeField] private bool teleportToMain;
    [SerializeField] private bool destroy;
    [SerializeField] private GameObject toEnable;
    [SerializeField] private GameObject toDisable;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private JumpCharController jumpCharController;

    [SerializeField] private AudioController audioController;
    [SerializeField] private AudioClip defaultClip;
    [SerializeField] private AudioClip platformerClip;

    private Animator animator;
    private bool catching = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (teleportToJumpgame || teleportToMain) animator.SetTrigger(Trigger.ANIMATION_START);
    }
    private void FixedUpdate()
    {
        if (catching) transform.position = new Vector3(cameraTarget.transform.position.x, transform.position.y, 0);
    }

    public void catchMe(bool catching)
    {
        this.catching = catching;
        StartCoroutine(catchRoutine());
    }

    private void transportPlayer()
    {
        if (teleportToJumpgame) transportToJumpgame();
        else if (teleportToMain) transportToMain();
    }

    private void transportToJumpgame()
    {
        lockControls = true;
        toEnable.SetActive(true);
        virtualCamera.Follow = cameraTarget.transform;
        audioController.playTrack(platformerClip);
        toDisable.SetActive(false);
    }

    private void transportToMain()
    {
        toEnable.SetActive(true);
        virtualCamera.Follow = cameraTarget.transform;
        jumpCharController.resetPos();
        lockControls = false;
        audioController.playTrack(defaultClip);
        StartCoroutine(delayedDisable());
    }

    // called by animation
    private void hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator catchRoutine()
    {
        animator.SetTrigger(Trigger.ANIMATION_START);
        yield return new WaitForSeconds(3);
        catching = false;
        animator.SetTrigger(Trigger.DESTROY);
        yield break;
    }

    private IEnumerator delayedDisable()
    {
        yield return null;
        yield return null;
        if (destroy) Destroy(toDisable);
        else toDisable.SetActive(false);
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            transportPlayer();
        }
    }
}
