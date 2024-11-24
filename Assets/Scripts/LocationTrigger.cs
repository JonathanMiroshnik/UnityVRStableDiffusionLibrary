using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LocationTrigger : MonoBehaviour
{
    // GameObject we want to check for its distance from the center of this gameObject
    public GameObject followedGameObject;

    // Radius underwhich an even is triggered
    public float radius = 5f;

    // Event that is triggered when followedGameObject gets closer than radius to the gameObject center
    public UnityEvent UnderRadiusUnityEvent;

    // To keep congestion of calling functions low, no need to do it every update
    private float repeatEvery = 1f;

    private void Start()
    {
        InvokeRepeating("UnderRadiusTrigger", repeatEvery, repeatEvery);
    }

    private void UnderRadiusTrigger()
    {
        if (followedGameObject == null) return;
        if (Vector3.Distance(followedGameObject.transform.position, transform.position) < radius)
        {
            UnderRadiusUnityEvent?.Invoke();
        }
    }   
}
