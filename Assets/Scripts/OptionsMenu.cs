using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";
    private const string ScreenShapeKey = "ScreenShape";

    [SerializeField] private GameObject panel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeValueText;
    [SerializeField] private Image shape169Image;
    [SerializeField] private Image shape43Image;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private int windowedHeight = 720;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform card;
    [SerializeField] private float openTime = 0.35f;
    [SerializeField] private float closeTime = 0.25f;

    private Coroutine routine;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.SetValueWithoutNotify(volume);
        ApplyVolume(volume);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        int shape = PlayerPrefs.GetInt(ScreenShapeKey, 0);
        ShowShape(shape);

        if (PlayerPrefs.HasKey(ScreenShapeKey))
        {
            ApplyShape(shape);
        }

        panel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        panelGroup.alpha = 0f;
        card.localScale = Vector3.one * 0.6f;
        card.anchoredPosition = new Vector2(0f, -80f);
        panel.SetActive(true);
        routine = StartCoroutine(Animate(true));
    }

    public void CloseOptions()
    {
        if (!panel.activeSelf)
        {
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(Animate(false));
    }

    private IEnumerator Animate(bool opening)
    {
        float duration = opening ? openTime : closeTime;
        float clock = 0f;

        panelGroup.interactable = false;

        while (clock < duration)
        {
            clock += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(clock / duration);

            if (opening)
            {
                float eased = EaseOutBack(t);
                panelGroup.alpha = t;
                card.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, eased);
                card.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(-80f, 0f, eased));
            }
            else
            {
                panelGroup.alpha = 1f - t;
                card.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
                card.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, -40f, t));
            }

            yield return null;
        }

        if (opening)
        {
            panelGroup.alpha = 1f;
            card.localScale = Vector3.one;
            card.anchoredPosition = Vector2.zero;
            panelGroup.interactable = true;
        }
        else
        {
            panel.SetActive(false);
        }

        routine = null;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    public void SetShape169()
    {
        SetShape(0);
    }

    public void SetShape43()
    {
        SetShape(1);
    }

    private void SetShape(int shape)
    {
        PlayerPrefs.SetInt(ScreenShapeKey, shape);
        PlayerPrefs.Save();
        ShowShape(shape);
        ApplyShape(shape);
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value);
        ApplyVolume(value);
    }

    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    private void ShowShape(int shape)
    {
        shape169Image.color = shape == 0 ? selectedColor : unselectedColor;
        shape43Image.color = shape == 1 ? selectedColor : unselectedColor;
    }

    private void ApplyShape(int shape)
    {
        int height = windowedHeight;
        int width = shape == 0 ? Mathf.RoundToInt(height * 16f / 9f) : Mathf.RoundToInt(height * 4f / 3f);

#if !UNITY_EDITOR
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
#endif
    }
}
