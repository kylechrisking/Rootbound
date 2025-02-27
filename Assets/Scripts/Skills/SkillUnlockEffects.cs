using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SkillUnlockEffects : MonoBehaviour
{
    private static SkillUnlockEffects instance;

    [Header("Unlock Effect")]
    [SerializeField] private GameObject unlockEffectPrefab;
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve alphaCurve;
    
    [Header("Notification")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private Vector2 notificationOffset = new Vector2(0, 100f);

    private void Awake()
    {
        instance = this;
    }

    public static void PlayUnlockEffect(SkillNodeUI node)
    {
        if (instance == null) return;
        instance.StartCoroutine(instance.PlayUnlockEffectCoroutine(node));
    }

    private IEnumerator PlayUnlockEffectCoroutine(SkillNodeUI node)
    {
        // Spawn effect
        GameObject effect = Instantiate(unlockEffectPrefab, node.transform.position, Quaternion.identity, transform);
        Image effectImage = effect.GetComponent<Image>();
        
        float startTime = Time.time;
        Vector3 originalScale = effect.transform.localScale;

        while (Time.time - startTime < effectDuration)
        {
            float progress = (Time.time - startTime) / effectDuration;
            
            // Scale effect
            effect.transform.localScale = originalScale * scaleCurve.Evaluate(progress);
            
            // Fade effect
            Color color = effectImage.color;
            color.a = alphaCurve.Evaluate(progress);
            effectImage.color = color;

            yield return null;
        }

        Destroy(effect);

        // Show notification
        ShowUnlockNotification(node.SkillData);
    }

    private void ShowUnlockNotification(SkillData skill)
    {
        StartCoroutine(ShowNotificationCoroutine(skill));
    }

    private IEnumerator ShowNotificationCoroutine(SkillData skill)
    {
        GameObject notification = Instantiate(notificationPrefab, transform);
        RectTransform rect = notification.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();

        // Set notification position
        rect.anchoredPosition = notificationOffset;

        // Set notification text
        TextMeshProUGUI text = notification.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"New Skill Unlocked!\n{skill.skillName}";

        // Fade in
        float fadeTime = 0.5f;
        float startTime = Time.time;
        while (Time.time - startTime < fadeTime)
        {
            canvasGroup.alpha = (Time.time - startTime) / fadeTime;
            yield return null;
        }

        // Wait
        yield return new WaitForSeconds(notificationDuration - fadeTime * 2);

        // Fade out
        startTime = Time.time;
        while (Time.time - startTime < fadeTime)
        {
            canvasGroup.alpha = 1 - (Time.time - startTime) / fadeTime;
            yield return null;
        }

        Destroy(notification);
    }
} 