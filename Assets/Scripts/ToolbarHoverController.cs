using UnityEngine;
using UnityEngine.InputSystem;

public class ToolbarHoverController : MonoBehaviour
{
    [SerializeField] private float baseScreenEdgeTriggerHeight = 50f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float baseHideDelay = 0.05f;
    [SerializeField] private float maxHideDelay = 5f;
    [SerializeField] private float decayRate = 1f;
    [SerializeField] private float speedThreshold = 250f;

    private RectTransform toolbar;
    private Vector2 visiblePos, hiddenPos, startPos;
    private bool shouldShow = false;
    private float moveTimer = 0f;

    private float hoverAccum = 0f;
    private float lastHoverTime = 0f;
    private float currentHideDelay;
    private Vector2 lastMousePosition;
    private float lastMouseMoveTime;
    private float currentMouseSpeed;
    private float exitSpeed = 0f;
    private bool wasHovering = false;
    private float currentTriggerHeight;

    public void Initialize(RectTransform toolbar)
    {
        this.toolbar = toolbar;

        visiblePos = toolbar.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(0, toolbar.rect.height);
        toolbar.anchoredPosition = hiddenPos;
        startPos = hiddenPos;

        currentHideDelay = baseHideDelay;
        currentTriggerHeight = baseScreenEdgeTriggerHeight;

        lastMousePosition = Mouse.current.position.ReadValue();
        lastMouseMoveTime = Time.unscaledTime;
    }

    public void UpdateToolbarPosition(bool forceShow)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        bool hover = CheckHover(mousePos);
        CalculateMouseSpeed(mousePos);
        UpdateExitSpeed(hover);

        float deltaTime = Time.unscaledDeltaTime;
        UpdateHoverAccum(deltaTime, hover);

        bool targetShow = forceShow || hover || (Time.unscaledTime - lastHoverTime <= currentHideDelay);

        if (targetShow != shouldShow)
        {
            shouldShow = targetShow;
            moveTimer = 0f;
            startPos = toolbar.anchoredPosition;
        }

        moveTimer += deltaTime / moveDuration;
        moveTimer = Mathf.Clamp01(moveTimer);
        float smoothT = moveTimer * moveTimer * (3f - 2f * moveTimer);
        Vector2 targetPos = shouldShow ? visiblePos : hiddenPos;
        toolbar.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
    }

    private bool CheckHover(Vector2 pos)
    {
        bool nearTop = pos.y > Screen.height - currentTriggerHeight;
        bool overToolbar = RectTransformUtility.RectangleContainsScreenPoint(toolbar, pos);
        return nearTop || overToolbar;
    }

    private void CalculateMouseSpeed(Vector2 currentPos)
    {
        float deltaTime = Time.unscaledTime - lastMouseMoveTime;
        if (deltaTime > 0.01f)
        {
            currentMouseSpeed = Vector2.Distance(currentPos, lastMousePosition) / deltaTime;
            lastMousePosition = currentPos;
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    private void UpdateExitSpeed(bool hover)
    {
        if (wasHovering && !hover) exitSpeed = currentMouseSpeed;
        wasHovering = hover;
    }

    private void UpdateHoverAccum(float deltaTime, bool hover)
    {
        if (hover)
        {
            hoverAccum += deltaTime;
            lastHoverTime = Time.unscaledTime;
        }
        else
        {
            hoverAccum = Mathf.Max(0f, hoverAccum - GetSpeedAdjustedDecayRate(exitSpeed) * deltaTime);
        }
        currentHideDelay = CalculateSmartHideDelay(hoverAccum, exitSpeed);
    }

    private float CalculateSmartHideDelay(float accumulatedTime, float speedAtExit)
    {
        float baseDelay = baseHideDelay + accumulatedTime * 0.1f;
        if (speedAtExit < speedThreshold)
        {
            float slowFactor = Mathf.Lerp(10f, 1f, speedAtExit / speedThreshold);
            return Mathf.Min(baseDelay * slowFactor, maxHideDelay);
        }
        else
        {
            float normalizedSpeed = Mathf.Clamp01((speedAtExit - speedThreshold) / speedThreshold);
            float fastFactor = Mathf.Lerp(1f, 0.01f, normalizedSpeed * normalizedSpeed);
            return Mathf.Min(baseDelay * fastFactor, maxHideDelay);
        }
    }

    private float GetSpeedAdjustedDecayRate(float speed)
    {
        return speed < speedThreshold ? decayRate * 0.5f : decayRate * 2f;
    }
}
