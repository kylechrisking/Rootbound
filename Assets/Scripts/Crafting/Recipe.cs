using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Rootbound/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Basic Info")]
    public string recipeName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Requirements")]
    public int requiredLevel;
    public bool requiresCraftingStation;
    public string craftingStationType; // e.g., "Forge", "Workbench", "Cauldron"
    public ResourceRequirement[] ingredients;
    public float craftingTime = 1f;
    public bool requiresBlueprint = true;
    
    [Header("Output")]
    public CraftingResult[] results;
    public float experienceGained = 10f;
    
    [Header("Special Effects")]
    public GameObject craftingEffectPrefab;
    public AudioClip craftingSound;
    public AudioClip completionSound;
    
    [Header("Categories")]
    public string[] categories; // e.g., "Tools", "Weapons", "Building"
    public int sortOrder;

    public string GetTooltip()
    {
        string tooltip = $"<b>{recipeName}</b>\n{description}\n\n";
        
        if (requiredLevel > 1)
            tooltip += $"Required Level: {requiredLevel}\n";
            
        if (requiresCraftingStation)
            tooltip += $"Crafting Station: {craftingStationType}\n";
            
        tooltip += "\nIngredients:\n";
        foreach (var ingredient in ingredients)
        {
            tooltip += $"- {ingredient.resourceType}: {ingredient.amount}\n";
        }
        
        tooltip += "\nProduces:\n";
        foreach (var result in results)
        {
            tooltip += $"- {result.item.name}";
            if (result.amount > 1)
                tooltip += $" x{result.amount}";
            tooltip += "\n";
        }
        
        return tooltip;
    }
}

[System.Serializable]
public class ResourceRequirement
{
    public ResourceType resourceType;
    public int amount;
}

[System.Serializable]
public class CraftingResult
{
    public Object item; // Can be Tool, Item, or any other craftable object
    public int amount = 1;
    [Range(0f, 1f)]
    public float bonusChance = 0f;
    public int bonusAmount = 1;
} 