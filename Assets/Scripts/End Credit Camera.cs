using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCreditCamera : MonoBehaviour
{
    public Camera XROriginCamera;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DragCameraToXROriginCamera()
    {
        transform.position = XROriginCamera.transform.position;
        transform.rotation = XROriginCamera.transform.rotation;
    }
}
