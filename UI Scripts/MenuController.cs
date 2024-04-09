using UnityEngine;
using static SceneLoader;

public class MenuController : MonoBehaviour
{
    private bool tutorial = false;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // ---------- MENU CONTROLS ------------
    public void buttonExit()
    {
        Application.Quit();
    }

    public void buttonEasy() {
        inTutorial = tutorial;
        SceneLoader.load(SceneLoader.Scene.GameScene);
    }

    public void toggleTutorial()
    {
        tutorial = !tutorial;
    }
}
