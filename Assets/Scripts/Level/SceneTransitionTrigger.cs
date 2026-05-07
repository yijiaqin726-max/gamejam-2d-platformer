using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayBeforeLoad = 0.3f;
    private bool hasTriggered;

    private void Awake()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        hasTriggered = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("SceneTransitionTrigger: nextSceneName is empty. Please set the scene name in Inspector.");
            return;
        }

        hasTriggered = true;
        Invoke(nameof(LoadNextScene), delayBeforeLoad);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
