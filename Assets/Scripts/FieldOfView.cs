using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to represent the field of view of a unit
public class FieldOfView : MonoBehaviour
{

    public float viewRadius; // How far the unit can see

    [Range(0,360)] // Value between 0 and 360
    public float viewAngle; // How wide the unit can see 

    public LayerMask targetMask; // Layer that the unit wants to see
    public LayerMask obstacleMask; // Layer that the unit can't see through

  // Hide the variable in the Unity Editor - Useful when the field needs to stay public but still remains hidden
    public List<Transform> visibleTargets = new List<Transform>(); // List of visible targets in sight of the entity

    public float meshResolution;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    // Launches the fielf of view algorithm
    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindTargetWithDelay", .2f); 
    }

    // Calls FindVisibleTargets continuously
    IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay); // Wait for the delay
            FindVisibleTarget();
        }
    }

    // Executes every frame
    void LateUpdate()
    {
        DrawFieldOfView();
    }

    // Finds every visible targets to the entity
    void FindVisibleTarget()
    {
        visibleTargets.Clear(); // Gets rid of all of the previously seen elements
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // Collects every transform that is in range of the unit with the target mask

        for (int i = 0; i < targetsInViewRadius.Length; i++) // Iterates through the collected transforms
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized; // Gets the angle from the unit to the target
            
            if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) // If the angle between the unit and the target is within the cone of sight of the unit
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask)) visibleTargets.Add(target); // It is added to the visible transforms if a ray shot from the unit can hit it
            }
        }
    }

    // Draws the field of view of the unit unto the world
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution); // Number of rays used to represent the cone of sight
        float stepAngleSize = viewAngle / stepCount; // Gap in degree between each ray
        List<Vector3> viewPoints = new List<Vector3>(); // List of points in space used to generate the mesh of our field of view

        for (int i = 0; i <= stepCount; i++) // For each ray
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i; // Compute the correct angle
            ViewCastInfo newViewCast = ViewCast(angle); // A new ray is cast at this angle
            viewPoints.Add(newViewCast.point); // And the coordinates of its hit are added in the list
        }

        int vertexCount = viewPoints.Count + 1; // Number of vertices used for the mesh
        Vector3[] vertices = new Vector3[vertexCount]; // Array of the coordinates of the vertices
        int[] triangles = new int[(vertexCount - 2) * 3]; // Array of the number of the vertices used for the triangles

        vertices[0] = Vector3.zero; // Position for the mesh will be relative to those of the unit

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0; // First vertex is the transform
                triangles[i * 3 + 1] = i + 1; // Second vertex is the first number of the vertex 
                triangles[i * 3 + 2] = i + 2; // Third vertex is the second number of the vertex
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // Returns the "logical" angle value in Vector3 terms
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Returns a ViewCastInfo about the collision of the ray with an obstacle or not 
    ViewCastInfo ViewCast (float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask)) // If a ray hit an obstacle
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    // Struct created to handle the multiple informations about each ray cast
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

}
