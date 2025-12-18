using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SpriteDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private int activePointerId = -1;

    private const float BoundaryX = 160f;
    private const float BoundaryY = 160f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        activePointerId = eventData.pointerId;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        Vector2 delta = eventData.delta / canvas.scaleFactor;
        Vector2 newPos = rectTransform.anchoredPosition + delta;

        newPos.x = Mathf.Clamp(newPos.x, -BoundaryX, BoundaryX);
        newPos.y = Mathf.Clamp(newPos.y, -BoundaryY, BoundaryY);

        rectTransform.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId)
        {
            activePointerId = -1;
        }
    }
}
