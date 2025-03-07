using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class Quest
{
    public string QuestId => questData.questId;
    public QuestData QuestData => questData;
    public bool IsCompleted => objectives.All(o => o.IsCompleted || o.IsOptional);
    public bool HasFailed => IsFailed();
    public float Progress => CalculateProgress();
    public float TimeRemaining => hasTimeLimit ? Mathf.Max(0, endTime - Time.time) : -1;

    private QuestData questData;
    private List<QuestObjectiveInstance> objectives;
    private bool hasTimeLimit;
    private float endTime;
    private Dictionary<string, Vector3> trackedLocations;

    public Quest(QuestData data)
    {
        questData = data;
        objectives = new List<QuestObjectiveInstance>();
        trackedLocations = new Dictionary<string, Vector3>();

        InitializeObjectives();
        
        if (data.hasTimeLimit)
        {
            hasTimeLimit = true;
            endTime = Time.time + data.timeLimitInSeconds;
        }
    }

    public void Update()
    {
        if (hasTimeLimit && Time.time > endTime)
        {
            return; // Quest has failed due to time limit
        }

        foreach (var objective in objectives)
        {
            objective.Update();
        }
    }

    public void UpdateObjectives(ObjectiveType type, string targetId, int amount)
    {
        foreach (var objective in objectives)
        {
            if (objective.Type == type && objective.TargetId == targetId)
            {
                objective.UpdateProgress(amount);
            }
        }
    }

    public void TrackLocation(string objectiveId, Vector3 location)
    {
        trackedLocations[objectiveId] = location;
        
        // Update reach location objectives
        foreach (var objective in objectives)
        {
            if (objective.Type == ObjectiveType.ReachLocation && objective.ObjectiveId == objectiveId)
            {
                float distance = Vector3.Distance(PlayerController.Instance.transform.position, location);
                if (distance <= 3f) // Threshold for reaching location
                {
                    objective.UpdateProgress(1);
                }
            }
        }
    }

    public Vector3? GetTrackedLocation(string objectiveId)
    {
        return trackedLocations.TryGetValue(objectiveId, out Vector3 location) ? location : null;
    }

    private void InitializeObjectives()
    {
        foreach (var objectiveData in questData.objectives)
        {
            objectives.Add(new QuestObjectiveInstance(objectiveData));
        }
    }

    private bool IsFailed()
    {
        if (hasTimeLimit && Time.time > endTime)
            return true;

        // Check if any required objective is impossible to complete
        foreach (var objective in objectives)
        {
            if (!objective.IsOptional && objective.IsFailed)
                return true;
        }

        return false;
    }

    private float CalculateProgress()
    {
        if (objectives.Count == 0) return 0f;

        float totalProgress = 0f;
        int relevantObjectives = 0;

        foreach (var objective in objectives)
        {
            if (!objective.IsOptional)
            {
                totalProgress += objective.Progress;
                relevantObjectives++;
            }
        }

        return relevantObjectives > 0 ? totalProgress / relevantObjectives : 1f;
    }
}

public class QuestObjectiveInstance
{
    public string ObjectiveId => data.objectiveId;
    public ObjectiveType Type => data.type;
    public string TargetId => data.targetId;
    public bool IsOptional => data.optional;
    public bool IsCompleted => currentAmount >= data.requiredAmount;
    public bool IsFailed => false; // Implement failure conditions if needed
    public float Progress => Mathf.Clamp01((float)currentAmount / data.requiredAmount);

    private QuestObjective data;
    private int currentAmount;
    private float lastUpdateTime;

    public QuestObjectiveInstance(QuestObjective objectiveData)
    {
        data = objectiveData;
        currentAmount = 0;
        lastUpdateTime = Time.time;
    }

    public void Update()
    {
        switch (data.type)
        {
            case ObjectiveType.SurviveTime:
                UpdateSurvivalTime();
                break;
            case ObjectiveType.GrowRoots:
                UpdateRootGrowth();
                break;
            // Add other continuous updates as needed
        }
    }

    public void UpdateProgress(int amount)
    {
        currentAmount = Mathf.Min(currentAmount + amount, data.requiredAmount);
        
        if (IsCompleted)
        {
            OnObjectiveCompleted();
        }
    }

    private void UpdateSurvivalTime()
    {
        float deltaTime = Time.time - lastUpdateTime;
        lastUpdateTime = Time.time;
        
        UpdateProgress(Mathf.RoundToInt(deltaTime));
    }

    private void UpdateRootGrowth()
    {
        // Update based on current root length/coverage
        if (RootSystem.Instance != null)
        {
            int currentRootLength = Mathf.RoundToInt(RootSystem.Instance.GetTotalRootLength());
            currentAmount = Mathf.Min(currentRootLength, data.requiredAmount);
        }
    }

    private void OnObjectiveCompleted()
    {
        NotificationSystem.ShowQuest(
            "Objective Completed",
            data.description,
            data.objectiveIcon
        );
    }
}

[System.Serializable]
public class QuestProgress
{
    public string questId;
    public bool isCompleted;
    public bool isFailed;
    public int completionCount;
    public System.DateTime lastCompletionTime;

    public bool IsCompleted => isCompleted;
    public bool IsFailed => isFailed;

    public QuestProgress(string id)
    {
        questId = id;
        isCompleted = false;
        isFailed = false;
        completionCount = 0;
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        isFailed = false;
        completionCount++;
        lastCompletionTime = System.DateTime.Now;
    }

    public void FailQuest()
    {
        isCompleted = false;
        isFailed = true;
    }

    public void AbandonQuest()
    {
        isCompleted = false;
        isFailed = false;
    }
} 