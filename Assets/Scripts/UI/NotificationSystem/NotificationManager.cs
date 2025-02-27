using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NotificationManager>();
            }
            return _instance;
        }
    }

    [Header("Notification Settings")]
    [SerializeField] private GameObject achievementNotificationPrefab;
    [SerializeField] private GameObject generalNotificationPrefab;
    [SerializeField] private GameObject resourceNotificationPrefab;
    [SerializeField] private RectTransform notificationContainer;
    [SerializeField] private float notificationSpacing = 10f;
    [SerializeField] private int maxNotifications = 3;
    [SerializeField] private float defaultDuration = 3f;
    [SerializeField] private float resourceNotificationDuration = 2f;
    
    private Queue<NotificationBase> notificationQueue = new Queue<NotificationBase>();
    private List<NotificationBase> activeNotifications = new List<NotificationBase>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to achievement events
        AchievementManager.Instance.OnAchievementUnlocked += ShowAchievementNotification;
    }

    public void ShowAchievementNotification(AchievementManager.Achievement achievement)
    {
        GameObject notificationObj = Instantiate(achievementNotificationPrefab);
        var notification = notificationObj.GetComponent<AchievementNotification>();
        notification.Initialize(achievement);
        QueueNotification(notification);
    }

    public void ShowNotification(string title, string message, float duration = -1)
    {
        GameObject notificationObj = Instantiate(generalNotificationPrefab);
        var notification = notificationObj.GetComponent<GeneralNotification>();
        notification.Initialize(title, message, duration < 0 ? defaultDuration : duration);
        QueueNotification(notification);
    }

    public void ShowResourceNotification(ResourceType type, float amount, bool isGain = true)
    {
        GameObject notificationObj = Instantiate(resourceNotificationPrefab);
        var notification = notificationObj.GetComponent<ResourceNotification>();
        notification.Initialize(type, amount, isGain);
        QueueNotification(notification);
    }

    private void QueueNotification(NotificationBase notification)
    {
        notification.transform.SetParent(notificationContainer, false);
        notification.gameObject.SetActive(false);
        
        notificationQueue.Enqueue(notification);
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        while (notificationQueue.Count > 0 && activeNotifications.Count < maxNotifications)
        {
            var notification = notificationQueue.Dequeue();
            ShowNotificationImmediate(notification);
        }
    }

    private void ShowNotificationImmediate(NotificationBase notification)
    {
        notification.gameObject.SetActive(true);
        activeNotifications.Add(notification);
        
        // Position the notification
        UpdateNotificationPositions();
        
        // Start fade out timer
        StartCoroutine(NotificationLifetime(notification));
    }

    private void UpdateNotificationPositions()
    {
        float currentY = 0;
        for (int i = activeNotifications.Count - 1; i >= 0; i--)
        {
            var notification = activeNotifications[i];
            var rectTransform = notification.GetComponent<RectTransform>();
            
            Vector2 position = rectTransform.anchoredPosition;
            position.y = currentY;
            rectTransform.anchoredPosition = position;
            
            currentY += rectTransform.rect.height + notificationSpacing;
        }
    }

    private IEnumerator NotificationLifetime(NotificationBase notification)
    {
        yield return notification.PlayShowAnimation();
        yield return new WaitForSeconds(notification.Duration);
        yield return notification.PlayHideAnimation();

        activeNotifications.Remove(notification);
        Destroy(notification.gameObject);
        
        UpdateNotificationPositions();
        ProcessQueue();
    }

    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked -= ShowAchievementNotification;
        }
    }
} 