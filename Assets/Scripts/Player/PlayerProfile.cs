using UnityEngine;
using System;

[Serializable]
public class PlayerProfile
{
    public string username;
    public int level = 1;
    public float currentXP;
    public float xpToNextLevel;
    public int playerIconIndex;
    public DateTime createdDate;
    public float totalPlayTime;
    public int totalIslandsUnlocked;
    public int totalResourcesCollected;
    public int totalCrittersDiscovered;
    public int totalQuestsCompleted;

    // Calculated XP requirements
    private const float BASE_XP_REQUIREMENT = 100f;
    private const float XP_SCALING_FACTOR = 1.5f;
    private const int MAX_LEVEL = 100;

    public static PlayerProfile CreateNew(string username)
    {
        return new PlayerProfile
        {
            username = username,
            level = 1,
            currentXP = 0,
            xpToNextLevel = BASE_XP_REQUIREMENT,
            playerIconIndex = 0,
            createdDate = DateTime.Now,
            totalPlayTime = 0,
            totalIslandsUnlocked = 1,
            totalResourcesCollected = 0,
            totalCrittersDiscovered = 0,
            totalQuestsCompleted = 0
        };
    }

    public void AddExperience(float xpAmount)
    {
        if (level >= MAX_LEVEL) return;

        currentXP += xpAmount;
        while (currentXP >= xpToNextLevel && level < MAX_LEVEL)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        // Cap XP at next level requirement
        if (currentXP > xpToNextLevel)
            currentXP = xpToNextLevel;
    }

    private void LevelUp()
    {
        level++;
        
        // Calculate new XP requirement
        xpToNextLevel = BASE_XP_REQUIREMENT * Mathf.Pow(XP_SCALING_FACTOR, level - 1);
        
        // Notify level up
        GameEvents.OnPlayerLevelUp?.Invoke(level);
        
        // Show level up notification
        NotificationSystem.ShowGeneral(
            "Level Up!",
            $"You've reached level {level}!",
            ResourceManager.GetIcon("level_up")
        );

        // Grant rewards
        GrantLevelUpRewards();
    }

    private void GrantLevelUpRewards()
    {
        // Grant skill points
        SkillManager.Instance.AddSkillPoints(2);
        
        // Grant bonus resources
        PlayerStats.Instance.AddResource(ResourceType.Gold, 100 * level);
        
        // Unlock new content based on level
        GameProgressionManager.Instance.CheckLevelBasedUnlocks(level);
    }

    public float GetLevelProgress()
    {
        return currentXP / xpToNextLevel;
    }

    // Save/Load methods
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static PlayerProfile FromJson(string json)
    {
        return JsonUtility.FromJson<PlayerProfile>(json);
    }
} 