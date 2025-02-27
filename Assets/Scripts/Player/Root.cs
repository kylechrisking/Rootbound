using UnityEngine;

public class Root : MonoBehaviour
{
    private Vector2 startPoint;
    private Vector2 growthDirection;
    private float growthSpeed;
    private float maxLength;
    private float currentLength;
    
    private LineRenderer lineRenderer;
    private bool isGrowing = true;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.05f;
    }

    public void Initialize(Vector2 start, Vector2 direction, float speed, float length)
    {
        startPoint = start;
        growthDirection = direction;
        growthSpeed = speed;
        maxLength = length;
        currentLength = 0f;

        // Set up line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);
    }

    public bool UpdateGrowth()
    {
        if (!isGrowing) return false;

        currentLength += growthSpeed * Time.deltaTime;
        
        if (currentLength >= maxLength)
        {
            currentLength = maxLength;
            isGrowing = false;
        }

        Vector2 endPoint = startPoint + (growthDirection * currentLength);
        lineRenderer.SetPosition(1, endPoint);

        // Check for resources near the growing tip
        CheckForNearbyResources(endPoint);

        return isGrowing;
    }

    private void CheckForNearbyResources(Vector2 tipPosition)
    {
        float detectionRadius = 0.5f;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(tipPosition, detectionRadius);

        foreach (Collider2D collider in nearbyColliders)
        {
            Resource resource = collider.GetComponent<Resource>();
            if (resource != null)
            {
                // Boost resource absorption rate when roots are near
                resource.BoostAbsorptionRate(1.5f);
            }
        }
    }

    public Vector2 GetTipPosition()
    {
        return lineRenderer.GetPosition(1);
    }

    public bool IsGrowing()
    {
        return isGrowing;
    }
} 