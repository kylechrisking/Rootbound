using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TipEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image completionIcon;
    [SerializeField] private GameObject newIndicator;
    
    [Header("Visual States")]
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color incompleteColor = Color.white;
    [SerializeField] private Sprite completedIcon;
    [SerializeField] private Sprite incompleteIcon;

    private TutorialManager.TutorialStep tutorialStep;

    public void Initialize(TutorialManager.TutorialStep step)
    {
        tutorialStep = step;
        bool isCompleted = TutorialManager.Instance.HasCompletedTutorial(step.id);
        
        titleText.text = step.title;
        messageText.text = step.message;
        
        // Update visual state
        titleText.color = isCompleted ? completedColor : incompleteColor;
        completionIcon.sprite = isCompleted ? completedIcon : incompleteIcon;
        
        // Show new indicator if completed recently
        newIndicator.SetActive(TutorialManager.Instance.IsRecentlyCompleted(step.id));
    }
} 