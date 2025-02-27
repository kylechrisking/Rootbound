#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AchievementCreator : EditorWindow
{
    [MenuItem("Rootbound/Create Example Achievements")]
    public static void CreateExampleAchievements()
    {
        CreateGrowthAchievements();
        CreateResourceAchievements();
        CreateExplorationAchievements();
        CreateSkillAchievements();
    }

    private static void CreateGrowthAchievements()
    {
        CreateAchievement(
            "FirstSprout",
            "First Steps",
            "Reach growth level 2",
            2f, // progressTarget
            1, // skillPointReward
            hasProgress: true
        );

        CreateAchievement(
            "MightyOak",
            "Mighty Oak",
            "Reach maximum growth level",
            10f,
            5,
            growthBoostReward: 0.2f,
            hasProgress: true
        );

        CreateAchievement(
            "RapidGrowth",
            "Rapid Growth",
            "Grow three levels in under a minute",
            isHidden: true
        );
    }

    private static void CreateResourceAchievements()
    {
        CreateAchievement(
            "WaterHoarder",
            "Water Hoarder",
            "Collect 1000 units of water",
            1000f,
            2,
            hasProgress: true
        );

        CreateAchievement(
            "NutrientMaster",
            "Nutrient Master",
            "Collect 1000 units of nutrients",
            1000f,
            2,
            hasProgress: true
        );

        CreateAchievement(
            "SunSeeker",
            "Sun Seeker",
            "Maintain maximum sunlight absorption for 5 minutes",
            300f,
            2,
            hasProgress: true
        );
    }

    private static void CreateExplorationAchievements()
    {
        CreateAchievement(
            "DeepDiver",
            "Deep Diver",
            "Reach the deepest soil layer",
            skillPointReward: 3,
            isHidden: true
        );

        CreateAchievement(
            "Networker",
            "Root Network",
            "Create 10 root branches",
            10f,
            2,
            hasProgress: true
        );

        CreateAchievement(
            "Discoverer",
            "Resource Explorer",
            "Discover all types of resource deposits",
            5f,
            3,
            hasProgress: true
        );
    }

    private static void CreateSkillAchievements()
    {
        CreateAchievement(
            "FirstSkill",
            "Adaptation",
            "Unlock your first skill",
            1,
            isHidden: false
        );

        CreateAchievement(
            "SkillMaster",
            "Master of Growth",
            "Unlock all basic skills",
            3f,
            5,
            hasProgress: true
        );

        CreateAchievement(
            "Specialist",
            "Specialization",
            "Max out one skill tree branch",
            skillPointReward: 3,
            unlocksSpecialAbility: true
        );
    }

    private static void CreateAchievement(
        string id, 
        string title, 
        string description, 
        float progressTarget = 1f,
        int skillPointReward = 0,
        float growthBoostReward = 0f,
        bool unlocksSpecialAbility = false,
        bool isHidden = false,
        bool hasProgress = false)
    {
        AchievementData achievement = ScriptableObject.CreateInstance<AchievementData>();
        
        achievement.id = id;
        achievement.title = title;
        achievement.description = description;
        achievement.progressTarget = progressTarget;
        achievement.skillPointReward = skillPointReward;
        achievement.growthBoostReward = growthBoostReward;
        achievement.unlocksSpecialAbility = unlocksSpecialAbility;
        achievement.isHidden = isHidden;
        achievement.hasProgress = hasProgress;

        string path = $"Assets/ScriptableObjects/Achievements/{id}.asset";
        AssetDatabase.CreateAsset(achievement, path);
        AssetDatabase.SaveAssets();
    }
}
#endif 