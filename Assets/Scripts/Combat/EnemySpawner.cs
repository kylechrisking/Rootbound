using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private EnemyData[] enemyTypes;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private int maxEnemiesPerIsland = 10;
    [SerializeField] private float minDistanceFromPlayer = 10f;
    [SerializeField] private float maxDistanceFromPlayer = 20f;
    
    [Header("Wave Settings")]
    [SerializeField] private float waveCooldown = 300f; // 5 minutes
    [SerializeField] private int baseWaveSize = 5;
    [SerializeField] private float waveSizeIncreasePerHour = 2f;
    [SerializeField] private bool enableWaves = true;
    
    private Dictionary<int, List<Enemy>> enemiesByIsland = new Dictionary<int, List<Enemy>>();
    private Dictionary<BiomeType, List<EnemyData>> enemyByBiome = new Dictionary<BiomeType, List<EnemyData>>();
    private float nextSpawnTime;
    private float nextWaveTime;
    private float gameStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeEnemyTypes();
    }

    private void Start()
    {
        gameStartTime = Time.time;
        nextSpawnTime = Time.time + spawnInterval;
        nextWaveTime = Time.time + waveCooldown;
        
        // Subscribe to events
        GameEvents.OnIslandUnlocked += HandleIslandUnlocked;
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDestroy()
    {
        GameEvents.OnIslandUnlocked -= HandleIslandUnlocked;
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
    }

    private void Update()
    {
        // Regular spawning
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemiesOnAllIslands();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // Wave spawning
        if (enableWaves && Time.time >= nextWaveTime)
        {
            SpawnWave();
            nextWaveTime = Time.time + waveCooldown;
        }
    }

    private void InitializeEnemyTypes()
    {
        // Group enemies by biome
        foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
        {
            enemyByBiome[biome] = new List<EnemyData>();
        }

        foreach (var enemy in enemyTypes)
        {
            foreach (var biome in enemy.allowedBiomes)
            {
                enemyByBiome[biome].Add(enemy);
            }
        }
    }

    private void SpawnEnemiesOnAllIslands()
    {
        foreach (int islandIndex in IslandManager.Instance.GetUnlockedIslands())
        {
            TrySpawnEnemyOnIsland(islandIndex);
        }
    }

    private void TrySpawnEnemyOnIsland(int islandIndex)
    {
        if (!enemiesByIsland.TryGetValue(islandIndex, out var islandEnemies))
        {
            islandEnemies = new List<Enemy>();
            enemiesByIsland[islandIndex] = islandEnemies;
        }

        // Clean up destroyed enemies
        islandEnemies.RemoveAll(e => e == null);

        if (islandEnemies.Count >= maxEnemiesPerIsland)
            return;

        Island island = IslandManager.Instance.GetIsland(islandIndex);
        if (island == null) return;

        // Get valid spawn position
        Vector2 spawnPos = GetValidSpawnPosition(island);
        if (spawnPos == Vector2.zero) return;

        // Select enemy type based on biome and conditions
        EnemyData enemyData = SelectEnemyType(island);
        if (enemyData == null) return;

        // Spawn enemy
        SpawnEnemy(enemyData, spawnPos, islandIndex);
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

    private EnemyData SelectEnemyType(Island island)
    {
        IslandData islandData = IslandManager.Instance.GetIslandData(island.islandIndex);
        if (islandData == null) return null;

        // Get enemies for this biome
        var availableEnemies = enemyByBiome[islandData.biomeType]
            .Where(e => IsEnemyValid(e, islandData))
            .ToList();

        if (availableEnemies.Count == 0) return null;

        // Calculate total weight
        float totalWeight = availableEnemies.Sum(e => e.spawnWeight);
        float randomWeight = Random.Range(0f, totalWeight);
        
        // Select enemy based on weight
        float currentWeight = 0f;
        foreach (var enemy in availableEnemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomWeight <= currentWeight)
            {
                return enemy;
            }
        }

        return availableEnemies[0];
    }

    private bool IsEnemyValid(EnemyData enemy, IslandData islandData)
    {
        if (enemy.minPlayerLevel > GameManager.Instance.PlayerProfile.level)
            return false;

        if (enemy.requiredConditions != null)
        {
            foreach (string condition in enemy.requiredConditions)
            {
                if (!islandData.HasCondition(condition))
                    return false;
            }
        }

        return true;
    }

    private void SpawnEnemy(EnemyData enemyData, Vector2 position, int islandIndex)
    {
        GameObject enemyObj = new GameObject(enemyData.enemyName);
        enemyObj.transform.position = position;
        
        // Add components
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        enemyObj.AddComponent<Rigidbody2D>().gravityScale = 0;
        enemyObj.AddComponent<CircleCollider2D>();
        
        // Set up sprite
        SpriteRenderer renderer = enemyObj.AddComponent<SpriteRenderer>();
        renderer.sprite = enemyData.icon;
        renderer.sortingLayerName = "Enemies";
        
        // Set up animator if available
        if (enemyData.animatorController != null)
        {
            Animator animator = enemyObj.AddComponent<Animator>();
            animator.runtimeAnimatorController = enemyData.animatorController;
        }
        
        // Initialize enemy
        enemy.Initialize(enemyData);
        
        // Add to tracking
        if (!enemiesByIsland.ContainsKey(islandIndex))
        {
            enemiesByIsland[islandIndex] = new List<Enemy>();
        }
        enemiesByIsland[islandIndex].Add(enemy);
    }

    private void SpawnWave()
    {
        float gameTimeHours = (Time.time - gameStartTime) / 3600f;
        int waveSize = baseWaveSize + Mathf.FloorToInt(waveSizeIncreasePerHour * gameTimeHours);
        
        // Get all valid spawn points across islands
        List<(Vector2 position, int islandIndex)> spawnPoints = new List<(Vector2, int)>();
        
        foreach (int islandIndex in IslandManager.Instance.GetUnlockedIslands())
        {
            Island island = IslandManager.Instance.GetIsland(islandIndex);
            if (island != null)
            {
                for (int i = 0; i < waveSize; i++)
                {
                    Vector2 pos = GetValidSpawnPosition(island);
                    if (pos != Vector2.zero)
                    {
                        spawnPoints.Add((pos, islandIndex));
                    }
                }
            }
        }

        // Spawn enemies at random points
        for (int i = 0; i < Mathf.Min(waveSize, spawnPoints.Count); i++)
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Island island = IslandManager.Instance.GetIsland(spawnPoint.islandIndex);
            
            EnemyData enemyData = SelectEnemyType(island);
            if (enemyData != null)
            {
                SpawnEnemy(enemyData, spawnPoint.position, spawnPoint.islandIndex);
            }
        }

        // Notify wave start
        NotificationSystem.ShowGeneral(
            "Enemy Wave Incoming!",
            $"A wave of {waveSize} enemies is approaching!",
            null // Add wave warning icon
        );
    }

    private void HandleIslandUnlocked(int islandIndex)
    {
        // Initial enemy population for new island
        for (int i = 0; i < maxEnemiesPerIsland / 2; i++)
        {
            TrySpawnEnemyOnIsland(islandIndex);
        }
    }

    private void HandleEnemyDefeated(GameObject enemyObj)
    {
        foreach (var islandEnemies in enemiesByIsland.Values)
        {
            islandEnemies.RemoveAll(e => e == null || e.gameObject == enemyObj);
        }
    }

    public List<Enemy> GetEnemiesOnIsland(int islandIndex)
    {
        return enemiesByIsland.TryGetValue(islandIndex, out var enemies) ? 
            new List<Enemy>(enemies) : new List<Enemy>();
    }

    public int GetTotalEnemyCount()
    {
        return enemiesByIsland.Values.Sum(list => list.Count);
    }
} 