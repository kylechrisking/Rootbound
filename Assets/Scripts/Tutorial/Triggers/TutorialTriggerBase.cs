using UnityEngine;

public abstract class TutorialTriggerBase : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] protected string tutorialId;
    [SerializeField] protected bool triggerOnce = true;
    [SerializeField] protected float triggerDelay = 0f;

    protected bool hasTriggered;

    protected virtual void Start()
    {
        // Don't trigger if already completed
        if (triggerOnce && TutorialManager.Instance.HasCompletedTutorial(tutorialId))
        {
            enabled = false;
        }
    }

    protected void TriggerTutorial()
    {
        if (hasTriggered && triggerOnce) return;
        
        if (triggerDelay > 0)
        {
            Invoke(nameof(ShowTutorial), triggerDelay);
        }
        else
        {
            ShowTutorial();
        }
    }

    private void ShowTutorial()
    {
        TutorialManager.Instance.ShowTutorialStep(tutorialId);
        hasTriggered = true;
        
        if (triggerOnce)
        {
            enabled = false;
        }
    }
} 