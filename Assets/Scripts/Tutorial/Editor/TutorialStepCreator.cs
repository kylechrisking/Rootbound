#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TutorialStepCreator : EditorWindow
{
    [MenuItem("Rootbound/Create Tutorial Steps")]
    public static void CreateTutorialSteps()
    {
        CreateBasicTutorials();
        CreateResourceTutorials();
        CreateSkillTutorials();
        CreateAdvancedTutorials();
    }

    private static void CreateBasicTutorials()
    {
        var steps = new[] {
            new TutorialManager.TutorialStep {
                id = "welcome",
                title = "Welcome to Rootbound!",
                message = "Guide your sapling to grow and thrive in this challenging environment.",
                isRequired = true,
                triggerDelay = 0f
            },
            new TutorialManager.TutorialStep {
                id = "basic_movement",
                title = "Growing Roots",
                message = "Click and drag to grow your roots in any direction.",
                isRequired = true,
                prerequisites = new[] { "welcome" },
                triggerDelay = 2f
            },
            new TutorialManager.TutorialStep {
                id = "resources_intro",
                title = "Resource Management",
                message = "You'll need water, nutrients, and sunlight to grow. Watch your resource levels!",
                isRequired = true,
                prerequisites = new[] { "basic_movement" },
                triggerDelay = 5f
            }
        };

        SaveTutorialSteps(steps, "BasicTutorials");
    }

    private static void CreateResourceTutorials()
    {
        var steps = new[] {
            new TutorialManager.TutorialStep {
                id = "water_gathering",
                title = "Finding Water",
                message = "Blue deposits contain water. Grow roots near them to absorb water.",
                prerequisites = new[] { "resources_intro" },
                triggerDelay = 2f
            },
            new TutorialManager.TutorialStep {
                id = "nutrient_gathering",
                title = "Gathering Nutrients",
                message = "Brown deposits are rich in nutrients. Absorb them to stay healthy.",
                prerequisites = new[] { "resources_intro" },
                triggerDelay = 4f
            },
            new TutorialManager.TutorialStep {
                id = "sunlight_tip",
                title = "Sunlight",
                message = "Grow towards the surface to gather more sunlight!",
                prerequisites = new[] { "resources_intro" },
                triggerDelay = 6f
            }
        };

        SaveTutorialSteps(steps, "ResourceTutorials");
    }

    private static void CreateSkillTutorials()
    {
        var steps = new[] {
            new TutorialManager.TutorialStep {
                id = "skills_unlocked",
                title = "Skills Available",
                message = "You've earned skill points! Open the skill tree to unlock new abilities.",
                isRequired = true,
                prerequisites = new[] { "resources_intro" },
                triggerDelay = 10f
            },
            new TutorialManager.TutorialStep {
                id = "skill_usage",
                title = "Using Skills",
                message = "Some skills are passive, while others need to be activated.",
                prerequisites = new[] { "skills_unlocked" },
                triggerDelay = 2f
            }
        };

        SaveTutorialSteps(steps, "SkillTutorials");
    }

    private static void CreateAdvancedTutorials()
    {
        var steps = new[] {
            new TutorialManager.TutorialStep {
                id = "storage_unlocked",
                title = "Resource Storage",
                message = "You can now store excess resources for later use!",
                prerequisites = new[] { "skills_unlocked" },
                triggerDelay = 0f
            },
            new TutorialManager.TutorialStep {
                id = "branching_unlocked",
                title = "Root Branching",
                message = "Your roots can now split into multiple branches!",
                prerequisites = new[] { "skills_unlocked" },
                triggerDelay = 0f
            }
        };

        SaveTutorialSteps(steps, "AdvancedTutorials");
    }

    private static void SaveTutorialSteps(TutorialManager.TutorialStep[] steps, string fileName)
    {
        var container = ScriptableObject.CreateInstance<TutorialStepContainer>();
        container.steps = steps;

        string path = $"Assets/ScriptableObjects/Tutorials/{fileName}.asset";
        AssetDatabase.CreateAsset(container, path);
        AssetDatabase.SaveAssets();
    }
}
#endif 