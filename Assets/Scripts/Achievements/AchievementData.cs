using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement", menuName = "Rootbound/Achievement")]
public class AchievementData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string title;
    public string description;
    public Sprite icon;
    
    [Header("Rewards")]
    public int skillPointReward;
    public float growthBoostReward;
    public bool unlocksSpecialAbility;
    
    [Header("Progress")]
    public bool isHidden = false;
    public bool hasProgress = false;
    public float progressTarget;
    public string progressFormat = "{0}/{1}"; // Example: "5/10"
} 