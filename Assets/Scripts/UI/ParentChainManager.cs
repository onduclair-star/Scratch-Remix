using System.Collections.Generic;
using UnityEngine;

public static class ParentChainManager
{
    private const string NULL_PARENT_NAME = "Null Parent";

    public static void AssignParent(UIPrefabController dragging, UIPrefabController target, float verticalOffset)
    {
        if (dragging == null || target == null) return;

        Transform newParentTransform;

        if (IsDebug(target.gameObject))
        {
            newParentTransform = target.transform;
        }
        else
        {
            Transform debugAncestor = FindAncestorWithDebug(target.transform);
            if (debugAncestor != null)
            {
                newParentTransform = debugAncestor;
                TryDissolveNullParentToDebug(debugAncestor);
            }
            else
            {
                GameObject nullParent = FindOrCreateNullParent(dragging);
                newParentTransform = nullParent.transform;
            }
        }

        dragging.transform.SetParent(newParentTransform, true);

        if (newParentTransform != target.transform && target.transform.parent != newParentTransform)
        {
            target.transform.SetParent(newParentTransform, true);
        }

        AttachToParent(dragging, target, verticalOffset);

        dragging.GetComponent<RectTransform>().SetAsLastSibling();
    }

    private static void AttachToParent(UIPrefabController dragging, UIPrefabController target, float verticalOffset)
    {
        RectTransform childRect = dragging.GetComponent<RectTransform>();
        RectTransform targetRect = target.GetComponent<RectTransform>();
        RectTransform newParentRect = childRect.parent as RectTransform;
        Canvas canvas = dragging.GetComponentInParent<Canvas>();

        if (newParentRect == null || canvas == null) return;

        Camera cam = (canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        Vector3[] targetCorners = new Vector3[4];
        targetRect.GetWorldCorners(targetCorners);
        Vector3 targetBottomCenterWorldPosition = (targetCorners[0] + targetCorners[3]) / 2f;

        float draggingHalfWorldHeight = childRect.rect.height * childRect.lossyScale.y / 2f;

        Vector3 desiredWorldPosition = targetRect.position;

        desiredWorldPosition.y = targetBottomCenterWorldPosition.y
                                 - verticalOffset * targetRect.lossyScale.y
                                 + draggingHalfWorldHeight;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, desiredWorldPosition);

        Vector2 finalLocalPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                newParentRect,
                screenPoint,
                cam,
                out finalLocalPoint))
        {
            Vector2 targetLocalPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    newParentRect,
                    RectTransformUtility.WorldToScreenPoint(cam, targetRect.position),
                    cam,
                    out targetLocalPoint))
            {
                Debug.LogError("[ParentChainManager] FAILED to calculate target X base, using calculated X!");
                targetLocalPoint.x = finalLocalPoint.x;
            }

            childRect.anchoredPosition = new Vector2(
                targetLocalPoint.x,
                finalLocalPoint.y
            );
        }
        else
        {
            Debug.LogError("[ParentChainManager] Failed to convert desired World Point to Local Point!");
        }
    }


    private static bool IsDebug(GameObject go)
    {
        return go != null && go.CompareTag("Debug");
    }

    private static Transform FindAncestorWithDebug(Transform t)
    {
        Transform cur = t.parent;
        while (cur != null)
        {
            if (cur.gameObject.CompareTag("Debug"))
                return cur;
            cur = cur.parent;
        }
        return null;
    }

    private static GameObject FindOrCreateNullParent(UIPrefabController anyBlock)
    {
        GameObject existing = GameObject.Find(NULL_PARENT_NAME);
        if (existing != null) return existing;

        Transform root = null;
        Canvas canvas = anyBlock != null ? anyBlock.GetComponentInParent<Canvas>() : null;
        if (canvas != null)
        {
            root = canvas.transform;
        }

        GameObject go = new(NULL_PARENT_NAME);
        if (root != null) go.transform.SetParent(root, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.localPosition = Vector3.zero;

        return go;
    }

    private static void TryDissolveNullParentToDebug(Transform debugAncestor)
    {
        GameObject nullParentGO = GameObject.Find(NULL_PARENT_NAME);
        if (nullParentGO == null) return;

        List<Transform> children = new();
        foreach (Transform ch in nullParentGO.transform)
            children.Add(ch);

        if (children.Count == 0)
        {
            Object.Destroy(nullParentGO);
            return;
        }

        foreach (var ch in children)
        {
            ch.SetParent(debugAncestor, true);
        }

        Object.Destroy(nullParentGO);
    }
}
