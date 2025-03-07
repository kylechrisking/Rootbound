using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Tool Progression", menuName = "Rootbound/Tool Progression")]
public class ToolProgression : ScriptableObject
{
    [System.Serializable]
    public class ToolTier
    {
        public string tierName;
        public int level;
        public Tool toolData;
        public ResourceType[] requiredResources;
        public int[] resourceAmounts;
        public bool requiresPreviousTier = true;
        [TextArea(2,4)]
        public string unlockMessage;
    }

    [System.Serializable]
    public class ToolTypeProgression
    {
        public ToolType toolType;
        public bool unlockedByDefault;
        public int unlockLevel;
        public ToolTier[] tiers;
    }

    public ToolTypeProgression[] toolProgressions;
    
    private Dictionary<ToolType, Dictionary<int, ToolTier>> toolTierMap;

    public void Initialize()
    {
        toolTierMap = new Dictionary<ToolType, Dictionary<int, ToolTier>>();
        
        foreach (var progression in toolProgressions)
        {
            var tierMap = new Dictionary<int, ToolTier>();
            foreach (var tier in progression.tiers)
            {
                tierMap[tier.level] = tier;
            }
            toolTierMap[progression.toolType] = tierMap;
        }
    }

    public bool IsToolTypeUnlocked(ToolType type, int playerLevel)
    {
        foreach (var progression in toolProgressions)
        {
            if (progression.toolType == type)
            {
                return progression.unlockedByDefault || playerLevel >= progression.unlockLevel;
            }
        }
        return false;
    }

    public Tool GetStartingTool(ToolType type)
    {
        foreach (var progression in toolProgressions)
        {
            if (progression.toolType == type && progression.unlockedByDefault && progression.tiers.Length > 0)
            {
                return progression.tiers[0].toolData;
            }
        }
        return null;
    }

    public bool CanUpgradeTool(ToolType type, int currentTier, int playerLevel, Dictionary<ResourceType, int> playerResources)
    {
        if (!toolTierMap.TryGetValue(type, out var tierMap)) return false;
        if (!tierMap.TryGetValue(currentTier + 1, out var nextTier)) return false;

        // Check level requirement
        if (playerLevel < nextTier.level) return false;

        // Check resource requirements
        for (int i = 0; i < nextTier.requiredResources.Length; i++)
        {
            ResourceType resourceType = nextTier.requiredResources[i];
            int requiredAmount = nextTier.resourceAmounts[i];

            if (!playerResources.TryGetValue(resourceType, out int playerAmount) || 
                playerAmount < requiredAmount)
            {
                return false;
            }
        }

        return true;
    }

    public Tool GetNextTierTool(ToolType type, int currentTier)
    {
        if (toolTierMap.TryGetValue(type, out var tierMap) &&
            tierMap.TryGetValue(currentTier + 1, out var nextTier))
        {
            return nextTier.toolData;
        }
        return null;
    }

    public string GetUpgradeRequirements(ToolType type, int currentTier)
    {
        if (!toolTierMap.TryGetValue(type, out var tierMap) ||
            !tierMap.TryGetValue(currentTier + 1, out var nextTier))
        {
            return "Max tier reached";
        }

        string requirements = $"Requirements for {nextTier.tierName}:\n";
        requirements += $"Level: {nextTier.level}\n\n";
        requirements += "Resources needed:\n";

        for (int i = 0; i < nextTier.requiredResources.Length; i++)
        {
            ResourceType resourceType = nextTier.requiredResources[i];
            int amount = nextTier.resourceAmounts[i];
            requirements += $"- {resourceType}: {amount}\n";
        }

        return requirements;
    }
} 