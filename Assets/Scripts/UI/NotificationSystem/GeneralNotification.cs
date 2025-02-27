using UnityEngine;
using TMPro;

public class GeneralNotification : NotificationBase
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Header("Settings")]
    [SerializeField] private Color defaultTitleColor = Color.white;
    [SerializeField] private Color warningTitleColor = Color.yellow;
    [SerializeField] private Color errorTitleColor = Color.red;

    public void Initialize(string title, string message, float duration, NotificationType type = NotificationType.Default)
    {
        Duration = duration;
        titleText.text = title;
        messageText.text = message;

        titleText.color = type switch
        {
            NotificationType.Warning => warningTitleColor,
            NotificationType.Error => errorTitleColor,
            _ => defaultTitleColor
        };
    }
}

public enum NotificationType
{
    Default,
    Warning,
    Error
} 