using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "S1";
    [SerializeField] private GameObject quitConfirmPanel;

    private void Start()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }

    public void StartGame()
    {
        Debug.Log("Main menu start game");

        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("MainMenuUI: firstSceneName is empty");
            return;
        }

        SceneManager.LoadScene(firstSceneName);
    }

    public void OpenQuitConfirm()
    {
        Debug.Log("Main menu quit requested");

        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(true);
    }

    public void CloseQuitConfirm()
    {
        if (quitConfirmPanel != null)
            quitConfirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Main menu quit confirmed");

#if UNITY_EDITOR
        Debug.Log("Quit Game");
#else
        Application.Quit();
#endif
    }
}
