using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
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
        SceneLoader.load(SceneLoader.Scene.GameScene);
    }
}
