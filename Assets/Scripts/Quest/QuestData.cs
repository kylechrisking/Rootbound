using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Quest", menuName = "Rootbound/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questId;
    public string displayName;
    [TextArea(3, 10)]
    public string description;
    public Sprite questIcon;
    public QuestType questType;
    public bool isRepeatable;
    
    [Header("Requirements")]
    public int levelRequirement;
    public string[] prerequisiteQuestIds;
    public string[] requiredConditions;
    
    [Header("Objectives")]
    public QuestObjective[] objectives;
    
    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;
    public QuestRewardItem[] itemRewards;
    public string[] unlockConditions;
    
    [Header("Time Constraints")]
    public bool hasTimeLimit;
    public float timeLimitInSeconds;
    public bool isAvailableOnlyAtNight;
    public bool isSeasonalQuest;
    public Season[] availableSeasons;
}

[Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public ObjectiveType type;
    public string targetId; // Resource type, enemy type, item id, etc.
    public int requiredAmount;
    public bool optional;
    public Sprite objectiveIcon;
    [TextArea(2, 5)]
    public string hint;
}

[Serializable]
public class QuestRewardItem
{
    public GameObject itemPrefab;
    public int amount;
    public float bonusChance;
}

public enum QuestType
{
    MainStory,
    SideQuest,
    Daily,
    Weekly,
    Achievement,
    Hidden
}

public enum ObjectiveType
{
    CollectResource,
    DefeatEnemy,
    ReachLocation,
    CraftItem,
    GrowRoots,
    UnlockIsland,
    CompleteResearch,
    DiscoverSpecies,
    SurviveTime,
    ReachLevel,
    CompleteQuests
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
} 