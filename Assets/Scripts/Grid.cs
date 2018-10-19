using UnityEngine;
using System.Collections.Generic;

// Grid class designed to automatically create nodes in a 3D environement based on the differnt walkable and unwalkable layer that signalises obstacles in the terrain for an A* pathfinding algorithm 
public class Grid : MonoBehaviour
{

    public bool displayGridGizmos; // Determines if we should see the grid
    public LayerMask unwalkableMask; // Layer that we want to test on, I.E. the layer we set in the editor for our obstacles 
    public Vector2 gridWorldSize; // Size of the grid in Unity
    public float nodeRadius; // Radius of a node in Unity
    public TerrainType[] walkableRegions; // Array of the different terrain types we want our players to be able to walk on
    public int obstacleProximityPenalty = 10; // The numerical dislike of an entity to walk close to an obstacle
    LayerMask walkableMask; // Combined mask of all of the terrain type masks
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>(); // Data Structure used for quick return of the desired value

    Node[,] grid; // Grid of nodes

    public static float nodeDiameter; // Diameter of a node
    int gridSizeX, gridSizeY; // Number of nodes in the grid

    int penaltyMin = int.MaxValue; // Minimum movement penalty of all of the nodes
    int penaltyMax = int.MinValue; // Maximum movement penalty of all of the nodes

    // Executes at the start of a game
    void Awake()
    {
        nodeDiameter = nodeRadius * 2; // Computes the diameter of a node
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter); // Computes the number of nodes of the grid
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions) // Iterates through all the different terrain types
        {
            walkableMask.value |= region.terrainMask.value; // Computes the value of the combined mask of all of the walkable masks
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty); // Adds to the dictionary the value of the walkable mask and its corresponding walk penalty
        }

        CreateGrid(); // Creates the grid
    }

    // Complete number of nodes in the grid should a heap contain them all
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    // Grid construction function
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY]; // Initializes the grid with the correct number of nodes
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2 + Vector3.up * 1; // Computes the coordinates of the bottom left corner of the grid

        for (int x = 0; x < gridSizeX; x++) // Iterates through the terrain
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius); // Computes where to check
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask)); // Checks whether the terrain should be walkable or not
                int movementPenalty = 0;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down); // Creates a ray above the node we want to check
                RaycastHit hit; // Creates a ray hit
                if (Physics.Raycast(ray, out hit, 100, walkableMask)) // Casts the ray unto the board and collides with the first layer it encounters out here means the hit is passed by reference instead of by value
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty); // Tries to modify the value of movementPenalty according to the first layer that the ray collided with

                if (!walkable) // If the node is unwalkable
                    movementPenalty += obstacleProximityPenalty; // The movementPenalty of this node is now the obstacleProximityPenalty

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty); // Creates according node
            }

        BlurPenaltyMap(3); // Blurs the movement penalty of the map
    }

    // Blurring Method used to give a more "human" like decision in the choice of the path of entities
    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1; // Computes the size of the bluring kernel used in the algorithm
        int kernelExtents = (kernelSize - 1) / 2; // Computes the margin a node will have to check on each side of itself

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY]; // Creates the horizontal part of the blurring algorithm
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY]; // Creates the vertical part

        for (int y = 0; y < gridSizeY; y++) // Iterates through each line of the grid - Horizontal
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++) // Computes the blurred movement penalty of the first node of each line
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents); // Makes sure not to check out of bound
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty; // Stores the new value in the temporary Horizontal grid
            }

            for (int x = 1; x < gridSizeX; x++) // Computes the blurred movement penalty of the other nodes of the line
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX); // Computes the x index of the newly removed node of the computation
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1); // Computes the x index of the newly added node of the computation

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty; // Computes the temporary horizontal grid movement penalty
            }
        }

        for (int x = 0; x < gridSizeX; x++) // Iterates through each column of the grid - Vertical
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++) // First node of the column
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize)); // Computes the final penalty value of the first nodes of each column
            grid[x, 0].movementPenalty = blurredPenalty; // Updates it 

            for (int y = 1; y < gridSizeY; y++) // Rest of the nodes of the column
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize)); // Computes the final penalty of the node
                grid[x, y].movementPenalty = blurredPenalty; // Updates the final penalty

                if (blurredPenalty > penaltyMax) penaltyMax = blurredPenalty; // If it's the highest value update penaltyMax

                if (blurredPenalty < penaltyMin) penaltyMin = blurredPenalty; // If it's the lowest value update penaltyMin
            }
        }
    }

    // Returns a list of neighbouring nodes
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>(); // Creates a new list of nodes

        for (int x = -1; x <= 1; x++) // Iterates through the neighbouring nodes
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Avoids self

                int checkX = node.gridX + x; // Computes temporary values;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) // Checks whether or not the node actually exists
                    neighbours.Add(grid[checkX, checkY]); // Adds the neighbours to the list
            }

        return neighbours; // Returns the list
    }

    // Returns the node that the parameter points to
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x; // Computes the X and Y percentages
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y; // CAREFUL WITH NOT "CENTERED" PLANES X:0 Y:0 Z:0
        percentX = Mathf.Clamp01(percentX); // Makes sure that we get coherent values
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX); // Computes the actual index of the node on the grid
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y]; // Returns the correct node from the grid
    }

    // Draws the grid unto the world for vizualisation
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); // Draws the dimension of the desired grid

        if (grid != null  && displayGridGizmos)
        {
            foreach (Node n in grid) // Iterates through the nodes of the grid
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty)); // Computes a color based on the penalty movement of the node

                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red; // Colors the nodes of a shade of black or white depending on walkability if walkable, red otherwise
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter)); 
            }
        }
        
    }

    [System.Serializable] // Makes the type appear in the Unity inspector
    public class TerrainType
    {
        public LayerMask terrainMask; // The designated terrain type
        public int terrainPenalty; // And its associated move penalty
    }
}