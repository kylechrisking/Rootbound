using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TutorialTipsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tipEntryPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI completionText;
    
    [Header("Categories")]
    [SerializeField] private string[] categories = { "All", "Basic", "Resources", "Skills", "Advanced" };
    
    private List<TipEntryUI> tipEntries = new List<TipEntryUI>();
    private string currentCategory = "All";

    private void Start()
    {
        InitializeUI();
        PopulateTips();
        
        // Subscribe to tutorial events
        TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
    }

    private void InitializeUI()
    {
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(categories.ToList());
        categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
        
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void PopulateTips()
    {
        // Clear existing entries
        foreach (var entry in tipEntries)
        {
            Destroy(entry.gameObject);
        }
        tipEntries.Clear();

        var tutorials = TutorialManager.Instance.GetAllTutorials();
        float yPosition = 0f;

        foreach (var tutorial in tutorials)
        {
            // Skip hidden tutorials that haven't been discovered
            if (!TutorialManager.Instance.HasCompletedTutorial(tutorial.id) && 
                !ShouldShowTutorial(tutorial)) continue;

            // Filter by category
            if (currentCategory != "All" && !tutorial.id.StartsWith(currentCategory.ToLower())) continue;

            GameObject entryObj = Instantiate(tipEntryPrefab, contentPanel);
            RectTransform rectTransform = entryObj.GetComponent<RectTransform>();
            
            // Position entry
            rectTransform.anchoredPosition = new Vector2(0f, -yPosition);
            
            // Setup entry
            var entry = entryObj.GetComponent<TipEntryUI>();
            entry.Initialize(tutorial);
            tipEntries.Add(entry);
            
            yPosition += rectTransform.rect.height + 5f;
        }

        // Update content size
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, yPosition);
        
        UpdateCompletionText();
    }

    private bool ShouldShowTutorial(TutorialManager.TutorialStep tutorial)
    {
        // Show if no prerequisites or all prerequisites are completed
        if (tutorial.prerequisites == null || tutorial.prerequisites.Length == 0) return true;
        
        return tutorial.prerequisites.All(prereq => 
            TutorialManager.Instance.HasCompletedTutorial(prereq));
    }

    private void OnCategoryChanged(int index)
    {
        currentCategory = categories[index];
        PopulateTips();
    }

    private void OnTutorialCompleted(string tutorialId)
    {
        PopulateTips();
    }

    private void UpdateCompletionText()
    {
        var tutorials = TutorialManager.Instance.GetAllTutorials();
        int total = tutorials.Length;
        int completed = tutorials.Count(t => 
            TutorialManager.Instance.HasCompletedTutorial(t.id));
        
        completionText.text = $"Tutorial Progress: {completed}/{total} ({(completed * 100f / total):F1}%)";
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialCompleted -= OnTutorialCompleted;
        }
    }
} 