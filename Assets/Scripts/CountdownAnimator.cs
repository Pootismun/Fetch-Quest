using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class CountdownAnimator : MonoBehaviour
{ 
    [Header("References")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image backdrop;

    [Header("Look")]
    [SerializeField] private Color[] stepColors =
    {
        new Color(0.90f, 0.22f, 0.27f),
        new Color(0.96f, 0.64f, 0.38f),
        new Color(1.00f, 0.84f, 0.04f),
        new Color(0.32f, 0.85f, 0.42f)
    };
    [SerializeField] private float popStartScale = 2.2f;
    [SerializeField] private float goStartScale = 3f;
    [SerializeField] private float popInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.2f;

    [Header("Backdrop")]
    [Range(0f, 1f)][SerializeField] private float backdropCenterAlpha = 0.35f;
    [Range(0f, 1f)][SerializeField] private float backdropEdgeAlpha = 0.85f;

    [Header("Screen pulse")]
    [SerializeField] private float pulseAmount = 0.02f;
    [SerializeField] private float goPulseAmount = 0.04f;
    [SerializeField] private float pulseTime = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip beepClip;
    [SerializeField] private AudioClip goClip;
    [Range(0f, 1f)][SerializeField] private float beepVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float goVolume = 1f;
    [SerializeField] private float[] beepPitches = { 0.9f, 1f, 1.15f };

    private AudioSource audioSource;
    private Image flash;
    private TextWave goWave;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        countdownText.outlineColor = new Color32(40, 24, 16, 255);
        countdownText.outlineWidth = 0.2f;

        if (backdrop != null)
        {
            backdrop.sprite = MakeVignette();
            backdrop.type = Image.Type.Simple;
            backdrop.color = Color.black;
        }

        flash = CreateFlash();

        goWave = countdownText.GetComponent<TextWave>();

        if (goWave != null)
        {
            goWave.enabled = false;
        }
    }

    public IEnumerator ShowStep(int index, int totalSteps, string label, float duration)
    {
        bool isGo = index == totalSteps - 1;
        Color color = stepColors.Length > 0 ? stepColors[Mathf.Min(index, stepColors.Length - 1)] : Color.white;
        float startScale = isGo ? goStartScale : popStartScale;
        RectTransform rect = countdownText.rectTransform;

        countdownText.text = label;
        countdownText.color = color;
        rect.localScale = Vector3.one * startScale;

        PlaySound(index, isGo);
        StartCoroutine(PulseCamera(isGo ? goPulseAmount : pulseAmount));

        if (isGo)
        {
            StartCoroutine(Flash());

            if (goWave != null)
            {
                goWave.enabled = true;
            }
        }

        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);

            float scale = 1f;
            float alpha = 1f;

            if (time < popInTime)
            {
                scale = Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(time / popInTime));
            }

            float fadeStart = duration - fadeOutTime;

            if (time > fadeStart)
            {
                float t = Mathf.Clamp01((time - fadeStart) / fadeOutTime);
                scale = Mathf.Lerp(1f, 0.6f, t);
                alpha = 1f - t;
            }

            rect.localScale = Vector3.one * scale;

            Color current = color;
            current.a = alpha;
            countdownText.color = current;

            yield return null;
        }

        rect.localScale = Vector3.one;

        if (goWave != null)
        {
            goWave.enabled = false;
        }
    }

    [ContextMenu("Preview Countdown")]
    private void Preview()
    {
        StartCoroutine(PreviewRoutine());
    }

    private IEnumerator PreviewRoutine()
    {
        string[] labels = { "3", "2", "1", "EAT!" };

        countdownText.gameObject.SetActive(true);

        if (backdrop != null)
        {
            backdrop.gameObject.SetActive(true);
        }

        for (int i = 0; i < labels.Length; i++)
        {
            yield return ShowStep(i, labels.Length, labels[i], 1f);
        }

        countdownText.gameObject.SetActive(false);

        if (backdrop != null)
        {
            backdrop.gameObject.SetActive(false);
        }
    }

    private void PlaySound(int index, bool isGo)
    {
        if (isGo)
        {
            if (goClip != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(goClip, goVolume);
            }

            return;
        }

        if (beepClip != null && beepPitches.Length > 0)
        {
            audioSource.pitch = beepPitches[Mathf.Min(index, beepPitches.Length - 1)];
            audioSource.PlayOneShot(beepClip, beepVolume);
        }
    }

    private IEnumerator PulseCamera(float amount)
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
        {
            yield break;
        }

        float baseSize = cam.orthographicSize;
        float time = 0f;

        while (time < pulseTime)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(time / pulseTime);
            cam.orthographicSize = baseSize * (1f - amount * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        cam.orthographicSize = baseSize;
    }

    private IEnumerator Flash()
    {
        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            flash.color = new Color(1f, 1f, 1f, 0.6f * (1f - time / duration));
            yield return null;
        }

        flash.color = new Color(1f, 1f, 1f, 0f);
    }

    private Image CreateFlash()
    {
        GameObject flashObject = new GameObject("CountdownFlash", typeof(RectTransform), typeof(Image));
        flashObject.transform.SetParent(countdownText.transform.parent, false);
        flashObject.transform.SetSiblingIndex(countdownText.transform.GetSiblingIndex());

        RectTransform rect = flashObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = flashObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;
        return image;
    }

    private Sprite MakeVignette()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 centre = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float maxDistance = size / 2f * 1.2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), centre) / maxDistance;
                float alpha = Mathf.Lerp(backdropCenterAlpha, backdropEdgeAlpha, Mathf.SmoothStep(0f, 1f, distance));
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}

