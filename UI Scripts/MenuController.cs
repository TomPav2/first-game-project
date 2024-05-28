using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ScenePersistence;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform lightPos;
    [SerializeField] private Toggle checkboxTutorial;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject startMenu;
    [SerializeField] private AudioController audioController;

    private List<FadeController> fadeControllers = new List<FadeController>();
    private bool tutorial = false;

    private void Awake()
    {
        foreach (FadeController fadeObject in startMenu.GetComponentsInChildren<FadeController>(true))
        {
            if (fadeObject != null) fadeControllers.Add(fadeObject);
        }
    }


    void Start()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1;
        checkboxTutorial.SetIsOnWithoutNotify(inTutorial);
        StartCoroutine(disableAnimations());
    }
    void Update()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        lightPos.localPosition = mousePos;
    }

    // ---------- MENU CONTROLS ------------
    public void buttonExit()
    {
        Application.Quit();
    }

    public void buttonEasy()
    {
        inTutorial = tutorial;
        levelDifficulty = LevelDifficulty.Easy;
        StartCoroutine(fadeAndLoad());
    }

    public void buttonHard()
    {
        levelDifficulty = LevelDifficulty.Hard;
        StartCoroutine(fadeAndLoad());
    }

    public void toggleTutorial()
    {
        tutorial = !tutorial;
    }

    // ---------- MENU ANIMATIONS ------------
    private IEnumerator fadeAndLoad()
    {
        audioController.stopPlaying();
        fadeControllers.ForEach(fadeController =>
        {
            fadeController.startFadeOut();
        });
        checkboxTutorial.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        load(Scene.GameScene);
        yield break;
    }

    private IEnumerator disableAnimations()
    {
        yield return new WaitForSeconds(0.6f);

        foreach (Animator anim in mainMenu.GetComponentsInChildren<Animator>(true))
        {
            anim.enabled = false;
        }

        yield break;
    }
}
