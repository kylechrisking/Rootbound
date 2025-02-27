using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceNotification : NotificationBase
{
    [Header("UI References")]
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI resourceNameText;
    
    [Header("Resource Icons")]
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite nutrientIcon;
    [SerializeField] private Sprite sunlightIcon;
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    public void Initialize(ResourceType type, float amount, bool isGain = true)
    {
        Duration = displayDuration;

        // Set icon based on resource type
        resourceIcon.sprite = type switch
        {
            ResourceType.Water => waterIcon,
            ResourceType.Nutrients => nutrientIcon,
            ResourceType.Sunlight => sunlightIcon,
            _ => null
        };

        // Set amount text with color
        string sign = isGain ? "+" : "-";
        amountText.text = $"{sign}{amount:F1}";
        amountText.color = isGain ? positiveColor : negativeColor;

        // Set resource name
        resourceNameText.text = type.ToString();
    }

    public override IEnumerator PlayShowAnimation()
    {
        // Add a slight bounce effect
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;

        yield return base.PlayShowAnimation();

        float elapsed = 0f;
        float bounceDuration = 0.2f;
        while (elapsed < bounceDuration)
        {
            float t = elapsed / bounceDuration;
            transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
} 