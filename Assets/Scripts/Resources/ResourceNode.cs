using UnityEngine;
using System.Collections;

public class ResourceNode : MonoBehaviour, IInteractable
{
    private ResourceData data;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D interactionCollider;
    private float currentAmount;
    private bool isDepleted;
    private float respawnTime;
    private int islandIndex;
    private bool isBeingCollected;

    [Header("Visual Feedback")]
    [SerializeField] private float collectionShakeAmount = 0.1f;
    [SerializeField] private float collectionShakeSpeed = 10f;
    [SerializeField] private GameObject highlightEffect;
    
    private Vector3 originalPosition;
    private Coroutine collectionCoroutine;
    private Coroutine respawnCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        interactionCollider = GetComponent<CircleCollider2D>();
    }

    public void Initialize(ResourceData resourceData, int island)
    {
        data = resourceData;
        islandIndex = island;
        currentAmount = data.baseCollectionAmount;
        isDepleted = false;
        
        // Set up visuals
        spriteRenderer.sprite = data.icon;
        originalPosition = transform.position;
        
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
    }

    public void OnInteractionStart(PlayerController player)
    {
        if (isDepleted || isBeingCollected) return;

        // Check if player has required tool
        if (data.requiresTool)
        {
            Tool playerTool = player.GetCurrentTool();
            if (playerTool == null || 
                playerTool.toolType != data.requiredToolType || 
                playerTool.tier < data.minimumToolTier)
            {
                NotificationSystem.ShowGeneral(
                    "Tool Required",
                    $"You need a {data.requiredToolType} (Tier {data.minimumToolTier}+) to collect this resource.",
                    ResourceManager.GetIcon("tool_required")
                );
                return;
            }
        }

        // Start collection
        collectionCoroutine = StartCoroutine(CollectionRoutine(player));
    }

    public void OnInteractionEnd(PlayerController player)
    {
        if (collectionCoroutine != null)
        {
            StopCoroutine(collectionCoroutine);
            collectionCoroutine = null;
        }
        
        isBeingCollected = false;
        transform.position = originalPosition;
    }

    private IEnumerator CollectionRoutine(PlayerController player)
    {
        isBeingCollected = true;
        float collectionTimer = 0f;
        
        // Play start collection effects
        if (data.collectionSound != null)
        {
            AudioManager.Instance.PlaySFX(data.collectionSound);
        }

        while (collectionTimer < data.baseCollectionTime)
        {
            collectionTimer += Time.deltaTime;
            
            // Visual feedback
            float shake = Mathf.Sin(Time.time * collectionShakeSpeed) * collectionShakeAmount;
            transform.position = originalPosition + new Vector3(shake, 0, 0);

            // Update collection progress
            float progress = collectionTimer / data.baseCollectionTime;
            GameEvents.OnShowTooltip?.Invoke($"Collecting {data.displayName}... {(progress * 100):0}%");

            yield return null;
        }

        // Collection complete
        CollectResource(player);
        GameEvents.OnHideTooltip?.Invoke();
        
        isBeingCollected = false;
        transform.position = originalPosition;
    }

    private void CollectResource(PlayerController player)
    {
        // Calculate collection amount (could be modified by tools, skills, etc.)
        float amount = data.baseCollectionAmount;
        
        // Add to player's inventory
        player.AddResource(data.type, amount);
        
        // Grant experience
        player.AddExperience(data.experiencePerCollection);
        
        // Trigger collection effects
        if (data.collectionEffectPrefab != null)
        {
            Instantiate(data.collectionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Update resource state
        currentAmount -= amount;
        if (currentAmount <= 0)
        {
            Deplete();
        }

        // Trigger events
        GameEvents.OnResourceCollected?.Invoke(data.type, amount);
    }

    private void Deplete()
    {
        isDepleted = true;
        
        // Play depletion effects
        if (data.depleteEffectPrefab != null)
        {
            Instantiate(data.depleteEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (data.depleteSound != null)
        {
            AudioManager.Instance.PlaySFX(data.depleteSound);
        }

        // Handle respawn if renewable
        if (data.isRenewable)
        {
            spriteRenderer.enabled = false;
            interactionCollider.enabled = false;
            respawnTime = data.renewalTime;
            respawnCoroutine = StartCoroutine(RespawnRoutine());
        }
        else
        {
            // Permanently remove non-renewable resources
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        // Respawn effects
        if (data.respawnEffectPrefab != null)
        {
            Instantiate(data.respawnEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (data.respawnSound != null)
        {
            AudioManager.Instance.PlaySFX(data.respawnSound);
        }

        // Reset resource
        currentAmount = data.baseCollectionAmount;
        isDepleted = false;
        spriteRenderer.enabled = true;
        interactionCollider.enabled = true;
    }

    public void OnPointerEnter()
    {
        if (!isDepleted && highlightEffect != null)
        {
            highlightEffect.SetActive(true);
        }
        
        GameEvents.OnShowTooltip?.Invoke(data.GetTooltip());
    }

    public void OnPointerExit()
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
        
        GameEvents.OnHideTooltip?.Invoke();
    }

    private void OnDestroy()
    {
        if (collectionCoroutine != null)
            StopCoroutine(collectionCoroutine);
            
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);
    }
} 