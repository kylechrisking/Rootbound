using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class TutorialStep
    {
        public string id;
        public string title;
        public string message;
        public bool isRequired;
        public string[] prerequisites;
        public float triggerDelay;
    }

    [Header("Tutorial Steps")]
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private float initialDelay = 2f;
    
    private HashSet<string> completedSteps = new HashSet<string>();
    private Dictionary<string, TutorialStep> stepLookup = new Dictionary<string, TutorialStep>();
    private float gameStartTime;
    private HashSet<string> recentlyCompleted = new HashSet<string>();
    private float recentCompletionDuration = 300f; // 5 minutes
    private Dictionary<string, float> completionTimes = new Dictionary<string, float>();

    public event System.Action<string> OnTutorialCompleted;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTutorial();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gameStartTime = Time.time;
        StartInitialTutorials();
    }

    private void InitializeTutorial()
    {
        foreach (var step in tutorialSteps)
        {
            stepLookup[step.id] = step;
        }
    }

    private void StartInitialTutorials()
    {
        foreach (var step in tutorialSteps)
        {
            if (step.prerequisites == null || step.prerequisites.Length == 0)
            {
                float delay = initialDelay + step.triggerDelay;
                Invoke(nameof(ShowTutorialStep), delay, step.id);
            }
        }
    }

    public void ShowTutorialStep(string stepId)
    {
        if (completedSteps.Contains(stepId)) return;

        if (stepLookup.TryGetValue(stepId, out TutorialStep step))
        {
            // Check prerequisites
            if (step.prerequisites != null)
            {
                foreach (string prereq in step.prerequisites)
                {
                    if (!completedSteps.Contains(prereq)) return;
                }
            }

            // Show tutorial notification
            NotificationManager.Instance.ShowNotification(
                step.title,
                step.message,
                duration: step.isRequired ? -1 : 5f // Required tutorials stay until acknowledged
            );

            completedSteps.Add(stepId);
            completionTimes[stepId] = Time.time;
            recentlyCompleted.Add(stepId);
            OnTutorialCompleted?.Invoke(stepId);
            
            // Trigger dependent tutorials
            foreach (var nextStep in tutorialSteps)
            {
                if (nextStep.prerequisites != null && 
                    System.Array.Exists(nextStep.prerequisites, p => p == stepId))
                {
                    Invoke(nameof(ShowTutorialStep), nextStep.triggerDelay, nextStep.id);
                }
            }
        }
    }

    public void TriggerContextualTutorial(string contextId)
    {
        ShowTutorialStep("context_" + contextId);
    }

    public bool HasCompletedTutorial(string stepId)
    {
        return completedSteps.Contains(stepId);
    }

    // Save/Load system integration
    public void SaveProgress(GameData data)
    {
        data.completedTutorials = new List<string>(completedSteps);
    }

    public void LoadProgress(GameData data)
    {
        completedSteps.Clear();
        if (data.completedTutorials != null)
        {
            foreach (string stepId in data.completedTutorials)
            {
                completedSteps.Add(stepId);
            }
        }
    }

    public TutorialStep[] GetAllTutorials() => tutorialSteps;

    public bool IsRecentlyCompleted(string stepId)
    {
        if (!completionTimes.TryGetValue(stepId, out float completionTime)) return false;
        return Time.time - completionTime < recentCompletionDuration;
    }
} 