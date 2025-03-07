using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Critter", menuName = "Rootbound/Critter")]
public class CritterData : ScriptableObject
{
    [Header("Basic Info")]
    public string critterId;
    public string displayName;
    [TextArea(3, 6)]
    public string description;
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    public CritterType type;
    public CritterRarity rarity;
    
    [Header("Spawn Settings")]
    public BiomeType[] validBiomes;
    public bool isNocturnal;
    public Season[] activeSeasons;
    public float spawnWeight = 1f;
    public Vector2 spawnTimeRange = new Vector2(6, 18); // 24-hour format
    
    [Header("Behavior")]
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public float fleeRange = 3f;
    public bool isShy = true;
    public float interactionCooldown = 1f;
    public CritterBehavior defaultBehavior;
    public CritterInterest[] interests;
    
    [Header("Collection")]
    public bool canBeCaught;
    public float catchDifficulty = 1f;
    public string[] requiredTools;
    public ItemData[] possibleDrops;
    public float[] dropChances;
    
    [Header("Museum")]
    public bool isMuseumExhibit;
    public string exhibitDescription;
    public MuseumCategory exhibitCategory;
    public Vector2Int exhibitSize = new Vector2Int(1, 1);
    public GameObject exhibitPrefab;
    
    [Header("Effects")]
    public GameObject catchEffectPrefab;
    public GameObject fleeEffectPrefab;
    public AudioClip[] idleSounds;
    public AudioClip[] fleeSounds;
    public AudioClip catchSound;
    public float soundInterval = 5f;
}

[Serializable]
public class CritterInterest
{
    public InterestType type;
    public string targetId; // Resource/plant/item ID
    public float attractionRange = 3f;
    public float interestDuration = 5f;
    public GameObject interactionEffectPrefab;
    public AnimationClip interactionAnimation;
}

public enum CritterType
{
    Insect,
    Bird,
    Mammal,
    Amphibian,
    Reptile,
    Fish,
    Mythical
}

public enum CritterRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum CritterBehavior
{
    Wander,
    Stationary,
    Patrol,
    Follow,
    Territorial
}

public enum InterestType
{
    Resource,
    Plant,
    Light,
    Water,
    Item,
    Other
}

public enum MuseumCategory
{
    Forest,
    Desert,
    Aquatic,
    Underground,
    Mythical,
    Seasonal
} 