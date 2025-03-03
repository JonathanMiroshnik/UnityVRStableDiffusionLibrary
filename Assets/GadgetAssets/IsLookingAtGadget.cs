using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks if the player is looking at the gadget.
/// </summary>
public class IsLookingAtGadget : MonoBehaviour
{
    // Head of the player
    [SerializeField] private GameObject _head;
    // Start is called before the first frame update
    [SerializeField] private float _viewingFieldDistance = 100;
    [SerializeField] private float _viewingFieldAngle = 30;

    public bool LookingAtGadget()
    {
        //1. Distance
        float distance = Vector3.Distance(
            transform.position,
            _head.transform.position);
        if (distance > _viewingFieldDistance)
        {
            return false;
        }

        //2. Angle
        Vector3 distanceVector = 
            _head.transform.position - transform.position;
        distanceVector.y = 0;
        float angle = Vector3.Angle(
            transform.forward,
            distanceVector);
        if (angle > _viewingFieldAngle / 2)
        {
            return false;
        }

        return true;
    }
}
