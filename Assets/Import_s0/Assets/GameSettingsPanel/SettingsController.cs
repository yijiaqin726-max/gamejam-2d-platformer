using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("音量滑块")]
    public Slider musicSlider;
    public Slider sfxSlider;

    // 把你的 MainMenuPanel 拖到这里
    public GameObject mainMenuPanel;

    void Start()
    {
        // 读取上次保存的音量，没有记录就默认1（100%）
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SfxVol", 1f);

        // 滑块拖动时自动保存音量
        musicSlider.onValueChanged.AddListener(SaveMusicVolume);
        sfxSlider.onValueChanged.AddListener(SaveSfxVolume);
    }

    // 保存音乐音量
    void SaveMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVol", value);
        PlayerPrefs.Save();
    }

    // 保存音效音量
    void SaveSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("SfxVol", value);
        PlayerPrefs.Save();
    }

    // 按钮1：返回游戏 → 只关闭自己
    public void BackToGame()
    {
        gameObject.SetActive(false);
    }

    // 按钮2：返回主菜单面板 → 关设置、显示首页
    public void BackToTitle()
    {
        // 关闭当前设置面板
        gameObject.SetActive(false);
        // 显示 MainMenuPanel 首页
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}