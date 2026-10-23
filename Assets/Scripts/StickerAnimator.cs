using UnityEngine;

public class StickerAnimator : MonoBehaviour
{
    [Header("Constant sway and breathing")]
    [SerializeField] private float baseAngle = -10f;
    [SerializeField] private float swingAngle = 6f;
    [SerializeField] private float swingSpeed = 2f;
    [SerializeField] private float breatheAmount = 0.05f;
    [SerializeField] private float breatheSpeed = 3f;

    [Header("Attention pop")]
    [SerializeField] private float popInterval = 4f;
    [SerializeField] private float popDuration = 0.6f;
    [SerializeField] private float popScale = 0.25f;
    [SerializeField] private float popWiggle = 20f;

    private RectTransform rect;
    private Vector3 baseScale;
    private float timer;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
    }

    private void Update()
    {
        float t = Time.unscaledTime;
        timer += Time.unscaledDeltaTime;

        float angle = baseAngle + Mathf.Sin(t * swingSpeed) * swingAngle;
        float scale = 1f + Mathf.Sin(t * breatheSpeed) * breatheAmount;

        if (timer >= popInterval)
        {
            float p = (timer - popInterval) / popDuration;

            if (p >= 1f)
            {
                timer = 0f;
            }
            else
            {
                float swell = Mathf.Sin(p * Mathf.PI);
                scale += swell * popScale;
                angle += Mathf.Sin(p * Mathf.PI * 4f) * popWiggle * (1f - p);
            }
        }

        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        rect.localScale = baseScale * scale;
    }
}
