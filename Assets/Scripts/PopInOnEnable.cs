using System.Collections;
using UnityEngine;

public class PopInOnEnable : MonoBehaviour
{
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float startScale = 0f;
    [SerializeField] private float wobbleDegrees = 8f;

    private Vector3 baseScale;
    private bool hasBaseScale;

    private void OnEnable()
    {
        if (!hasBaseScale)
        {
            baseScale = transform.localScale;
            hasBaseScale = true;
        }

        StartCoroutine(Pop());
    }

    private void OnDisable()
    {
        if (hasBaseScale)
        {
            transform.localScale = baseScale;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void Show()
    {
        bool wasActive = gameObject.activeSelf;
        StopAllCoroutines();
        gameObject.SetActive(true);

        if (wasActive)
        {
            StartCoroutine(Pop());
        }
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PopOut());
    }

    private IEnumerator PopOut()
    {
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(time / duration);

            transform.localScale = baseScale * (1f - EaseInBack(t));
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * wobbleDegrees * (1f - t));

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private static float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    private IEnumerator Pop()
    {
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(time / duration);

            transform.localScale = baseScale * Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(t));
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * wobbleDegrees * (1f - t));

            yield return null;
        }

        transform.localScale = baseScale;
        transform.localRotation = Quaternion.identity;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
