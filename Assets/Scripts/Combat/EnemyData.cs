using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Rootbound/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public string description;
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    
    [Header("Stats")]
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float defense = 5f;
    public float moveSpeed = 3f;
    public float attackCooldown = 1f;
    public float knockbackForce = 5f;
    public DamageType damageType = DamageType.Physical;
    
    [Header("Rewards")]
    public int experienceValue = 10;
    public int goldValue = 5;
    public LootDrop[] possibleDrops;
    
    [Header("Effects")]
    public GameObject attackEffectPrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    
    [Header("Spawn Rules")]
    public bool isBoss;
    public int minPlayerLevel;
    public string[] requiredConditions;
    public float spawnWeight = 1f;
    public BiomeType[] allowedBiomes;
    
    [Header("Advanced Properties")]
    public bool isAggressive = true;
    public bool canBeStunned = true;
    public DamageType[] resistances;
    public DamageType[] weaknesses;
    public float[] resistanceValues;
    public float[] weaknessValues;
}

[System.Serializable]
public class LootDrop
{
    public GameObject itemPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;
    public int minAmount = 1;
    public int maxAmount = 1;
}

public enum BiomeType
{
    Forest,
    Desert,
    Swamp,
    Mountain,
    Cave,
    Volcano,
    Crystal,
    Corrupted
} 