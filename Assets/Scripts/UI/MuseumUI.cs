using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MuseumUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset wingEntryTemplate;
    [SerializeField] private VisualTreeAsset themeEntryTemplate;
    [SerializeField] private VisualTreeAsset exhibitSlotTemplate;
    [SerializeField] private VisualTreeAsset milestoneEntryTemplate;
    
    private VisualElement root;
    private ScrollView wingList;
    private ScrollView themeList;
    private ScrollView exhibitGrid;
    private VisualElement wingDetails;
    private VisualElement exhibitDetails;
    private Button placeButton;
    private Button closeButton;
    
    private Dictionary<string, VisualElement> wingEntries = new Dictionary<string, VisualElement>();
    private Dictionary<string, VisualElement> exhibitSlots = new Dictionary<string, VisualElement>();
    private string selectedWingId;
    private string selectedExhibitId;
    private Vector2Int? selectedSlotPosition;

    private void Awake()
    {
        root = document.rootVisualElement.Q("museum-panel");
        wingList = root.Q<ScrollView>("wing-list");
        themeList = root.Q<ScrollView>("theme-list");
        exhibitGrid = root.Q<ScrollView>("exhibit-grid");
        wingDetails = root.Q("wing-details");
        exhibitDetails = root.Q("exhibit-details");
        placeButton = root.Q<Button>("place-button");
        closeButton = root.Q<Button>("close-button");
        
        InitializeUI();
        
        // Subscribe to events
        GameEvents.OnExhibitPlaced += HandleExhibitPlaced;
        GameEvents.OnExhibitRemoved += HandleExhibitRemoved;
        GameEvents.OnMuseumWingUnlocked += HandleWingUnlocked;
    }

    private void OnDestroy()
    {
        GameEvents.OnExhibitPlaced -= HandleExhibitPlaced;
        GameEvents.OnExhibitRemoved -= HandleExhibitRemoved;
        GameEvents.OnMuseumWingUnlocked -= HandleWingUnlocked;
    }

    private void Start()
    {
        RefreshUI();
        Hide();
    }

    private void InitializeUI()
    {
        closeButton.clicked += Hide;
        placeButton.clicked += PlaceSelectedExhibit;
        
        // Initialize grid layout
        exhibitGrid.style.flexDirection = FlexDirection.Row;
        exhibitGrid.style.flexWrap = Wrap.Wrap;
        exhibitGrid.style.justifyContent = Justify.Center;
    }

    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
        RefreshUI();
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
        ClearSelection();
    }

    private void RefreshUI()
    {
        UpdateStats();
        RefreshWingList();
        RefreshThemeList();
        RefreshMilestones();
        
        if (selectedWingId != null)
        {
            UpdateWingDetails();
        }
    }

    private void UpdateStats()
    {
        var totalExhibits = MuseumManager.Instance.TotalExhibits;
        var totalPossible = MuseumManager.Instance.GetAvailableExhibits().Count;
        var completionRate = totalPossible > 0 ? (float)totalExhibits / totalPossible * 100 : 0;
        
        root.Q<Label>("total-exhibits").text = $"Total Exhibits: {totalExhibits}";
        root.Q<Label>("completion-rate").text = $"Completion: {completionRate:F1}%";
    }

    private void RefreshWingList()
    {
        wingList.Clear();
        wingEntries.Clear();

        foreach (var wing in MuseumManager.Instance.GetUnlockedWings())
        {
            AddWingEntry(wing);
        }
    }

    private void AddWingEntry(MuseumWing wing)
    {
        var entry = wingEntryTemplate.Instantiate();
        var wingEntry = entry.Q("wing-entry");
        
        // Set up entry content
        entry.Q<Image>("wing-icon").sprite = wing.wingIcon;
        entry.Q<Label>("wing-name").text = wing.displayName;
        
        // Calculate completion
        int totalExhibits = wing.exhibits.Length;
        int placedExhibits = wing.exhibits.Count(e => 
            MuseumManager.Instance.GetExhibitCount(e.exhibitId) > 0);
        float completion = totalExhibits > 0 ? (float)placedExhibits / totalExhibits * 100 : 0;
        
        entry.Q<Label>("wing-completion").text = $"{completion:F0}%";
        
        if (!MuseumManager.Instance.IsWingUnlocked(wing.wingId))
        {
            wingEntry.AddToClassList("locked");
            entry.Q<Label>("wing-status").text = "Locked";
        }
        
        // Add click handler
        wingEntry.RegisterCallback<ClickEvent>(evt => SelectWing(wing.wingId));
        
        wingList.Add(entry);
        wingEntries[wing.wingId] = wingEntry;
    }

    private void RefreshThemeList()
    {
        themeList.Clear();

        foreach (var theme in MuseumManager.Instance.museumData.themes)
        {
            var entry = themeEntryTemplate.Instantiate();
            
            entry.Q<Image>("theme-icon").sprite = theme.themeIcon;
            entry.Q<Label>("theme-name").text = theme.displayName;
            
            if (MuseumManager.Instance.IsThemeApplied(theme.themeId))
            {
                entry.AddToClassList("active");
            }
            
            // Add tooltip with theme description
            entry.tooltip = theme.description;
            
            themeList.Add(entry);
        }
    }

    private void RefreshMilestones()
    {
        var milestoneList = root.Q<ScrollView>("milestone-list");
        milestoneList.Clear();

        var totalExhibits = MuseumManager.Instance.TotalExhibits;
        foreach (var milestone in MuseumManager.Instance.museumData.milestones)
        {
            var entry = milestoneEntryTemplate.Instantiate();
            var progress = Mathf.Min(1f, (float)totalExhibits / milestone.exhibitsRequired);
            
            entry.Q<Image>("milestone-icon").sprite = milestone.milestoneIcon;
            entry.Q<Label>("milestone-name").text = milestone.displayName;
            entry.Q<Label>("milestone-description").text = milestone.description;
            entry.Q<ProgressBar>("milestone-progress").value = progress * 100;
            
            if (totalExhibits >= milestone.exhibitsRequired)
            {
                entry.AddToClassList("completed");
            }
            
            milestoneList.Add(entry);
        }
    }

    private void SelectWing(string wingId)
    {
        // Update selection visuals
        foreach (var entry in wingEntries.Values)
        {
            entry.RemoveFromClassList("selected");
        }
        
        if (wingEntries.TryGetValue(wingId, out var selectedEntry))
        {
            selectedEntry.AddToClassList("selected");
        }

        selectedWingId = wingId;
        ClearExhibitSelection();
        UpdateWingDetails();
    }

    private void UpdateWingDetails()
    {
        var wing = MuseumManager.Instance.museumData.wings
            .FirstOrDefault(w => w.wingId == selectedWingId);
            
        if (wing == null) return;

        wingDetails.AddToClassList("visible");
        
        // Update header
        wingDetails.Q<Image>("wing-icon").sprite = wing.wingIcon;
        wingDetails.Q<Label>("wing-title").text = wing.displayName;
        wingDetails.Q<Label>("wing-description").text = wing.description;
        
        // Calculate completion
        int totalExhibits = wing.exhibits.Length;
        int placedExhibits = wing.exhibits.Count(e => 
            MuseumManager.Instance.GetExhibitCount(e.exhibitId) > 0);
        float completion = totalExhibits > 0 ? (float)placedExhibits / totalExhibits * 100 : 0;
        
        wingDetails.Q<Label>("wing-completion").text = $"Completion: {completion:F0}%";
        
        // Update exhibit grid
        RefreshExhibitGrid(wing);
    }

    private void RefreshExhibitGrid(MuseumWing wing)
    {
        exhibitGrid.Clear();
        exhibitSlots.Clear();

        // Create grid based on wing size
        for (int y = 0; y < wing.size.y; y++)
        {
            for (int x = 0; x < wing.size.x; x++)
            {
                var position = new Vector2Int(x, y);
                var slot = exhibitSlotTemplate.Instantiate();
                var slotKey = $"{x}_{y}";
                
                // Check if slot is occupied
                var exhibit = wing.exhibits.FirstOrDefault(e => e.position == position);
                if (exhibit != null)
                {
                    slot.Q<Image>("exhibit-icon").sprite = exhibit.exhibitIcon;
                    slot.AddToClassList("occupied");
                    
                    if (MuseumManager.Instance.GetExhibitCount(exhibit.exhibitId) > 0)
                    {
                        slot.AddToClassList("placed");
                    }
                }
                
                // Add click handler
                slot.RegisterCallback<ClickEvent>(evt => SelectExhibitSlot(position, exhibit));
                
                exhibitGrid.Add(slot);
                exhibitSlots[slotKey] = slot;
            }
        }
    }

    private void SelectExhibitSlot(Vector2Int position, MuseumExhibit exhibit)
    {
        ClearExhibitSelection();
        
        if (exhibit == null) return;

        selectedExhibitId = exhibit.exhibitId;
        selectedSlotPosition = position;
        
        var slotKey = $"{position.x}_{position.y}";
        if (exhibitSlots.TryGetValue(slotKey, out var slot))
        {
            slot.AddToClassList("selected");
        }
        
        UpdateExhibitDetails(exhibit);
    }

    private void UpdateExhibitDetails(MuseumExhibit exhibit)
    {
        exhibitDetails.AddToClassList("visible");
        
        exhibitDetails.Q<Image>("exhibit-icon").sprite = exhibit.exhibitIcon;
        exhibitDetails.Q<Label>("exhibit-name").text = exhibit.displayName;
        exhibitDetails.Q<Label>("exhibit-description").text = exhibit.description;
        
        // Update requirements
        var requirementsList = exhibitDetails.Q<ScrollView>("requirements-list");
        requirementsList.Clear();
        
        if (exhibit.requiredItems != null)
        {
            for (int i = 0; i < exhibit.requiredItems.Length; i++)
            {
                var requirement = new VisualElement();
                requirement.AddToClassList("requirement-entry");
                
                var itemId = exhibit.requiredItems[i];
                var amount = exhibit.requiredAmounts[i];
                var hasItem = InventoryManager.Instance.HasItem(itemId, amount);
                
                if (hasItem)
                {
                    requirement.AddToClassList("met");
                }
                
                requirement.Add(new Label($"{amount}x {itemId}"));
                requirementsList.Add(requirement);
            }
        }
        
        // Update rewards
        var rewardsList = exhibitDetails.Q<ScrollView>("rewards-list");
        rewardsList.Clear();
        
        foreach (var reward in exhibit.rewards)
        {
            var rewardEntry = new VisualElement();
            rewardEntry.AddToClassList("reward-entry");
            
            string rewardText = reward.type switch
            {
                RewardType.Item => $"{reward.amount}x {reward.rewardId}",
                RewardType.Currency => $"{reward.amount} Gold",
                RewardType.Experience => $"{reward.amount} XP",
                RewardType.SkillPoint => $"{reward.amount} Skill Points",
                _ => reward.rewardId
            };
            
            if (reward.bonusChance > 0)
            {
                rewardText += $" ({reward.bonusChance * 100:F0}% Bonus Chance)";
            }
            
            rewardEntry.Add(new Label(rewardText));
            rewardsList.Add(rewardEntry);
        }
        
        // Update place button
        bool canPlace = MuseumManager.Instance.CanPlaceExhibit && 
                       HasRequirements(exhibit) &&
                       MuseumManager.Instance.GetExhibitCount(exhibit.exhibitId) == 0;
                       
        placeButton.SetEnabled(canPlace);
    }

    private bool HasRequirements(MuseumExhibit exhibit)
    {
        if (exhibit.requiredItems == null) return true;

        for (int i = 0; i < exhibit.requiredItems.Length; i++)
        {
            if (!InventoryManager.Instance.HasItem(exhibit.requiredItems[i], exhibit.requiredAmounts[i]))
                return false;
        }

        return true;
    }

    private void PlaceSelectedExhibit()
    {
        if (selectedExhibitId == null || !selectedSlotPosition.HasValue) return;

        if (MuseumManager.Instance.TryPlaceExhibit(selectedExhibitId, selectedSlotPosition.Value))
        {
            RefreshUI();
            ClearExhibitSelection();
        }
    }

    private void ClearExhibitSelection()
    {
        selectedExhibitId = null;
        selectedSlotPosition = null;
        
        foreach (var slot in exhibitSlots.Values)
        {
            slot.RemoveFromClassList("selected");
        }
        
        exhibitDetails.RemoveFromClassList("visible");
    }

    private void ClearSelection()
    {
        selectedWingId = null;
        ClearExhibitSelection();
        
        foreach (var entry in wingEntries.Values)
        {
            entry.RemoveFromClassList("selected");
        }
        
        wingDetails.RemoveFromClassList("visible");
    }

    #region Event Handlers
    private void HandleExhibitPlaced(string exhibitId, Vector2Int position)
    {
        RefreshUI();
    }

    private void HandleExhibitRemoved(string exhibitId)
    {
        RefreshUI();
    }

    private void HandleWingUnlocked(string wingId)
    {
        RefreshWingList();
    }
    #endregion
} 