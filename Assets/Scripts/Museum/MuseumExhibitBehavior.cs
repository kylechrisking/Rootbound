using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class MuseumExhibitBehavior : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private ParticleSystem bonusParticles;
    [SerializeField] private Light exhibitLight;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.2f;
    
    [Header("Animation")]
    [SerializeField] private bool rotateExhibit;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private Vector2 floatRange = new Vector2(-0.1f, 0.1f);
    [SerializeField] private float floatSpeed = 1f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float bonusMultiplier = 1f;
    private bool hasBonus;
    private Material originalMaterial;
    private Material instanceMaterial;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // Create instance material to avoid affecting other exhibits
        if (spriteRenderer.material != null)
        {
            originalMaterial = spriteRenderer.material;
            instanceMaterial = new Material(originalMaterial);
            spriteRenderer.material = instanceMaterial;
        }
    }

    private void Start()
    {
        if (exhibitLight != null)
        {
            exhibitLight.intensity = 0f;
        }
    }

    private void Update()
    {
        if (rotateExhibit)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Floating animation
        if (floatRange != Vector2.zero)
        {
            float yOffset = Mathf.Lerp(floatRange.x, floatRange.y, 
                (Mathf.Sin(Time.time * floatSpeed) + 1f) * 0.5f);
            transform.position = startPosition + Vector3.up * yOffset;
        }

        // Bonus visual effects
        if (hasBonus)
        {
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
            
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat("_Glow", pulse);
            }
            
            if (exhibitLight != null)
            {
                exhibitLight.intensity = pulse * bonusMultiplier;
            }
        }
    }

    public void ApplyBonus(float multiplier)
    {
        bonusMultiplier = multiplier;
        hasBonus = true;

        // Enable particle effects
        if (bonusParticles != null)
        {
            var emission = bonusParticles.emission;
            emission.rateOverTime = 10f * multiplier;
            bonusParticles.Play();
        }

        // Enable light
        if (exhibitLight != null)
        {
            exhibitLight.enabled = true;
            exhibitLight.intensity = bonusMultiplier;
        }

        // Apply glow effect
        if (instanceMaterial != null)
        {
            instanceMaterial.EnableKeyword("_EMISSION");
            Color emissionColor = Color.white * bonusMultiplier;
            instanceMaterial.SetColor("_EmissionColor", emissionColor);
        }

        StartCoroutine(BonusAppliedEffect());
    }

    private IEnumerator BonusAppliedEffect()
    {
        // Scale up effect
        Vector3 originalScale = transform.localScale;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void RemoveBonus()
    {
        bonusMultiplier = 1f;
        hasBonus = false;

        // Disable particle effects
        if (bonusParticles != null)
        {
            bonusParticles.Stop();
        }

        // Disable light
        if (exhibitLight != null)
        {
            exhibitLight.enabled = false;
        }

        // Remove glow effect
        if (instanceMaterial != null)
        {
            instanceMaterial.DisableKeyword("_EMISSION");
            instanceMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    private void OnDestroy()
    {
        // Clean up instance material
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }

    private void OnValidate()
    {
        // Ensure float range is properly ordered
        if (floatRange.x > floatRange.y)
        {
            float temp = floatRange.x;
            floatRange.x = floatRange.y;
            floatRange.y = temp;
        }
    }
} 