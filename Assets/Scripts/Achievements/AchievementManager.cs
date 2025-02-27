using UnityEngine;
using System.Collections.Generic;
using System;

public class AchievementManager : MonoBehaviour
{
    private static AchievementManager _instance;
    public static AchievementManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AchievementManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private AchievementData[] achievements;
    
    private Dictionary<string, Achievement> achievementProgress = new Dictionary<string, Achievement>();
    public event Action<Achievement> OnAchievementUnlocked;
    public event Action<Achievement> OnProgressUpdated;

    [System.Serializable]
    public class Achievement
    {
        public AchievementData data;
        public bool isUnlocked;
        public float currentProgress;
        public DateTime unlockTime;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAchievements()
    {
        foreach (var data in achievements)
        {
            achievementProgress[data.id] = new Achievement
            {
                data = data,
                isUnlocked = false,
                currentProgress = 0f
            };
        }
    }

    public void UpdateProgress(string achievementId, float progress)
    {
        if (!achievementProgress.TryGetValue(achievementId, out Achievement achievement))
            return;

        if (achievement.isUnlocked)
            return;

        achievement.currentProgress = progress;
        OnProgressUpdated?.Invoke(achievement);

        if (achievement.data.hasProgress && 
            achievement.currentProgress >= achievement.data.progressTarget)
        {
            UnlockAchievement(achievementId);
        }
    }

    public void IncrementProgress(string achievementId, float amount = 1f)
    {
        if (achievementProgress.TryGetValue(achievementId, out Achievement achievement))
        {
            UpdateProgress(achievementId, achievement.currentProgress + amount);
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!achievementProgress.TryGetValue(achievementId, out Achievement achievement))
            return;

        if (achievement.isUnlocked)
            return;

        achievement.isUnlocked = true;
        achievement.unlockTime = DateTime.Now;
        
        // Grant rewards
        if (achievement.data.skillPointReward > 0)
        {
            SkillManager.Instance.AddSkillPoints(achievement.data.skillPointReward);
        }

        if (achievement.data.growthBoostReward > 0)
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            playerStats?.AddGrowthRateBonus(achievement.data.growthBoostReward);
        }

        OnAchievementUnlocked?.Invoke(achievement);
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return achievementProgress.TryGetValue(achievementId, out Achievement achievement) 
            && achievement.isUnlocked;
    }

    public float GetProgress(string achievementId)
    {
        if (achievementProgress.TryGetValue(achievementId, out Achievement achievement))
        {
            return achievement.currentProgress;
        }
        return 0f;
    }

    public Achievement[] GetAllAchievements()
    {
        Achievement[] result = new Achievement[achievementProgress.Count];
        achievementProgress.Values.CopyTo(result, 0);
        return result;
    }

    // Save/Load system integration
    public void SaveProgress(GameData data)
    {
        data.achievementStates = new Dictionary<string, bool>();
        data.achievementProgress = new Dictionary<string, float>();
        data.achievementUnlockTimes = new Dictionary<string, DateTime>();

        foreach (var kvp in achievementProgress)
        {
            data.achievementStates[kvp.Key] = kvp.Value.isUnlocked;
            data.achievementProgress[kvp.Key] = kvp.Value.currentProgress;
            if (kvp.Value.unlockTime != default)
            {
                data.achievementUnlockTimes[kvp.Key] = kvp.Value.unlockTime;
            }
        }
    }

    public void LoadProgress(GameData data)
    {
        if (data.achievementStates == null) return;

        foreach (var kvp in data.achievementStates)
        {
            if (achievementProgress.TryGetValue(kvp.Key, out Achievement achievement))
            {
                achievement.isUnlocked = kvp.Value;
                achievement.currentProgress = data.achievementProgress[kvp.Key];
                if (data.achievementUnlockTimes.TryGetValue(kvp.Key, out DateTime unlockTime))
                {
                    achievement.unlockTime = unlockTime;
                }
            }
        }
    }
} 