using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CritterManager : MonoBehaviour
{
    public static CritterManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private CritterData[] availableCritters;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private int maxCrittersPerIsland = 15;
    [SerializeField] private float minDistanceFromPlayer = 10f;
    [SerializeField] private float maxDistanceFromPlayer = 20f;
    
    [Header("Population Control")]
    [SerializeField] private int maxTotalCritters = 50;
    [SerializeField] private float populationCheckInterval = 60f;
    [SerializeField] private AnimationCurve raritySpawnChance;
    
    private Dictionary<int, List<Critter>> crittersPerIsland = new Dictionary<int, List<Critter>>();
    private Dictionary<BiomeType, List<CritterData>> crittersByBiome = new Dictionary<BiomeType, List<CritterData>>();
    private Dictionary<string, int> critterPopulation = new Dictionary<string, int>();
    private float nextSpawnTime;
    private float nextPopulationCheckTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeCritterDatabase();
    }

    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        nextPopulationCheckTime = Time.time + populationCheckInterval;
        
        // Subscribe to events
        GameEvents.OnIslandUnlocked += HandleIslandUnlocked;
        GameEvents.OnCritterCaught += HandleCritterCaught;
    }

    private void OnDestroy()
    {
        GameEvents.OnIslandUnlocked -= HandleIslandUnlocked;
        GameEvents.OnCritterCaught -= HandleCritterCaught;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnCrittersOnAllIslands();
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (Time.time >= nextPopulationCheckTime)
        {
            CheckPopulation();
            nextPopulationCheckTime = Time.time + populationCheckInterval;
        }
    }

    private void InitializeCritterDatabase()
    {
        // Group critters by biome
        foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
        {
            crittersByBiome[biome] = new List<CritterData>();
        }

        foreach (var critter in availableCritters)
        {
            foreach (var biome in critter.validBiomes)
            {
                crittersByBiome[biome].Add(critter);
            }
            critterPopulation[critter.critterId] = 0;
        }
    }

    private void SpawnCrittersOnAllIslands()
    {
        foreach (int islandIndex in IslandManager.Instance.GetUnlockedIslands())
        {
            TrySpawnCritterOnIsland(islandIndex);
        }
    }

    private void TrySpawnCritterOnIsland(int islandIndex)
    {
        if (!crittersPerIsland.TryGetValue(islandIndex, out var islandCritters))
        {
            islandCritters = new List<Critter>();
            crittersPerIsland[islandIndex] = islandCritters;
        }

        // Clean up destroyed critters
        islandCritters.RemoveAll(c => c == null);

        if (islandCritters.Count >= maxCrittersPerIsland)
            return;

        Island island = IslandManager.Instance.GetIsland(islandIndex);
        if (island == null) return;

        // Get valid spawn position
        Vector2 spawnPos = GetValidSpawnPosition(island);
        if (spawnPos == Vector2.zero) return;

        // Select critter type based on biome and conditions
        CritterData critterData = SelectCritterType(island);
        if (critterData == null) return;

        // Check total population
        if (GetTotalCritterCount() >= maxTotalCritters)
            return;

        // Spawn critter
        SpawnCritter(critterData, spawnPos, islandIndex);
    }

    private Vector2 GetValidSpawnPosition(Island island)
    {
        const int MAX_ATTEMPTS = 30;
        Transform player = PlayerController.Instance.transform;
        
        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            Vector2 randomPos = island.GetRandomPoint();
            float distanceToPlayer = Vector2.Distance(randomPos, player.position);
            
            if (distanceToPlayer >= minDistanceFromPlayer && 
                distanceToPlayer <= maxDistanceFromPlayer &&
                !Physics2D.OverlapCircle(randomPos, 1f)) // Check if position is clear
            {
                return randomPos;
            }
        }

        return Vector2.zero;
    }

    private CritterData SelectCritterType(Island island)
    {
        IslandData islandData = IslandManager.Instance.GetIslandData(island.islandIndex);
        if (islandData == null) return null;

        // Get critters for this biome
        var availableCritters = crittersByBiome[islandData.biomeType]
            .Where(c => IsCritterValid(c))
            .ToList();

        if (availableCritters.Count == 0) return null;

        // Apply rarity weights
        float totalWeight = 0f;
        var weights = new List<float>();

        foreach (var critter in availableCritters)
        {
            float rarityMultiplier = raritySpawnChance.Evaluate((int)critter.rarity / 4f);
            float weight = critter.spawnWeight * rarityMultiplier;
            weights.Add(weight);
            totalWeight += weight;
        }

        // Select random critter based on weights
        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < availableCritters.Count; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum)
            {
                return availableCritters[i];
            }
        }

        return availableCritters[0];
    }

    private bool IsCritterValid(CritterData critter)
    {
        // Check time of day
        if (critter.isNocturnal != GameManager.Instance.IsNightTime)
            return false;

        // Check season
        if (!critter.activeSeasons.Contains(GameManager.Instance.CurrentSeason))
            return false;

        // Check spawn time
        float currentHour = GameManager.Instance.CurrentTimeOfDay;
        if (currentHour < critter.spawnTimeRange.x || currentHour > critter.spawnTimeRange.y)
            return false;

        // Check population limit
        if (critterPopulation[critter.critterId] >= GetMaxPopulationForRarity(critter.rarity))
            return false;

        return true;
    }

    private int GetMaxPopulationForRarity(CritterRarity rarity)
    {
        return rarity switch
        {
            CritterRarity.Common => 20,
            CritterRarity.Uncommon => 15,
            CritterRarity.Rare => 10,
            CritterRarity.Epic => 5,
            CritterRarity.Legendary => 2,
            _ => 10
        };
    }

    private void SpawnCritter(CritterData critterData, Vector2 position, int islandIndex)
    {
        GameObject critterObj = new GameObject(critterData.displayName);
        critterObj.transform.position = position;
        
        // Add components
        Critter critter = critterObj.AddComponent<Critter>();
        critterObj.AddComponent<CircleCollider2D>();
        
        // Set up sprite
        SpriteRenderer renderer = critterObj.AddComponent<SpriteRenderer>();
        renderer.sprite = critterData.icon;
        renderer.sortingLayerName = "Critters";
        
        // Initialize critter
        critter.Initialize(critterData);
        
        // Add to tracking
        if (!crittersPerIsland.ContainsKey(islandIndex))
        {
            crittersPerIsland[islandIndex] = new List<Critter>();
        }
        crittersPerIsland[islandIndex].Add(critter);
        
        // Update population count
        critterPopulation[critterData.critterId]++;
    }

    private void CheckPopulation()
    {
        // Reset population counts
        foreach (var critterId in critterPopulation.Keys.ToList())
        {
            critterPopulation[critterId] = 0;
        }

        // Recount all living critters
        foreach (var critters in crittersPerIsland.Values)
        {
            foreach (var critter in critters)
            {
                if (critter != null)
                {
                    critterPopulation[critter.Data.critterId]++;
                }
            }
        }

        // Remove excess critters if needed
        if (GetTotalCritterCount() > maxTotalCritters)
        {
            RemoveExcessCritters();
        }
    }

    private void RemoveExcessCritters()
    {
        var allCritters = crittersPerIsland.Values
            .SelectMany(list => list)
            .Where(c => c != null)
            .OrderBy(c => (int)c.Data.rarity)
            .ToList();

        int excess = GetTotalCritterCount() - maxTotalCritters;
        for (int i = 0; i < excess; i++)
        {
            if (i < allCritters.Count)
            {
                Destroy(allCritters[i].gameObject);
            }
        }
    }

    private void HandleIslandUnlocked(int islandIndex)
    {
        // Initial critter population for new island
        for (int i = 0; i < maxCrittersPerIsland / 2; i++)
        {
            TrySpawnCritterOnIsland(islandIndex);
        }
    }

    private void HandleCritterCaught(string critterId)
    {
        if (critterPopulation.ContainsKey(critterId))
        {
            critterPopulation[critterId]--;
        }
    }

    public List<Critter> GetCrittersOnIsland(int islandIndex)
    {
        return crittersPerIsland.TryGetValue(islandIndex, out var critters) ? 
            new List<Critter>(critters) : new List<Critter>();
    }

    public int GetTotalCritterCount()
    {
        return critterPopulation.Values.Sum();
    }

    public int GetCritterCount(string critterId)
    {
        return critterPopulation.TryGetValue(critterId, out int count) ? count : 0;
    }
} 