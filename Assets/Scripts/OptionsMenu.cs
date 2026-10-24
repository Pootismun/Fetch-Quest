using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    [SerializeField] private GameObject panel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeValueText;
    // No longer accurate, this is the fullscreen image now.
    [SerializeField] private Image shape169Image;
    // No longer accurate, this is the windowed image now.
    [SerializeField] private Image shape43Image;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private int windowedWidth = 1280;
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

        ShowMode(Screen.fullScreen);

#if !UNITY_EDITOR
        if (Screen.fullScreen)
        {
            ApplyMode(true);
        }
#endif

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

    public void SetFullscreen()
    {
        ApplyMode(true);
    }

    public void SetWindowed()
    {
        ApplyMode(false);
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

    private void ShowMode(bool fullscreen)
    {
        shape169Image.color = fullscreen ? selectedColor : unselectedColor;
        shape43Image.color = fullscreen ? unselectedColor : selectedColor;
    }

    private void ApplyMode(bool fullscreen)
    {
        ShowMode(fullscreen);

        int width = windowedWidth;
        int height = windowedHeight;


#if !UNITY_EDITOR
        if (fullscreen)
        {
            DisplayInfo display = Screen.mainWindowDisplayInfo;
            Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }
#endif
    }
}
