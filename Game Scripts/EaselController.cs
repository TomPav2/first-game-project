using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;
using static GameValues;

public class EaselController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SliderController progressSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Transform mainChar;

    private readonly Color active = new Color(0.4f, 0.4f, 1);
    private readonly Color inactive = new Color(0.4f, 0.4f, 0.4f);

    private short target;
    private short progress = 0;
    private short notifyAtValue = -1;
    private bool currentlyPainting = false;
    private bool readyToPaint = false;
    private Coroutine painting;

    private void Awake()
    {
        target = Difficulty.paintingTarget;
    }

    private void Update()
    {
        if (isPaused) return;
        if (Input.GetKeyDown(KeyCode.Space) && inArea() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && readyToPaint)
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
        readyToPaint = true;
    }

    public void speedUpgrade()
    {
        if (progress > 0) progress = (short)(progress * 2 / 3);
        if (notifyAtValue > 0) notifyAtValue = (short)(notifyAtValue * 2 / 3);
        target = (short)(target * 2 / 3);
    }

    private void complete()
    {
        stopPainting();
        readyToPaint = false;
        levelManager.finishStage();
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
        currentlyPainting = true;
    }

    public void stopPainting()
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
            if (progress == notifyAtValue)
            {
                notifyAtValue = -1;
                levelManager.showBonusItem();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}