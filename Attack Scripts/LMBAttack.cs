using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static GameValues;
using static SceneLoader;

public class LMBAttack : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform mainCharTransform;
    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private byte damage;
    [SerializeField] private float cooldown;
    [SerializeField] private float fadeTime;
    [SerializeField] private byte speed;

    private Vector2 targetPos = Vector2.zero;
    private bool move = false;
    private bool fading = false;
    private float alpha = 1f;
    private float fadeAmount;

    private Coroutine timer;

    private void Start()
    {
        StartCoroutine(controlRoutine());
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;
        fadeAmount =  1f / (fadeTime * 60f);
    }

    private void FixedUpdate()
    {
        if (!move) return;
        targetPos = Vector2.MoveTowards(targetPos, transform.position, Time.deltaTime * -speed);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);

        if (fading && alpha > 0f)
        {
            alpha -= fadeAmount;
            if (alpha < 0f) alpha = 0f;
            Color color = Color.white.WithAlpha(alpha);
            GetComponent<SpriteRenderer>().color = color;
        }
    }

    public byte Hit(bool needDamage)
    {
        byte dmg = needDamage ? calculateDmg() : (byte) 0;

        if (!fading)
        {
            StopCoroutine(timer);
            timer = StartCoroutine(timerRoutine(false));
        }

        return dmg;
    }

    private byte calculateDmg()
    {
        if (fading) return (byte)Mathf.Round((damage * alpha) / 3 + (damage / 5));
        else return damage;
    }

    private void startAttack()
    {
        // setup position, rotation, and target
        transform.position = mainCharTransform.position;
        Vector3 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        clickPosition.z = 0;
        transform.right = clickPosition - transform.position;
        targetPos = clickPosition;

        // perform one move so that it does not start at the centre of character
        targetPos = Vector2.MoveTowards(targetPos, transform.position, -speed / 10);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed / 10);

        // set moving, visible, enable collider
        move = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<PolygonCollider2D>().enabled = true;
        alpha = 1f;
        attackParticles.Play();

        // start the timer
        timer = StartCoroutine(timerRoutine(true));
    }

    private void endAdttack()
    {
        fading = false;
        move = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;
    }

    private IEnumerator controlRoutine()
    {
        while (true)
        {
            if (!isPaused && !lockControls && Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                startAttack();
                yield return new WaitForSeconds(1);
            }
            yield return null;
        }
    }

    // if true, wait for 0.5 and switch to fade
    // if false, wait for 'fade' and end
    private IEnumerator timerRoutine(bool mainPhase)
    {
        if (mainPhase)
        {
            yield return new WaitForSeconds(0.5f);
            timer = StartCoroutine(timerRoutine(false));
            yield break;
        }
        else
        {
            fading = true;
            attackParticles.Stop();
            yield return new WaitForSeconds(fadeTime);
            endAdttack();
            yield break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == Tag.wall)
        {
            Hit(false);
        }
    }
}