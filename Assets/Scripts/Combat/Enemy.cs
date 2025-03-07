using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyData data;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Combat")]
    [SerializeField] private CombatStats stats;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem hitParticles;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector2 startPosition;
    private float lastAttackTime;
    private bool isDead;
    private EnemyState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<CombatStats>();
        
        if (stats == null)
        {
            stats = gameObject.AddComponent<CombatStats>();
        }
        
        startPosition = transform.position;
    }

    private void Start()
    {
        InitializeEnemy();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void InitializeEnemy()
    {
        if (data != null)
        {
            stats.maxHealth = data.maxHealth;
            stats.attackPower = data.attackPower;
            stats.defense = data.defense;
            moveSpeed = data.moveSpeed;
            attackCooldown = data.attackCooldown;
        }
        
        stats.Initialize();
        currentState = EnemyState.Idle;
    }

    private void Update()
    {
        if (isDead || stats.IsStunned) return;

        UpdateState();
        UpdateAnimation();
    }

    private void UpdateState()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chase;
                }
                else if (Vector2.Distance(transform.position, startPosition) > 0.1f)
                {
                    MoveTowards(startPosition);
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Idle;
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                }
                else
                {
                    MoveTowards(player.position);
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                }
                else if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformAttack();
                }
                break;
        }
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 direction = ((Vector2)target - rb.position).normalized;
        rb.velocity = direction * moveSpeed;
        
        // Flip sprite if needed
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // Play attack animation
        animator?.SetTrigger("Attack");
        
        // Perform attack after animation delay
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.2f); // Attack animation wind-up
        
        if (isDead) yield break;

        // Check for player hit
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<CombatStats>(out var targetStats))
            {
                DamageResult result = targetStats.TakeDamage(
                    stats.attackPower,
                    data.damageType,
                    true
                );
                
                if (result.damageDealt > 0)
                {
                    // Show damage number
                    DamageNumberSystem.Show(
                        result.damageDealt,
                        hit.transform.position,
                        result.wasCritical
                    );
                    
                    // Apply knockback
                    if (hit.TryGetComponent<Rigidbody2D>(out var targetRb))
                    {
                        Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                        targetRb.AddForce(knockbackDirection * data.knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }

        // Play attack effects
        if (data.attackEffectPrefab != null)
        {
            Instantiate(data.attackEffectPrefab, attackPoint.position, attackPoint.rotation);
        }
        
        if (data.attackSound != null)
        {
            AudioManager.Instance.PlaySFX(data.attackSound);
        }
    }

    public void TakeDamage(DamageResult result)
    {
        if (isDead) return;

        // Visual feedback
        StartCoroutine(FlashSprite());
        hitParticles?.Play();
        
        if (result.wasFatal)
        {
            Die();
        }
        else
        {
            // Play hit sound
            AudioManager.Instance.PlaySFX(data.hitSound);
        }
    }

    private void Die()
    {
        isDead = true;
        
        // Stop movement and disable collisions
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        
        // Play death effects
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (deathSound != null)
        {
            AudioManager.Instance.PlaySFX(deathSound);
        }
        
        // Drop loot
        DropLoot();
        
        // Trigger events
        GameEvents.OnEnemyDefeated?.Invoke(gameObject);
        
        // Destroy after delay
        Destroy(gameObject, 1f);
    }

    private void DropLoot()
    {
        if (data.possibleDrops == null || data.possibleDrops.Length == 0) return;

        foreach (var drop in data.possibleDrops)
        {
            if (Random.value <= drop.dropChance)
            {
                Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
                Instantiate(drop.itemPrefab, dropPosition, Quaternion.identity);
            }
        }
    }

    private IEnumerator FlashSprite()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        animator.SetFloat("Speed", rb.velocity.magnitude);
        animator.SetInteger("State", (int)currentState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}

public enum EnemyState
{
    Idle,
    Chase,
    Attack
} 