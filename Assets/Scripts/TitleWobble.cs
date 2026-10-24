using UnityEngine;

public class TitleWobble : MonoBehaviour
{
    [SerializeField] private float angle = 2.5f;
    [SerializeField] private float speed = 1.6f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        float swing = Mathf.Sin(Time.unscaledTime * speed) * angle;
        rect.localRotation = Quaternion.Euler(0f, 0f, swing);
    }
}
