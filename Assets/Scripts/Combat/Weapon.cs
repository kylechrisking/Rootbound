using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Rootbound/Weapon")]
public class Weapon : Tool
{
    [Header("Combat Properties")]
    public float baseDamage = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 1.5f;
    public DamageType damageType = DamageType.Physical;
    public bool canCritical = true;
    
    [Header("Attack Pattern")]
    public float attackAngle = 90f;
    public int comboSteps = 1;
    public float[] comboMultipliers = new float[] { 1f };
    public float comboCooldown = 1f;
    
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public GameObject swingEffectPrefab;
    public AudioClip swingSound;
    public AudioClip hitSound;
    public float screenShakeAmount = 0.1f;
    
    [Header("Special Properties")]
    public bool canBeCharged;
    public float maxChargeTime = 2f;
    public float chargedDamageMultiplier = 2f;
    public GameObject chargeEffectPrefab;
    public AudioClip chargeSound;
    
    [Header("Status Effects")]
    public StatusEffectData[] statusEffects;
    public float[] statusEffectChances;

    public override string GetTooltip()
    {
        string tooltip = base.GetTooltip();
        
        tooltip += $"\nDamage: {baseDamage}";
        tooltip += $"\nAttack Speed: {attackSpeed}/s";
        tooltip += $"\nDamage Type: {damageType}";
        
        if (statusEffects != null && statusEffects.Length > 0)
        {
            tooltip += "\n\nStatus Effects:";
            for (int i = 0; i < statusEffects.Length; i++)
            {
                tooltip += $"\n- {statusEffects[i].effectName} ({statusEffectChances[i] * 100}%)";
            }
        }
        
        if (canBeCharged)
        {
            tooltip += $"\n\nCan be charged (up to {chargedDamageMultiplier}x damage)";
        }
        
        return tooltip;
    }
}

[System.Serializable]
public class StatusEffectData
{
    public string effectName;
    public float duration;
    public float tickRate;
    public float damagePerTick;
    public GameObject effectPrefab;
    public AudioClip effectSound;
    public Color effectColor = Color.white;
} 