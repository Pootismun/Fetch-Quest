using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FireworksUI : MonoBehaviour
{
    [SerializeField] private float startDelay = 0.3f;
    [SerializeField] private float burstInterval = 0.45f;
    [SerializeField] private float rocketTime = 0.55f;
    [SerializeField] private int sparksPerBurst = 28;
    [SerializeField] private float sparkSpeed = 520f;
    [SerializeField] private float sparkLife = 1.3f;
    [SerializeField] private float gravity = 420f;
    [SerializeField] private float sparkSize = 14f;
    [SerializeField] private Color[] colors;

    [Header("Sound")]
    [SerializeField] private AudioClip popClip;
    [SerializeField] private float popVolume = 0.8f;

    private AudioSource[] popSources;
    private int nextPopSource;
    private float soundScale = 1f;

    private RectTransform area;
    private bool running;

    private void Awake()
    {
        area = (RectTransform)transform;

        popSources = new AudioSource[4];

        for (int i = 0; i < popSources.Length; i++)
        {
            popSources[i] = gameObject.AddComponent<AudioSource>();
            popSources[i].playOnAwake = false;
            popSources[i].loop = false;
        }

        if (colors == null || colors.Length == 0)
        {
            colors = new Color[]
            {
                new Color32(255, 200, 60, 255),
                new Color32(245, 130, 60, 255),
                new Color32(255, 110, 160, 255),
                new Color32(110, 220, 255, 255),
                new Color32(140, 255, 140, 255),
                new Color32(255, 255, 255, 255)
            };
        }
    }

    public void Play()
    {
        running = true;
        StartCoroutine(Loop());
    }

    public void Stop()
    {
        running = false;
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        while (running)
        {
            StartCoroutine(Rocket(PickTarget()));
            yield return new WaitForSecondsRealtime(burstInterval * Random.Range(0.7f, 1.3f));
        }
    }

    private Vector2 PickTarget()
    {
        float width = area.rect.width;
        float height = area.rect.height;

        if (Random.value < 0.25f)
        {
            return new Vector2(Random.Range(-0.25f, 0.25f) * width, Random.Range(0.30f, 0.45f) * height);
        }

        float side = Random.value < 0.5f ? -1f : 1f;
        return new Vector2(side * Random.Range(0.30f, 0.46f) * width, Random.Range(-0.25f, 0.42f) * height);
    }

    private Image MakeSquare(Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject("Spark", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(area, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private IEnumerator Rocket(Vector2 target)
    {
        Vector2 start = new Vector2(target.x, -area.rect.height * 0.5f - 30f);
        Image rocket = MakeSquare(start, new Vector2(10f, 28f), new Color32(255, 240, 180, 255));

        float t = 0f;

        while (t < rocketTime)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float x = Mathf.Clamp01(t / rocketTime);
            float e = 1f - (1f - x) * (1f - x);
            rocket.rectTransform.anchoredPosition = Vector2.Lerp(start, target, e);
            yield return null;
        }

        Destroy(rocket.gameObject);

        Color first = colors[Random.Range(0, colors.Length)];
        Color second = colors[Random.Range(0, colors.Length)];
        PlayPop();
        StartCoroutine(Burst(target, first, second));
    }

    public void SetSoundScale(float scale)
    {
        soundScale = scale;

        if (popSources == null)
        {
            return;
        }

        for (int i = 0; i < popSources.Length; i++)
        {
            popSources[i].volume = popVolume * soundScale;
        }
    }

    private void PlayPop()
    {
        if (popClip == null)
        {
            return;
        }

        AudioSource source = popSources[nextPopSource];
        nextPopSource = (nextPopSource + 1) % popSources.Length;

        source.pitch = Random.Range(0.85f, 1.2f);
        source.volume = popVolume * soundScale;
        source.clip = popClip;
        source.Play();
    }

    private IEnumerator Burst(Vector2 centre, Color first, Color second)
    {
        int count = sparksPerBurst;
        Image[] sparks = new Image[count];
        Vector2[] velocity = new Vector2[count];
        Color[] baseColor = new Color[count];

        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.1f, 0.1f);
            float speed = sparkSpeed * Random.Range(0.55f, 1f);
            velocity[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            baseColor[i] = i % 2 == 0 ? first : second;
            sparks[i] = MakeSquare(centre, new Vector2(sparkSize, sparkSize), baseColor[i]);
        }

        float t = 0f;

        while (t < sparkLife)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            t += dt;
            float x = Mathf.Clamp01(t / sparkLife);

            for (int i = 0; i < count; i++)
            {
                if (sparks[i] == null)
                {
                    continue;
                }

                velocity[i].y -= gravity * dt;
                velocity[i] *= 1f - 1.2f * dt;

                RectTransform rect = sparks[i].rectTransform;
                rect.anchoredPosition += velocity[i] * dt;
                rect.sizeDelta = Vector2.one * sparkSize * (1f - 0.6f * x);

                Color c = baseColor[i];
                c.a = 1f - x * x;
                sparks[i].color = c;
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            if (sparks[i] != null)
            {
                Destroy(sparks[i].gameObject);
            }
        }
    }
}
