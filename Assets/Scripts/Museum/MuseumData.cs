using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Museum Data", menuName = "Rootbound/Museum/Museum Data")]
public class MuseumData : ScriptableObject
{
    [Header("Museum Settings")]
    public Vector2Int museumSize = new Vector2Int(20, 20);
    public MuseumWing[] wings;
    public MuseumMilestone[] milestones;
    public MuseumTheme[] themes;
}

[Serializable]
public class MuseumWing
{
    public string wingId;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite wingIcon;
    public MuseumCategory category;
    public Vector2Int position;
    public Vector2Int size;
    public bool isLocked;
    public string[] unlockConditions;
    public MuseumExhibit[] exhibits;
}

[Serializable]
public class MuseumExhibit
{
    public string exhibitId;
    public string displayName;
    [TextArea(2, 6)]
    public string description;
    public Sprite exhibitIcon;
    public Vector2Int position;
    public Vector2Int size;
    public GameObject exhibitPrefab;
    public ExhibitType type;
    public string[] requiredItems;
    public int[] requiredAmounts;
    public ExhibitReward[] rewards;
    public MuseumThemeBonus[] themeBonuses;
}

[Serializable]
public class ExhibitReward
{
    public RewardType type;
    public string rewardId; // Item ID, resource type, etc.
    public int amount;
    public float bonusChance;
}

[Serializable]
public class MuseumMilestone
{
    public int exhibitsRequired;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite milestoneIcon;
    public ExhibitReward[] rewards;
    public string[] unlockConditions;
}

[Serializable]
public class MuseumTheme
{
    public string themeId;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite themeIcon;
    public Color themeColor;
    public MuseumCategory[] applicableCategories;
    public ThemeRequirement[] requirements;
}

[Serializable]
public class ThemeRequirement
{
    public string exhibitId;
    public int minimumCount;
    public bool isOptional;
}

[Serializable]
public class MuseumThemeBonus
{
    public string themeId;
    public float bonusMultiplier;
}

public enum ExhibitType
{
    Critter,
    Resource,
    Artifact,
    Plant,
    Tool,
    Trophy,
    Decoration
}

public enum RewardType
{
    Item,
    Resource,
    Currency,
    Experience,
    UnlockCondition,
    SkillPoint
}

[CreateAssetMenu(fileName = "Museum Layout", menuName = "Rootbound/Museum/Museum Layout")]
public class MuseumLayout : ScriptableObject
{
    public Vector2Int gridSize;
    public MuseumTile[,] tiles;
    public MuseumPathfinding pathfinding;
}

[Serializable]
public class MuseumTile
{
    public bool isWalkable;
    public bool isExhibitSpace;
    public string occupyingExhibitId;
    public Vector2Int[] occupiedTiles;
    public MuseumCategory category;
    public string themeId;
} 