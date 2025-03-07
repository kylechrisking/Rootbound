using UnityEngine;
using UnityEngine.UIElements;

public class TipEntryUI : MonoBehaviour
{
    private Label titleLabel;
    private Label messageLabel;
    private VisualElement completionIcon;
    private VisualElement newIndicator;
    
    private UIDocument document;
    
    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on TipEntryUI!");
            return;
        }

        var root = document.rootVisualElement;
        
        // Get references to UI elements
        titleLabel = root.Q<Label>("title-text");
        messageLabel = root.Q<Label>("message-text");
        completionIcon = root.Q<VisualElement>("completion-icon");
        newIndicator = root.Q<VisualElement>("new-indicator");
    }

    public void Initialize(TutorialManager.TutorialStep step)
    {
        bool isCompleted = TutorialManager.Instance.HasCompletedTutorial(step.id);
        
        titleLabel.text = step.title;
        messageLabel.text = step.message;
        
        // Update visual state
        titleLabel.AddToClassList(isCompleted ? "completed" : "incomplete");
        completionIcon.AddToClassList(isCompleted ? "completed-icon" : "incomplete-icon");
        
        // Show new indicator if completed recently
        newIndicator.style.display = TutorialManager.Instance.IsRecentlyCompleted(step.id) ? 
            DisplayStyle.Flex : DisplayStyle.None;
    }
} 