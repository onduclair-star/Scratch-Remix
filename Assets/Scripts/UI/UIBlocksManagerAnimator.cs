#if UNITY_IOS || UNITY_ANDROID
    #define MOBILE
#endif

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(RectTransform))]
public class UIBlocksManagerAnimator : MonoBehaviour
{
#if MOBILE
    [SerializeField] private float doubleTapMaxTime = 0.3f;
    private float lastTapTime = -1f;
#endif

    [Header("Hover / Touch Settings")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float baseHideDelay = 0.05f;
    [SerializeField] private float maxHideDelay = 5f;
    [SerializeField] private float decayRate = 1f;
    [SerializeField] private float speedThreshold = 250f;

    [Header("Positions")]
    [SerializeField] private float visibleOffsetX = 70f;
    [SerializeField] private float hiddenOffsetX = -70f;

    [Header("Controlled Objects")]
    // List of CanvasGroups to mark which objects need their image materials controlled
    [SerializeField] private List<CanvasGroup> controlledCanvasGroups;

    // New fields for Collider2D detection
    [Header("Collider Detection")]
    [SerializeField] private string triggerColliderTag = "Block Manager"; // Updated tag name based on image
    private List<Collider2D> triggerAreaColliders = new(); // Use a List for multiple colliders
    private Camera mainCamera;

    private RectTransform toolbar;
    private Vector2 visiblePos, hiddenPos, startPos;
    private bool shouldShow = false, forceShow;
    private float moveTimer = 0f;

    private float hoverAccum = 0f;
    private float lastHoverTime = 0f;
    private float currentHideDelay;
    private Vector2 lastMousePosition;
    private float lastMouseMoveTime;
    private float currentMouseSpeed;
    private float exitSpeed = 0f;
    private bool wasHovering = false;
    private const float TotalXDistance = 140f;

    void Awake()
    {
        toolbar = GetComponent<RectTransform>();
        Initialize();
    }

    void Update()
    {
#if MOBILE
        forceShow = CheckMobileDoubleTap();
#else
        forceShow = UIManager.shouldShow;
#endif

        UpdateToolbarPosition(forceShow);

        UpdateChildrenAlphaByPosition();
    }

    void Initialize()
    {
        float currentY = toolbar.anchoredPosition.y;
        visiblePos = new Vector2(visibleOffsetX, currentY);
        hiddenPos = new Vector2(hiddenOffsetX, currentY);

        toolbar.anchoredPosition = hiddenPos;
        startPos = hiddenPos;
        currentHideDelay = baseHideDelay;

        mainCamera = Camera.main;

        GameObject[] triggerObjects = GameObject.FindGameObjectsWithTag(triggerColliderTag);
        if (triggerObjects != null && triggerObjects.Length > 0)
        {
            triggerAreaColliders = triggerObjects
                .Select(go => go.GetComponent<Collider2D>())
                .Where(c => c != null)
                .ToList();
        }

        if (triggerAreaColliders.Count == 0)
        {
            Debug.LogError($"Could not find any Collider2D with tag '{triggerColliderTag}'. Hover detection will not work!");
        }

        UpdateChildrenVisualAlpha(0f);

        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
            lastMouseMoveTime = Time.unscaledTime;
        }
    }

    public void UpdateToolbarPosition(bool forceShow)
    {
        bool hover = false;

#if !MOBILE
        Vector2 currentMousePos = Mouse.current.position.ReadValue();
        hover = CheckHover(currentMousePos);
        CalculateMouseSpeed(currentMousePos);
        UpdateExitSpeed(hover);

        float deltaTime = Time.unscaledDeltaTime;
        UpdateHoverAccum(deltaTime, hover);
#endif

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

    private void UpdateChildrenAlphaByPosition()
    {
        float currentX = toolbar.anchoredPosition.x;
        float progress = currentX - hiddenOffsetX;

        float targetAlpha = Mathf.Clamp01(progress / TotalXDistance);

        UpdateChildrenVisualAlpha(targetAlpha);
    }

    private bool CheckHover(Vector2 screenPos)
    {
#if !MOBILE
        if (mainCamera == null || triggerAreaColliders.Count == 0)
            return false;

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);

        bool overTrigger = false;
        foreach (Collider2D col in triggerAreaColliders)
        {
            if (col.OverlapPoint(worldPos))
            {
                overTrigger = true;
                break;
            }
        }

        bool overToolbar = RectTransformUtility.RectangleContainsScreenPoint(toolbar, screenPos);

        return overTrigger || overToolbar;
#else
        return false;
#endif
    }


    private void CalculateMouseSpeed(Vector2 currentPos)
    {
        float deltaTime = Time.unscaledTime - lastMouseMoveTime;
        if (deltaTime > 0.01f)
        {
            // Calculate distance based only on the horizontal (X) movement
            float distance = Mathf.Abs(currentPos.x - lastMousePosition.x);
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

    // Controls the visibility of the controlled objects by setting both CanvasGroup alpha and Image material alpha
    private void UpdateChildrenVisualAlpha(float targetAlpha)
    {
        foreach (var cg in controlledCanvasGroups)
        {
            if (cg == null) continue;

            // 1. Set CanvasGroup Alpha (for overall UI visibility and interaction control)
            cg.alpha = targetAlpha;

            // 2. Image on the CanvasGroup's GameObject (Self Image Material Alpha)
            Image selfImage = cg.GetComponent<Image>();
            if (selfImage != null && selfImage.material != null)
            {
                Color color = selfImage.material.color;
                color.a = targetAlpha;
                selfImage.material.color = color;
            }

            // 3. Image on the first child's Material Alpha
            if (cg.transform.childCount > 0)
            {
                Transform firstChild = cg.transform.GetChild(0);
                Image childImage = firstChild.GetComponent<Image>();

                if (childImage != null && childImage.material != null)
                {
                    Color color = childImage.material.color;
                    color.a = targetAlpha;
                    childImage.material.color = color;
                }
            }
        }
    }

#if MOBILE
    private bool CheckMobileDoubleTap()
    {
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0)
            return false;

        var touch = Touchscreen.current.touches[0];
        if (!touch.press.isPressed) return false;

        float currentTime = Time.unscaledTime;
        if (lastTapTime < 0f)
        {
            lastTapTime = currentTime;
            return false;
        }

        if (currentTime - lastTapTime <= doubleTapMaxTime)
        {
            lastTapTime = -1f;
            Vector2 pos = touch.position.ReadValue();
            
            // Check if the double tap position is over ANY of the trigger colliders
            if (mainCamera != null && triggerAreaColliders.Count > 0)
            {
                Vector2 worldPos = mainCamera.ScreenToWorldPoint(pos);
                foreach (Collider2D col in triggerAreaColliders)
                {
                    if (col.OverlapPoint(worldPos))
                    {
                        Handheld.Vibrate();
                        return true;
                    }
                }
            }
        }

        lastTapTime = currentTime;
        return false;
    }

#endif
}
