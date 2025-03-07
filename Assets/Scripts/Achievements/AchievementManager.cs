using UnityEngine;
using System.Collections.Generic;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<Achievement> achievements = new List<Achievement>();
    private Dictionary<string, Achievement> achievementMap = new Dictionary<string, Achievement>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize achievement map
        foreach (var achievement in achievements)
        {
            achievementMap[achievement.id] = achievement;
        }

        // Load saved achievement progress
        LoadAchievements();
    }

    public void UpdateProgress(string achievementId, float progress)
    {
        if (achievementMap.TryGetValue(achievementId, out Achievement achievement))
        {
            if (!achievement.isLocked && !achievement.isCompleted)
            {
                achievement.UpdateProgress(progress);
                SaveAchievements();
            }
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        if (achievementMap.TryGetValue(achievementId, out Achievement achievement))
        {
            if (achievement.isLocked)
            {
                achievement.Unlock();
                SaveAchievements();
            }
        }
    }

    public Achievement GetAchievement(string achievementId)
    {
        return achievementMap.TryGetValue(achievementId, out Achievement achievement) ? achievement : null;
    }

    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements);
    }

    public float GetCompletionPercentage()
    {
        if (achievements.Count == 0) return 0f;

        int completed = 0;
        foreach (var achievement in achievements)
        {
            if (achievement.isCompleted) completed++;
        }

        return (float)completed / achievements.Count * 100f;
    }

    private void SaveAchievements()
    {
        // Create achievement save data
        var saveData = new Dictionary<string, AchievementSaveData>();
        foreach (var achievement in achievements)
        {
            saveData[achievement.id] = new AchievementSaveData
            {
                isCompleted = achievement.isCompleted,
                isLocked = achievement.isLocked,
                progress = achievement.progress
            };
        }

        // Save to PlayerPrefs as JSON
        string json = JsonUtility.ToJson(new AchievementSaveDataWrapper { achievements = saveData });
        PlayerPrefs.SetString("Achievements", json);
        PlayerPrefs.Save();
    }

    private void LoadAchievements()
    {
        string json = PlayerPrefs.GetString("Achievements", "");
        if (string.IsNullOrEmpty(json)) return;

        var saveData = JsonUtility.FromJson<AchievementSaveDataWrapper>(json);
        if (saveData?.achievements == null) return;

        foreach (var kvp in saveData.achievements)
        {
            if (achievementMap.TryGetValue(kvp.Key, out Achievement achievement))
            {
                achievement.isCompleted = kvp.Value.isCompleted;
                achievement.isLocked = kvp.Value.isLocked;
                achievement.progress = kvp.Value.progress;
            }
        }
    }

    [System.Serializable]
    private class AchievementSaveData
    {
        public bool isCompleted;
        public bool isLocked;
        public float progress;
    }

    [System.Serializable]
    private class AchievementSaveDataWrapper
    {
        public Dictionary<string, AchievementSaveData> achievements;
    }
} 