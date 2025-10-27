using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单独负责父链管理逻辑（Debug 优先 / Null Parent 管理 / 提升子物体逻辑）
/// - Null Parent 名称： "Null Parent"
/// - AssignParent(dragging, target, verticalOffset) 会按照你制定的规则完成父子关系变化并对齐位置
/// </summary>
public static class ParentChainManager
{
    private const string NULL_PARENT_NAME = "Null Parent";

    public static void AssignParent(UIPrefabController dragging, UIPrefabController target, float verticalOffset)
    {
        if (dragging == null || target == null) return;

        // 规则 1：目标是 Debug → Debug 直接成为父物体
        if (IsDebug(target.gameObject))
        {
            AttachToParent(dragging, target.transform, target, verticalOffset);
            return;
        }

        // 规则 2：目标不是 Debug → 查找 Debug 祖先
        Transform debugAncestor = FindAncestorWithDebug(target.transform);
        if (debugAncestor != null)
        {
            AttachToParent(dragging, debugAncestor, target, verticalOffset);
            TryDissolveNullParentToDebug(debugAncestor);
            return;
        }

        // 规则 3：无 Debug → 使用 Null Parent
        GameObject nullParent = FindOrCreateNullParent(dragging);

        // dragging 进入 Null Parent
        AttachToParent(dragging, nullParent.transform, target, verticalOffset);

        // target 也要在 Null Parent 下（若还不是的话）
        if (target.transform.parent != nullParent.transform)
        {
            Vector3 targetWorld = target.GetComponent<RectTransform>().position;
            target.transform.SetParent(nullParent.transform, false);
            target.transform.position = targetWorld;
        }
    }

    // -------------------------
    // Helpers
    // -------------------------

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
        if (anyBlock != null && anyBlock.GetComponentInParent<Canvas>() != null)
        {
            root = anyBlock.GetComponentInParent<Canvas>().transform;
        }

        GameObject go = new(NULL_PARENT_NAME);
        if (root != null) go.transform.SetParent(root, false);

        return go;
    }

    private static void AttachToParent(UIPrefabController dragging, Transform parentTransform, UIPrefabController target, float verticalOffset)
    {
        RectTransform childRect = dragging.GetComponent<RectTransform>();
        RectTransform targetRect = target.GetComponent<RectTransform>();

        Vector3 targetWorld = targetRect.position;
        Vector3 childWorld = childRect.position;
        Vector3 desiredWorld = new(childWorld.x, targetWorld.y - verticalOffset, childWorld.z);

        childRect.SetParent(parentTransform, false);
        childRect.position = desiredWorld;
    }

    private static void TryDissolveNullParentToDebug(Transform debugAncestor)
    {
        GameObject nullParentGO = GameObject.Find(NULL_PARENT_NAME);
        if (nullParentGO == null) return;

        if (nullParentGO.transform.childCount == 0)
        {
            Object.Destroy(nullParentGO);
            return;
        }

        List<Transform> children = new();
        foreach (Transform ch in nullParentGO.transform)
            children.Add(ch);

        foreach (var ch in children)
        {
            Vector3 worldPos = ch.position;
            ch.SetParent(debugAncestor, false);
            ch.position = worldPos;
        }

        Object.Destroy(nullParentGO);
    }
}
