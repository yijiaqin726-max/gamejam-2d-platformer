using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float delayBeforeLoad = 0.1f;
    [SerializeField] private bool useFade = true;
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
        StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator TransitionToNextScene()
    {
        if (useFade)
        {
            ScreenFadeController fadeController = FindObjectOfType<ScreenFadeController>();
            if (fadeController != null)
            {
                yield return StartCoroutine(fadeController.FadeOut(fadeOutDuration));
            }
            else
            {
                yield return new WaitForSeconds(delayBeforeLoad);
            }
        }
        else
        {
            yield return new WaitForSeconds(delayBeforeLoad);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
