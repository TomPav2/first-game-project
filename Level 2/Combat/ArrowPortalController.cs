using System.Collections;
using UnityEngine;
using static GameValues;

public class ArrowPortalController : ShooterController
{
    [SerializeField] private MainCharController mainChar;

    private static readonly Vector3 MOVEMENT = new Vector3(6, 0, 0);
    private static readonly Vector3 START_OFFSET = new Vector3(12, 0, 0);
    private Vector3 startpos;
    private bool dirRight;
    private bool isMoving;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        startpos = transform.position;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        transform.position += (dirRight ? MOVEMENT : -MOVEMENT) * Time.deltaTime;
    }

    public void startShooting()
    {
        setupMovement();
        animator.SetTrigger(Trigger.ANIMATION_START);
        StartCoroutine(shootingRoutine());
    }

    private void setupMovement()
    {
        dirRight = mainChar.transform.position.x > startpos.x;
        transform.position = startpos + (dirRight ? START_OFFSET : -START_OFFSET);
    }

    private void close()
    {
        animator.SetTrigger("destroyAnim");
    }

    private void hide()
    {
        // do nothing, called by animation
    }

    private IEnumerator shootingRoutine()
    {
        byte count = 0;
        yield return new WaitForSeconds(2);
        isMoving = true;
        while (count < 40)
        {
            getProjectile().shootStraight(transform.position);
            count++;
            yield return new WaitForSeconds(0.1f);
        }
        isMoving = false;
        yield return new WaitForSeconds(0.3f);
        close(); 
        yield break;
    }
}
