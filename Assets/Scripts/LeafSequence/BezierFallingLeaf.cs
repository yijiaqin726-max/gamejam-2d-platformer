using UnityEngine;
using System.Collections;

public sealed class BezierFallingLeaf : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform controlPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float duration = 2.8f;
    [SerializeField] private bool useGentleEase = true;
    [SerializeField] private float swayAmplitude = 0.15f;
    [SerializeField] private float swayFrequency = 2.0f;

    public IEnumerator PlayFall()
    {
        if (startPoint == null || controlPoint == null || endPoint == null)
        {
            Debug.LogError("BezierFallingLeaf: Missing start/control/end points");
            yield break;
        }

        Vector3 start = startPoint.position;
        Vector3 control = controlPoint.position;
        Vector3 end = endPoint.position;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float pathT = useGentleEase ? Mathf.Lerp(t, Mathf.SmoothStep(0f, 1f, t), 0.45f) : t;

            Vector3 pos = BezierQuadratic(start, control, end, pathT);

            float sway = Mathf.Sin(elapsedTime * swayFrequency * Mathf.PI) * swayAmplitude;
            pos.x += sway;

            transform.position = pos;

            if (elapsedTime > 0.1f)
            {
                float rotZ = sway * 10f;
                transform.rotation = Quaternion.Euler(0, 0, rotZ);
            }

            yield return null;
        }

        transform.position = end;
        transform.rotation = Quaternion.identity;
    }

    private Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float mt = 1f - t;
        return mt * mt * p0 + 2f * mt * t * p1 + t * t * p2;
    }
}
