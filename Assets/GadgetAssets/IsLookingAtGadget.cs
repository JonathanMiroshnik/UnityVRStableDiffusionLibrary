using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsLookingAtGadget : MonoBehaviour
{
    [SerializeField] private GameObject head;
    // Start is called before the first frame update
    [SerializeField] private float viewingFieldDistance = 100;
    [SerializeField] private float viewingFieldAngle = 30;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool LookingAtGadget()
    {
        //1. Distance
        float distance = Vector3.Distance(
            transform.position,
            head.transform.position);
        if (distance > viewingFieldDistance)
        {
            return false;
        }

        //2. Angle
        Vector3 distanceVector = 
            head.transform.position - transform.position;
        distanceVector.y = 0;
        float angle = Vector3.Angle(
            transform.forward,
            distanceVector);
        if (angle > viewingFieldAngle / 2)
        {
            return false;
        }

        return true;
    }
}
