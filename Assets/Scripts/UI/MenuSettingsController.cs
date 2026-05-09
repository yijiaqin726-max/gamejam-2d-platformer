using UnityEngine;
using UnityEngine.UI;

public class MenuSettingsController : MonoBehaviour
{
    [SerializeField] protected Slider musicSlider;
    [SerializeField] protected Slider sfxSlider;
    [SerializeField] protected GameObject settingPanel;
    [SerializeField] protected GameObject mainMenuPanel;

    protected virtual void Start()
    {
        GlobalAudioSettingsManager audioSettings = GlobalAudioSettingsManager.EnsureInstance();

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(audioSettings.MusicVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(audioSettings.PlayerSfxVolume);
            sfxSlider.onValueChanged.AddListener(SetPlayerSfxVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        GlobalAudioSettingsManager.EnsureInstance().SetMusicVolume(value);
    }

    public void SetPlayerSfxVolume(float value)
    {
        GlobalAudioSettingsManager.EnsureInstance().SetPlayerSfxVolume(value);
    }

    public void OpenSettingPanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        GameObject targetPanel = settingPanel != null ? settingPanel : gameObject;
        targetPanel.SetActive(true);
    }

    public void CloseSettingPanel()
    {
        GameObject targetPanel = settingPanel != null ? settingPanel : gameObject;
        targetPanel.SetActive(false);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void BackToTitle()
    {
        CloseSettingPanel();
    }

    public void BackToGame()
    {
        CloseSettingPanel();
    }
}
