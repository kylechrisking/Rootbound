using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Settings")]
    [SerializeField] private QuestData[] availableQuests;
    [SerializeField] private int maxActiveQuests = 10;
    [SerializeField] private float questSaveInterval = 300f; // 5 minutes
    
    private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();
    private Dictionary<string, QuestProgress> questProgress = new Dictionary<string, QuestProgress>();
    private float nextSaveTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeQuestDatabase();
    }

    private void Start()
    {
        nextSaveTime = Time.time + questSaveInterval;
        
        // Subscribe to relevant events
        GameEvents.OnResourceCollected += HandleResourceCollected;
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        GameEvents.OnItemCrafted += HandleItemCrafted;
        GameEvents.OnIslandUnlocked += HandleIslandUnlocked;
        GameEvents.OnSpeciesDiscovered += HandleSpeciesDiscovered;
        
        LoadQuestProgress();
    }

    private void OnDestroy()
    {
        GameEvents.OnResourceCollected -= HandleResourceCollected;
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        GameEvents.OnItemCrafted -= HandleItemCrafted;
        GameEvents.OnIslandUnlocked -= HandleIslandUnlocked;
        GameEvents.OnSpeciesDiscovered -= HandleSpeciesDiscovered;
    }

    private void Update()
    {
        if (Time.time >= nextSaveTime)
        {
            SaveQuestProgress();
            nextSaveTime = Time.time + questSaveInterval;
        }

        UpdateActiveQuests();
    }

    private void InitializeQuestDatabase()
    {
        foreach (var quest in availableQuests)
        {
            if (!string.IsNullOrEmpty(quest.questId))
            {
                questDatabase[quest.questId] = quest;
            }
            else
            {
                Debug.LogError($"Quest {quest.name} has no questId!");
            }
        }
    }

    private void UpdateActiveQuests()
    {
        foreach (var quest in activeQuests.Values.ToList())
        {
            quest.Update();

            if (quest.IsCompleted)
            {
                CompleteQuest(quest.QuestId);
            }
            else if (quest.HasFailed)
            {
                FailQuest(quest.QuestId);
            }
        }

        // Check for new available quests
        CheckForAvailableQuests();
    }

    public bool AcceptQuest(string questId)
    {
        if (activeQuests.Count >= maxActiveQuests)
        {
            NotificationSystem.ShowWarning("Quest Log Full", "Cannot accept more quests!");
            return false;
        }

        if (!questDatabase.TryGetValue(questId, out QuestData questData))
        {
            Debug.LogError($"Quest {questId} not found in database!");
            return false;
        }

        if (!IsQuestAvailable(questData))
        {
            NotificationSystem.ShowWarning("Quest Unavailable", "Requirements not met!");
            return false;
        }

        var quest = new Quest(questData);
        activeQuests[questId] = quest;
        
        // Create or update progress tracking
        if (!questProgress.ContainsKey(questId))
        {
            questProgress[questId] = new QuestProgress(questId);
        }

        NotificationSystem.ShowQuest("New Quest", $"Accepted: {questData.displayName}", questData.questIcon);
        GameEvents.OnQuestAccepted?.Invoke(questId);
        
        return true;
    }

    public void CompleteQuest(string questId)
    {
        if (!activeQuests.TryGetValue(questId, out Quest quest)) return;

        // Award rewards
        var questData = quest.QuestData;
        var player = GameManager.Instance.PlayerProfile;
        
        player.AddExperience(questData.experienceReward);
        player.AddGold(questData.goldReward);

        foreach (var reward in questData.itemRewards)
        {
            if (reward.bonusChance > 0 && Random.value <= reward.bonusChance)
            {
                // Double the reward amount for bonus
                InventoryManager.Instance.AddItem(reward.itemPrefab, reward.amount * 2);
            }
            else
            {
                InventoryManager.Instance.AddItem(reward.itemPrefab, reward.amount);
            }
        }

        // Unlock conditions
        if (questData.unlockConditions != null)
        {
            foreach (var condition in questData.unlockConditions)
            {
                GameManager.Instance.UnlockCondition(condition);
            }
        }

        // Update progress tracking
        questProgress[questId].CompleteQuest();
        activeQuests.Remove(questId);

        NotificationSystem.ShowQuest("Quest Completed", questData.displayName, questData.questIcon);
        GameEvents.OnQuestCompleted?.Invoke(questId);
        
        SaveQuestProgress();
    }

    public void FailQuest(string questId)
    {
        if (!activeQuests.TryGetValue(questId, out Quest quest)) return;

        questProgress[questId].FailQuest();
        activeQuests.Remove(questId);

        NotificationSystem.ShowWarning("Quest Failed", quest.QuestData.displayName);
        GameEvents.OnQuestFailed?.Invoke(questId);
        
        SaveQuestProgress();
    }

    public void AbandonQuest(string questId)
    {
        if (!activeQuests.ContainsKey(questId)) return;

        questProgress[questId].AbandonQuest();
        activeQuests.Remove(questId);

        NotificationSystem.ShowQuest("Quest Abandoned", $"Abandoned: {questDatabase[questId].displayName}", null);
        GameEvents.OnQuestAbandoned?.Invoke(questId);
        
        SaveQuestProgress();
    }

    private bool IsQuestAvailable(QuestData quest)
    {
        // Check level requirement
        if (GameManager.Instance.PlayerProfile.level < quest.levelRequirement)
            return false;

        // Check prerequisites
        if (quest.prerequisiteQuestIds != null)
        {
            foreach (var prereqId in quest.prerequisiteQuestIds)
            {
                if (!IsQuestCompleted(prereqId))
                    return false;
            }
        }

        // Check conditions
        if (quest.requiredConditions != null)
        {
            foreach (var condition in quest.requiredConditions)
            {
                if (!GameManager.Instance.HasCondition(condition))
                    return false;
            }
        }

        // Check if repeatable or not completed
        if (!quest.isRepeatable && IsQuestCompleted(quest.questId))
            return false;

        // Check time/season restrictions
        if (quest.isAvailableOnlyAtNight && !GameManager.Instance.IsNightTime)
            return false;

        if (quest.isSeasonalQuest && !quest.availableSeasons.Contains(GameManager.Instance.CurrentSeason))
            return false;

        return true;
    }

    private void CheckForAvailableQuests()
    {
        foreach (var quest in questDatabase.Values)
        {
            if (!activeQuests.ContainsKey(quest.questId) && IsQuestAvailable(quest))
            {
                if (quest.questType == QuestType.Daily || quest.questType == QuestType.Weekly)
                {
                    // Auto-accept daily/weekly quests
                    AcceptQuest(quest.questId);
                }
                else
                {
                    // Notify player of new available quest
                    NotificationSystem.ShowQuest("New Quest Available", quest.displayName, quest.questIcon);
                }
            }
        }
    }

    private void SaveQuestProgress()
    {
        // Save quest progress to PlayerPrefs or your save system
        string progressJson = JsonUtility.ToJson(new QuestProgressData { progress = questProgress });
        PlayerPrefs.SetString("QuestProgress", progressJson);
        PlayerPrefs.Save();
    }

    private void LoadQuestProgress()
    {
        if (PlayerPrefs.HasKey("QuestProgress"))
        {
            string progressJson = PlayerPrefs.GetString("QuestProgress");
            var progressData = JsonUtility.FromJson<QuestProgressData>(progressJson);
            questProgress = progressData.progress;
        }
    }

    #region Event Handlers
    private void HandleResourceCollected(string resourceId, int amount)
    {
        foreach (var quest in activeQuests.Values)
        {
            quest.UpdateObjectives(ObjectiveType.CollectResource, resourceId, amount);
        }
    }

    private void HandleEnemyDefeated(GameObject enemy)
    {
        if (enemy.TryGetComponent<Enemy>(out var enemyComponent))
        {
            string enemyId = enemyComponent.Data.enemyName;
            foreach (var quest in activeQuests.Values)
            {
                quest.UpdateObjectives(ObjectiveType.DefeatEnemy, enemyId, 1);
            }
        }
    }

    private void HandleItemCrafted(string itemId, int amount)
    {
        foreach (var quest in activeQuests.Values)
        {
            quest.UpdateObjectives(ObjectiveType.CraftItem, itemId, amount);
        }
    }

    private void HandleIslandUnlocked(int islandIndex)
    {
        foreach (var quest in activeQuests.Values)
        {
            quest.UpdateObjectives(ObjectiveType.UnlockIsland, islandIndex.ToString(), 1);
        }
    }

    private void HandleSpeciesDiscovered(string speciesId)
    {
        foreach (var quest in activeQuests.Values)
        {
            quest.UpdateObjectives(ObjectiveType.DiscoverSpecies, speciesId, 1);
        }
    }
    #endregion

    #region Public Getters
    public Quest GetQuest(string questId)
    {
        return activeQuests.TryGetValue(questId, out Quest quest) ? quest : null;
    }

    public bool IsQuestActive(string questId)
    {
        return activeQuests.ContainsKey(questId);
    }

    public bool IsQuestCompleted(string questId)
    {
        return questProgress.TryGetValue(questId, out QuestProgress progress) && progress.IsCompleted;
    }

    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests.Values);
    }

    public List<QuestData> GetAvailableQuests()
    {
        return questDatabase.Values.Where(q => IsQuestAvailable(q)).ToList();
    }
    #endregion
}

[System.Serializable]
public class QuestProgressData
{
    public Dictionary<string, QuestProgress> progress;
} 