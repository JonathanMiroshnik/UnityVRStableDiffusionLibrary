using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Moves a GameObject from its position to a final position at a certain speed
/// </summary>

public class ObjectFlightToTile : MonoBehaviour
{       
    private Vector3 initialPos = Vector3.zero;
    private Vector3 finalPos = Vector3.zero;
    private float moveSpeed = 5f;
    private float percentage = 0;
    private bool begin = false;     

    /// <summary>
    /// Starting the flight of the gameObject towards the final position
    /// </summary>
    /// <param name="iPos">Initial position</param>
    /// <param name="fPos">Final tile position</param>
    public void StartMovement(Vector3 iPos, Vector3 fPos)
    {
        if (iPos == null || fPos == null) return;
        initialPos = iPos;
        finalPos = fPos;

        begin = true;
    }

    private void Update()
    {
        // Trigger stopping the updates before beginning
        if (!begin) return;

        transform.position = Vector3.Lerp(initialPos, finalPos, percentage);
        percentage += Time.deltaTime * (moveSpeed/100);
        
        if (percentage >= 1)
        {
            DestroyImmediate(gameObject);
        }
    }
}
