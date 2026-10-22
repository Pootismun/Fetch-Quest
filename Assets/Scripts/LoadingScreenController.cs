using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private TileWipe wipe;
    [SerializeField] private TextWave wave;
    [SerializeField] private GameObject loadingLabel;
    [SerializeField] private float minimumShowTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioListener loadingListener;
    [SerializeField] private AudioClip stampedeClip;
    [SerializeField] private AudioClip boingInClip;
    [SerializeField] private AudioClip boingOutClip;
    [SerializeField] private AudioClip musicClip;
    [Range(0f, 1f)][SerializeField] private float stampedeVolume = 0.4f;
    [Range(0f, 1f)][SerializeField] private float boingVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource stampedeSource;

    private void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.clip = musicClip;
        musicSource.volume = 0f;

        stampedeSource = gameObject.AddComponent<AudioSource>();
        stampedeSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive && scene.name == SceneLoader.TargetScene)
        {
            SceneManager.SetActiveScene(scene);

            if (loadingListener != null)
            {
                loadingListener.enabled = false;
            }

        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        TakeOverAudioListener();
        StartCoroutine(RunTransition());
    }

    private void TakeOverAudioListener()
    {
        if (loadingListener == null)
        {
            return;
        }

        foreach (AudioListener other in FindObjectsByType<AudioListener>())
        {
            if (other != loadingListener)
            {
                other.enabled = false;
            }
        }

        loadingListener.enabled = true;
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private void PlayStampede()
    {
        if (stampedeClip == null)
        {
            return;
        }

        stampedeSource.clip = stampedeClip;
        stampedeSource.volume = stampedeVolume;
        stampedeSource.Play();
    }

    private IEnumerator StopStampede()
    {
        float start = stampedeSource.volume;
        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            stampedeSource.volume = Mathf.Lerp(start, 0f, time / duration);
            yield return null;
        }

        stampedeSource.Stop();
    }

    private IEnumerator FadeMusic(float target, float duration)
    {
        float start = musicSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            musicSource.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        musicSource.volume = target;
    }

    private IEnumerator SlideLabel(bool show)
    {
        RectTransform rect = loadingLabel.GetComponent<RectTransform>();
        Vector2 shownPosition = Vector2.zero;
        Vector2 hiddenPosition = new Vector2(0f, -400f);

        loadingLabel.SetActive(true);
        PlaySfx(show ? boingInClip : (boingOutClip != null ? boingOutClip : boingInClip), boingVolume);

        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(time / duration);

            if (show)
            {
                float eased = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
                rect.anchoredPosition = Vector2.LerpUnclamped(hiddenPosition, shownPosition, eased);
            }
            else
            {
                float eased = 2.70158f * t * t * t - 1.70158f * t * t;
                rect.anchoredPosition = Vector2.LerpUnclamped(shownPosition, hiddenPosition, eased);
            }

            yield return null;
        }

        rect.anchoredPosition = show ? shownPosition : hiddenPosition;
        loadingLabel.SetActive(show);
    }

    private IEnumerator RunTransition()
    {
        string target = SceneLoader.TargetScene;
        string previous = SceneLoader.PreviousScene;

        loadingLabel.SetActive(false);

        PlayStampede();

        yield return wipe.Cover();
        yield return StopStampede();

        if (musicClip != null)
        {
            musicSource.Play();
            StartCoroutine(FadeMusic(musicVolume, 0.2f));
        }

        yield return SlideLabel(true);

        if (!string.IsNullOrEmpty(previous) && previous != gameObject.scene.name)
        {
            Scene oldScene = SceneManager.GetSceneByName(previous);

            if (oldScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(oldScene);
            }
        }

        while (wave != null && !wave.IsResting)
        {
            yield return null;
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);

        if (load != null)
        {
            load.allowSceneActivation = false;
            float elapsed = 0f;

            while (load.progress < 0.9f || elapsed < minimumShowTime || (wave != null && !wave.IsResting))
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                yield return null;
            }

            load.allowSceneActivation = true;
            yield return load;

            yield return null;
            yield return null;
        }

        StartCoroutine(FadeMusic(0f, 0.3f));
        yield return SlideLabel(false);

        PlayStampede();
        
        yield return wipe.Reveal();
        yield return StopStampede();

        SceneLoader.Finish();

        if (SceneManager.sceneCount > 1)
        {
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }
    }
}

