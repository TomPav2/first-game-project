using UnityEngine.SceneManagement;

public static class SceneLoader
{

    public static bool inTutorial = false;
    public static bool isPaused = false;
    public static bool lockControls = false;
    public static bool currentlyPainting = false;

    public enum Scene
    {
        MenuScene, GameScene
    }
    public static void load(Scene sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }
}
