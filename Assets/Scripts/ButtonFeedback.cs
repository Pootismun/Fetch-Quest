using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]

public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{

    [Header("Scale reaction")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float scaleSpeed = 14f;

    [Header("Label")]
    [SerializeField] private float pressedLabelOffset = -8f;

    private Button button;
    private RectTransform label;
    private Vector2 labelBasePosition;
    private Vector3 baseScale;
    private float targetFactor = 1f;
    private float currentFactor = 1f;
    private bool hovering;
    private bool held;

    private void Awake()
    {
        button = GetComponent<Button>();
        baseScale = transform.localScale;

        TMP_Text text = GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            label = text.rectTransform;
            labelBasePosition = label.anchoredPosition;
        }
    }

    private void OnDisable()
    {
        hovering = false;
        held = false;
        currentFactor = 1f;
        targetFactor = 1f;
        transform.localScale = baseScale;

        if (label != null)
        {
            label.anchoredPosition = labelBasePosition;
        }
    }

    private void Update()
    {
        currentFactor = Mathf.Lerp(currentFactor, targetFactor, 1f - Mathf.Exp(-scaleSpeed * Time.unscaledDeltaTime));
        transform.localScale = baseScale * currentFactor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
        {
            return;
        }

        hovering = true;
        RefreshState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        RefreshState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable)
        {
            return;
        }

        held = true;
        RefreshState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        held = false;
        RefreshState();
    }

    private void RefreshState()
    {
        if (held)
        {
            targetFactor = pressedScale;
        }
        else if (hovering)
        {
            targetFactor = hoverScale;
        }
        else
        {
            targetFactor = 1f;
        }

        if (label != null)
        {
            label.anchoredPosition = labelBasePosition + new Vector2(0f, held ? pressedLabelOffset : 0f);
        }
    }
}
