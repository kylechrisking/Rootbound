using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    private static SkillManager _instance;
    public static SkillManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SkillManager>();
            }
            return _instance;
        }
    }

    [Header("Skill Settings")]
    [SerializeField] private SkillData[] availableSkills;
    
    private Dictionary<string, bool> unlockedSkills = new Dictionary<string, bool>();
    private PlayerStats playerStats;
    private int currentSkillPoints;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSkills();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void InitializeSkills()
    {
        foreach (SkillData skill in availableSkills)
        {
            unlockedSkills[skill.skillName] = false;
        }
    }

    public bool CanUnlockSkill(string skillName)
    {
        SkillData skill = GetSkillData(skillName);
        if (skill == null) return false;

        // Check requirements
        if (currentSkillPoints < skill.skillPointCost) return false;
        if (playerStats.GetGrowthLevel() < skill.levelRequired) return false;

        // Check prerequisites
        foreach (string prereq in skill.prerequisiteSkills)
        {
            if (!IsSkillUnlocked(prereq)) return false;
        }

        return true;
    }

    public bool UnlockSkill(string skillName)
    {
        if (!CanUnlockSkill(skillName)) return false;

        SkillData skill = GetSkillData(skillName);
        currentSkillPoints -= skill.skillPointCost;
        unlockedSkills[skillName] = true;

        // Apply skill effects
        ApplySkillEffects(skill);

        return true;
    }

    private void ApplySkillEffects(SkillData skill)
    {
        if (playerStats == null) return;

        playerStats.AddResourceMultiplier(skill.resourceMultiplier);
        playerStats.AddGrowthRateBonus(skill.growthRateBonus);
        
        // Apply special effects
        if (skill.enablesWaterStorage)
            playerStats.EnableWaterStorage();
        if (skill.enablesNutrientStorage)
            playerStats.EnableNutrientStorage();
        // Add more special effects as needed
    }

    public bool IsSkillUnlocked(string skillName)
    {
        return unlockedSkills.ContainsKey(skillName) && unlockedSkills[skillName];
    }

    private SkillData GetSkillData(string skillName)
    {
        return System.Array.Find(availableSkills, skill => skill.skillName == skillName);
    }

    public void AddSkillPoints(int amount)
    {
        currentSkillPoints += amount;
    }

    public int GetCurrentSkillPoints()
    {
        return currentSkillPoints;
    }

    // Save/Load skill data
    public void SaveSkillData(GameData data)
    {
        data.unlockedSkills = new bool[availableSkills.Length];
        for (int i = 0; i < availableSkills.Length; i++)
        {
            data.unlockedSkills[i] = unlockedSkills[availableSkills[i].skillName];
        }
        data.skillPoints = currentSkillPoints;
    }

    public void LoadSkillData(GameData data)
    {
        for (int i = 0; i < data.unlockedSkills.Length; i++)
        {
            if (i < availableSkills.Length)
            {
                unlockedSkills[availableSkills[i].skillName] = data.unlockedSkills[i];
                if (data.unlockedSkills[i])
                {
                    ApplySkillEffects(availableSkills[i]);
                }
            }
        }
        currentSkillPoints = data.skillPoints;
    }
} 