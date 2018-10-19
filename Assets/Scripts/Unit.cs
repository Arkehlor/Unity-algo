using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class using the pathfinding algotithm
public class Unit : MonoBehaviour
{

    const float pathUpdateMoveThreshold = .5f; // Minimum distance that the target must moves for the path to update

    public float speed = 20f; // The speed of the unit
    public float turnDst = 5f; // The distance apart from a waypoint where a unit should start to turn towards the next one
    public float turnSpeed = 3f; // The speed at which a unit rotates on itself to face the next waypoint
    public float stoppingDst = 10;

    Path path;

    // Function passed as parameter to be called later
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful) // If a path has been found
        {
            path = new Path(waypoints, transform.position, turnDst, stoppingDst); // Creates a new natural path based on the waypoints we found to be the best path

            StopCoroutine("FollowPath"); // Stops the previous path following
            StartCoroutine("FollowPath"); // Begins to follow the new path
        }
    }

    // Responsible for the movement of the unit, registers the inputs of the player
    void Update()
    {
            
        if (Input.GetKeyDown(KeyCode.Mouse0)) // If the mouse is pressed down (left click)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // A ray is created at the position of the cursor
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200)) // And then cast onto the world to get the position of the click on the grid, if it hits
                PathRequestManager.RequestPath(new PathRequest(transform.position, hit.point, OnPathFound)); // Asks for a new path
        }

    }

    // Function that moves the unit along the path of nodes returned by the pathfinding algorithm
    IEnumerator FollowPath()
    {
        bool followingPath = true; // The unit is now following a path
        int pathIndex = 0; // Starting from the first waypoint
        transform.LookAt(path.lookPoints[0]); // The unit faces the first point

        float speedPercent = 1;

        while (followingPath) // While the unit is in travel
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);  // Gets the coordinates of the unit for a 2D computation

            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) // If the unit passed the turn boundary of the current waypoint - while is used here because the unit could cross multiple turn boundaries at once
                if (pathIndex == path.finishLineIndex) // If it was the last waypoint
                {
                    followingPath = false; // Stops following the path
                    break; // Exits the loop
                }
                else // In the other case
                {
                    pathIndex++; // Increments the next waypoint
                }

            if (followingPath) // If the unit is currently following the path
            {
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst); // Computes how close the unit is from the finish line

                    if (speedPercent < 0.01f) followingPath = false;
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position); // Moves the unit using quaternions and a lerp function
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null; // Wait one frame
        }
    }

    // Draws the path with waypoints and boundary lines
    public void OnDrawGizmos()
    {
        if (path != null) path.DrawWithGizmos(); // Calls the path function for drawing
    }
}
