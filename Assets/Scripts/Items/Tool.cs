using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Rootbound/Tool")]
public class Tool : ScriptableObject
{
    [Header("Basic Info")]
    public string toolName;
    public string toolType; // e.g., "Axe", "Pickaxe", "Shovel"
    public int tier;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Tool Properties")]
    public float collectionSpeedMultiplier = 1f;
    public float resourceBonusMultiplier = 1f;
    public float durability = 100f;
    public bool canBreak = true;
    
    [Header("Crafting")]
    public bool canBeCrafted = true;
    public CraftingRequirement[] craftingRequirements;
    
    [Header("Special Effects")]
    public bool hasSpecialEffect;
    public string specialEffectDescription;
    public GameObject useEffectPrefab;
    public AudioClip useSound;
    
    public string GetTooltip()
    {
        string tooltip = $"<b>{toolName}</b> (Tier {tier})\n{description}\n\n";
        tooltip += $"Speed: {collectionSpeedMultiplier}x\n";
        tooltip += $"Resource Bonus: +{(resourceBonusMultiplier - 1f) * 100}%\n";
        
        if (hasSpecialEffect)
        {
            tooltip += $"\nSpecial Effect: {specialEffectDescription}";
        }
        
        return tooltip;
    }
}

[System.Serializable]
public class CraftingRequirement
{
    public ResourceType resourceType;
    public int amount;
} 