using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Generator : MonoBehaviour
{
    public GameObject moveSteps, toDebug;
    public Transform parent;

    public void ToDebug()
    {
        var debug = Instantiate(toDebug, parent);
        debug.AddComponent<ToDebug>();
        debug.AddComponent<UIPrefabController>();
        _ = StartCoroutine(FollowCursorTillClick(debug));
    }

    public void MoveSteps()
    {
        var steps = Instantiate(moveSteps, parent);
        // steps.AddComponent<MoveSteps>();
        steps.AddComponent<UIPrefabController>();
        _ = StartCoroutine(FollowCursorTillClick(steps));
    }

    private IEnumerator FollowCursorTillClick(GameObject follower)
    {
        while (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            var mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            follower.transform.position = worldPos;
            yield return null;
        }
    }
}
