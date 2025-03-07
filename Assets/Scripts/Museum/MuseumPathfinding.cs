using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MuseumPathfinding : MonoBehaviour
{
    [SerializeField] private MuseumLayout layout;
    [SerializeField] private float nodeSize = 1f;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Node[,] grid;
    private List<Node> openSet;
    private HashSet<Node> closedSet;
    private Vector2Int gridSize;

    private void Awake()
    {
        if (layout == null)
        {
            Debug.LogError("No MuseumLayout assigned to MuseumPathfinding!");
            return;
        }

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        gridSize = layout.gridSize;
        grid = new Node[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = GridToWorldPoint(new Vector2Int(x, y));
                bool walkable = layout.tiles[x, y].isWalkable && 
                              !Physics2D.OverlapCircle(worldPoint, nodeSize * 0.4f, obstacleLayer);
                
                grid[x, y] = new Node(walkable, worldPoint, new Vector2Int(x, y));
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector2Int startNode = WorldToGridPoint(startPos);
        Vector2Int targetNode = WorldToGridPoint(targetPos);
        
        if (!IsValidGridPoint(startNode) || !IsValidGridPoint(targetNode))
            return null;

        openSet = new List<Node>();
        closedSet = new HashSet<Node>();
        openSet.Add(grid[startNode.x, startNode.y]);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.gridPosition == targetNode)
            {
                return RetracePath(grid[startNode.x, startNode.y], currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, grid[targetNode.x, targetNode.y]);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int checkPos = node.gridPosition + new Vector2Int(x, y);
                
                if (IsValidGridPoint(checkPos))
                {
                    // Only allow diagonal movement if both adjacent nodes are walkable
                    if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                    {
                        if (!grid[checkPos.x, node.gridPosition.y].walkable || 
                            !grid[node.gridPosition.x, checkPos.y].walkable)
                            continue;
                    }
                    
                    neighbors.Add(grid[checkPos.x, checkPos.y]);
                }
            }
        }

        return neighbors;
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }
        
        path.Add(startNode.worldPosition);
        path.Reverse();
        
        return SmoothPath(path);
    }

    private List<Vector3> SmoothPath(List<Vector3> path)
    {
        List<Vector3> smoothedPath = new List<Vector3>();
        smoothedPath.Add(path[0]);
        
        int currentPoint = 0;
        while (currentPoint < path.Count - 1)
        {
            int furthestVisible = currentPoint + 1;
            
            for (int checkPoint = currentPoint + 2; checkPoint < path.Count; checkPoint++)
            {
                if (IsDirectPathClear(path[currentPoint], path[checkPoint]))
                {
                    furthestVisible = checkPoint;
                }
            }
            
            smoothedPath.Add(path[furthestVisible]);
            currentPoint = furthestVisible;
        }
        
        return smoothedPath;
    }

    private bool IsDirectPathClear(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        
        RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, obstacleLayer);
        return !hit;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    private Vector3 GridToWorldPoint(Vector2Int gridPoint)
    {
        return new Vector3(
            gridPoint.x * nodeSize,
            gridPoint.y * nodeSize,
            0
        );
    }

    private Vector2Int WorldToGridPoint(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / nodeSize),
            Mathf.RoundToInt(worldPosition.y / nodeSize)
        );
    }

    private bool IsValidGridPoint(Vector2Int point)
    {
        return point.x >= 0 && point.x < gridSize.x && 
               point.y >= 0 && point.y < gridSize.y;
    }

    public void UpdateNode(Vector2Int position, bool walkable)
    {
        if (IsValidGridPoint(position))
        {
            grid[position.x, position.y].walkable = walkable;
        }
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        foreach (Node node in grid)
        {
            Gizmos.color = node.walkable ? Color.white : Color.red;
            Gizmos.DrawWireCube(node.worldPosition, Vector3.one * nodeSize * 0.9f);
        }
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public Vector2Int gridPosition;
    public int gCost;
    public int hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool _walkable, Vector3 _worldPos, Vector2Int _gridPos)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridPosition = _gridPos;
    }
} 