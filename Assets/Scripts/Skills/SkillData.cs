using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Rootbound/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public string description;
    public Sprite icon;
    
    [Header("Requirements")]
    public int levelRequired = 1;
    public int skillPointCost = 1;
    public string[] prerequisiteSkills; // Names of required skills
    
    [Header("Effects")]
    public float resourceMultiplier = 1f;
    public float growthRateBonus = 0f;
    public float rootRangeBonus = 0f;
    public float resourceDetectionRange = 0f;
    
    [Header("Special Effects")]
    public bool enablesWaterStorage = false;
    public bool enablesNutrientStorage = false;
    public bool enablesRootBranching = false;
    public bool enablesAutoAbsorb = false;
} 