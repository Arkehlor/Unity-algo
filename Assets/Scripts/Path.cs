using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to smooth out the deplacement of entities on the grid
public class Path
{

    public readonly Vector3[] lookPoints; // Array of points we want to go through
    public readonly Line[] turnBoundaries; // Lines used to determine when an entity should start turning
    public readonly int finishLineIndex; // Index of the last point we want to reach
    public readonly int slowDownIndex;

    // Basic constructor
    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos); // Stores the starting postion as the last position

        for (int i = 0; i < lookPoints.Length; i++) // Iterates through the different node we want to go through
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]); // Updates the position we want to go through
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized; // Get the direction of this point
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst; // Stores the  position where we want our entity to start turning
            // The if clause is here to ensure that we do not turn before the point if this is the last position we want to passs through
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst); // Creates the lines that we'll check passing through before turning
            // The computation of the point parallel to the line we want to make is here to ensure that the turn boundary will be at its right side even if the turn Distance is great
            previousPoint = turnBoundaryPoint;
        }

        float dstFromEndPoint = 0;

        for (int i = lookPoints.Length - 1; i > 0; i--)
        {
            dstFromEndPoint = +Vector3.Distance(lookPoints[i], lookPoints[i - 1]);

            if (dstFromEndPoint > stoppingDst)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    // Function used for simplicity reasons to transform a vector3 into a vector2
    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    // Draws the waypoints used in the path
    public void DrawWithGizmos()
    {
        Gizmos.color = Color.green; // Each waypoint is colored green
        foreach (Vector3 p in lookPoints)
            Gizmos.DrawCube(p + Vector3.up, Vector3.one * (Grid.nodeDiameter));

        Gizmos.color = Color.white; // Each line is colored white

        /*
        foreach (Line l in turnBoundaries)
            l.DrawWithGizmos(10);
        */
    }
}
