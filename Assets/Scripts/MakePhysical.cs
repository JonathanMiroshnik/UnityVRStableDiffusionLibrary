using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a List of Objects become independent of their former context and physical
/// </summary>
public class MakePhysical : MonoBehaviour
{
    // List of Objects to effect
    public List<GameObject> GameObjectsToMakePhysical = new List<GameObject>();

    // New parent of the objects
    public Transform NewParent;

    /// <summary>
    /// Moves the objects in the list to the New Parent and turns them physical
    /// </summary>
    public void MakeObjectsPhysical()
    {
        foreach (GameObject obj in GameObjectsToMakePhysical)
        {            
            obj.transform.parent = NewParent;
            obj.AddComponent<Rigidbody>();
            if (obj.TryGetComponent<MeshCollider>(out MeshCollider MC))
            {
                MC.enabled = true;
            }
        }        
    }
}
