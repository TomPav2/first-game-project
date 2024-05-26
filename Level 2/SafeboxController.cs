using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScenePersistence;

public class SafeboxController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI digitLeft;
    [SerializeField] private TextMeshProUGUI digitMiddle;
    [SerializeField] private TextMeshProUGUI digitRight;
    [SerializeField] private Button submitButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private GameObject safeBoxUI;
    [SerializeField] private MainCharacterSheet mainChar;
    [SerializeField] private UniversalManager universalManager;

    private static readonly Color COLOR_CORRECT = new Color(0.2f, 0.8f, 0.2f);
    private static readonly Color COLOR_INCORRECT = new Color(0.8f, 0.2f, 0.2f);
    private static Color colorDefault;

    private int[] values = new int[3];

    private bool playerPresent = false;
    private Coroutine indicatorTimer;

    private void Awake()
    {
        values[0] = 0;
        values[1] = 0;
        values[2] = 0;
    }

    private void Start()
    {
        colorDefault = submitButton.colors.normalColor;
    }

    private void Update()
    {
        if (playerPresent && Input.GetKeyDown(KeyCode.Space))
        {
            openGame();
        }
    }

    public void leftValue(bool add)
    {
        updateValue(add, 0);
        digitLeft.text = values[0].ToString();
    }

    public void midValue(bool add)
    {
        updateValue(add, 1);
        digitMiddle.text = values[1].ToString();
    }

    public void rightValue(bool add)
    {
        updateValue(add, 2);
        digitRight.text = values[2].ToString();
    }

    public void submit()
    {
        if (indicatorTimer != null) StopCoroutine(indicatorTimer);
        if (values[0] == 6 &&
            values[1] == 6 &&
            values[2] == 6 )
            indicatorTimer = StartCoroutine(indicatorRoutine(true));
        else indicatorTimer = StartCoroutine(indicatorRoutine(false));
    }

    private void updateValue(bool add, int index)
    {
        if (add)
        {
            if (values[index] == 9) values[index] = 0;
            else values[index]++;
        }
        else
        {
            if (values[index] == 0) values[index] = 9;
            else values[index]--;
        }
    }

    private void openGame()
    {
        universalManager.waitingForEscape(() => closeGame(false));
        lockControls = true;
        safeBoxUI.SetActive(true);
    }

    private void closeGame(bool success)
    {
        buttonImage.color = colorDefault;
        lockControls = false;
        safeBoxUI.SetActive(false);
        if (success)
        {
            playerPresent = false;
            GetComponent<BoxCollider2D>().enabled = false;
            mainChar.applyLMBBonus();
        }
    }

    private IEnumerator indicatorRoutine(bool correct)
    {
        if (correct)
        {
            submitButton.interactable = false;
            buttonImage.color = COLOR_CORRECT;
            yield return new WaitForSeconds(1f);
            closeGame(true);
        }
        else
        {
            buttonImage.color = COLOR_INCORRECT;
            yield return new WaitForSeconds(1);
            buttonImage.color = colorDefault;
        }

        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        playerPresent = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerPresent = false;
    }
}