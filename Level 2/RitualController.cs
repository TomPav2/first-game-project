using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameValues;
using static ScenePersistence;

public class RitualController : MonoBehaviour
{
    // ritual objects
    [SerializeField] private GameObject heart1;
    [SerializeField] private GameObject heart2;
    [SerializeField] private GameObject heart3;

    // world object pointers
    [SerializeField] private WorldTransition transition;
    [SerializeField] private GameObject mainArea;
    [SerializeField] private GameObject bossArea;
    [SerializeField] private GameObject bossFight;
    [SerializeField] private GameObject platformer;

    // UI controllers
    [SerializeField] private TextHudController hudController;

    // audio
    [SerializeField] AudioController audioController;
    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;
    [SerializeField] AudioClip clip3;

    private bool playerInPosition = false;
    private bool firstPhaseCompleted = false;
    private byte heartsCount = 0;

    private List<GameObject> hearts = new List<GameObject>();

    private void Awake()
    {
        hearts.Add(heart1);
        hearts.Add(heart2);
        hearts.Add(heart3);
    }

    // -------- ENDGAME RITUAL ----------
    public void submitHeart()
    {
        hearts[heartsCount].SetActive(true);
        heartsCount++;
        if (heartsCount == 3) StartCoroutine(startRitual());
    }

    // called by animation
    public void ritualDone()
    {
        Destroy(heart1);
        Destroy(heart2);
        Destroy(heart3);
        
        firstPhaseCompleted = true;
        notifyToBeginSecondPhase();
    }

    private void notifyToBeginSecondPhase()
    {
        if (playerInPosition)
        {
            hudController.popUp("", "Please keep your hands and feet inside the pentagram at all times!", "");
            lockControls = true;
            GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
            transition.transformWorld();
        } else
        {
            hudController.popUp("Ritual ready!", "", "Please stand in the centre of the pentagram.");
        }
    }

    public void secondPhaseDone()
    {
        lockControls = false;
        GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_STOP);
        Destroy(mainArea);
        Destroy(platformer);
        bossArea.SetActive(true);
        bossFight.SetActive(true);
    }

    private IEnumerator startRitual()
    {
        audioController.playTrackQuick(new List<AudioClip>() { clip1, clip2, clip3 });
        yield return new WaitForSeconds(3.5f);
        heart1.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        heart2.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        heart3.GetComponent<Animator>().SetTrigger(Trigger.ANIMATION_START);
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            playerInPosition = true;
            if (firstPhaseCompleted) notifyToBeginSecondPhase();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.PLAYER))
        {
            playerInPosition = false;
        }
    }
}