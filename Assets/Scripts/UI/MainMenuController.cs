using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string FirstSceneName = "S1";
    private const string LastSceneKey = "LastScene";

    public Button startButton;
    public Button loadButton;
    public Button exitButton;
    public Button settingButton;
    public GameObject mainMenuPanel;
    public GameObject mainSettingPanel;

    private void Start()
    {
        GlobalAudioSettingsManager.EnsureInstance().ApplyVolumes();

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadGame);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        if (settingButton != null)
        {
            settingButton.onClick.AddListener(OpenSettingPanel);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        if (mainSettingPanel != null)
        {
            mainSettingPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        PlayerPrefs.SetString(LastSceneKey, FirstSceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene(FirstSceneName);
    }

    public void LoadGame()
    {
        string sceneName = PlayerPrefs.GetString(LastSceneKey, FirstSceneName);
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = FirstSceneName;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenSettingPanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (mainSettingPanel != null)
        {
            mainSettingPanel.SetActive(true);
        }
    }

    public void CloseSettingPanel()
    {
        if (mainSettingPanel != null)
        {
            mainSettingPanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}
