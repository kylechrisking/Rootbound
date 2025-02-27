using UnityEngine;
using System.Collections.Generic;

public class SkillPointRewards : MonoBehaviour
{
    [System.Serializable]
    public class GrowthLevelReward
    {
        public float growthLevel;
        public int skillPoints;
        public bool claimed;
    }

    [Header("Growth Level Rewards")]
    [SerializeField] private GrowthLevelReward[] growthRewards;
    
    [Header("Resource Milestones")]
    [SerializeField] private int pointsPerResourceMilestone = 1;
    [SerializeField] private float resourceMilestoneInterval = 1000f;

    private Dictionary<ResourceType, float> resourceTotals = new Dictionary<ResourceType, float>();
    private Dictionary<ResourceType, int> milestonesReached = new Dictionary<ResourceType, int>();

    private void Start()
    {
        InitializeResourceTracking();
    }

    private void InitializeResourceTracking()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resourceTotals[type] = 0f;
            milestonesReached[type] = 0;
        }
    }

    public void CheckGrowthRewards(float currentGrowthLevel)
    {
        foreach (var reward in growthRewards)
        {
            if (!reward.claimed && currentGrowthLevel >= reward.growthLevel)
            {
                AwardSkillPoints(reward.skillPoints);
                reward.claimed = true;
                
                ShowRewardNotification($"Growth Level {reward.growthLevel} Reached!", 
                    $"+{reward.skillPoints} Skill Points");
            }
        }
    }

    public void TrackResourceGathered(ResourceType type, float amount)
    {
        resourceTotals[type] += amount;
        
        int newMilestones = Mathf.FloorToInt(resourceTotals[type] / resourceMilestoneInterval);
        
        if (newMilestones > milestonesReached[type])
        {
            int additionalMilestones = newMilestones - milestonesReached[type];
            int points = additionalMilestones * pointsPerResourceMilestone;
            
            AwardSkillPoints(points);
            milestonesReached[type] = newMilestones;

            ShowRewardNotification($"{type} Milestone Reached!", 
                $"+{points} Skill Points");
        }
    }

    private void AwardSkillPoints(int amount)
    {
        SkillManager.Instance.AddSkillPoints(amount);
    }

    private void ShowRewardNotification(string title, string description)
    {
        // You can implement this using the UI notification system
        Debug.Log($"{title}: {description}");
    }

    // Save/Load system integration
    public void SaveProgress(GameData data)
    {
        // Save claimed rewards and resource totals
        data.claimedGrowthRewards = new bool[growthRewards.Length];
        for (int i = 0; i < growthRewards.Length; i++)
        {
            data.claimedGrowthRewards[i] = growthRewards[i].claimed;
        }

        data.resourceTotals = new Dictionary<ResourceType, float>(resourceTotals);
        data.resourceMilestones = new Dictionary<ResourceType, int>(milestonesReached);
    }

    public void LoadProgress(GameData data)
    {
        if (data.claimedGrowthRewards != null)
        {
            for (int i = 0; i < data.claimedGrowthRewards.Length; i++)
            {
                if (i < growthRewards.Length)
                {
                    growthRewards[i].claimed = data.claimedGrowthRewards[i];
                }
            }
        }

        if (data.resourceTotals != null)
        {
            resourceTotals = new Dictionary<ResourceType, float>(data.resourceTotals);
            milestonesReached = new Dictionary<ResourceType, int>(data.resourceMilestones);
        }
    }
} 