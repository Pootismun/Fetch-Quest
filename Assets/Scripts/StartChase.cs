using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartChase : MonoBehaviour
{
    [Serializable]
    public class SquirrelActor
    {
        public Image image;
        public Sprite[] runLeft;
    }

    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image dog;
    [SerializeField] private Sprite[] dogRunLeft;
    [SerializeField] private Sprite[] dogRunRight;
    [SerializeField] private SquirrelActor[] squirrels;
    [SerializeField] private Sprite[] recoveringFrames;

    [SerializeField] private float speed = 520f;
    [SerializeField] private float gapToDog = 260f;
    [SerializeField] private float squirrelSpacing = 150f;
    [SerializeField] private float frameTime = 0.12f;
    [SerializeField] private float bobHeight = 8f;
    [SerializeField] private float pauseBetween = 3f;
    [SerializeField] private float edgeMargin = 150f;

    private float dogBaseY;
    private float[] squirrelBaseY;

    private void Start()
    {
        dogBaseY = dog.rectTransform.anchoredPosition.y;
        squirrelBaseY = new float[squirrels.Length];

        for (int i = 0; i < squirrels.Length; i++)
        {
            squirrelBaseY[i] = squirrels[i].image.rectTransform.anchoredPosition.y;
        }

        SetActors(false);
        StartCoroutine(ChaseLoop());
    }

    private IEnumerator ChaseLoop()
    {
        while (true)
        {
            yield return RunChase(true);
            yield return new WaitForSecondsRealtime(pauseBetween);
            yield return RunChase(false);
            yield return new WaitForSecondsRealtime(pauseBetween);
        }
    }

    private IEnumerator RunChase(bool squirrelsChase)
    {
        float half = canvasRect.rect.width * 0.5f + edgeMargin;
        float lastOffset = gapToDog + squirrelSpacing * Mathf.Max(0, squirrels.Length - 1);

        float start;
        float end;
        float direction;

        if (squirrelsChase)
        {
            start = half;
            end = -half - lastOffset;
            direction = -1f;
        }
        else
        {
            start = -half - lastOffset;
            end = half;
            direction = 1f;
        }

        float lead = start;
        float clock = 0f;

        SetActors(true);

        while ((direction < 0f && lead > end) || (direction > 0f && lead < end))
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            lead += direction * speed * dt;
            clock += dt;

            int frame = (int)(clock / frameTime);
            Place(lead, frame, squirrelsChase);

            yield return null;
        }

        SetActors(false);
    }

    private void Place(float lead, int frame, bool squirrelsChase)
    {
        Sprite[] dogFrames = squirrelsChase ? dogRunLeft : dogRunRight;
        float bob = (frame % 2) * bobHeight;

        dog.sprite = dogFrames[frame % dogFrames.Length];
        SetX(dog.rectTransform, lead, dogBaseY + bob);

        for (int i = 0; i < squirrels.Length; i++)
        {
            Sprite[] frames = squirrelsChase ? squirrels[i].runLeft : recoveringFrames;
            squirrels[i].image.sprite = frames[(frame + i) % frames.Length];

            float x = lead + gapToDog + squirrelSpacing * i;
            float squirrelBob = ((frame + i) % 2) * bobHeight;
            SetX(squirrels[i].image.rectTransform, x, squirrelBaseY[i] + squirrelBob);
        }
    }

    private void SetX(RectTransform rect, float x, float y)
    {
        rect.anchoredPosition = new Vector2(x, y);
    }

    private void SetActors(bool active)
    {
        dog.gameObject.SetActive(active);

        for (int i = 0; i < squirrels.Length; i++)
        {
            squirrels[i].image.gameObject.SetActive(active);
        }
    }
}
