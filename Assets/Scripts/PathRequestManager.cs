using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

// Class made to balance the computing load when multiple entities try to request the use of the pathfinding
public class PathRequestManager : MonoBehaviour
{

    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance; // Single PathRequestManager instance
    Pathfinding pathfinding; // Reference to the pathfinding script



    // Executes at the start of a game
    private void Awake()
    {
        instance = this; // Makes sure there is only one instance of the class ?
        pathfinding = GetComponent<Pathfinding>(); // Loads the pathfinding functions
    }

    // Function that's called every frame
    void Update ()
    {
        if (results.Count > 0) // If there are results paths to give back to their specific askers
        {
            int itemsInQueue = results.Count; // Get the number of responses
            lock (results) // Locks them
            {
                for (int i = 0; i < itemsInQueue; i++) // Iterates through the results
                {
                    PathResult result = results.Dequeue(); // Dequeues it
                    result.callback(result.path, result.success); // Callbaks the results
                }
            }
        }
    }

    // Creates a new thread to compute a path
    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };

        threadStart.Invoke();
    }

    // Enqueues the results of the pathfinding to be called back by their respective asker
    public void FinishedProcessingPath(PathResult result)
    {
        lock (results) // Locks the queue before adding the results
        {
            results.Enqueue(result);
        }

    }
}

// Structure created to pass information through threads
public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    // Basic constructor
    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

// Structure created to store a request
public struct PathRequest
{
    public Vector3 pathStart; // Starting position
    public Vector3 pathEnd; // End positon
    public Action<Vector3[], bool> callback; // Function that takes an array of Vector3 and a boolean as parameters

    // Basic constructor
    public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}