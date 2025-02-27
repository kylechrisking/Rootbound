using UnityEngine;

public class Resource : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private float resourceAmount = 10f;
    [SerializeField] private float maxCapacity = 10f;
    [SerializeField] private float regenerationRate = 1f;
    [SerializeField] private GameObject absorbEffectPrefab;

    private float currentAmount;

    private void Start()
    {
        currentAmount = resourceAmount;
    }

    private void Update()
    {
        // Regenerate resource over time
        if (currentAmount < maxCapacity)
        {
            currentAmount = Mathf.Min(currentAmount + regenerationRate * Time.deltaTime, maxCapacity);
        }
    }

    public float AbsorbResource(float absorbAmount)
    {
        float absorbed = Mathf.Min(currentAmount, absorbAmount);
        currentAmount -= absorbed;

        if (absorbEffectPrefab != null)
        {
            Instantiate(absorbEffectPrefab, transform.position, Quaternion.identity);
        }

        // If resource is depleted, start regeneration
        if (currentAmount <= 0)
        {
            // Optional: Make the resource visually appear depleted
            // You could change the sprite or scale here
        }

        return absorbed;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }
} 