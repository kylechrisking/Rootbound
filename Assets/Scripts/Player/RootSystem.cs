using UnityEngine;
using System.Collections.Generic;

public class RootSystem : MonoBehaviour
{
    [Header("Root Settings")]
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private float rootGrowthSpeed = 2f;
    [SerializeField] private float maxRootLength = 5f;
    [SerializeField] private float rootCost = 10f; // Resource cost to grow roots
    [SerializeField] private float branchAngleRange = 45f;
    
    [Header("Root Limits")]
    [SerializeField] private int maxRootBranches = 3;
    [SerializeField] private float minDistanceBetweenRoots = 1f;

    private List<Root> activeRoots = new List<Root>();
    private PlayerStats playerStats;
    private bool isGrowing = false;
    private Vector2 growthDirection;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        HandleRootGrowth();
    }

    private void HandleRootGrowth()
    {
        // Right mouse button to start/stop growing roots
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            
            if (!isGrowing && CanGrowNewRoot())
            {
                StartNewRoot(direction);
            }
        }

        // Update growing roots
        for (int i = activeRoots.Count - 1; i >= 0; i--)
        {
            if (!activeRoots[i].UpdateGrowth())
            {
                // Root has finished growing
                activeRoots.RemoveAt(i);
            }
        }
    }

    private bool CanGrowNewRoot()
    {
        // Check if we have enough resources and haven't exceeded root limit
        return activeRoots.Count < maxRootBranches && 
               playerStats.HasEnoughResources(ResourceType.Nutrients, rootCost);
    }

    private void StartNewRoot(Vector2 direction)
    {
        if (playerStats.ConsumeResource(ResourceType.Nutrients, rootCost))
        {
            GameObject newRootObj = Instantiate(rootPrefab, transform.position, Quaternion.identity);
            Root newRoot = newRootObj.GetComponent<Root>();
            
            if (newRoot != null)
            {
                newRoot.Initialize(transform.position, direction, rootGrowthSpeed, maxRootLength);
                activeRoots.Add(newRoot);
            }
        }
    }

    public List<Root> GetActiveRoots()
    {
        return activeRoots;
    }
} 