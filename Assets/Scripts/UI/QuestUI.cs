using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset questEntryTemplate;
    [SerializeField] private VisualTreeAsset objectiveEntryTemplate;
    
    private VisualElement root;
    private ScrollView questList;
    private VisualElement questDetails;
    private Button closeButton;
    private Button acceptButton;
    private Button abandonButton;
    private Button trackButton;
    
    private Dictionary<string, VisualElement> questEntries = new Dictionary<string, VisualElement>();
    private string selectedQuestId;
    private QuestType currentFilter = QuestType.MainStory;

    private void Awake()
    {
        root = document.rootVisualElement.Q("quest-panel");
        questList = root.Q<ScrollView>("quest-list");
        questDetails = root.Q("quest-details");
        
        InitializeButtons();
        InitializeFilters();
        
        // Subscribe to quest events
        GameEvents.OnQuestAccepted += HandleQuestAccepted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
        GameEvents.OnQuestFailed += HandleQuestFailed;
        GameEvents.OnQuestAbandoned += HandleQuestAbandoned;
    }

    private void OnDestroy()
    {
        GameEvents.OnQuestAccepted -= HandleQuestAccepted;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
        GameEvents.OnQuestFailed -= HandleQuestFailed;
        GameEvents.OnQuestAbandoned -= HandleQuestAbandoned;
    }

    private void Start()
    {
        RefreshQuestList();
        Hide();
    }

    private void Update()
    {
        if (selectedQuestId != null)
        {
            UpdateQuestProgress();
        }
    }

    private void InitializeButtons()
    {
        closeButton = root.Q<Button>("close-button");
        acceptButton = root.Q<Button>("accept-button");
        abandonButton = root.Q<Button>("abandon-button");
        trackButton = root.Q<Button>("track-button");
        
        closeButton.clicked += Hide;
        acceptButton.clicked += () => AcceptSelectedQuest();
        abandonButton.clicked += () => AbandonSelectedQuest();
        trackButton.clicked += () => ToggleTrackSelectedQuest();
    }

    private void InitializeFilters()
    {
        var allButton = root.Q<Button>("all-quests");
        var mainButton = root.Q<Button>("main-quests");
        var sideButton = root.Q<Button>("side-quests");
        var dailyButton = root.Q<Button>("daily-quests");

        allButton.clicked += () => SetFilter(null);
        mainButton.clicked += () => SetFilter(QuestType.MainStory);
        sideButton.clicked += () => SetFilter(QuestType.SideQuest);
        dailyButton.clicked += () => SetFilter(QuestType.Daily);
    }

    private void SetFilter(QuestType? filter)
    {
        if (filter.HasValue)
            currentFilter = filter.Value;
        else
            currentFilter = QuestType.MainStory; // Default filter
            
        RefreshQuestList();
        
        // Update filter button visuals
        var buttons = root.Q("quest-filters").Children();
        foreach (var button in buttons)
        {
            button.RemoveFromClassList("selected");
        }
        
        string buttonName = filter switch
        {
            QuestType.MainStory => "main-quests",
            QuestType.SideQuest => "side-quests",
            QuestType.Daily => "daily-quests",
            _ => "all-quests"
        };
        
        root.Q(buttonName)?.AddToClassList("selected");
    }

    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
        RefreshQuestList();
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    private void RefreshQuestList()
    {
        questList.Clear();
        questEntries.Clear();

        var availableQuests = QuestManager.Instance.GetAvailableQuests();
        var activeQuests = QuestManager.Instance.GetActiveQuests();

        // First add active quests
        foreach (var quest in activeQuests)
        {
            if (currentFilter == null || quest.QuestData.questType == currentFilter)
            {
                AddQuestEntry(quest.QuestData, true);
            }
        }

        // Then add available quests
        foreach (var quest in availableQuests)
        {
            if (!QuestManager.Instance.IsQuestActive(quest.questId) &&
                (currentFilter == null || quest.questType == currentFilter))
            {
                AddQuestEntry(quest, false);
            }
        }
    }

    private void AddQuestEntry(QuestData questData, bool isActive)
    {
        var entry = questEntryTemplate.Instantiate();
        var questEntry = entry.Q("quest-entry");
        
        // Set up entry content
        entry.Q<Image>("quest-icon").sprite = questData.questIcon;
        entry.Q<Label>("quest-title").text = questData.displayName;
        entry.Q<Label>("quest-type").text = questData.questType.ToString();
        
        if (isActive)
        {
            var quest = QuestManager.Instance.GetQuest(questData.questId);
            entry.Q<ProgressBar>("quest-progress").value = quest.Progress;
            questEntry.AddToClassList("active");
        }
        
        if (QuestManager.Instance.IsQuestCompleted(questData.questId))
        {
            questEntry.AddToClassList("completed");
        }
        
        // Add click handler
        questEntry.RegisterCallback<ClickEvent>(evt => SelectQuest(questData.questId));
        
        questList.Add(entry);
        questEntries[questData.questId] = questEntry;
    }

    private void SelectQuest(string questId)
    {
        // Update selection visuals
        foreach (var entry in questEntries.Values)
        {
            entry.RemoveFromClassList("selected");
        }
        
        if (questEntries.TryGetValue(questId, out var selectedEntry))
        {
            selectedEntry.AddToClassList("selected");
        }

        selectedQuestId = questId;
        UpdateQuestDetails();
    }

    private void UpdateQuestDetails()
    {
        if (string.IsNullOrEmpty(selectedQuestId))
        {
            questDetails.RemoveFromClassList("visible");
            return;
        }

        questDetails.AddToClassList("visible");
        var questData = QuestManager.Instance.GetQuest(selectedQuestId)?.QuestData;
        if (questData == null) return;

        // Update header
        questDetails.Q<Image>("quest-icon").sprite = questData.questIcon;
        questDetails.Q<Label>("quest-title").text = questData.displayName;
        questDetails.Q<Label>("quest-type").text = questData.questType.ToString();
        questDetails.Q<Label>("quest-description").text = questData.description;

        // Update objectives
        var objectivesList = questDetails.Q<ScrollView>("objectives-list");
        objectivesList.Clear();

        if (QuestManager.Instance.IsQuestActive(selectedQuestId))
        {
            var quest = QuestManager.Instance.GetQuest(selectedQuestId);
            foreach (var objective in questData.objectives)
            {
                var objectiveEntry = objectiveEntryTemplate.Instantiate();
                objectiveEntry.Q<Label>("objective-text").text = objective.description;
                
                // Update progress
                var progress = quest.Progress;
                objectiveEntry.Q<Label>("objective-progress").text = 
                    $"{Mathf.RoundToInt(progress * 100)}%";
                
                if (progress >= 1f)
                {
                    objectiveEntry.AddToClassList("completed");
                }
                
                objectivesList.Add(objectiveEntry);
            }
        }

        // Update rewards
        var rewardsContainer = questDetails.Q("rewards-container");
        rewardsContainer.Clear();

        if (questData.experienceReward > 0)
        {
            AddRewardItem(rewardsContainer, null, "XP", questData.experienceReward);
        }
        
        if (questData.goldReward > 0)
        {
            AddRewardItem(rewardsContainer, null, "Gold", questData.goldReward);
        }

        foreach (var reward in questData.itemRewards)
        {
            AddRewardItem(
                rewardsContainer,
                reward.itemPrefab.GetComponent<SpriteRenderer>()?.sprite,
                reward.itemPrefab.name,
                reward.amount
            );
        }

        // Update buttons
        bool isActive = QuestManager.Instance.IsQuestActive(selectedQuestId);
        acceptButton.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
        abandonButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        trackButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void AddRewardItem(VisualElement container, Sprite icon, string name, int amount)
    {
        var rewardItem = new VisualElement();
        rewardItem.AddToClassList("reward-item");
        
        var rewardIcon = new Image();
        rewardIcon.AddToClassList("reward-icon");
        rewardIcon.sprite = icon;
        
        var rewardAmount = new Label($"{amount}x {name}");
        rewardAmount.AddToClassList("reward-amount");
        
        rewardItem.Add(rewardIcon);
        rewardItem.Add(rewardAmount);
        container.Add(rewardItem);
    }

    private void UpdateQuestProgress()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;

        var quest = QuestManager.Instance.GetQuest(selectedQuestId);
        if (quest == null) return;

        // Update progress bar
        var progressBar = questDetails.Q<ProgressBar>("progress-bar");
        progressBar.value = quest.Progress;

        // Update time remaining if applicable
        var timeLabel = questDetails.Q<Label>("time-remaining");
        if (quest.TimeRemaining > 0)
        {
            var timeSpan = System.TimeSpan.FromSeconds(quest.TimeRemaining);
            timeLabel.text = $"Time Remaining: {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            timeLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            timeLabel.style.display = DisplayStyle.None;
        }
    }

    private void AcceptSelectedQuest()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;
        
        if (QuestManager.Instance.AcceptQuest(selectedQuestId))
        {
            RefreshQuestList();
            UpdateQuestDetails();
        }
    }

    private void AbandonSelectedQuest()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;
        
        QuestManager.Instance.AbandonQuest(selectedQuestId);
        RefreshQuestList();
        UpdateQuestDetails();
    }

    private void ToggleTrackSelectedQuest()
    {
        if (string.IsNullOrEmpty(selectedQuestId)) return;
        
        var quest = QuestManager.Instance.GetQuest(selectedQuestId);
        if (quest == null) return;

        // Toggle quest tracking (implement in your navigation/minimap system)
        // GameManager.Instance.ToggleQuestTracking(selectedQuestId);
        
        trackButton.text = trackButton.text == "Track Quest" ? "Untrack Quest" : "Track Quest";
    }

    #region Event Handlers
    private void HandleQuestAccepted(string questId)
    {
        RefreshQuestList();
        if (selectedQuestId == questId)
        {
            UpdateQuestDetails();
        }
    }

    private void HandleQuestCompleted(string questId)
    {
        RefreshQuestList();
        if (selectedQuestId == questId)
        {
            UpdateQuestDetails();
        }
    }

    private void HandleQuestFailed(string questId)
    {
        RefreshQuestList();
        if (selectedQuestId == questId)
        {
            UpdateQuestDetails();
        }
    }

    private void HandleQuestAbandoned(string questId)
    {
        RefreshQuestList();
        if (selectedQuestId == questId)
        {
            UpdateQuestDetails();
        }
    }
    #endregion
} 