using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardText;
    
    [Header("Visual States")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);

    public AchievementManager.Achievement Achievement { get; private set; }

    public void Initialize(AchievementManager.Achievement achievement)
    {
        Achievement = achievement;
        
        // Set basic info
        titleText.text = achievement.data.title;
        descriptionText.text = achievement.data.description;
        
        if (achievement.data.icon != null)
        {
            iconImage.sprite = achievement.data.icon;
        }

        // Set reward info
        string rewardString = "";
        if (achievement.data.skillPointReward > 0)
        {
            rewardString += $"+{achievement.data.skillPointReward} SP";
        }
        if (achievement.data.growthBoostReward > 0)
        {
            if (rewardString != "") rewardString += ", ";
            rewardString += $"+{achievement.data.growthBoostReward * 100}% Growth";
        }
        
        rewardText.text = rewardString;
        rewardIcon.SetActive(!string.IsNullOrEmpty(rewardString));

        UpdateProgress();
        UpdateVisualState();
    }

    public void UpdateProgress()
    {
        if (Achievement.data.hasProgress)
        {
            float progress = Achievement.currentProgress / Achievement.data.progressTarget;
            progressBar.fillAmount = progress;
            progressText.text = string.Format(Achievement.data.progressFormat, 
                Achievement.currentProgress, Achievement.data.progressTarget);
        }
        else
        {
            progressBar.fillAmount = Achievement.isUnlocked ? 1f : 0f;
            progressText.text = Achievement.isUnlocked ? "Completed!" : "Incomplete";
        }

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        Color targetColor = Achievement.isUnlocked ? unlockedColor : lockedColor;
        iconImage.color = targetColor;
        titleText.color = targetColor;
    }
} 