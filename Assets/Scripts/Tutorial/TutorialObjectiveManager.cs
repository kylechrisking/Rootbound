using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TutorialObjectiveManager : MonoBehaviour
{
    private static TutorialObjectiveManager _instance;
    public static TutorialObjectiveManager Instance => _instance;

    [Header("Objectives")]
    [SerializeField] private TutorialObjective[] availableObjectives;
    
    private Dictionary<string, TutorialObjective> objectiveLookup = new Dictionary<string, TutorialObjective>();
    private HashSet<string> completedObjectives = new HashSet<string>();
    private List<TutorialObjective> activeObjectives = new List<TutorialObjective>();

    public event System.Action<TutorialObjective> OnObjectiveCompleted;
    public event System.Action<TutorialObjective> OnObjectiveActivated;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeObjectives();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to tutorial completion events
        TutorialManager.Instance.OnTutorialCompleted += CheckObjectiveProgress;
    }

    private void InitializeObjectives()
    {
        foreach (var objective in availableObjectives)
        {
            objectiveLookup[objective.id] = objective;
        }
        
        // Activate initial objectives
        foreach (var objective in availableObjectives)
        {
            if (CanActivateObjective(objective))
            {
                ActivateObjective(objective);
            }
        }
    }

    private bool CanActivateObjective(TutorialObjective objective)
    {
        if (completedObjectives.Contains(objective.id)) return false;
        if (activeObjectives.Contains(objective)) return false;

        // Check required tutorials
        if (objective.requiredTutorials != null)
        {
            foreach (string tutorialId in objective.requiredTutorials)
            {
                if (!TutorialManager.Instance.HasCompletedTutorial(tutorialId))
                {
                    return false;
                }
            }
        }

        // Check required objectives
        if (objective.requiredObjectives != null)
        {
            foreach (string objectiveId in objective.requiredObjectives)
            {
                if (!completedObjectives.Contains(objectiveId))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void ActivateObjective(TutorialObjective objective)
    {
        if (!objective.isHidden)
        {
            NotificationManager.Instance.ShowNotification(
                "New Objective",
                objective.title,
                duration: 5f
            );
        }

        activeObjectives.Add(objective);
        OnObjectiveActivated?.Invoke(objective);
    }

    public void CompleteObjective(string objectiveId)
    {
        if (objectiveLookup.TryGetValue(objectiveId, out TutorialObjective objective))
        {
            CompleteObjective(objective);
        }
    }

    private void CompleteObjective(TutorialObjective objective)
    {
        if (completedObjectives.Contains(objective.id)) return;

        // Grant rewards
        if (objective.skillPointReward > 0)
        {
            SkillManager.Instance.AddSkillPoints(objective.skillPointReward);
        }
        if (objective.growthBoostReward > 0)
        {
            PlayerStats.Instance.AddGrowthRateBonus(objective.growthBoostReward);
        }
        if (objective.resourceRewards != null)
        {
            foreach (var reward in objective.resourceRewards)
            {
                // Add resources through appropriate system
                // ResourceManager.Instance.AddResource(reward.type, reward.amount);
            }
        }

        // Trigger additional tutorials
        if (objective.triggerTutorialsOnComplete != null)
        {
            foreach (string tutorialId in objective.triggerTutorialsOnComplete)
            {
                TutorialManager.Instance.ShowTutorialStep(tutorialId);
            }
        }

        completedObjectives.Add(objective.id);
        activeObjectives.Remove(objective);
        OnObjectiveCompleted?.Invoke(objective);

        // Check for new objectives to activate
        foreach (var nextObjective in availableObjectives)
        {
            if (CanActivateObjective(nextObjective))
            {
                ActivateObjective(nextObjective);
            }
        }
    }

    private void CheckObjectiveProgress(string completedTutorialId)
    {
        foreach (var objective in activeObjectives.ToList())
        {
            if (objective.autoComplete && 
                objective.requiredTutorials != null &&
                objective.requiredTutorials.Contains(completedTutorialId))
            {
                bool allTutorialsCompleted = objective.requiredTutorials.All(
                    tutId => TutorialManager.Instance.HasCompletedTutorial(tutId)
                );

                if (allTutorialsCompleted)
                {
                    CompleteObjective(objective);
                }
            }
        }
    }

    // Save/Load integration
    public void SaveProgress(GameData data)
    {
        data.completedObjectives = new List<string>(completedObjectives);
    }

    public void LoadProgress(GameData data)
    {
        completedObjectives.Clear();
        activeObjectives.Clear();

        if (data.completedObjectives != null)
        {
            foreach (string objectiveId in data.completedObjectives)
            {
                completedObjectives.Add(objectiveId);
            }
        }

        // Reactivate appropriate objectives
        InitializeObjectives();
    }
} 