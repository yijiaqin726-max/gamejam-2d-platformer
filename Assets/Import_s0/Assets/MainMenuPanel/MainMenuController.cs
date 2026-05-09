using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button loadButton;
    public Button exitButton;
    public Button settingButton;

    // 首页面板
    public GameObject mainMenuPanel;
    // 设置面板
    public GameObject mainSettingPanel;


    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
        if (settingButton != null)
            settingButton.onClick.AddListener(OpenSettingPanel);

        // 初始：显示首页，隐藏设置
        if(mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if(mainSettingPanel != null) mainSettingPanel.SetActive(false);
    }

    void StartGame()
    {
        SceneManager.LoadScene("S1");
    }

    void LoadGame()
    {
        Debug.Log("LOAD 按钮点击");
    }

    void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 打开设置：隐藏首页，显示设置
    void OpenSettingPanel()
    {
        if(mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if(mainSettingPanel != null) mainSettingPanel.SetActive(true);
    }

    // 关闭设置：显示首页，隐藏设置
    public void CloseSettingPanel()
    {
        if(mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if(mainSettingPanel != null) mainSettingPanel.SetActive(false);
    }
}