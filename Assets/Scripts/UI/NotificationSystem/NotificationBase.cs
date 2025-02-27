using UnityEngine;
using System.Collections;

public abstract class NotificationBase : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] protected float showDuration = 0.5f;
    [SerializeField] protected float hideDuration = 0.3f;
    [SerializeField] protected AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    public float Duration { get; protected set; }
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public virtual IEnumerator PlayShowAnimation()
    {
        canvasGroup.alpha = 0f;
        Vector2 startPos = rectTransform.anchoredPosition + Vector2.right * 100f;
        Vector2 endPos = rectTransform.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            float t = showCurve.Evaluate(elapsed / showDuration);
            canvasGroup.alpha = t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = endPos;
    }

    public virtual IEnumerator PlayHideAnimation()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.right * 100f;

        float elapsed = 0f;
        while (elapsed < hideDuration)
        {
            float t = hideCurve.Evaluate(elapsed / hideDuration);
            canvasGroup.alpha = 1 - t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
} 