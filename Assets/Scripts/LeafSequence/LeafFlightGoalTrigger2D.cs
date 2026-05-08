using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LeafFlightGoalTrigger2D : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayBeforeLoad = 0.2f;
    [SerializeField] private CrowLaneSpawner crowLaneSpawner;
    [SerializeField] private LeafFlightController leafFlightController;
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        LeafFlightController leafFlight = other.GetComponentInParent<LeafFlightController>();
        if (leafFlight == null)
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("LeafFlightGoalTrigger2D: nextSceneName is empty");
            return;
        }

        hasTriggered = true;
        Debug.Log("Leaf flight goal triggered by: " + other.name);
        StartCoroutine(CompleteLeafSequence(leafFlight));
    }

    private System.Collections.IEnumerator CompleteLeafSequence(LeafFlightController triggeringLeafFlight)
    {
        if (crowLaneSpawner != null)
            crowLaneSpawner.StopSpawning();

        if (leafFlightController != null)
            leafFlightController.EndFlight();
        else if (triggeringLeafFlight != null)
            triggeringLeafFlight.EndFlight();

        if (useFade)
        {
            ScreenFadeController fade = Object.FindAnyObjectByType<ScreenFadeController>();
            if (fade != null)
                yield return fade.FadeOut(fadeOutDuration);
        }

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(nextSceneName);
    }
}
