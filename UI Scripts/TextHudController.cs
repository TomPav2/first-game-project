using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameValues;
using static ScenePersistence;

public class TextHudController : MonoBehaviour
{
    [SerializeField] private Image backdrop;
    [SerializeField] private UIElementController mainText;
    [SerializeField] private UIElementController subText;
    [SerializeField] private UIElementController thirdText;
    [SerializeField] private GameObject pauseButtons;
    [SerializeField] private GameObject endButtons;
    [SerializeField] private GameObject settingsButtons;
    [SerializeField] private GameObject upgradeButtons;
    [SerializeField] private TextMeshProUGUI buttonRaven;
    [SerializeField] private TextMeshProUGUI buttonCrow;
    [SerializeField] private RavenController raven;
    [SerializeField] private CrowController crow;
    [SerializeField] private LevelManager levelManager;

    private static readonly Vector2 SMALL_TEXT_SIZE = new Vector2(800, 50);
    private static readonly Vector2 LARGE_TEXT_SIZE = new Vector2(1600, 50);

    private bool inSettings = false;

    public void popUp(string main, string desc, string tertiary)
    {
        mainText.popUpText(main);
        subText.popUpText(desc);
        thirdText.popUpText(tertiary);
    }

    public void popUp(string main, string desc, string tertiary, float delay)
    {
        StartCoroutine(popupWithDelay(main, desc, tertiary, delay));
    }

    public bool pauseMenu(bool show)
    {
        if (inSettings)
        {
            settingsButtons.SetActive(false);
            pauseButtons.SetActive(true);
            inSettings = false;
            return false;
        }
        backdrop.enabled = show;
        if (show)
        {
            mainText.showText("Paused");
            subText.showText("");
            thirdText.showText("");
            pauseButtons.SetActive(true);
        }
        else
        {
            mainText.hideText();
            pauseButtons.SetActive(false);
        }
        return true;
    }

    public void showUpgradeScreen()
    {
        lockControls = true;
        backdrop.enabled = true;
        mainText.showText("Painting complete!");
        subText.showText("You found some magical seed...");
        thirdText.showText("Choose a bird to summon or upgrade: ");
        upgradeButtons.SetActive(true);
    }

    public void dismissUpgradeScreen()
    {
        lockControls = false;
        backdrop.enabled = false;
        mainText.hideText();
        subText.hideText();
        thirdText.hideText();
        upgradeButtons.SetActive(false);
        levelManager.setupNextStage();
    }

    public void pickRaven()
    {
        if (!raven.isUpgraded()) buttonRaven.text = "Upgrade Raven";
        else buttonRaven.enabled = false;
        raven.summon();
        dismissUpgradeScreen();
    }

    public void pickCrow()
    {
        if (!crow.isUpgraded()) buttonCrow.text = "Upgrade Crow";
        else buttonCrow.enabled = false;
        crow.summon();
        dismissUpgradeScreen();
    }

    public void goToSettings(bool entering)
    { inSettings = entering; }

    public void wonGameMenu(string result)
    {
        showEndMenu("You won", "", result);
    }

    public void stopQueuedText()
    {
        StopAllCoroutines();
    }

    public void lostGameMenu(CauseOfLoss cause, int score)
    {
        string title = "";
        string subtitle = "";
        switch (cause)
        {
            case CauseOfLoss.Damage:
                title = "You died";
                subtitle = "";
                break;
            case CauseOfLoss.Overrun:
                title = "Too many enemies";
                subtitle = "They piled up and destroyed your art";
                break;
        }

        string scoreToShow;
        if (inTutorial) scoreToShow = "TODO";
        else if (score == 0) scoreToShow = "You must complete at least one level for score to count.";
        else scoreToShow = "Score: " + score;

        showEndMenu(title, subtitle, scoreToShow);
    }

    private void showEndMenu(string title, string subtitle, string scoreToShow)
    {
        backdrop.enabled = true;
        mainText.showText(title);
        subText.showText(subtitle);
        thirdText.showText(scoreToShow);
        endButtons.SetActive(true);
    }

    private IEnumerator popupWithDelay(string main, string desc, string tertiary, float delay)
    {
        yield return new WaitForSeconds(delay);
        popUp(main, desc, tertiary);
        yield break;
    }
}