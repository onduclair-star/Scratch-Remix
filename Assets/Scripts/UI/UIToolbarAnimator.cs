using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIToolbarAnimator : MonoBehaviour
{
    [Header("Positions")]
    public Vector3 hiddenPos = new Vector3(0, 375, 0);
    public Vector3 visiblePos = new Vector3(0, -314, 0);
    public float moveDuration = 0.3f;

    [Header("Cursor Trigger Settings")]
    public float triggerHeight = 10f;

    private Coroutine moveCoroutine;
    private bool isVisible = false;

    private void Awake()
    {
        transform.localPosition = hiddenPos;
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        bool shouldShow = mousePos.y >= Screen.height - triggerHeight;

        if (shouldShow != isVisible)
        {
            SetVisible(shouldShow);
        }
    }

    public void SetVisible(bool visible, bool instant = false)
    {
        isVisible = visible;
        Vector3 target = visible ? visiblePos : hiddenPos;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveTo(target, instant));
    }

    private IEnumerator MoveTo(Vector3 target, bool instant)
    {
        if (instant)
        {
            transform.localPosition = target;
            yield break;
        }

        Vector3 start = transform.localPosition;
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.unscaledDeltaTime;
            transform.localPosition = Vector3.Lerp(start, target, time / moveDuration);
            yield return null;
        }

        transform.localPosition = target;
    }
}
