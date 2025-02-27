using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementNotification : NotificationBase
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private GameObject rewardContainer;
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private Color defaultIconColor = Color.white;

    public void Initialize(AchievementManager.Achievement achievement)
    {
        Duration = displayDuration;

        // Set icon
        if (achievement.data.icon != null)
        {
            iconImage.sprite = achievement.data.icon;
            iconImage.color = defaultIconColor;
        }

        // Set texts
        titleText.text = achievement.data.title;
        descriptionText.text = achievement.data.description;

        // Set rewards
        string rewardString = "";
        if (achievement.data.skillPointReward > 0)
        {
            rewardString += $"+{achievement.data.skillPointReward} Skill Points";
        }
        if (achievement.data.growthBoostReward > 0)
        {
            if (rewardString != "") rewardString += "\n";
            rewardString += $"+{achievement.data.growthBoostReward * 100}% Growth Rate";
        }

        if (!string.IsNullOrEmpty(rewardString))
        {
            rewardText.text = rewardString;
            rewardContainer.SetActive(true);
        }
        else
        {
            rewardContainer.SetActive(false);
        }
    }
} 