using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public float playerGrowthLevel;
    public float[] resourceLevels; // [water, nutrients, sunlight]
    public Vector2 playerPosition;
    public float playTime;
    public DateTime lastSaveTime;
    public int unlockedZones;
    public float currency; // Gold coins

    // Unlocked abilities/skills
    public bool[] unlockedSkills;
    public int skillPoints;

    public bool[] claimedGrowthRewards;
    public Dictionary<ResourceType, float> resourceTotals;
    public Dictionary<ResourceType, int> resourceMilestones;

    public Dictionary<string, bool> achievementStates;
    public Dictionary<string, float> achievementProgress;
    public Dictionary<string, DateTime> achievementUnlockTimes;

    public List<string> completedTutorials;
    public List<string> completedObjectives;

    public GameData()
    {
        // Initialize with default values
        playerGrowthLevel = 1f;
        resourceLevels = new float[3] { 50f, 50f, 50f };
        playerPosition = Vector2.zero;
        playTime = 0f;
        lastSaveTime = DateTime.Now;
        unlockedZones = 1;
        currency = 0f;
        unlockedSkills = new bool[10]; // Adjust size based on total skills
        skillPoints = 0;
    }
} 