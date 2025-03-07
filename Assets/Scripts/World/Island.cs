using UnityEngine;
using System.Collections.Generic;

public class Island : MonoBehaviour
{
    [SerializeField] private SpriteRenderer groundRenderer;
    [SerializeField] private Transform decorationContainer;
    [SerializeField] private EdgeCollider2D boundaryCollider;
    
    private IslandData data;
    private int islandIndex;
    private List<GameObject> decorations = new List<GameObject>();
    private bool isInitialized;

    public void Initialize(int index, IslandData islandData)
    {
        if (isInitialized) return;
        
        islandIndex = index;
        data = islandData;
        
        // Set up ground
        transform.localScale = new Vector3(data.size.x, data.size.y, 1f);
        groundRenderer.color = data.groundTint;
        
        if (data.groundTileVariants != null && data.groundTileVariants.Length > 0)
        {
            groundRenderer.sprite = data.groundTileVariants[
                Random.Range(0, data.groundTileVariants.Length)
            ];
        }
        
        // Set up boundary collider
        SetupBoundaryCollider();
        
        // Add decorations
        SpawnDecorations();
        
        isInitialized = true;
    }

    private void SetupBoundaryCollider()
    {
        // Create points for a rectangular boundary
        Vector2 halfSize = data.size * 0.5f;
        Vector2[] points = new Vector2[]
        {
            new Vector2(-halfSize.x, -halfSize.y),
            new Vector2(halfSize.x, -halfSize.y),
            new Vector2(halfSize.x, halfSize.y),
            new Vector2(-halfSize.x, halfSize.y),
            new Vector2(-halfSize.x, -halfSize.y) // Close the loop
        };
        
        boundaryCollider.points = points;
    }

    private void SpawnDecorations()
    {
        if (data.decorativePrefabs == null || data.decorativePrefabs.Length == 0)
            return;

        int decorationCount = Random.Range(data.minDecorations, data.maxDecorations + 1);
        Vector2 halfSize = data.size * 0.5f;

        for (int i = 0; i < decorationCount; i++)
        {
            Vector2 position = new Vector2(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y)
            );

            GameObject prefab = data.decorativePrefabs[
                Random.Range(0, data.decorativePrefabs.Length)
            ];
            
            GameObject decoration = Instantiate(
                prefab,
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
                decorationContainer
            );
            
            decorations.Add(decoration);
        }
    }

    public bool IsPointInside(Vector2 point)
    {
        // Convert point to local space
        Vector2 localPoint = transform.InverseTransformPoint(point);
        Vector2 halfSize = data.size * 0.5f;
        
        return localPoint.x >= -halfSize.x && localPoint.x <= halfSize.x &&
               localPoint.y >= -halfSize.y && localPoint.y <= halfSize.y;
    }

    public Vector2 GetRandomPoint()
    {
        Vector2 halfSize = data.size * 0.5f;
        Vector2 localPoint = new Vector2(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y)
        );
        
        return transform.TransformPoint(localPoint);
    }

    public void OnDrawGizmos()
    {
        if (!isInitialized || data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(data.size.x, data.size.y, 0.1f));
    }
} 