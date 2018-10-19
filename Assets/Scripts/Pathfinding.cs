using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// A* Pathfinding algorithm
public class Pathfinding : MonoBehaviour
{

    Grid grid; // Grid used in the algorithm

    // Executes at the start of a game
    void Awake()
    {
        grid = GetComponent<Grid>(); // Fetches the grid 
    }
    
    // Finds a way from one position to the other
    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        /*
        Stopwatch sw = new Stopwatch(); // Stopwatch used to test the viability and speed of our method
        sw.Start(); // Start of the stopwatch
        */

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false; // Sets the success of the pathfinding

        Node startNode = grid.NodeFromWorldPoint(request.pathStart); // Gets the node of the starting position
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd); // Gets the node of the end position

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize); // Heap of nodes to be evaluated
            HashSet<Node> closedSet = new HashSet<Node>(); // Set of nodes that were already evaluated
            openSet.Add(startNode); // Adds the starting node to the open set

            while (openSet.Count > 0) // While there are still nodes to be evaluated
            {
                Node node = openSet.RemoveFirst(); // Takes the first node of the open set
                closedSet.Add(node); // Adds it to the closed set

                if (node == targetNode) // If the node is the end node
                {
                    /*
                    sw.Stop(); // End of the stopwatch
                    print("Path found " + sw.ElapsedMilliseconds + " ms"); // Displays the elapsed time in ms
                    */
                    pathSuccess = true;
                    break; // End the function
                }

                foreach (Node neighbour in grid.GetNeighbours(node)) // Iterates through the neighbours of the current node 
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) continue; // Skips the unwalkable nodes or the already evaluated nodes

                    int newCostToNeighbour = node.gCost + GetDistance(node, neighbour) + neighbour.movementPenalty; // Computes the cost of the neighbour nodes

                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) // If the newly discovered path to the node is shorter or the node is not in the open set
                    {
                        neighbour.gCost = newCostToNeighbour; // Updates the cost of the neighbour
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = node; // Links the neighbour to the current node

                        if (!openSet.Contains(neighbour)) // If the neighbour is not in the open set
                        {
                            openSet.Add(neighbour); // Adds it to the open set
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour); // Updates the position of the node in the heap
                        }
                    }
                }
            }
        }

        if (pathSuccess)// If a path has been found
        {
            waypoints = RetracePath(startNode, targetNode); // Retrace the steps we took to get to this node
            pathSuccess = waypoints.Length > 0;
        }

        callback(new PathResult(waypoints, pathSuccess, request.callback)); // Mark the request as successful or unsuccessful
    }

    // Retraces the path from the startNode to the endNode through the parent nodes
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>(); // Creates a list of nodes
        Node currentNode = endNode; // Makes the endnode the current node

        while (currentNode != startNode) // While we didn't come back to the starting point
        {
            path.Add(currentNode); // Adds the current node to the path
            currentNode = currentNode.parent; // Retrace the current node's steps through its parent node
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints); // Reverses the path as we want starting => end but iterated from the other side
        return waypoints;
    }

    // Transforms a full path into a sucession of waypoints
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>(); // Creates the soon to be returned object
        Vector2 directionOld = Vector2.zero; // Sets the original direction

        for (int i = 1; i < path.Count; i++) // Iterates through all the found nodes 
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY); // Stores the new direction from node i - 1 to node i

            if (directionNew != directionOld) // If the direction changed
                waypoints.Add(path[i - 1].worldPosition); // Add the new node to the path

            directionOld = directionNew; // Stores the now old direction
        }

        return waypoints.ToArray(); // Returns the set of waypoints
    }

    // Returns the distance between two nodes without consideration for obstacles
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX); // Computes the difference of nodes horizontally or vertically
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return (dstX > dstY) ? 4 * dstY + 10 * dstX : 4 * dstX + 10 * dstY; // Distance directly horizontally or vertically : 10 / Diagonally : 14
    }
}