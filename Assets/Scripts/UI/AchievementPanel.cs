using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AchievementPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject achievementEntryPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private Toggle hideCompletedToggle;
    [SerializeField] private TextMeshProUGUI completionText;
    
    [Header("Layout Settings")]
    [SerializeField] private float entrySpacing = 5f;
    [SerializeField] private Vector2 entrySize = new Vector2(800f, 100f);

    private List<AchievementEntryUI> achievementEntries = new List<AchievementEntryUI>();
    private string currentCategory = "All";
    private bool hideCompleted;

    private void Start()
    {
        InitializeUI();
        PopulateAchievements();
        
        // Listen for achievement updates
        AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
        AchievementManager.Instance.OnProgressUpdated += OnProgressUpdated;
    }

    private void InitializeUI()
    {
        // Setup category dropdown
        categoryDropdown.ClearOptions();
        List<string> categories = new List<string> { "All", "Growth", "Resources", "Exploration", "Skills" };
        categoryDropdown.AddOptions(categories);
        categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);

        // Setup toggle
        hideCompletedToggle.onValueChanged.AddListener(OnHideCompletedChanged);
    }

    private void PopulateAchievements()
    {
        // Clear existing entries
        foreach (var entry in achievementEntries)
        {
            Destroy(entry.gameObject);
        }
        achievementEntries.Clear();

        // Get achievements
        var achievements = AchievementManager.Instance.GetAllAchievements();
        
        // Create entries
        float yPosition = 0f;
        foreach (var achievement in achievements)
        {
            if (achievement.data.isHidden && !achievement.isUnlocked) continue;
            if (hideCompleted && achievement.isUnlocked) continue;
            if (currentCategory != "All" && !achievement.data.id.StartsWith(currentCategory)) continue;

            GameObject entryObj = Instantiate(achievementEntryPrefab, contentPanel);
            RectTransform rectTransform = entryObj.GetComponent<RectTransform>();
            
            // Position entry
            rectTransform.anchoredPosition = new Vector2(0f, -yPosition);
            rectTransform.sizeDelta = entrySize;
            
            // Setup entry
            AchievementEntryUI entry = entryObj.GetComponent<AchievementEntryUI>();
            entry.Initialize(achievement);
            achievementEntries.Add(entry);
            
            yPosition += entrySize.y + entrySpacing;
        }

        // Update content size
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, yPosition);
        
        // Update completion text
        UpdateCompletionText();
    }

    private void OnCategoryChanged(int index)
    {
        currentCategory = categoryDropdown.options[index].text;
        PopulateAchievements();
    }

    private void OnHideCompletedChanged(bool hide)
    {
        hideCompleted = hide;
        PopulateAchievements();
    }

    private void OnAchievementUnlocked(AchievementManager.Achievement achievement)
    {
        PopulateAchievements();
    }

    private void OnProgressUpdated(AchievementManager.Achievement achievement)
    {
        var entry = achievementEntries.FirstOrDefault(e => e.Achievement.data.id == achievement.data.id);
        entry?.UpdateProgress();
    }

    private void UpdateCompletionText()
    {
        var achievements = AchievementManager.Instance.GetAllAchievements();
        int total = achievements.Length;
        int completed = achievements.Count(a => a.isUnlocked);
        
        completionText.text = $"Completion: {completed}/{total} ({(completed * 100f / total):F1}%)";
    }

    private void OnDestroy()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            AchievementManager.Instance.OnProgressUpdated -= OnProgressUpdated;
        }
    }
} 