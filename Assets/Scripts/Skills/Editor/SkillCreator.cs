#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SkillCreator : EditorWindow
{
    [MenuItem("Rootbound/Create Example Skills")]
    public static void CreateExampleSkills()
    {
        CreateBasicSkills();
        CreateAdvancedSkills();
        CreateSpecializations();
    }

    private static void CreateBasicSkills()
    {
        CreateSkill("WaterAbsorption", "Basic Water Absorption", 
            "Improves water absorption rate by 25%.", 1, 1, 
            resourceMultiplier: 1.25f);

        CreateSkill("NutrientEfficiency", "Nutrient Efficiency", 
            "Reduces nutrient consumption rate by 20%.", 1, 1);

        CreateSkill("SunlightGathering", "Sunlight Gathering", 
            "Increases sunlight absorption by 25%.", 1, 1, 
            resourceMultiplier: 1.25f);
    }

    private static void CreateAdvancedSkills()
    {
        CreateSkill("WaterStorage", "Water Storage", 
            "Enables water storage for later use.", 2, 2, 
            prerequisites: new[] { "WaterAbsorption" }, 
            enablesWaterStorage: true);

        CreateSkill("DeepRoots", "Deep Roots", 
            "Increases resource detection range by 50%.", 2, 2, 
            prerequisites: new[] { "NutrientEfficiency" }, 
            resourceDetectionRange: 1.5f);

        CreateSkill("RootBranching", "Root Branching", 
            "Allows roots to split into multiple branches.", 3, 2, 
            prerequisites: new[] { "DeepRoots" }, 
            enablesRootBranching: true);
    }

    private static void CreateSpecializations()
    {
        CreateSkill("HydroMastery", "Hydro Mastery", 
            "Doubles water absorption and storage capacity.", 4, 3, 
            prerequisites: new[] { "WaterStorage" }, 
            resourceMultiplier: 2f);

        CreateSkill("MineralSensing", "Mineral Sensing", 
            "Enables detection of rare mineral deposits.", 4, 3, 
            prerequisites: new[] { "DeepRoots" });

        CreateSkill("RootNetwork", "Root Network", 
            "Creates an interconnected network of roots for resource sharing.", 5, 4, 
            prerequisites: new[] { "RootBranching", "HydroMastery" });
    }

    private static void CreateSkill(string id, string name, string description, 
        int levelRequired, int cost, float resourceMultiplier = 1f, 
        float growthRateBonus = 0f, float rootRangeBonus = 0f, 
        float resourceDetectionRange = 0f, string[] prerequisites = null,
        bool enablesWaterStorage = false, bool enablesNutrientStorage = false,
        bool enablesRootBranching = false, bool enablesAutoAbsorb = false)
    {
        SkillData skill = ScriptableObject.CreateInstance<SkillData>();
        skill.skillName = name;
        skill.description = description;
        skill.levelRequired = levelRequired;
        skill.skillPointCost = cost;
        skill.prerequisiteSkills = prerequisites ?? new string[0];
        skill.resourceMultiplier = resourceMultiplier;
        skill.growthRateBonus = growthRateBonus;
        skill.rootRangeBonus = rootRangeBonus;
        skill.resourceDetectionRange = resourceDetectionRange;
        skill.enablesWaterStorage = enablesWaterStorage;
        skill.enablesNutrientStorage = enablesNutrientStorage;
        skill.enablesRootBranching = enablesRootBranching;
        skill.enablesAutoAbsorb = enablesAutoAbsorb;

        string path = $"Assets/ScriptableObjects/Skills/{id}.asset";
        AssetDatabase.CreateAsset(skill, path);
        AssetDatabase.SaveAssets();
    }
}
#endif 