using UnityEngine;

[CreateAssetMenu(fileName = "New Island", menuName = "Rootbound/Island Data")]
public class IslandData : ScriptableObject
{
    [Header("Basic Info")]
    public int islandIndex;
    public string displayName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Unlock Requirements")]
    public bool unlockedByDefault;
    public int requiredLevel;
    public int requiredResourcesCollected;
    public ResourceType[] unlockResourceTypes;
    public int[] unlockResourceAmounts;
    
    [Header("Island Properties")]
    public Vector2 size = new Vector2(20f, 20f);
    public int minResources = 5;
    public int maxResources = 15;
    public ResourceType[] allowedResources;
    public string[] specialConditions;
    
    [Header("Visual Settings")]
    public Color groundTint = Color.white;
    public Sprite[] groundTileVariants;
    public GameObject[] decorativePrefabs;
    public int minDecorations = 3;
    public int maxDecorations = 8;
    
    [Header("Gameplay Settings")]
    public bool allowsCombat = true;
    public bool allowsBuilding = true;
    public bool allowsFarming = true;
    public float resourceRegenerationMultiplier = 1f;
    
    public Bounds bounds
    {
        get
        {
            return new Bounds(Vector3.zero, new Vector3(size.x, size.y, 1f));
        }
    }

    public bool HasCondition(string condition)
    {
        if (specialConditions == null) return false;
        return System.Array.IndexOf(specialConditions, condition) != -1;
    }

    public string GetUnlockRequirementsText()
    {
        string text = $"Requirements to unlock {displayName}:\n\n";
        
        if (requiredLevel > 1)
            text += $"Level: {requiredLevel}\n";
            
        if (requiredResourcesCollected > 0)
            text += $"Total Resources Collected: {requiredResourcesCollected}\n";
            
        if (unlockResourceTypes != null && unlockResourceTypes.Length > 0)
        {
            text += "\nRequired Resources:\n";
            for (int i = 0; i < unlockResourceTypes.Length; i++)
            {
                text += $"- {unlockResourceTypes[i]}: {unlockResourceAmounts[i]}\n";
            }
        }
        
        return text;
    }
} 