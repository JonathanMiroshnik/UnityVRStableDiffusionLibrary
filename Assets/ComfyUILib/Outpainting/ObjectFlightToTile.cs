using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Moves a GameObject from its position to a final position at a certain speed
/// </summary>

public class ObjectFlightToTile : MonoBehaviour
{       
    private Vector3 _initialPos = Vector3.zero;
    private Vector3 _finalPos = Vector3.zero;
    private float _moveSpeed = 5f;
    private float _percentage = 0;
    private bool _begin = false;     

    /// <summary>
    /// Starting the flight of the gameObject towards the final position
    /// </summary>
    /// <param name="iPos">Initial position</param>
    /// <param name="fPos">Final tile position</param>
    public void StartMovement(Vector3 iPos, Vector3 fPos)
    {
        if (iPos == null || fPos == null) return;
        _initialPos = iPos;
        _finalPos = fPos;

        _begin = true;
    }

    private void Update()
    {
        // Trigger stopping the updates before beginning
        if (!_begin) return;

        transform.position = Vector3.Lerp(_initialPos, _finalPos, _percentage);
        _percentage += Time.deltaTime * (_moveSpeed/100);
        
        if (_percentage >= 1)
        {
            DestroyImmediate(gameObject);
        }
    }
}
