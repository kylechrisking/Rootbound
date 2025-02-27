using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Tutorial Objective", menuName = "Rootbound/Tutorial/Objective")]
public class TutorialObjective : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string title;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    
    [Header("Requirements")]
    public string[] requiredTutorials;
    public string[] requiredObjectives;
    
    [Header("Rewards")]
    public int skillPointReward;
    public float growthBoostReward;
    public ResourceReward[] resourceRewards;

    [Header("Optional Settings")]
    public bool isHidden = false;
    public bool autoComplete = true;
    public string[] triggerTutorialsOnComplete;

    [Serializable]
    public struct ResourceReward
    {
        public ResourceType type;
        public float amount;
    }
} 