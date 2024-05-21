using Cinemachine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScenePersistence;
using static GameValues;

public class BossFightController : FightController
{
    // UI and camera
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject bossHealthBar;

    // enemies and camera targets
    [SerializeField] private MainCharController mainChar;
    [SerializeField] private BossController boss;
    [SerializeField] private BossExplosionController explosions;
    [SerializeField] private BossEnemyController enemy1;
    [SerializeField] private BossEnemyController enemy2;
    [SerializeField] private BossEnemyController enemy3;
    [SerializeField] private LeverMaster enemy4;
    [SerializeField] private GameObject leverCamTarget;

    private static readonly Vector3 CAMERA_OFFSET = new Vector3(0, 10, 0);
    private static readonly float CAMERA_SPEED = 80;
    private Vector3 movement;
    private bool isMoving = false;

    private bool waitingForInput = false;
    private bool continueRoutine = false;

    private static readonly Color textBoxFaded = new Color(1, 1, 1, 0.1f);
    private static readonly Color textFontFaded = new Color(0, 0, 0, 0.5f);
    private Coroutine intro;
    private bool dialogBoxDamaged = false;
    private bool introRunning = false;

    private void Awake()
    {
        enemiesToKill = 4;
    }

    private void Update()
    {
        if (!isPaused && waitingForInput && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForInput = false;
            continueRoutine = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        transform.position += movement * Time.deltaTime;
    }

    public override void registerTakedown()
    {
        base.registerTakedown();
        if (enemiesToKill == 1) explosions.startExtraExplosions();
        else if (enemiesToKill == 3) boss.GetComponent<RapidBlastController>().startShooting();
    }

    public void hitIntroBox()
    {
        if (!dialogBoxDamaged)
        {
            dialogBoxDamaged = true;
            textPanel.GetComponent<Image>().color = textBoxFaded;
            textField.color = textFontFaded;
            textPanel.transform.eulerAngles = new Vector3(0, 0, flipACoin() ? 3 : -3);
            Vector3 panelPos = textPanel.transform.position;
            panelPos.y -= 1;
            textPanel.transform.position = panelPos;
        } else
        {
            finishIntro();
        }
    }

    public override void begin()
    {
        intro = StartCoroutine(introRoutine());
        introRunning = true;
    }

    protected override IEnumerator introRoutine()
    {
        lockControls = true;
        setCamToManual();

        showMessage("There it is...");
        yield return waitForInput();

        // move camera to boss
        Vector2 bossFocus = boss.transform.position + CAMERA_OFFSET;
        yield return moveCamRoutine(bossFocus);

        showMessage("The Big Bone");
        yield return waitForInput();

        // move camera back down and blow up entrance
        yield return moveCamRoutine(mainChar.transform.position);
        yield return new WaitForSeconds(1);
        explosions.explode();
        yield return new WaitForSeconds(4);

        showMessage("Woah, good thing you weren't standing there! No going back now.");
        yield return waitForInput();
        showMessage("And it seems the ritual brought back his lackeys...");
        yield return waitForInput();

        // enable and show enemies
        yield return moveCamRoutine(enemy1.transform.position + CAMERA_OFFSET);
        enemy1.gameObject.SetActive(true);
        yield return new WaitForSeconds(4);

        enemy2.gameObject.SetActive(true);
        yield return moveCamRoutine(enemy2.transform.position + CAMERA_OFFSET);
        enemy2.beginFight();
        yield return new WaitForSeconds(2);

        enemy3.gameObject.SetActive(true);
        yield return moveCamRoutine(enemy3.transform.position + CAMERA_OFFSET);
        yield return new WaitForSeconds(1.5f);

        yield return moveCamRoutine(leverCamTarget.transform.position);
        enemy4.gameObject.SetActive(true);
        enemy4.spawn(enemy4.transform.position, 400);
        yield return new WaitForSeconds(8);

        // unlock camera and wait for player to start
        setCamToAuto();
        showMessage("Well, good luck with that!");
        yield return waitForInput();

        boss.introAttack();
        for (int i = 0; i < INTROTIPS.Length; i++)
        {
            showMessage(INTROTIPS[i]);
            yield return waitForInput();
        }

        finishIntro();
        yield break;
    }

    private void finishIntro()
    {
        if (!introRunning) return;
        if (intro != null) StopCoroutine(intro);
        introRunning = false;
        // disable dialog box
        GetComponentInParent<BossArenaController>().disableCollider();
        hideMessage();

        // start fight and unlock movement
        enemy1.beginFight();
        enemy3.beginFight();
        boss.beginFight();

        lockControls = false;
    }

    private void showMessage(String message)
    {
        textPanel.SetActive(true);
        textField.text = message;
    }

    private void hideMessage()
    {
        textPanel.SetActive(false);
    }

    private IEnumerator waitForInput()
    {
        waitingForInput = true;
        while (!continueRoutine)
        {
            yield return null;
        }
        hideMessage();
        continueRoutine = false;
        yield break;
    }

    private IEnumerator moveCamRoutine(Vector2 toGo)
    {
        Vector2 pos = transform.position;
        Vector2 heading = toGo - pos;
        float distance = heading.magnitude;
        movement = heading / distance * CAMERA_SPEED;
        isMoving = true;
        yield return new WaitForSeconds(distance / CAMERA_SPEED);
        isMoving = false;
        yield break;
    }

    private void setCamToManual()
    {
        transform.position = mainChar.transform.position;
        virtualCamera.Follow = transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 1.2f;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 1.2f;
    }

    private void setCamToAuto()
    {
        virtualCamera.Follow = mainChar.transform;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = 0.6f;
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = 0.6f;
    }

    private static readonly string[] INTROTIPS = new string[]
    {
        "Actually, I may provide some helpful advice here.",
        "It is important to prioritise your targets so you don't get overwhelmed.",
        "For example, note that the spike traps are controlled manually.",
        "If you take out the skeleton controlling the levers while they are off, you won't have to worry about them.",
        "As to the other enemies, when you kill all mages, the boss will enter another phase.",
        "Therefore, keeping one of the mages alive until you kill the boss is viable strategy.",
        "Speaking of which, the green mage will focus  on healing the boss.",
        "If you're quick, you may be able to snatch the healing bolt for yourself.",
        "But he's not dumb, if you block his line of sight, he will send a harmfull spell.",
        "Which can harm the boss, of course. Just a suggestion.",
        "This is all the advice I have for you.",
        "I don't know how i managed to talk for so long while you're in mortal danger.",
        "Well, good luck again!"
    };
}