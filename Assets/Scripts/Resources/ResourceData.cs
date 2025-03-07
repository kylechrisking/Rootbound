using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Rootbound/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("Basic Info")]
    public ResourceType type;
    public string displayName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Resource Properties")]
    public float baseValue = 1f;
    public bool isStackable = true;
    public int maxStackSize = 99;
    public bool isRenewable = true;
    public float renewalTime = 300f; // Time in seconds for resource to respawn
    
    [Header("Collection Settings")]
    public float baseCollectionTime = 1f;
    public float baseCollectionAmount = 1f;
    public bool requiresTool = false;
    public string requiredToolType = "";
    public int minimumToolTier = 0;
    
    [Header("Experience")]
    public float experiencePerCollection = 5f;
    
    [Header("Visual Effects")]
    public GameObject collectionEffectPrefab;
    public GameObject depleteEffectPrefab;
    public GameObject respawnEffectPrefab;
    
    [Header("Sound Effects")]
    public AudioClip collectionSound;
    public AudioClip depleteSound;
    public AudioClip respawnSound;
    
    [Header("Advanced Properties")]
    public bool canBeAutomated = true;
    public bool requiresSpecialConditions = false;
    public string[] specialConditions;
    
    public string GetTooltip()
    {
        string tooltip = $"<b>{displayName}</b>\n{description}\n\n";
        
        if (requiresTool)
        {
            tooltip += $"Requires: {requiredToolType} (Tier {minimumToolTier}+)\n";
        }
        
        if (requiresSpecialConditions && specialConditions != null && specialConditions.Length > 0)
        {
            tooltip += "\nSpecial Conditions:\n";
            foreach (string condition in specialConditions)
            {
                tooltip += $"- {condition}\n";
            }
        }
        
        return tooltip;
    }
} 