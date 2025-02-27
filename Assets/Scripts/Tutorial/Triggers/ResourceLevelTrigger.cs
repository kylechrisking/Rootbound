using UnityEngine;

public class ResourceLevelTrigger : TutorialTriggerBase
{
    [Header("Resource Settings")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private float triggerLevel;
    [SerializeField] private bool triggerOnLow = true;

    private PlayerStats playerStats;

    protected override void Start()
    {
        base.Start();
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        float currentLevel = resourceType switch
        {
            ResourceType.Water => playerStats.GetWaterLevel(),
            ResourceType.Nutrients => playerStats.GetNutrientLevel(),
            ResourceType.Sunlight => playerStats.GetSunlightLevel(),
            _ => 0f
        };

        bool shouldTrigger = triggerOnLow ? 
            currentLevel <= triggerLevel : 
            currentLevel >= triggerLevel;

        if (shouldTrigger)
        {
            TriggerTutorial();
        }
    }
} 