using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFeedback : MonoBehaviour
{
    public static DamageFeedback Instance { get; private set; }

    [Header("Red flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashStartAlpha = 0.45f;
    [SerializeField] private float flashTime = 0.45f;

    [Header("Camera shake")]
    [SerializeField] private Camera shakeCamera;
    [SerializeField] private float cameraShakeAmount = 0.25f;

    [Header("Lives icon shake")]
    [SerializeField] private RectTransform[] livesIcons;
    [SerializeField] private float iconShakeAmount = 14f;

    [Header("Timing")]
    [SerializeField] private float shakeTime = 0.45f;

    private Coroutine running;
    private Vector3 cameraBase;
    private Vector2[] iconBases;

    private void Awake()
    {
        Instance = this;

        if (shakeCamera == null)
        {
            shakeCamera = Camera.main;
        }

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void Play()
    {
        if (running != null)
        {
            StopCoroutine(running);
            Restore();
        }

        running = StartCoroutine(PlayRoutine());
    }

    [ContextMenu("Preview")]
    private void Preview()
    {
        Play();
    }

    private IEnumerator PlayRoutine()
    {
        if (shakeCamera != null)
        {
            cameraBase = shakeCamera.transform.position;
        }

        iconBases = new Vector2[livesIcons.Length];

        for (int i = 0; i < livesIcons.Length; i++)
        {
            if (livesIcons[i] != null)
            {
                iconBases[i] = livesIcons[i].anchoredPosition;
            }
        }

        float t = 0f;
        float longest = Mathf.Max(flashTime, shakeTime);

        while (t < longest)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);

            float shakeFade = 1f - Mathf.Clamp01(t / shakeTime);
            float flashFade = 1f - Mathf.Clamp01(t / flashTime);

            if (shakeCamera != null)
            {
                Vector2 offset = Random.insideUnitCircle * cameraShakeAmount * shakeFade;
                shakeCamera.transform.position = cameraBase + new Vector3(offset.x, offset.y, 0f);
            }

            for (int i = 0; i < livesIcons.Length; i++)
            {
                if (livesIcons[i] != null)
                {
                    livesIcons[i].anchoredPosition = iconBases[i] + Random.insideUnitCircle * iconShakeAmount * shakeFade;
                }
            }

            if (flashImage != null)
            {
                Color c = flashImage.color;
                c.a = flashStartAlpha * flashFade * flashFade;
                flashImage.color = c;
            }

            yield return null;
        }

        Restore();
        running = null;
    }

    private void Restore()
    {
        if (shakeCamera != null)
        {
            shakeCamera.transform.position = cameraBase;
        }

        if (iconBases != null)
        {
            for (int i = 0; i < livesIcons.Length && i < iconBases.Length; i++)
            {
                if (livesIcons[i] != null)
                {
                    livesIcons[i].anchoredPosition = iconBases[i];
                }
            }
        }

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }
}
