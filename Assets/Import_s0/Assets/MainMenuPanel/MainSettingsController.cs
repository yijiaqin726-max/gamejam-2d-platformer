using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSettingsController : MonoBehaviour
{
    [Header("音量条")]
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // 读取保存的音量
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SfxVol", 1f);
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVol", value);
        PlayerPrefs.Save();
    }

    void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("SfxVol", value);
        PlayerPrefs.Save();
    }

    // ====================== 按钮功能 ======================
    // 返回游戏（关闭设置面板）
    public void CloseSettings()
    {
        gameObject.SetActive(false);
        Debug.Log("已关闭设置面板");
    }

    // 返回主界面（跳回S0）
    public void GoToMainMenu()
    {
        Debug.Log("正在返回主菜单 S0...");
        SceneManager.LoadScene("S0");
    }
}