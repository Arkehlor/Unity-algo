using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))] // Creation of a custom editor for edition of a class in the Unity engine
public class FieldOfViewEditor : Editor
{

    // Visualisation of the different parameters of the class
    void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target; // Retrieves the fov class of the entity
        Handles.color = Color.white; // Changes the color of the drawing mechanism to white

        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius); // Draws the "potential" sight of the unit

        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false); // Computes the left end of the fov
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false); // Computes the right end of the fov

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius); // Draws the left end of the fov
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius); // Draws the right end of the fov

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fov.visibleTargets) // For each target in the fov of the current unit
            Handles.DrawLine(fov.transform.position, visibleTarget.position); // Draws a line between the unit and the target

    }
}
