using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{

    public static bool inTutorial = true;

    public enum Scene
    {
        MenuScene, GameScene
    }
    public static void load(Scene sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }
}
