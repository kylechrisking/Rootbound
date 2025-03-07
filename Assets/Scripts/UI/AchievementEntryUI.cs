using UnityEngine;
using UnityEngine.UIElements;

public class AchievementEntryUI : MonoBehaviour
{
    private VisualElement root;
    private VisualElement icon;
    private Label nameLabel;
    private Label descriptionLabel;
    private ProgressBar progressBar;
    private VisualElement completionIndicator;

    private string achievementId;

    private void Awake()
    {
        var document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on AchievementEntryUI!");
            return;
        }

        root = document.rootVisualElement.Q<VisualElement>("achievement-entry");
        icon = root.Q<VisualElement>("achievement-icon");
        nameLabel = root.Q<Label>("achievement-name");
        descriptionLabel = root.Q<Label>("achievement-description");
        progressBar = root.Q<ProgressBar>("achievement-progress");
        completionIndicator = root.Q<VisualElement>("completion-indicator");
    }

    public void Initialize(Achievement achievement)
    {
        achievementId = achievement.id;
        
        nameLabel.text = achievement.name;
        descriptionLabel.text = achievement.description;
        
        if (achievement.icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(achievement.icon);
        }

        UpdateProgress(achievement);
    }

    public void UpdateProgress(Achievement achievement)
    {
        if (achievement.isCompleted)
        {
            root.AddToClassList("completed");
            root.RemoveFromClassList("locked");
            progressBar.style.display = DisplayStyle.None;
        }
        else if (achievement.isLocked)
        {
            root.AddToClassList("locked");
            root.RemoveFromClassList("completed");
            progressBar.style.display = DisplayStyle.None;
        }
        else
        {
            root.RemoveFromClassList("completed");
            root.RemoveFromClassList("locked");
            progressBar.style.display = DisplayStyle.Flex;
            progressBar.value = achievement.progress * 100;
        }
    }
} 