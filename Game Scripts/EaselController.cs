using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EaselController : MonoBehaviour
{
    [SerializeField] private LevelManager level1;
    [SerializeField] private SliderController progressSlider;
    [SerializeField] private Image sliderFill;

    private readonly Color active = new Color(0.4f, 0.4f, 1);
    private readonly Color inactive = new Color(0.4f, 0.4f, 0.4f);

    private short target = 1200;
    private short progress = 0;
    private short notifyAtValue = -1;
    private bool currentlyPainting = false;
    private Coroutine painting;

    private void Start()
    {
        // TODO this will not be needed when level is properly set up
        setUp(1, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inArea() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            painting = StartCoroutine(paintRoutine());
            sliderFill.color = active;
            currentlyPainting = true;
        }
        if (currentlyPainting)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                stopPainting();
            }
        }
    }

    public void setUp(float notifyAt, bool reset)
    {
        progress = 0;
        if (reset) target = 1200;
        notifyAtValue = (short) (Mathf.Round(target / notifyAt));
        progressSlider.updateValue(target, 0);
    }

    public void speedUpgrade()
    {
        if (progress > 0) progress = (short)(progress * 2 / 3);
        if (notifyAtValue > 0) notifyAtValue = (short)(notifyAtValue * 2 / 3);
        target = 800;
    }

    private void complete()
    {
        stopPainting();
        // TODO notify levelmanager
        Debug.Log("DONE");
    }

    private void stopPainting()
    {
        if (painting != null) StopCoroutine(painting);
        sliderFill.color = inactive;
        currentlyPainting = false;
    }

    private bool inArea()
    {
        return true; // TODO
    }

    private IEnumerator paintRoutine()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            progress++;
            progressSlider.updateValue(target, progress);
            if (progress >= target)
            {
                complete();
                yield break;
            }
            if (progress >= notifyAtValue)
            {
                notifyAtValue = -1;
                // TODO notify levelmanager
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}