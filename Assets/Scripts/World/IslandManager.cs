using UnityEngine;
using System.Collections.Generic;

public class IslandManager : MonoBehaviour
{
    public static IslandManager Instance { get; private set; }

    [Header("Island Generation")]
    [SerializeField] private GameObject islandPrefab;
    [SerializeField] private float islandSpacing = 20f;
    [SerializeField] private int initialIslandCount = 1;
    [SerializeField] private IslandData[] islandTypes;
    
    [Header("Resource Generation")]
    [SerializeField] private float resourceRegenerationInterval = 300f; // 5 minutes
    [SerializeField] private int maxResourcesPerIsland = 15;
    [SerializeField] private int minResourcesPerSpawn = 2;
    [SerializeField] private int maxResourcesPerSpawn = 5;
    
    private Dictionary<int, Island> activeIslands = new Dictionary<int, Island>();
    private Dictionary<int, IslandData> islandDataMap = new Dictionary<int, IslandData>();
    private List<int> unlockedIslands = new List<int>();
    private float nextRegenerationTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeIslandTypes();
    }

    private void Start()
    {
        // Generate initial islands
        for (int i = 0; i < initialIslandCount; i++)
        {
            GenerateIsland(i);
        }

        nextRegenerationTime = Time.time + resourceRegenerationInterval;
    }

    private void Update()
    {
        // Handle resource regeneration
        if (Time.time >= nextRegenerationTime)
        {
            RegenerateResources();
            nextRegenerationTime = Time.time + resourceRegenerationInterval;
        }
    }

    private void InitializeIslandTypes()
    {
        foreach (var islandType in islandTypes)
        {
            islandDataMap[islandType.islandIndex] = islandType;
        }
    }

    private void GenerateIsland(int index)
    {
        // Calculate island position based on index
        Vector2 position = CalculateIslandPosition(index);
        
        // Get island data
        IslandData data = GetIslandData(index);
        if (data == null) return;

        // Instantiate island
        GameObject islandObj = Instantiate(islandPrefab, position, Quaternion.identity);
        Island island = islandObj.GetComponent<Island>();
        
        if (island != null)
        {
            island.Initialize(index, data);
            activeIslands[index] = island;
            
            // If this is the first island or it should be unlocked by default
            if (index == 0 || data.unlockedByDefault)
            {
                UnlockIsland(index);
            }
        }
    }

    private Vector2 CalculateIslandPosition(int index)
    {
        // Create a spiral pattern for island placement
        float angle = index * 137.5f * Mathf.Deg2Rad; // Golden angle
        float radius = Mathf.Sqrt(index) * islandSpacing;
        
        return new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
    }

    public bool UnlockIsland(int index)
    {
        if (unlockedIslands.Contains(index)) return false;

        IslandData data = GetIslandData(index);
        if (data == null) return false;

        // Check unlock requirements
        if (!data.unlockedByDefault)
        {
            PlayerProfile profile = GameManager.Instance.PlayerProfile;
            
            if (profile.level < data.requiredLevel) return false;
            if (profile.totalResourcesCollected < data.requiredResourcesCollected) return false;
            
            // Check if player has required resources
            for (int i = 0; i < data.unlockResourceTypes.Length; i++)
            {
                ResourceType type = data.unlockResourceTypes[i];
                int required = data.unlockResourceAmounts[i];
                
                if (!PlayerInventory.Instance.HasResource(type, required))
                {
                    return false;
                }
            }

            // Consume resources
            for (int i = 0; i < data.unlockResourceTypes.Length; i++)
            {
                PlayerInventory.Instance.RemoveResource(
                    data.unlockResourceTypes[i],
                    data.unlockResourceAmounts[i]
                );
            }
        }

        // Unlock the island
        unlockedIslands.Add(index);
        
        // Generate neighboring islands
        GenerateNeighboringIslands(index);
        
        // Trigger events
        GameEvents.OnIslandUnlocked?.Invoke(index);
        
        // Show notification
        NotificationSystem.ShowGeneral(
            "New Island Unlocked!",
            $"You've unlocked {data.displayName}!",
            data.icon
        );

        return true;
    }

    private void GenerateNeighboringIslands(int unlockedIndex)
    {
        // Generate the next few islands that aren't yet generated
        for (int i = 1; i <= 3; i++)
        {
            int neighborIndex = unlockedIndex + i;
            if (!activeIslands.ContainsKey(neighborIndex))
            {
                GenerateIsland(neighborIndex);
            }
        }
    }

    private void RegenerateResources()
    {
        foreach (int islandIndex in unlockedIslands)
        {
            if (activeIslands.TryGetValue(islandIndex, out Island island))
            {
                // Count current resources
                int currentResources = ResourceManager.Instance.GetResourceCountOnIsland(islandIndex);
                if (currentResources >= maxResourcesPerIsland) continue;

                // Calculate how many resources to spawn
                int availableSlots = maxResourcesPerIsland - currentResources;
                int spawnCount = Mathf.Min(
                    Random.Range(minResourcesPerSpawn, maxResourcesPerSpawn + 1),
                    availableSlots
                );

                // Spawn new resources
                IslandData data = GetIslandData(islandIndex);
                if (data != null)
                {
                    ResourceManager.Instance.SpawnResourcesOnIsland(islandIndex, data, spawnCount);
                }
            }
        }
    }

    public IslandData GetIslandData(int index)
    {
        return islandDataMap.TryGetValue(index, out IslandData data) ? data : null;
    }

    public bool IsIslandUnlocked(int index)
    {
        return unlockedIslands.Contains(index);
    }

    public List<int> GetUnlockedIslands()
    {
        return new List<int>(unlockedIslands);
    }

    public Island GetIsland(int index)
    {
        return activeIslands.TryGetValue(index, out Island island) ? island : null;
    }
} 