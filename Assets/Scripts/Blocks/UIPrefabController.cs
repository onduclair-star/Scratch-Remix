using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIPrefabController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private RectTransform ghostRect;
    private GameObject ghostObject;

    private UIPrefabController currentSnapTarget;

    public const float snapOffset = 30f;
    private const float xThreshold = 25f;
    private const float snapThreshold = 45f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIPrefabController could not find a parent Canvas!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
        CreateGhost();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            cam,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;

        DetectSnapTarget();
        PreviewGhost();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();

        if (currentSnapTarget != null && canvas != null)
        {
            ParentChainManager.AssignParent(this, currentSnapTarget, snapOffset);
        }

        currentSnapTarget = null;
    }

    private void CreateGhost()
    {
        ghostObject = Instantiate(gameObject, rectTransform.parent);
        ghostObject.name = gameObject.name + " (Ghost)";
        DestroyImmediate(ghostObject.GetComponent<UIPrefabController>());

        ghostRect = ghostObject.GetComponent<RectTransform>();
        ghostRect.anchoredPosition = rectTransform.anchoredPosition;

        var cg = ghostObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.6f;
        cg.blocksRaycasts = false;

        var interactables = ghostObject.GetComponentsInChildren<UnityEngine.UI.Selectable>();
        foreach (var selectable in interactables)
        {
            selectable.interactable = false;
        }

        ghostObject.SetActive(false);
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
            ghostRect = null;
        }
    }

    private void PreviewGhost()
    {
        if (ghostRect == null) return;

        if (currentSnapTarget != null)
        {
            RectTransform targetRect = currentSnapTarget.GetComponent<RectTransform>();

            if (ghostRect.parent != rectTransform.parent)
            {
                ghostRect.SetParent(rectTransform.parent, false);
            }

            float idealSnapY = targetRect.anchoredPosition.y - snapOffset;

            ghostRect.anchoredPosition = new Vector2(
                targetRect.anchoredPosition.x,
                idealSnapY
            );
            ghostObject.SetActive(true);
        }
        else
        {
            ghostObject.SetActive(false);
        }
    }

    private void DetectSnapTarget()
    {
        currentSnapTarget = null;
        var allBlocks = FindObjectsByType<UIPrefabController>(FindObjectsSortMode.None);
        float bestSnapDistance = snapThreshold;

        Vector3[] draggingCorners = new Vector3[4];
        rectTransform.GetWorldCorners(draggingCorners);
        Vector3 draggingBottomWorldPosition = (draggingCorners[0] + draggingCorners[3]) / 2f;


        foreach (var block in allBlocks)
        {
            if (block == this || block.transform.IsChildOf(rectTransform)) continue;

            RectTransform targetRect = block.rectTransform;

            Vector3[] targetCorners = new Vector3[4];
            targetRect.GetWorldCorners(targetCorners);
            Vector3 targetBottomWorldPosition = (targetCorners[0] + targetCorners[3]) / 2f;

            float idealSnapWorldY = targetBottomWorldPosition.y - snapOffset * targetRect.lossyScale.y;

            float dx = Mathf.Abs(targetRect.position.x - rectTransform.position.x);
            if (dx > xThreshold) continue;

            float dyDistance = Mathf.Abs(draggingBottomWorldPosition.y - idealSnapWorldY);

            if (dyDistance < bestSnapDistance)
            {
                if (targetRect.position.y > rectTransform.position.y)
                {
                    bestSnapDistance = dyDistance;
                    currentSnapTarget = block;
                }
            }
        }
    }
}
