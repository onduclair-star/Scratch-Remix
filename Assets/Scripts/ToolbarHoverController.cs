using UnityEngine;
using UnityEngine.InputSystem;

public class ToolbarHoverController : MonoBehaviour
{
    [Header("Hover Settings")]
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
        if (this.toolbar != null) return;
        
        this.toolbar = toolbar;
        visiblePos = toolbar.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(0, toolbar.rect.height);
        toolbar.anchoredPosition = hiddenPos;
        startPos = hiddenPos;
        currentHideDelay = baseHideDelay;
        currentTriggerHeight = baseScreenEdgeTriggerHeight;

        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    public void UpdateToolbarPosition(bool forceShow)
    {
        if (toolbar == null) return;

        bool hover = false;

        if (Mouse.current != null)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            hover = CheckHover(currentMousePos);
            CalculateMouseSpeed(currentMousePos);
            UpdateExitSpeed(hover);

            float deltaTime = Time.unscaledDeltaTime;
            UpdateHoverAccum(deltaTime, hover);
        }

        bool targetShow = forceShow || hover || (Time.unscaledTime - lastHoverTime <= currentHideDelay);
        
        if (targetShow != shouldShow)
        {
            shouldShow = targetShow;
            moveTimer = 0f;
            startPos = toolbar.anchoredPosition;
        }

        moveTimer += Time.unscaledDeltaTime / moveDuration;
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
            float distance = Vector2.Distance(currentPos, lastMousePosition);
            currentMouseSpeed = distance / deltaTime;
            lastMousePosition = currentPos;
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    private void UpdateExitSpeed(bool hover)
    {
        if (wasHovering && !hover) 
            exitSpeed = currentMouseSpeed;
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
            float effectiveDecayRate = GetSpeedAdjustedDecayRate(exitSpeed);
            hoverAccum = Mathf.Max(0f, hoverAccum - effectiveDecayRate * deltaTime);
        }
        currentHideDelay = CalculateSmartHideDelay(hoverAccum, exitSpeed);
    }

    float CalculateSmartHideDelay(float accumulatedTime, float speedAtExit)
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

    float GetSpeedAdjustedDecayRate(float speed)
    {
        return speed < speedThreshold ? decayRate * 0.5f : decayRate * 2f;
    }
}
