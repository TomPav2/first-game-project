using UnityEngine;
using UnityEngine.UI;
using static GameValues;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Toggle checkboxFullscreen;
    [SerializeField] private Toggle checkboxFont;
    [SerializeField] private UniversalManager manager;
    [SerializeField] private Slider sliderVolume;
    [SerializeField] private AudioController audioController;

    private void Awake()
    {
        checkboxFullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
        checkboxFont.SetIsOnWithoutNotify(Settings.altFont);
        sliderVolume.SetValueWithoutNotify(Settings.volume);
        sliderVolume.onValueChanged.AddListener((vol)  => { updateVolume(vol); });
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

    public void updateVolume(float volume)
    {
        Settings.volume = volume;
        audioController.updateVolume(volume);
    }
}
