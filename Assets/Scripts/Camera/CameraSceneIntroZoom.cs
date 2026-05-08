using UnityEngine;
using System.Collections;

public sealed class CameraSceneIntroZoom : MonoBehaviour
{
    [SerializeField] private float zoomMultiplier = 0.85f;
    [SerializeField] private float zoomDuration = 0.8f;
    [SerializeField] private float startDelay = 0.05f;

    private Camera mainCamera;
    private float originalSize;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("CameraSceneIntroZoom: Camera component not found.");
            return;
        }

        if (!mainCamera.orthographic)
        {
            Debug.LogWarning("CameraSceneIntroZoom: Camera is not orthographic. Zoom effect will not be applied.");
            return;
        }

        originalSize = mainCamera.orthographicSize;
        StartCoroutine(PlayZoomIntro());
    }

    private IEnumerator PlayZoomIntro()
    {
        yield return new WaitForSeconds(startDelay);

        float zoomedSize = originalSize * zoomMultiplier;
        mainCamera.orthographicSize = zoomedSize;

        float elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / zoomDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.orthographicSize = Mathf.Lerp(zoomedSize, originalSize, smoothT);
            yield return null;
        }

        mainCamera.orthographicSize = originalSize;
    }
}
