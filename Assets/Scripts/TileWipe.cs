using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TileWipe : MonoBehaviour
{
    [System.Serializable]
    public class SquirrelSprites
    {
        public Sprite[] right;
        public Sprite[] left;
    }

    [SerializeField] private LoadingTileGrid grid;
    [SerializeField] private RectTransform squirrelLayer;
    [SerializeField] private SquirrelSprites[] squirrels;
    [SerializeField] private float crossingTime = 0.7f;
    [SerializeField] private float speedVariation = 0.25f;
    [SerializeField] private float maxStartDelay = 0.15f;
    [SerializeField] private float squirrelSize = 128f;
    [SerializeField] private float frameTime = 0.07f;

    public IEnumerator Cover()
    {
        grid.SetAllVisible(false);
        yield return Run(true);
    }

    public IEnumerator Reveal()
    {
        grid.SetAllVisible(true);
        yield return Run(false);
    }

    [ContextMenu("Test Cover")]
    private void TestCover()
    {
        StartCoroutine(Cover());
    }

    [ContextMenu("Test Reveal")]
    private void TestReveal()
    {
        StartCoroutine(Reveal());
    }

    private IEnumerator Run(bool covering)
    {
        int rows = grid.Rows;
        int columns = grid.Columns;

        Image[] images = new Image[rows];
        float[] duration = new float[rows];
        float[] delay = new float[rows];
        int[] nextColumn = new int[rows];

        for (int r = 0; r < rows; r++)
        {
            GameObject squirrel = new GameObject("Squirrel_" + r, typeof(RectTransform), typeof(Image));
            squirrel.transform.SetParent(squirrelLayer, false);

            RectTransform rect = squirrel.GetComponent<RectTransform>();
            float y = 1f - (r + 0.5f) / rows;
            rect.anchorMin = new Vector2(0f, y);
            rect.anchorMax = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(squirrelSize, squirrelSize);

            Image image = squirrel.GetComponent<Image>();
            image.raycastTarget = false;
            images[r] = image;

            duration[r] = crossingTime * Random.Range(1f - speedVariation, 1f + speedVariation);
            delay[r] = Random.Range(0f, maxStartDelay);
            nextColumn[r] = covering ? 0 : columns - 1;
        }

        float clock = 0f;
        bool running = true;

        while (running)
        {
            // Clamped so a long frame (like a scene load) never makes the squirrels jump ahead.
            clock += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            running = false;

            for (int r = 0; r < rows; r++)
            {
                if (images[r] == null)
                {
                    continue;
                }

                float progress = Mathf.Clamp01((clock - delay[r]) / duration[r]);
                float x = covering ? Mathf.Lerp(-0.05f, 1.05f, progress) : Mathf.Lerp(1.05f, -0.05f, progress);

                RectTransform rect = images[r].rectTransform;
                rect.anchorMin = new Vector2(x, rect.anchorMin.y);
                rect.anchorMax = new Vector2(x, rect.anchorMax.y);
                rect.anchoredPosition = Vector2.zero;

                SquirrelSprites set = squirrels.Length > 0 ? squirrels[r % squirrels.Length] : null;
                Sprite[] frames = set == null ? null : (covering ? set.right : set.left);

                if (frames != null && frames.Length > 0)
                {
                    images[r].sprite = frames[((int)(clock / frameTime) + r) % frames.Length];
                }

                if (covering)
                {
                    while (nextColumn[r] < columns && x >= (nextColumn[r] + 0.5f) / columns)
                    {
                        grid.SetTileVisible(r, nextColumn[r], true);
                        nextColumn[r]++;
                    }
                }
                else
                {
                    while (nextColumn[r] >= 0 && x <= (nextColumn[r] + 0.5f) / columns)
                    {
                        grid.SetTileVisible(r, nextColumn[r], false);
                        nextColumn[r]--;
                    }
                }

                if (progress >= 1f)
                {
                    Destroy(images[r].gameObject);
                    images[r] = null;
                }
                else
                {
                    running = true;
                }
            }

            yield return null;
        }

        grid.SetAllVisible(covering);
    }
}
