using System.Collections;
using UnityEngine;

public class EaselController : MonoBehaviour
{
    [SerializeField] private LevelManager level1;
    [SerializeField] private SliderController progressSlider;

    private short target = 1200;
    private short progress = 0;
    private short notifyAt = -1;
    private bool currentlyPainting = false;
    private Coroutine painting;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inArea() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            painting = StartCoroutine(paintRoutine());
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

    public void setUp(short notifyAt, bool reset)
    {
        this.notifyAt = notifyAt;
        progress = 0;
        if (reset) target = 1200;
        progressSlider.updateValue(target, 0);
    }

    public void speedUpgrade()
    {
        if (progress > 0) progress = (short)(progress * 2 / 3); 
        target = 800;
    }

    private void complete()
    {
        stopPainting();
        // TODO notify levelmanager
    }

    private void stopPainting()
    {
        if (painting != null) StopCoroutine(painting);
        currentlyPainting = false;
    }

    private bool inArea()
    {
        return true;
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
            if (progress >= notifyAt)
            {
                notifyAt = -1;
                // TODO notify levelmanager
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}