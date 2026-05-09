using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndMenuUI : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "StartMenu";
    [SerializeField] private string firstSceneName = "S1";

    public void ReturnToMainMenu()
    {
        Debug.Log("Return to main menu");

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("EndMenuUI: mainMenuSceneName is empty");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartGame()
    {
        Debug.Log("Restart game");

        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("EndMenuUI: firstSceneName is empty");
            return;
        }

        SceneManager.LoadScene(firstSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit game from end menu");

#if UNITY_EDITOR
        Debug.Log("Quit Game");
#else
        Application.Quit();
#endif
    }
}
