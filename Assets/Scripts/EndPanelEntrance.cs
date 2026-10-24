using System.Collections;
using UnityEngine;

public class EndPanelEntrance : MonoBehaviour
{
    [Header("Victory: bouncing drop")]
    [SerializeField] private float victoryDropDistance = 1500f;
    [SerializeField] private float victoryDuration = 1.1f;
    [SerializeField] private float victoryTiltDegrees = 8f;

    [Header("Defeat: slow heavy slide")]
    [SerializeField] private float defeatDropDistance = 300f;
    [SerializeField] private float defeatDuration = 1.2f;

    private RectTransform rect;
    private Vector2 basePosition;
    private bool victory = true;
    private bool hasBase;

    public void SetVictory(bool isVictory)
    {
        victory = isVictory;
    }

    private void OnEnable()
    {
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        if (!hasBase)
        {
            basePosition = rect.anchoredPosition;
            hasBase = true;
        }

        StopAllCoroutines();
        StartCoroutine(victory ? VictoryRoutine() : DefeatRoutine());
    }

    private void OnDisable()
    {
        if (rect != null && hasBase)
        {
            rect.anchoredPosition = basePosition;
            rect.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator VictoryRoutine()
    {
        float t = 0f;
        rect.anchoredPosition = basePosition + Vector2.up * victoryDropDistance;

        while (t < victoryDuration)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / victoryDuration);
            float e = EaseOutBounce(x);

            rect.anchoredPosition = basePosition + Vector2.up * Mathf.LerpUnclamped(victoryDropDistance, 0f, e);

            float tilt = victoryTiltDegrees * (1f - x) * Mathf.Sin(x * Mathf.PI * 5f);
            rect.localRotation = Quaternion.Euler(0f, 0f, tilt);

            yield return null;
        }

        rect.anchoredPosition = basePosition;
        rect.localRotation = Quaternion.identity;
    }

    private IEnumerator DefeatRoutine()
    {
        float t = 0f;
        rect.anchoredPosition = basePosition + Vector2.up * defeatDropDistance;

        while (t < defeatDuration)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / defeatDuration);
            float e = 1f - Mathf.Pow(1f - x, 3f);

            rect.anchoredPosition = basePosition + Vector2.up * Mathf.Lerp(defeatDropDistance, 0f, e);

            yield return null;
        }

        rect.anchoredPosition = basePosition;
    }

    private static float EaseOutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (x < 1f / d1)
        {
            return n1 * x * x;
        }

        if (x < 2f / d1)
        {
            x -= 1.5f / d1;
            return n1 * x * x + 0.75f;
        }

        if (x < 2.5f / d1)
        {
            x -= 2.25f / d1;
            return n1 * x * x + 0.9375f;
        }

        x -= 2.625f / d1;
        return n1 * x * x + 0.984375f;
    }
}
