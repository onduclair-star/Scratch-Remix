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

    private const float snapOffset = 30f;
    private const float xThreshold = 25f;
    private const float snapThreshold = 45f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CreateGhost();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 本体跟随鼠标（自由拖）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;

        // 逻辑：根据距离寻找吸附目标
        DetectSnapTarget();
        PreviewGhost();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();

        if (currentSnapTarget != null)
        {
            // 贴上真实位置
            SnapToTarget(currentSnapTarget);

            // 走你自己的父逻辑
            ParentChainManager.AssignParent(this, currentSnapTarget, snapOffset);
        }

        currentSnapTarget = null;
    }

    // ---------------------------
    // Ghost 逻辑
    // ---------------------------
    private void CreateGhost()
    {
        ghostObject = Instantiate(gameObject, rectTransform.parent);
        ghostObject.name = gameObject.name + " (Ghost)";
        DestroyImmediate(ghostObject.GetComponent<UIPrefabController>());

        ghostRect = ghostObject.GetComponent<RectTransform>();

        var cg = ghostObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.6f;
        cg.blocksRaycasts = false;
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
            ghostRect.position = new Vector3(
                targetRect.position.x,
                targetRect.position.y - snapOffset,
                targetRect.position.z
            );
            ghostObject.SetActive(true);
        }
        else ghostObject.SetActive(false);
    }

    // ---------------------------
    // 距离吸附逻辑（核心修复点）
    // ---------------------------
    private void DetectSnapTarget()
    {
        currentSnapTarget = null;

        var allBlocks = FindObjectsByType<UIPrefabController>(FindObjectsSortMode.None);
        float bestYDistance = snapThreshold;

        foreach (var block in allBlocks)
        {
            if (block == this) continue;

            float dx = Mathf.Abs(block.rectTransform.position.x - rectTransform.position.x);
            float dy = rectTransform.position.y - block.rectTransform.position.y;

            // 只吸附“其下方的积木”，并且 X 方向基本对齐，且距离足够近
            if (dx < xThreshold && dy > 0 && dy < bestYDistance)
            {
                bestYDistance = dy;
                currentSnapTarget = block;
            }
        }
    }

    // ---------------------------
    // 真实 Snap（松手后执行）
    // ---------------------------
    private void SnapToTarget(UIPrefabController target)
    {
        RectTransform targetRect = target.GetComponent<RectTransform>();
        rectTransform.position = new Vector3(
            targetRect.position.x,
            targetRect.position.y - snapOffset,
            rectTransform.position.z
        );
    }
}
