using UnityEngine;

public class BorderAnimator : MonoBehaviour
{
    [SerializeField] private float speed = 200f;
    [SerializeField] private float inset = 40f;

    private RectTransform[] dots;
    private RectTransform area;

    private void Awake()
    {
        area = GetComponent<RectTransform>();
        dots = new RectTransform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            dots[i] = transform.GetChild(i) as RectTransform;
        }
    }

    private void Update()
    {
        Rect r = area.rect;
        float w = Mathf.Max(1f, r.width - inset * 2f);
        float h = Mathf.Max(1f, r.height - inset * 2f);
        float perimeter = 2f * (w + h);

        float travelled = Time.time * speed;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null)
            {
                continue;
            }

            float offset = perimeter * i / dots.Length;
            float d = Mathf.Repeat(travelled + offset, perimeter);
            dots[i].anchoredPosition = PointOnPerimeter(d, w, h);
        }
    }

    private Vector2 PointOnPerimeter(float d, float w, float h)
    {
        float x0 = -w / 2f;
        float y0 = -h / 2f;

        if (d < w) return new Vector2(x0 + d, y0);
        d -= w;

        if (d < h) return new Vector2(x0 + w, y0 + d);
        d -= h;

        if (d < w) return new Vector2(x0 + w - d, y0 + h);
        d -= w;

        return new Vector2(x0, y0 + h - d);
    }
}
