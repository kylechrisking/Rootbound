using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;

public class NotificationSystem : MonoBehaviour
{
    private static NotificationSystem instance;
    private UIDocument document;
    private VisualElement container;
    
    [SerializeField] private float notificationDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Queue<VisualElement> activeNotifications = new Queue<VisualElement>();
    private const int MaxNotifications = 5;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on NotificationSystem!");
            return;
        }

        container = document.rootVisualElement.Q<VisualElement>("notification-container");
    }

    public static void ShowAchievement(string title, string message, Sprite icon = null)
    {
        if (instance == null) return;
        instance.CreateNotification(title, message, "achievement", icon);
    }

    public static void ShowResource(string title, string message, Sprite icon = null)
    {
        if (instance == null) return;
        instance.CreateNotification(title, message, "resource", icon);
    }

    public static void ShowGeneral(string title, string message, Sprite icon = null)
    {
        if (instance == null) return;
        instance.CreateNotification(title, message, "general", icon);
    }

    private void CreateNotification(string title, string message, string type, Sprite icon)
    {
        var notification = new VisualElement();
        notification.AddToClassList("notification");
        notification.AddToClassList(type);
        notification.AddToClassList("entering");

        var header = new VisualElement();
        header.AddToClassList("header");

        if (icon != null)
        {
            var iconElement = new VisualElement();
            iconElement.AddToClassList("icon");
            iconElement.style.backgroundImage = new StyleBackground(icon);
            header.Add(iconElement);
        }

        var titleLabel = new Label(title);
        titleLabel.AddToClassList("title");
        header.Add(titleLabel);

        var closeButton = new VisualElement();
        closeButton.AddToClassList("close-button");
        closeButton.RegisterCallback<ClickEvent>(evt => RemoveNotification(notification));
        header.Add(closeButton);

        notification.Add(header);

        var messageLabel = new Label(message);
        messageLabel.AddToClassList("message");
        notification.Add(messageLabel);

        // Manage notification queue
        if (activeNotifications.Count >= MaxNotifications)
        {
            RemoveNotification(activeNotifications.Dequeue());
        }

        container.Add(notification);
        activeNotifications.Enqueue(notification);

        // Animate in
        notification.schedule.Execute(() => {
            notification.RemoveFromClassList("entering");
            notification.AddToClassList("visible");
        }).StartingIn(10);

        // Schedule removal
        notification.schedule.Execute(() => {
            RemoveNotification(notification);
        }).StartingIn((long)(notificationDuration * 1000));
    }

    private void RemoveNotification(VisualElement notification)
    {
        if (notification == null) return;

        notification.RemoveFromClassList("visible");
        notification.AddToClassList("exiting");

        notification.schedule.Execute(() => {
            notification.RemoveFromHierarchy();
            activeNotifications = new Queue<VisualElement>(activeNotifications);
        }).StartingIn((long)(fadeOutDuration * 1000));
    }
} 