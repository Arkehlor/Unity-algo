using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Temporary class used to make the enemies rotate on themselves
public class EnemyMovement : MonoBehaviour
{

    public float rotationSpeed = 45; // Speed of rotation of the enemies cone of sight
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed); // The enemy rotates at a set speed
    }
}
