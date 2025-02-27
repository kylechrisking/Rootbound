using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Absorption Settings")]
    [SerializeField] private float absorbRange = 1.5f;
    [SerializeField] private float absorbRate = 1f;
    [SerializeField] private LayerMask resourceLayer;
    [SerializeField] private float absorbCooldown = 0.1f;

    private float nextAbsorbTime;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextAbsorbTime)
        {
            TryAbsorbResources();
        }
    }

    private void TryAbsorbResources()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, absorbRange, resourceLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (Vector2.Distance(mousePosition, collider.transform.position) <= absorbRange)
            {
                Resource resource = collider.GetComponent<Resource>();
                if (resource != null)
                {
                    float absorbed = resource.AbsorbResource(absorbRate);
                    playerStats.AddResource(resource.GetResourceType(), absorbed);
                    nextAbsorbTime = Time.time + absorbCooldown;
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, absorbRange);
    }
} 