using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private TrailRenderer weaponTrail;
    
    [Header("Attack Settings")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float comboTimeWindow = 0.8f;
    
    private Weapon currentWeapon;
    private CombatStats combatStats;
    private int currentComboStep;
    private float lastAttackTime;
    private float chargeStartTime;
    private bool isCharging;
    private List<GameObject> activeEffects = new List<GameObject>();
    private Dictionary<GameObject, StatusEffectTracker> activeStatusEffects = new Dictionary<GameObject, StatusEffectTracker>();

    private void Awake()
    {
        combatStats = GetComponent<CombatStats>();
        if (combatStats == null)
        {
            combatStats = gameObject.AddComponent<CombatStats>();
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        
        // Update visuals
        if (weaponSprite != null && weapon.icon != null)
        {
            weaponSprite.sprite = weapon.icon;
        }
        
        // Update trail
        if (weaponTrail != null)
        {
            weaponTrail.enabled = false;
        }
        
        // Reset combat state
        currentComboStep = 0;
        lastAttackTime = -attackCooldown;
        isCharging = false;
    }

    public void StartCharging()
    {
        if (!currentWeapon || !currentWeapon.canBeCharged || isCharging) return;
        
        isCharging = true;
        chargeStartTime = Time.time;
        
        // Spawn charge effect
        if (currentWeapon.chargeEffectPrefab != null)
        {
            GameObject effect = Instantiate(currentWeapon.chargeEffectPrefab, weaponHolder.position, Quaternion.identity);
            effect.transform.parent = weaponHolder;
            activeEffects.Add(effect);
        }
        
        // Play charge sound
        if (currentWeapon.chargeSound != null)
        {
            AudioManager.Instance.PlaySFX(currentWeapon.chargeSound);
        }
    }

    public void ReleaseCharge()
    {
        if (!isCharging) return;
        
        float chargeTime = Time.time - chargeStartTime;
        float chargeRatio = Mathf.Clamp01(chargeTime / currentWeapon.maxChargeTime);
        float damageMultiplier = 1f + (currentWeapon.chargedDamageMultiplier - 1f) * chargeRatio;
        
        PerformAttack(damageMultiplier);
        
        // Clean up charge effects
        foreach (var effect in activeEffects)
        {
            if (effect != null)
                Destroy(effect);
        }
        activeEffects.Clear();
        
        isCharging = false;
    }

    public void TriggerAttack()
    {
        if (!CanAttack()) return;
        
        if (currentWeapon.canBeCharged)
        {
            StartCharging();
        }
        else
        {
            PerformAttack(1f);
        }
    }

    private bool CanAttack()
    {
        return currentWeapon != null && 
               Time.time >= lastAttackTime + attackCooldown &&
               !combatStats.IsStunned;
    }

    private void PerformAttack(float damageMultiplier = 1f)
    {
        lastAttackTime = Time.time;
        
        // Get combo multiplier
        float comboMultiplier = currentWeapon.comboMultipliers[currentComboStep];
        float finalDamage = currentWeapon.baseDamage * damageMultiplier * comboMultiplier;
        
        // Create attack hitbox
        Vector2 attackDirection = GetAttackDirection();
        Vector2 attackOrigin = (Vector2)weaponHolder.position;
        
        // Perform overlap check for hits
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackOrigin + attackDirection * currentWeapon.attackRange * 0.5f,
            currentWeapon.attackRange * 0.5f,
            targetLayers
        );

        bool hitSomething = false;
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            // Apply damage
            if (hit.TryGetComponent<CombatStats>(out var targetStats))
            {
                DamageResult result = targetStats.TakeDamage(
                    finalDamage,
                    currentWeapon.damageType,
                    currentWeapon.canCritical
                );
                
                if (result.damageDealt > 0)
                {
                    hitSomething = true;
                    OnHitTarget(hit.gameObject, result, attackDirection);
                }
            }
        }

        // Visual and audio effects
        PlayAttackEffects(hitSomething, attackDirection);
        
        // Update combo
        UpdateCombo();
    }

    private void OnHitTarget(GameObject target, DamageResult result, Vector2 direction)
    {
        // Spawn hit effect
        if (currentWeapon.hitEffectPrefab != null)
        {
            Instantiate(currentWeapon.hitEffectPrefab, target.transform.position, Quaternion.identity);
        }
        
        // Play hit sound
        if (currentWeapon.hitSound != null)
        {
            AudioManager.Instance.PlaySFX(currentWeapon.hitSound);
        }
        
        // Apply knockback
        if (target.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(direction * currentWeapon.knockbackForce, ForceMode2D.Impulse);
        }
        
        // Apply status effects
        ApplyStatusEffects(target);
        
        // Show damage number
        DamageNumberSystem.Show(
            result.damageDealt,
            target.transform.position,
            result.wasCritical
        );
        
        // Screen shake
        CameraController.Instance.AddTrauma(currentWeapon.screenShakeAmount);
    }

    private void ApplyStatusEffects(GameObject target)
    {
        if (currentWeapon.statusEffects == null) return;
        
        for (int i = 0; i < currentWeapon.statusEffects.Length; i++)
        {
            if (Random.value < currentWeapon.statusEffectChances[i])
            {
                var effectData = currentWeapon.statusEffects[i];
                
                // Check if target already has this effect
                if (!activeStatusEffects.TryGetValue(target, out var tracker))
                {
                    tracker = new StatusEffectTracker();
                    activeStatusEffects[target] = tracker;
                }
                
                // Add or refresh effect
                tracker.AddEffect(effectData, target.transform);
            }
        }
    }

    private void PlayAttackEffects(bool hitSomething, Vector2 direction)
    {
        // Swing effect
        if (currentWeapon.swingEffectPrefab != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Instantiate(
                currentWeapon.swingEffectPrefab,
                weaponHolder.position,
                Quaternion.Euler(0, 0, angle)
            );
        }
        
        // Swing sound
        if (currentWeapon.swingSound != null)
        {
            AudioManager.Instance.PlaySFX(currentWeapon.swingSound);
        }
        
        // Weapon trail
        if (weaponTrail != null)
        {
            StartCoroutine(ShowTrail());
        }
    }

    private IEnumerator ShowTrail()
    {
        weaponTrail.enabled = true;
        yield return new WaitForSeconds(0.1f);
        weaponTrail.enabled = false;
    }

    private void UpdateCombo()
    {
        currentComboStep = (currentComboStep + 1) % currentWeapon.comboSteps;
        
        if (currentComboStep == 0)
        {
            // End of combo, start cooldown
            lastAttackTime = Time.time + currentWeapon.comboCooldown;
        }
    }

    private Vector2 GetAttackDirection()
    {
        // This should be updated based on your input system
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePosition - (Vector2)transform.position).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentWeapon == null || weaponHolder == null) return;
        
        Gizmos.color = Color.red;
        Vector2 attackDirection = GetAttackDirection();
        Vector2 attackOrigin = (Vector2)weaponHolder.position;
        Gizmos.DrawWireSphere(
            attackOrigin + attackDirection * currentWeapon.attackRange * 0.5f,
            currentWeapon.attackRange * 0.5f
        );
    }
}

public class StatusEffectTracker
{
    private Dictionary<string, StatusEffect> activeEffects = new Dictionary<string, StatusEffect>();

    public void AddEffect(StatusEffectData data, Transform target)
    {
        if (activeEffects.TryGetValue(data.effectName, out var existingEffect))
        {
            // Refresh duration
            existingEffect.RefreshDuration();
        }
        else
        {
            // Create new effect
            var effect = new StatusEffect(data, target);
            activeEffects[data.effectName] = effect;
        }
    }

    public void Update()
    {
        var effectsToRemove = new List<string>();
        
        foreach (var effect in activeEffects.Values)
        {
            effect.Update();
            if (effect.IsExpired)
            {
                effectsToRemove.Add(effect.Name);
            }
        }
        
        foreach (var effectName in effectsToRemove)
        {
            activeEffects.Remove(effectName);
        }
    }
}

public class StatusEffect
{
    public string Name => data.effectName;
    public bool IsExpired => Time.time >= endTime;

    private StatusEffectData data;
    private Transform target;
    private float endTime;
    private float nextTickTime;
    private GameObject effectInstance;

    public StatusEffect(StatusEffectData data, Transform target)
    {
        this.data = data;
        this.target = target;
        
        RefreshDuration();
        
        if (data.effectPrefab != null)
        {
            effectInstance = GameObject.Instantiate(data.effectPrefab, target);
        }
        
        if (data.effectSound != null)
        {
            AudioManager.Instance.PlaySFX(data.effectSound);
        }
    }

    public void RefreshDuration()
    {
        endTime = Time.time + data.duration;
        nextTickTime = Time.time;
    }

    public void Update()
    {
        if (IsExpired)
        {
            if (effectInstance != null)
            {
                GameObject.Destroy(effectInstance);
            }
            return;
        }

        if (Time.time >= nextTickTime)
        {
            // Apply damage tick
            if (target.TryGetComponent<CombatStats>(out var stats))
            {
                stats.TakeDamage(data.damagePerTick, DamageType.Magic, false);
            }
            
            nextTickTime = Time.time + data.tickRate;
        }
    }
} 