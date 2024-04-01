using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EaselController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SliderController progressSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Transform mainChar;

    private readonly Color active = new Color(0.4f, 0.4f, 1);
    private readonly Color inactive = new Color(0.4f, 0.4f, 0.4f);

    private short target = 1200;
    private short progress = 0;
    private short notifyAtValue = -1;
    private bool currentlyPainting = false;
    private Coroutine painting;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inArea() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            startPainting();
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

    public void setUp(float notifyAt)
    {
        progress = 0;
        notifyAtValue = (short)(Mathf.Round(target * notifyAt));
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
        levelManager.finishStage();
        Debug.Log("DONE"); // TODO remove
    }

    private void startPainting()
    {
        if (progress == 0)
        {
            progress++;
            levelManager.startStage();
        }
        sliderFill.color = active;
        painting = StartCoroutine(paintRoutine());
        Debug.Log("Painting");
        currentlyPainting = true;
    }

    private void stopPainting()
    {
        if (painting != null) StopCoroutine(painting);
        sliderFill.color = inactive;
        currentlyPainting = false;
    }

    private bool inArea()
    {
        float distX = transform.position.x - mainChar.position.x;
        if (Mathf.Abs(distX) > 7) return false;
        float distY = transform.position.y - mainChar.position.y;
        if (Mathf.Abs(distY) > 7) return false;
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
            if (progress >= notifyAtValue)
            {
                notifyAtValue = -1;
                Debug.Log("TO spawn bonus"); // TODO notify levelmanager
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}