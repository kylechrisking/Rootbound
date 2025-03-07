using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Critter : MonoBehaviour
{
    public CritterData Data => data;
    public bool IsFleeing => isFleeing;
    public bool IsInteracting => currentInterest != null;

    [SerializeField] private CritterData data;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private ParticleSystem interactionParticles;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 spawnPoint;
    private Vector2 targetPosition;
    private float nextBehaviorTime;
    private float nextSoundTime;
    private bool isFleeing;
    private CritterInterest currentInterest;
    private Dictionary<string, float> interestCooldowns = new Dictionary<string, float>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        spawnPoint = transform.position;
        targetPosition = spawnPoint;
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError("No CritterData assigned to Critter!");
            return;
        }

        // Initialize animation controller
        if (data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }

        // Start behavior routine
        StartCoroutine(BehaviorRoutine());
        StartCoroutine(SoundRoutine());
    }

    private void Update()
    {
        if (!CanBeActive())
        {
            // Despawn if conditions are not met
            StartCoroutine(DespawnRoutine());
            return;
        }

        UpdateMovement();
        CheckForPlayer();
        CheckForInterests();
        UpdateAnimation();
    }

    private bool CanBeActive()
    {
        if (data.isNocturnal != GameManager.Instance.IsNightTime)
            return false;

        if (!data.activeSeasons.Contains(GameManager.Instance.CurrentSeason))
            return false;

        float currentHour = GameManager.Instance.CurrentTimeOfDay;
        return currentHour >= data.spawnTimeRange.x && currentHour <= data.spawnTimeRange.y;
    }

    private void UpdateMovement()
    {
        if (isFleeing || IsInteracting) return;

        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);

        if (distance > 0.1f)
        {
            rb.velocity = moveDirection * data.moveSpeed;
            
            // Flip sprite based on movement direction
            if (moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            
            if (Time.time >= nextBehaviorTime)
            {
                SetNewTarget();
            }
        }
    }

    private void CheckForPlayer()
    {
        if (!data.isShy || isFleeing) return;

        var player = PlayerController.Instance;
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= data.fleeRange)
            {
                StartFleeing(player.transform.position);
            }
        }
    }

    private void CheckForInterests()
    {
        if (isFleeing || IsInteracting) return;

        foreach (var interest in data.interests)
        {
            // Skip if on cooldown
            if (interestCooldowns.TryGetValue(interest.targetId, out float cooldownTime) && 
                Time.time < cooldownTime)
                continue;

            // Find potential targets
            switch (interest.type)
            {
                case InterestType.Resource:
                    CheckResourceInterest(interest);
                    break;
                case InterestType.Plant:
                    CheckPlantInterest(interest);
                    break;
                case InterestType.Water:
                    CheckWaterInterest(interest);
                    break;
                // Add other interest type checks
            }
        }
    }

    private void CheckResourceInterest(CritterInterest interest)
    {
        var resources = Physics2D.OverlapCircleAll(transform.position, interest.attractionRange);
        foreach (var collider in resources)
        {
            if (collider.TryGetComponent<Resource>(out var resource) && 
                resource.GetResourceType().ToString() == interest.targetId)
            {
                StartInteraction(interest, collider.transform.position);
                break;
            }
        }
    }

    private void CheckPlantInterest(CritterInterest interest)
    {
        // Implement plant interaction check
    }

    private void CheckWaterInterest(CritterInterest interest)
    {
        // Implement water source check
    }

    private void StartInteraction(CritterInterest interest, Vector2 position)
    {
        currentInterest = interest;
        targetPosition = position;
        
        if (interest.interactionAnimation != null)
        {
            animator.Play(interest.interactionAnimation.name);
        }
        
        if (interest.interactionEffectPrefab != null)
        {
            Instantiate(interest.interactionEffectPrefab, position, Quaternion.identity);
        }
        
        StartCoroutine(InteractionRoutine(interest));
    }

    private IEnumerator InteractionRoutine(CritterInterest interest)
    {
        yield return new WaitForSeconds(interest.interestDuration);
        
        // Set cooldown
        interestCooldowns[interest.targetId] = Time.time + data.interactionCooldown;
        currentInterest = null;
        
        // Return to normal behavior
        SetNewTarget();
    }

    private void StartFleeing(Vector2 threatPosition)
    {
        if (isFleeing) return;
        
        isFleeing = true;
        Vector2 fleeDirection = ((Vector2)transform.position - threatPosition).normalized;
        targetPosition = (Vector2)transform.position + fleeDirection * data.fleeRange * 2f;
        
        // Play flee effect
        if (data.fleeEffectPrefab != null)
        {
            Instantiate(data.fleeEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play flee sound
        if (data.fleeSounds != null && data.fleeSounds.Length > 0)
        {
            AudioManager.Instance.PlaySFX(data.fleeSounds[Random.Range(0, data.fleeSounds.Length)]);
        }
        
        StartCoroutine(FleeingRoutine());
    }

    private IEnumerator FleeingRoutine()
    {
        yield return new WaitForSeconds(1f);
        isFleeing = false;
        SetNewTarget();
    }

    private void SetNewTarget()
    {
        switch (data.defaultBehavior)
        {
            case CritterBehavior.Wander:
                targetPosition = spawnPoint + (Vector2)Random.insideUnitCircle * wanderRadius;
                break;
            case CritterBehavior.Stationary:
                targetPosition = spawnPoint;
                break;
            case CritterBehavior.Patrol:
                // Implement patrol path behavior
                break;
            case CritterBehavior.Follow:
                // Implement following behavior
                break;
            case CritterBehavior.Territorial:
                // Implement territorial behavior
                break;
        }

        // Ensure target is valid
        if (Physics2D.OverlapCircle(targetPosition, 0.5f, obstacleLayer))
        {
            targetPosition = transform.position;
        }

        nextBehaviorTime = Time.time + Random.Range(3f, 6f);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsMoving", rb.velocity.magnitude > 0.1f);
        animator.SetBool("IsFleeing", isFleeing);
        animator.SetBool("IsInteracting", IsInteracting);
    }

    private IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if (!isFleeing && !IsInteracting)
            {
                SetNewTarget();
            }
        }
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(data.soundInterval);
            
            if (!isFleeing && !IsInteracting && data.idleSounds != null && data.idleSounds.Length > 0)
            {
                AudioManager.Instance.PlaySFX(data.idleSounds[Random.Range(0, data.idleSounds.Length)]);
            }
        }
    }

    private IEnumerator DespawnRoutine()
    {
        // Fade out
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    public bool TryCatch(string toolId)
    {
        if (!data.canBeCaught) return false;

        if (data.requiredTools != null && data.requiredTools.Length > 0)
        {
            bool hasRequiredTool = false;
            foreach (string requiredTool in data.requiredTools)
            {
                if (requiredTool == toolId)
                {
                    hasRequiredTool = true;
                    break;
                }
            }

            if (!hasRequiredTool) return false;
        }

        float catchChance = 1f / data.catchDifficulty;
        bool caught = Random.value < catchChance;

        if (caught)
        {
            OnCaught();
        }
        else
        {
            StartFleeing(PlayerController.Instance.transform.position);
        }

        return caught;
    }

    private void OnCaught()
    {
        // Spawn catch effect
        if (data.catchEffectPrefab != null)
        {
            Instantiate(data.catchEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play catch sound
        if (data.catchSound != null)
        {
            AudioManager.Instance.PlaySFX(data.catchSound);
        }

        // Drop items
        if (data.possibleDrops != null)
        {
            for (int i = 0; i < data.possibleDrops.Length; i++)
            {
                if (Random.value < data.dropChances[i])
                {
                    InventoryManager.Instance.AddItem(data.possibleDrops[i], 1);
                }
            }
        }

        // Add to museum collection if applicable
        if (data.isMuseumExhibit)
        {
            MuseumManager.Instance.AddCritterToCollection(data.critterId);
        }

        // Notify systems
        GameEvents.OnCritterCaught?.Invoke(data.critterId);

        // Destroy the critter
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        // Draw flee range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.fleeRange);

        // Draw wander radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPoint, wanderRadius);
    }
} 