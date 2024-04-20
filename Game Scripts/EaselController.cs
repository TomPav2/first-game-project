using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static SceneLoader;
using static GameValues;
using System.Collections.Generic;

public class EaselController : MonoBehaviour, IFading
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SliderController progressSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Transform mainChar;
    [SerializeField] private FadeController brush;

    [SerializeField] private SpriteRenderer art1;
    [SerializeField] private SpriteRenderer art2;
    [SerializeField] private SpriteRenderer art3;
    [SerializeField] private SpriteRenderer art4;

    [SerializeField] private GameObject painting1;
    [SerializeField] private GameObject painting2;
    [SerializeField] private GameObject painting3;
    [SerializeField] private GameObject painting4;

    private readonly List<SpriteRenderer> art = new List<SpriteRenderer>();
    private readonly List<GameObject> painting = new List<GameObject>();

    private readonly Color ACTIVE_COLOR = new Color(0.4f, 0.4f, 1);
    private readonly Color INACTIVE_COLOR = new Color(0.4f, 0.4f, 0.4f);

    private float target;
    private short progress = 0;
    private short notifyAtValue = -1;
    private bool currentlyPainting = false;
    private bool readyToPaint = false;
    private byte index = 0;

    private SpriteRenderer currentArt;
    private GameObject currentPainting;

    private void Awake()
    {
        target = Difficulty.PAINTING_TARGET;

        art.Add(art1);
        art.Add(art2);
        art.Add(art3);
        art.Add(art4);

        painting.Add(painting1);
        painting.Add(painting2);
        painting.Add(painting3);
        painting.Add(painting4);
    }

    private void Update()
    {
        if (isPaused) return;
        if (Input.GetKeyDown(KeyCode.Space) && inArea() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && readyToPaint && !currentlyPainting)
        {
            startPainting();
        }
        if (currentlyPainting)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                currentlyPainting = false;
            }
        }
    }

    public void setUp(float notifyAt)
    {
        progress = 0;
        notifyAtValue = (short)(Mathf.Round(target * notifyAt));
        progressSlider.updateValue(target, 0);

        currentArt = art[index];
        currentPainting = painting[index];
        index++;

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
        currentlyPainting = false;
        currentArt.gameObject.SetActive(false);
        currentPainting.SetActive(true);
        levelManager.finishStage();
    }

    private void startPainting()
    {
        if (progress == 0)
        {
            progress++;
            levelManager.startStage();
        }
        sliderFill.color = ACTIVE_COLOR;
        readyToPaint = false;
        currentlyPainting = true;
        brush.startFadeIn();
    }

    void IFading.afterFadeIn()
    {
        StartCoroutine(paintRoutine());
    }

    public void stopPainting()
    {
        sliderFill.color = INACTIVE_COLOR;
        brush.startFadeOut();
    }

    void IFading.afterFadeOut()
    {
        readyToPaint = true;
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
        while (currentlyPainting)
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
            currentArt.color = new Color(1, 1, 1, (progress / target));
            yield return new WaitForSeconds(0.1f);
        }
        stopPainting();
    }
}