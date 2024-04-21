using UnityEngine.SceneManagement;

public static class SceneLoader
{

    public static bool inTutorial = false;
    public static bool isPaused = false;
    public static bool lockControls = false;
    public static bool currentlyPainting = false;
    public static LevelDifficulty levelDifficulty = LevelDifficulty.None;

    public enum Scene
    {
        MenuScene, GameScene
    }

    public enum LevelDifficulty
    {
        None, Easy, Hard
    }

    public static void load(Scene sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }
}
