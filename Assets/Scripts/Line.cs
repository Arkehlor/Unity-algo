using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Line class used to in the Path class
public struct Line {

    const float verticalLineGradient = 1e5f; // Large number to emulate a vertical line

    float gradient; // slope of the line - a in y = ax + b
    float y_intercept; // y-intercept of the line - b in y = ax + b
    Vector2 pointOnLine_1; // A point of the line
    Vector2 pointOnLine_2; // Another point of the same line

    float gradientPerpendicular; // gradient of a perpendicular line to this one

    bool approachSide;

    // Constructor that takes a point on this line and a point part of a line perpendicular to this one
    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        // The perpendicular line is basically the line between the two points and we're computing a line from the perpendicular one
        float dx = pointOnLine.x - pointPerpendicularToLine.x; // Computes delta x
        float dy = pointOnLine.y - pointPerpendicularToLine.y; // Computes delta y

        if (dx == 0) // If the two points are "on the same column"
        {
            gradientPerpendicular = verticalLineGradient; // The gradient of the line perpendicular to the one we're building gets very high to emulate a vertical line
        }
        else
        {
            gradientPerpendicular = dy / dx; // Otherwise the exact gradient of the perpendicular line is computed
        }

        if (gradientPerpendicular == 0) // If the two points are at the same height
        {
            gradient = verticalLineGradient; // The gradient of the line is vertical
        }
        else
        {
            gradient = -1 / gradientPerpendicular; // Otherwise the exact gradient of the current line is computed
        }

        y_intercept = pointOnLine.y - gradient * pointOnLine.x; // Computes the exact y-intercept of the line

        pointOnLine_1 = pointOnLine; // Stores one point of the line
        pointOnLine_2 = pointOnLine + new Vector2(1, gradient); // Stores an other one

        approachSide = false; // If approachSide is not set before, the method GetSide cannot be called
        approachSide = GetSide(pointPerpendicularToLine);
    }

    // Function that determines if a point is on one side of a line or the other
    bool GetSide(Vector2 p)
    {
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    // Function that determines if a point has crossed the line
    public bool HasCrossedLine(Vector2 p)
    {
        return GetSide(p) != approachSide;
    }

    // Function that computes the distance from the line to a point p
    public float DistanceFromPoint(Vector2 p)
    {
        float y_interceptPerpendicular = p.y - gradientPerpendicular * p.x;
        float intersectX = (y_interceptPerpendicular - y_intercept) / (gradient - gradientPerpendicular); // Computes the X coordinates
        float intersectY = gradient * intersectX + y_intercept; // Computes the Y coordinates
        return Vector2.Distance(p, new Vector2(intersectX, intersectY)); // Returns the distance
    }

    // Draws the lines used to compute a smooth path for units
    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized; // Desired direction of the lines
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up; // Center of the line
        Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f); // Draws the line
    }
}
