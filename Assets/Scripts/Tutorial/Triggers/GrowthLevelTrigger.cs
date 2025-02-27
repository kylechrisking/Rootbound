using UnityEngine;

public class GrowthLevelTrigger : TutorialTriggerBase
{
    [SerializeField] private float targetGrowthLevel;
    private PlayerStats playerStats;

    protected override void Start()
    {
        base.Start();
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        if (playerStats.GetGrowthLevel() >= targetGrowthLevel)
        {
            TriggerTutorial();
        }
    }
} 