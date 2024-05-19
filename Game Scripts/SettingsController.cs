using UnityEngine;
using UnityEngine.UI;
using static GameValues;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Toggle checkboxFullscreen;
    [SerializeField] private Toggle checkboxFont;
    [SerializeField] private UniversalManager manager;


    private void Awake()
    {
        checkboxFullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
        checkboxFont.SetIsOnWithoutNotify(Settings.altFont);
    }

    public void toggleFullscren()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void toggleFont()
    {
        Settings.altFont = !Settings.altFont;
        if (manager != null) manager.updateFont();
    }
}
