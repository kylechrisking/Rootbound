using UnityEngine;
using System;

[Serializable]
public class CombatStats
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float attackPower = 10f;
    public float defense = 5f;
    public float attackSpeed = 1f;
    public float criticalChance = 0.05f;
    public float criticalMultiplier = 1.5f;
    
    [Header("Combat Properties")]
    public float attackRange = 1.5f;
    public float knockbackForce = 5f;
    public float stunDuration = 0.2f;
    public bool canBeStunned = true;
    
    [Header("Advanced Properties")]
    public DamageType[] resistances;
    public DamageType[] weaknesses;
    public float[] resistanceValues;
    public float[] weaknessValues;
    
    // Runtime stats
    private float currentHealth;
    private float lastAttackTime;
    private bool isStunned;
    private float stunEndTime;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsStunned => isStunned && Time.time < stunEndTime;
    public float HealthPercentage => currentHealth / maxHealth;

    public void Initialize()
    {
        currentHealth = maxHealth;
        lastAttackTime = -attackSpeed; // Allow immediate first attack
        isStunned = false;
    }

    public bool CanAttack()
    {
        return !IsStunned && Time.time >= lastAttackTime + attackSpeed;
    }

    public void RecordAttack()
    {
        lastAttackTime = Time.time;
    }

    public float CalculateDamage(float baseDamage, DamageType damageType)
    {
        float multiplier = 1f;

        // Check resistances
        for (int i = 0; i < resistances.Length; i++)
        {
            if (resistances[i] == damageType)
            {
                multiplier *= (1f - resistanceValues[i]);
                break;
            }
        }

        // Check weaknesses
        for (int i = 0; i < weaknesses.Length; i++)
        {
            if (weaknesses[i] == damageType)
            {
                multiplier *= (1f + weaknessValues[i]);
                break;
            }
        }

        float damage = baseDamage * multiplier;
        if (defense > 0)
        {
            damage = damage * (100f / (100f + defense));
        }

        return Mathf.Max(1f, damage); // Minimum 1 damage
    }

    public DamageResult TakeDamage(float amount, DamageType damageType, bool canCrit = true)
    {
        if (IsDead) return new DamageResult { wasFatal = true };

        float finalDamage = CalculateDamage(amount, damageType);
        bool isCritical = canCrit && UnityEngine.Random.value < criticalChance;
        
        if (isCritical)
        {
            finalDamage *= criticalMultiplier;
        }

        currentHealth -= finalDamage;
        bool fatal = currentHealth <= 0;
        currentHealth = Mathf.Max(0, currentHealth);

        return new DamageResult
        {
            damageDealt = finalDamage,
            wasCritical = isCritical,
            wasFatal = fatal
        };
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void ApplyStun(float duration)
    {
        if (!canBeStunned) return;
        isStunned = true;
        stunEndTime = Time.time + duration;
    }

    public void RemoveStun()
    {
        isStunned = false;
        stunEndTime = Time.time;
    }
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Magic
}

public struct DamageResult
{
    public float damageDealt;
    public bool wasCritical;
    public bool wasFatal;
} 