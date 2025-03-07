using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager Instance { get; private set; }

    [Header("Museum Settings")]
    [SerializeField] private MuseumData museumData;
    [SerializeField] private MuseumLayout layout;
    [SerializeField] private float exhibitPlacementCooldown = 0.5f;
    
    private Dictionary<string, MuseumExhibit> exhibitDatabase = new Dictionary<string, MuseumExhibit>();
    private Dictionary<string, GameObject> activeExhibits = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> unlockedWings = new Dictionary<string, bool>();
    private Dictionary<string, int> exhibitCounts = new Dictionary<string, int>();
    private Dictionary<string, bool> appliedThemes = new Dictionary<string, bool>();
    private HashSet<int> claimedMilestones = new HashSet<int>();
    private float nextPlacementTime;

    public int TotalExhibits => exhibitCounts.Values.Sum();
    public bool CanPlaceExhibit => Time.time >= nextPlacementTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeMuseum();
    }

    private void Start()
    {
        LoadMuseumState();
        CheckMilestones();
        
        // Subscribe to events
        GameEvents.OnCritterCaught += HandleCritterCaught;
        GameEvents.OnItemCollected += HandleItemCollected;
        GameEvents.OnConditionUnlocked += HandleConditionUnlocked;
    }

    private void OnDestroy()
    {
        GameEvents.OnCritterCaught -= HandleCritterCaught;
        GameEvents.OnItemCollected -= HandleItemCollected;
        GameEvents.OnConditionUnlocked -= HandleConditionUnlocked;
    }

    private void InitializeMuseum()
    {
        // Initialize exhibit database
        foreach (var wing in museumData.wings)
        {
            unlockedWings[wing.wingId] = !wing.isLocked;
            
            foreach (var exhibit in wing.exhibits)
            {
                exhibitDatabase[exhibit.exhibitId] = exhibit;
                exhibitCounts[exhibit.exhibitId] = 0;
            }
        }

        // Initialize themes
        foreach (var theme in museumData.themes)
        {
            appliedThemes[theme.themeId] = false;
        }

        // Initialize layout grid if needed
        if (layout.tiles == null)
        {
            layout.tiles = new MuseumTile[layout.gridSize.x, layout.gridSize.y];
            for (int x = 0; x < layout.gridSize.x; x++)
            {
                for (int y = 0; y < layout.gridSize.y; y++)
                {
                    layout.tiles[x, y] = new MuseumTile
                    {
                        isWalkable = true,
                        isExhibitSpace = true
                    };
                }
            }
        }
    }

    public bool TryPlaceExhibit(string exhibitId, Vector2Int position)
    {
        if (!CanPlaceExhibit) return false;

        if (!exhibitDatabase.TryGetValue(exhibitId, out MuseumExhibit exhibit))
        {
            Debug.LogError($"Exhibit {exhibitId} not found in database!");
            return false;
        }

        if (!IsSpaceAvailable(position, exhibit.size))
        {
            NotificationSystem.ShowWarning("Invalid Position", "This space is already occupied!");
            return false;
        }

        if (!HasRequiredItems(exhibit))
        {
            NotificationSystem.ShowWarning("Missing Items", "You don't have all required items!");
            return false;
        }

        // Place exhibit
        GameObject exhibitObj = Instantiate(exhibit.exhibitPrefab, 
            new Vector3(position.x, position.y, 0), 
            Quaternion.identity);
        
        activeExhibits[exhibit.exhibitId] = exhibitObj;
        
        // Mark tiles as occupied
        for (int x = 0; x < exhibit.size.x; x++)
        {
            for (int y = 0; y < exhibit.size.y; y++)
            {
                Vector2Int tile = position + new Vector2Int(x, y);
                layout.tiles[tile.x, tile.y].occupyingExhibitId = exhibit.exhibitId;
                layout.tiles[tile.x, tile.y].isWalkable = false;
            }
        }

        // Consume required items
        ConsumeRequiredItems(exhibit);
        
        // Update counts and check themes
        exhibitCounts[exhibit.exhibitId]++;
        CheckThemes();
        CheckMilestones();
        
        // Set cooldown
        nextPlacementTime = Time.time + exhibitPlacementCooldown;
        
        // Notify systems
        GameEvents.OnExhibitPlaced?.Invoke(exhibit.exhibitId, position);
        
        return true;
    }

    public bool TryRemoveExhibit(string exhibitId)
    {
        if (!activeExhibits.TryGetValue(exhibitId, out GameObject exhibitObj))
            return false;

        if (!exhibitDatabase.TryGetValue(exhibitId, out MuseumExhibit exhibit))
            return false;

        // Find and clear occupied tiles
        for (int x = 0; x < layout.gridSize.x; x++)
        {
            for (int y = 0; y < layout.gridSize.y; y++)
            {
                if (layout.tiles[x, y].occupyingExhibitId == exhibitId)
                {
                    layout.tiles[x, y].occupyingExhibitId = null;
                    layout.tiles[x, y].isWalkable = true;
                }
            }
        }

        // Return items if possible
        ReturnExhibitItems(exhibit);
        
        // Update tracking
        exhibitCounts[exhibitId]--;
        activeExhibits.Remove(exhibitId);
        Destroy(exhibitObj);
        
        // Recheck themes
        CheckThemes();
        
        // Notify systems
        GameEvents.OnExhibitRemoved?.Invoke(exhibitId);
        
        return true;
    }

    private bool IsSpaceAvailable(Vector2Int position, Vector2Int size)
    {
        // Check bounds
        if (position.x < 0 || position.y < 0 || 
            position.x + size.x > layout.gridSize.x || 
            position.y + size.y > layout.gridSize.y)
            return false;

        // Check tiles
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int tile = position + new Vector2Int(x, y);
                if (!layout.tiles[tile.x, tile.y].isExhibitSpace || 
                    layout.tiles[tile.x, tile.y].occupyingExhibitId != null)
                    return false;
            }
        }

        return true;
    }

    private bool HasRequiredItems(MuseumExhibit exhibit)
    {
        if (exhibit.requiredItems == null) return true;

        for (int i = 0; i < exhibit.requiredItems.Length; i++)
        {
            if (!InventoryManager.Instance.HasItem(exhibit.requiredItems[i], exhibit.requiredAmounts[i]))
                return false;
        }

        return true;
    }

    private void ConsumeRequiredItems(MuseumExhibit exhibit)
    {
        if (exhibit.requiredItems == null) return;

        for (int i = 0; i < exhibit.requiredItems.Length; i++)
        {
            InventoryManager.Instance.RemoveItem(exhibit.requiredItems[i], exhibit.requiredAmounts[i]);
        }
    }

    private void ReturnExhibitItems(MuseumExhibit exhibit)
    {
        if (exhibit.requiredItems == null) return;

        for (int i = 0; i < exhibit.requiredItems.Length; i++)
        {
            InventoryManager.Instance.AddItem(exhibit.requiredItems[i], exhibit.requiredAmounts[i]);
        }
    }

    private void CheckThemes()
    {
        foreach (var theme in museumData.themes)
        {
            bool wasApplied = appliedThemes[theme.themeId];
            bool isApplied = CheckThemeRequirements(theme);
            
            if (isApplied != wasApplied)
            {
                appliedThemes[theme.themeId] = isApplied;
                if (isApplied)
                {
                    ApplyThemeBonus(theme);
                    NotificationSystem.ShowGeneral(
                        "Theme Complete!",
                        $"The {theme.displayName} theme has been completed!",
                        theme.themeIcon
                    );
                }
            }
        }
    }

    private bool CheckThemeRequirements(MuseumTheme theme)
    {
        int requiredCount = theme.requirements.Count(r => !r.isOptional);
        int metCount = 0;

        foreach (var req in theme.requirements)
        {
            if (exhibitCounts.TryGetValue(req.exhibitId, out int count))
            {
                if (count >= req.minimumCount)
                {
                    if (!req.isOptional) metCount++;
                }
                else if (!req.isOptional)
                {
                    return false;
                }
            }
            else if (!req.isOptional)
            {
                return false;
            }
        }

        return metCount >= requiredCount;
    }

    private void ApplyThemeBonus(MuseumTheme theme)
    {
        foreach (var wing in museumData.wings)
        {
            if (theme.applicableCategories.Contains(wing.category))
            {
                foreach (var exhibit in wing.exhibits)
                {
                    foreach (var bonus in exhibit.themeBonuses)
                    {
                        if (bonus.themeId == theme.themeId)
                        {
                            // Apply bonus effects
                            ApplyExhibitBonus(exhibit, bonus.bonusMultiplier);
                        }
                    }
                }
            }
        }
    }

    private void ApplyExhibitBonus(MuseumExhibit exhibit, float multiplier)
    {
        if (!activeExhibits.TryGetValue(exhibit.exhibitId, out GameObject exhibitObj))
            return;

        // Apply visual effects or other bonuses
        var exhibitComponent = exhibitObj.GetComponent<MuseumExhibitBehavior>();
        exhibitComponent?.ApplyBonus(multiplier);
    }

    private void CheckMilestones()
    {
        int totalExhibits = TotalExhibits;
        
        for (int i = 0; i < museumData.milestones.Length; i++)
        {
            var milestone = museumData.milestones[i];
            if (!claimedMilestones.Contains(i) && totalExhibits >= milestone.exhibitsRequired)
            {
                AwardMilestoneRewards(milestone);
                claimedMilestones.Add(i);
                
                NotificationSystem.ShowGeneral(
                    "Museum Milestone!",
                    milestone.description,
                    milestone.milestoneIcon
                );
            }
        }
    }

    private void AwardMilestoneRewards(MuseumMilestone milestone)
    {
        foreach (var reward in milestone.rewards)
        {
            if (Random.value <= reward.bonusChance)
            {
                switch (reward.type)
                {
                    case RewardType.Item:
                        InventoryManager.Instance.AddItem(reward.rewardId, reward.amount);
                        break;
                    case RewardType.Currency:
                        GameManager.Instance.PlayerProfile.AddGold(reward.amount);
                        break;
                    case RewardType.Experience:
                        GameManager.Instance.PlayerProfile.AddExperience(reward.amount);
                        break;
                    case RewardType.UnlockCondition:
                        GameManager.Instance.UnlockCondition(reward.rewardId);
                        break;
                    case RewardType.SkillPoint:
                        SkillManager.Instance.AddSkillPoints(reward.amount);
                        break;
                }
            }
        }
    }

    #region Event Handlers
    private void HandleCritterCaught(string critterId)
    {
        // Check if this critter can be added to the museum
        foreach (var wing in museumData.wings)
        {
            foreach (var exhibit in wing.exhibits)
            {
                if (exhibit.type == ExhibitType.Critter && 
                    exhibit.requiredItems.Contains(critterId))
                {
                    NotificationSystem.ShowGeneral(
                        "New Museum Exhibit Available!",
                        $"You can now display the {exhibit.displayName} in the museum!",
                        exhibit.exhibitIcon
                    );
                }
            }
        }
    }

    private void HandleItemCollected(string itemId)
    {
        // Similar to critter caught, check for new possible exhibits
    }

    private void HandleConditionUnlocked(string condition)
    {
        // Check for wing unlocks
        foreach (var wing in museumData.wings)
        {
            if (!unlockedWings[wing.wingId] && 
                wing.unlockConditions != null && 
                wing.unlockConditions.Contains(condition))
            {
                bool allConditionsMet = wing.unlockConditions.All(c => 
                    GameManager.Instance.HasCondition(c));
                
                if (allConditionsMet)
                {
                    UnlockWing(wing);
                }
            }
        }
    }
    #endregion

    private void UnlockWing(MuseumWing wing)
    {
        unlockedWings[wing.wingId] = true;
        
        NotificationSystem.ShowGeneral(
            "New Museum Wing Unlocked!",
            $"The {wing.displayName} is now available!",
            wing.wingIcon
        );
        
        GameEvents.OnMuseumWingUnlocked?.Invoke(wing.wingId);
    }

    #region Save/Load
    private void SaveMuseumState()
    {
        var saveData = new MuseumSaveData
        {
            exhibitCounts = exhibitCounts,
            unlockedWings = unlockedWings,
            appliedThemes = appliedThemes,
            claimedMilestones = claimedMilestones.ToList()
        };

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("MuseumData", json);
        PlayerPrefs.Save();
    }

    private void LoadMuseumState()
    {
        if (PlayerPrefs.HasKey("MuseumData"))
        {
            string json = PlayerPrefs.GetString("MuseumData");
            var saveData = JsonUtility.FromJson<MuseumSaveData>(json);
            
            exhibitCounts = saveData.exhibitCounts;
            unlockedWings = saveData.unlockedWings;
            appliedThemes = saveData.appliedThemes;
            claimedMilestones = new HashSet<int>(saveData.claimedMilestones);
        }
    }
    #endregion

    #region Public Getters
    public bool IsWingUnlocked(string wingId)
    {
        return unlockedWings.TryGetValue(wingId, out bool unlocked) && unlocked;
    }

    public int GetExhibitCount(string exhibitId)
    {
        return exhibitCounts.TryGetValue(exhibitId, out int count) ? count : 0;
    }

    public bool IsThemeApplied(string themeId)
    {
        return appliedThemes.TryGetValue(themeId, out bool applied) && applied;
    }

    public List<MuseumExhibit> GetAvailableExhibits()
    {
        return exhibitDatabase.Values
            .Where(e => HasRequiredItems(e))
            .ToList();
    }

    public List<MuseumWing> GetUnlockedWings()
    {
        return museumData.wings
            .Where(w => unlockedWings[w.wingId])
            .ToList();
    }
    #endregion
}

[System.Serializable]
public class MuseumSaveData
{
    public Dictionary<string, int> exhibitCounts;
    public Dictionary<string, bool> unlockedWings;
    public Dictionary<string, bool> appliedThemes;
    public List<int> claimedMilestones;
} 