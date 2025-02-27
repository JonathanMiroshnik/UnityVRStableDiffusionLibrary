using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowGameObject : MonoBehaviour
{
    public Transform transformToFollow;
    private Transform _startTransform;

    // TODO: what if transformToFollow BECOMES null? do I save a new startTransform until a new one appears? or do I simply keep the original startTransform?

    private void Update()
    {
        transform.position = transformToFollow.position;
        transform.rotation = transformToFollow.rotation;
        transform.localScale = transformToFollow.localScale * 0.01f;
    }

    /*void Update()
    {
        Vector3 rotatedOffset = startTransform.rotation * transformToFollow.position; //Rotates the offset vector by the parent's rotation
        transform.position = startTransform.position + rotatedOffset;
    }*/
}
