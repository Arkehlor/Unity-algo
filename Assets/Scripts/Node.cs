using UnityEngine;

// Node class used for the grid of our Pathfinding algorithm
public class Node : IHeapItem<Node>
{

    public bool walkable; // Ability to pass through the terrain
    public Vector3 worldPosition; // Position of the terrain
    public int gridX, gridY; // Grid coordinates
    public int movementPenalty;

    public int gCost; // Distance from the starting node
    public int hCost; // Distance from the end node

    public Node parent; // Parent node relative to the path
    int heapIndex;

    // Basic constructor
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penatly)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penatly;
    }

    // Score of the node
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    // Index of the node in a heap should one be used
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    // Implementation of CompareTo for the purposes of Heap : Compares the fCost and hCost selects the one with the lowest
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);

        return -compare;
    }
}