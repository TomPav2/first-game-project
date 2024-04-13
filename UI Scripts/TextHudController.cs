using UnityEngine;
using UnityEngine.UI;
using static GameValues;
using static SceneLoader;

public class TextHudController : MonoBehaviour
{
    [SerializeField] private Image backdrop;
    [SerializeField] private UIElementController mainText;
    [SerializeField] private UIElementController subText;
    [SerializeField] private UIElementController scoreText;
    [SerializeField] private GameObject pauseButtons;
    [SerializeField] private GameObject endButtons;
    [SerializeField] private GameObject settingsButtons;

    private static readonly Vector2 smallTextSize = new Vector2(800, 50);
    private static readonly Vector2 largeTextSize = new Vector2(1600, 50);

    private bool inSettings = false;

    public void popUp(string main, string desc)
    {
        mainText.popUpText(main);
        subText.popUpText(desc);
    }

    public void popUp(string main)
    {
        mainText.popUpText(main);
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
            pauseButtons.SetActive(true);
        }
        else
        {
            mainText.hideText();
            pauseButtons.SetActive(false);
        }
        return true;
    }

    public void goToSettings(bool entering)
    { inSettings = entering; }

    public void endGameMenu(CauseOfLoss cause, int score)
    {
        string scoreToShow;
        if (inTutorial) scoreToShow = "TODO";
        else if (score == 0) scoreToShow = "You must complete at least one level for score to count.";
        else scoreToShow = "Score: " + score;

        showEndGameMenu(cause, scoreToShow);
    }

    private void showEndGameMenu(CauseOfLoss cause, string scoreToShow)
    {
        backdrop.enabled = true;
        endButtons.SetActive(true);
        switch (cause)
        {
            case CauseOfLoss.Damage:
                mainText.showText("You died");
                scoreText.showText(scoreToShow);
                break;
        }
    }
}