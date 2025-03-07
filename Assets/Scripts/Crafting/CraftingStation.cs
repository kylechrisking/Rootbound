using UnityEngine;

public class CraftingStation : MonoBehaviour, IInteractable
{
    [Header("Station Settings")]
    [SerializeField] private string stationType;
    [SerializeField] private string displayName;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private Transform effectPoint;
    [SerializeField] private GameObject highlightEffect;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem idleEffect;
    [SerializeField] private ParticleSystem activeEffect;
    [SerializeField] private Light stationLight;
    
    private bool isPlayerInRange;
    private bool isActive;

    private void Start()
    {
        // Register with CraftingManager
        CraftingManager.Instance.RegisterCraftingStation(stationType, this);
        
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
            
        if (idleEffect != null)
            idleEffect.Play();
            
        if (activeEffect != null)
            activeEffect.Stop();
            
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        CraftingManager.Instance?.UnregisterCraftingStation(stationType);
    }

    public void OnInteractionStart(PlayerController player)
    {
        if (!isPlayerInRange) return;

        // Open crafting UI
        UIManager.Instance.ShowCraftingUI(stationType);
        
        SetActive(true);
    }

    public void OnInteractionEnd(PlayerController player)
    {
        UIManager.Instance.HideCraftingUI();
        SetActive(false);
    }

    public void OnPointerEnter()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(true);
            
        GameEvents.OnShowTooltip?.Invoke($"{displayName}\nPress E to craft");
    }

    public void OnPointerExit()
    {
        if (highlightEffect != null)
            highlightEffect.SetActive(false);
            
        GameEvents.OnHideTooltip?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            OnInteractionEnd(other.GetComponent<PlayerController>());
        }
    }

    public bool IsAccessible()
    {
        return isPlayerInRange;
    }

    public Vector3 GetEffectPosition()
    {
        return effectPoint != null ? effectPoint.position : transform.position;
    }

    private void SetActive(bool active)
    {
        isActive = active;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (activeEffect != null)
        {
            if (isActive && !activeEffect.isPlaying)
                activeEffect.Play();
            else if (!isActive && activeEffect.isPlaying)
                activeEffect.Stop();
        }

        if (stationLight != null)
        {
            stationLight.intensity = isActive ? 2f : 1f;
        }
    }
} 