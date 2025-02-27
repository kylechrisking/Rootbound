using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Growth Stats")]
    [SerializeField] private float maxWaterLevel = 100f;
    [SerializeField] private float maxNutrientLevel = 100f;
    [SerializeField] private float maxSunlightLevel = 100f;
    
    [Header("Current Levels")]
    [SerializeField] private float currentWaterLevel;
    [SerializeField] private float currentNutrientLevel;
    [SerializeField] private float currentSunlightLevel;
    
    [Header("Consumption Rates")]
    [SerializeField] private float waterConsumptionRate = 1f;
    [SerializeField] private float nutrientConsumptionRate = 0.5f;
    [SerializeField] private float sunlightConsumptionRate = 0.75f;

    [Header("Growth Settings")]
    [SerializeField] private float baseGrowthRate = 1f;
    [SerializeField] private float currentGrowthLevel = 1f;
    [SerializeField] private float maxGrowthLevel = 10f;

    [Header("Skill Bonuses")]
    private float resourceMultiplier = 1f;
    private float growthRateBonus = 0f;
    private bool hasWaterStorage = false;
    private bool hasNutrientStorage = false;

    private float lastGrowthCheck = 0f;
    private float growthCheckTime = 1f;

    private void Start()
    {
        // Start with 50% resources
        currentWaterLevel = maxWaterLevel * 0.5f;
        currentNutrientLevel = maxNutrientLevel * 0.5f;
        currentSunlightLevel = maxSunlightLevel * 0.5f;
    }

    private void Update()
    {
        ConsumeResources();
        UpdateGrowth();
        CheckAchievements();
    }

    private void ConsumeResources()
    {
        currentWaterLevel -= waterConsumptionRate * Time.deltaTime;
        currentNutrientLevel -= nutrientConsumptionRate * Time.deltaTime;
        currentSunlightLevel -= sunlightConsumptionRate * Time.deltaTime;

        // Clamp values
        currentWaterLevel = Mathf.Max(0, currentWaterLevel);
        currentNutrientLevel = Mathf.Max(0, currentNutrientLevel);
        currentSunlightLevel = Mathf.Max(0, currentSunlightLevel);
    }

    private void UpdateGrowth()
    {
        if (currentWaterLevel > 0 && currentNutrientLevel > 0 && currentSunlightLevel > 0)
        {
            float growthMultiplier = (currentWaterLevel / maxWaterLevel + 
                                    currentNutrientLevel / maxNutrientLevel + 
                                    currentSunlightLevel / maxSunlightLevel) / 3f;
            
            currentGrowthLevel += (baseGrowthRate + growthRateBonus) * growthMultiplier * Time.deltaTime;
            currentGrowthLevel = Mathf.Min(currentGrowthLevel, maxGrowthLevel);
        }
    }

    public void AddResource(ResourceType type, float amount)
    {
        amount *= resourceMultiplier;
        switch (type)
        {
            case ResourceType.Water:
                currentWaterLevel = Mathf.Min(currentWaterLevel + amount, maxWaterLevel);
                break;
            case ResourceType.Nutrients:
                currentNutrientLevel = Mathf.Min(currentNutrientLevel + amount, maxNutrientLevel);
                break;
            case ResourceType.Sunlight:
                currentSunlightLevel = Mathf.Min(currentSunlightLevel + amount, maxSunlightLevel);
                break;
        }
    }

    public float GetGrowthLevel()
    {
        return currentGrowthLevel;
    }

    public bool HasEnoughResources(ResourceType type, float amount)
    {
        switch (type)
        {
            case ResourceType.Water:
                return currentWaterLevel >= amount;
            case ResourceType.Nutrients:
                return currentNutrientLevel >= amount;
            case ResourceType.Sunlight:
                return currentSunlightLevel >= amount;
            default:
                return false;
        }
    }

    public bool ConsumeResource(ResourceType type, float amount)
    {
        if (!HasEnoughResources(type, amount)) return false;

        switch (type)
        {
            case ResourceType.Water:
                currentWaterLevel -= amount;
                break;
            case ResourceType.Nutrients:
                currentNutrientLevel -= amount;
                break;
            case ResourceType.Sunlight:
                currentSunlightLevel -= amount;
                break;
        }
        return true;
    }

    public float GetWaterLevel() => currentWaterLevel;
    public float GetNutrientLevel() => currentNutrientLevel;
    public float GetSunlightLevel() => currentSunlightLevel;

    public void SetResourceLevels(float[] levels)
    {
        if (levels.Length >= 3)
        {
            currentWaterLevel = levels[0];
            currentNutrientLevel = levels[1];
            currentSunlightLevel = levels[2];
        }
    }

    public void SetGrowthLevel(float level)
    {
        currentGrowthLevel = Mathf.Clamp(level, 1f, maxGrowthLevel);
    }

    public void AddResourceMultiplier(float multiplier)
    {
        resourceMultiplier *= multiplier;
    }

    public void AddGrowthRateBonus(float bonus)
    {
        growthRateBonus += bonus;
    }

    public void EnableWaterStorage()
    {
        hasWaterStorage = true;
        // Implement storage mechanics
    }

    public void EnableNutrientStorage()
    {
        hasNutrientStorage = true;
        // Implement storage mechanics
    }

    private void CheckAchievements()
    {
        // Check less frequently to optimize performance
        if (Time.time - lastGrowthCheck < growthCheckTime) return;
        lastGrowthCheck = Time.time;

        // Growth achievements
        AchievementManager.Instance.UpdateProgress("FirstSprout", currentGrowthLevel);
        AchievementManager.Instance.UpdateProgress("MightyOak", currentGrowthLevel);

        // Resource achievements
        if (currentWaterLevel >= maxWaterLevel)
        {
            AchievementManager.Instance.IncrementProgress("WaterHoarder");
        }
        if (currentNutrientLevel >= maxNutrientLevel)
        {
            AchievementManager.Instance.IncrementProgress("NutrientMaster");
        }
        if (currentSunlightLevel >= maxSunlightLevel)
        {
            AchievementManager.Instance.IncrementProgress("SunSeeker");
        }
    }
} 