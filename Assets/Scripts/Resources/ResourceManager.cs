using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resource Data")]
    [SerializeField] private ResourceData[] resourceDataList;
    [SerializeField] private GameObject resourceNodePrefab;
    
    [Header("Spawning Settings")]
    [SerializeField] private float baseSpawnInterval = 300f; // 5 minutes
    [SerializeField] private float spawnIntervalVariance = 60f; // 1 minute
    [SerializeField] private int maxResourcesPerIsland = 20;
    [SerializeField] private float minDistanceBetweenResources = 2f;
    
    [Header("Regeneration Settings")]
    [SerializeField] private float regenerationCheckInterval = 30f;
    [SerializeField] private int resourcesPerRegeneration = 3;
    [SerializeField] private float regenerationRadius = 10f;
    
    private Dictionary<ResourceType, ResourceData> resourceDataMap;
    private Dictionary<ResourceType, List<ResourceNode>> activeResourceNodes;
    private Dictionary<int, List<ResourceNode>> resourcesByIsland;
    private float nextRegenerationCheck;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeResourceData();
    }

    private void Start()
    {
        // Subscribe to events
        GameEvents.OnIslandUnlocked += HandleIslandUnlocked;
        GameEvents.OnResourceDepleted += HandleResourceDepleted;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnIslandUnlocked -= HandleIslandUnlocked;
        GameEvents.OnResourceDepleted -= HandleResourceDepleted;
    }

    private void InitializeResourceData()
    {
        resourceDataMap = new Dictionary<ResourceType, ResourceData>();
        activeResourceNodes = new Dictionary<ResourceType, List<ResourceNode>>();
        resourcesByIsland = new Dictionary<int, List<ResourceNode>>();

        foreach (ResourceData data in resourceDataList)
        {
            resourceDataMap[data.type] = data;
            activeResourceNodes[data.type] = new List<ResourceNode>();
        }
    }

    public ResourceData GetResourceData(ResourceType type)
    {
        return resourceDataMap.TryGetValue(type, out ResourceData data) ? data : null;
    }

    public void SpawnResourcesOnIsland(int islandIndex, IslandData islandData, int count)
    {
        if (!resourcesByIsland.ContainsKey(islandIndex))
        {
            resourcesByIsland[islandIndex] = new List<ResourceNode>();
        }

        List<ResourceType> availableResources = DetermineAvailableResources(islandData);
        if (availableResources.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition(islandData.bounds, islandIndex);
            if (spawnPos != Vector2.zero)
            {
                ResourceType resourceType = availableResources[Random.Range(0, availableResources.Count)];
                SpawnResourceNode(resourceType, spawnPos, islandIndex);
            }
        }
    }

    private List<ResourceType> DetermineAvailableResources(IslandData islandData)
    {
        List<ResourceType> available = new List<ResourceType>();
        
        foreach (ResourceType type in islandData.allowedResources)
        {
            if (CanSpawnResource(type, islandData))
            {
                available.Add(type);
            }
        }
        
        return available;
    }

    private bool CanSpawnResource(ResourceType type, IslandData islandData)
    {
        ResourceData data = GetResourceData(type);
        if (data == null) return false;

        // Check if resource requires special conditions
        if (data.requiresSpecialConditions)
        {
            foreach (string condition in data.specialConditions)
            {
                if (!islandData.HasCondition(condition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Vector2 GetValidSpawnPosition(Bounds islandBounds, int islandIndex)
    {
        const int MAX_ATTEMPTS = 30;
        
        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(islandBounds.min.x, islandBounds.max.x),
                Random.Range(islandBounds.min.y, islandBounds.max.y)
            );

            // Check if position is valid (not too close to other resources, not on water, etc.)
            if (IsValidSpawnPosition(randomPos, islandIndex))
            {
                return randomPos;
            }
        }

        return Vector2.zero;
    }

    private bool IsValidSpawnPosition(Vector2 position, int islandIndex)
    {
        // Check distance from other resources
        if (resourcesByIsland.TryGetValue(islandIndex, out List<ResourceNode> islandResources))
        {
            foreach (ResourceNode node in islandResources)
            {
                if (Vector2.Distance(position, node.transform.position) < minDistanceBetweenResources)
                {
                    return false;
                }
            }
        }

        // Check if position is on valid ground
        // This would use your game's terrain/tile system
        return true; // Placeholder
    }

    private void SpawnResourceNode(ResourceType type, Vector2 position, int islandIndex)
    {
        ResourceData data = GetResourceData(type);
        if (data == null) return;

        GameObject nodeObj = Instantiate(resourceNodePrefab, position, Quaternion.identity);
        ResourceNode node = nodeObj.GetComponent<ResourceNode>();
        
        if (node != null)
        {
            node.Initialize(data, islandIndex);
            activeResourceNodes[type].Add(node);
            resourcesByIsland[islandIndex].Add(node);
        }
    }

    private void HandleIslandUnlocked(int islandIndex)
    {
        // Get island data and spawn resources
        IslandData islandData = IslandManager.Instance.GetIslandData(islandIndex);
        if (islandData != null)
        {
            SpawnResourcesOnIsland(islandIndex, islandData, maxResourcesPerIsland);
        }
    }

    private void HandleResourceDepleted(ResourceType type, float amount)
    {
        // Resource depletion is now handled by individual nodes
        // They will destroy themselves when depleted if non-renewable
        // This method is kept for event handling and potential future use
    }

    public static Sprite GetIcon(string iconName)
    {
        // Load icon from resources folder
        return Resources.Load<Sprite>($"Icons/{iconName}");
    }

    public static bool TryGetPlayerIcon(int iconIndex, out Sprite icon)
    {
        icon = Resources.Load<Sprite>($"Icons/PlayerIcons/icon_{iconIndex}");
        return icon != null;
    }

    public void RemoveResourceNode(ResourceNode node, int islandIndex)
    {
        if (resourcesByIsland.TryGetValue(islandIndex, out var islandResources))
        {
            islandResources.Remove(node);
        }

        if (activeResourceNodes.TryGetValue(node.ResourceType, out var typeResources))
        {
            typeResources.Remove(node);
        }
    }

    public List<ResourceNode> GetNearbyResources(Vector2 position, float radius)
    {
        List<ResourceNode> nearbyResources = new List<ResourceNode>();
        
        foreach (var resourceList in activeResourceNodes.Values)
        {
            foreach (var node in resourceList)
            {
                if (Vector2.Distance(position, node.transform.position) <= radius)
                {
                    nearbyResources.Add(node);
                }
            }
        }
        
        return nearbyResources;
    }

    private void Update()
    {
        // Check for resource regeneration
        if (Time.time >= nextRegenerationCheck)
        {
            RegenerateResources();
            nextRegenerationCheck = Time.time + regenerationCheckInterval;
        }
    }

    private void RegenerateResources()
    {
        foreach (int islandIndex in IslandManager.Instance.GetUnlockedIslands())
        {
            Island island = IslandManager.Instance.GetIsland(islandIndex);
            if (island == null) continue;

            IslandData islandData = IslandManager.Instance.GetIslandData(islandIndex);
            if (islandData == null) continue;

            // Get current resource count
            int currentCount = GetResourceCountOnIsland(islandIndex);
            if (currentCount >= maxResourcesPerIsland) continue;

            // Calculate how many resources to spawn
            int spawnCount = Mathf.Min(resourcesPerRegeneration, maxResourcesPerIsland - currentCount);
            
            // Spawn new resources
            SpawnResourcesOnIsland(islandIndex, islandData, spawnCount);
        }
    }

    public int GetResourceCountOnIsland(int islandIndex)
    {
        return resourcesByIsland.TryGetValue(islandIndex, out var resources) ? resources.Count : 0;
    }
} 