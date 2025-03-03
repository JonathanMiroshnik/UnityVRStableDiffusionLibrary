using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCreditCamera : MonoBehaviour
{
    public Camera XROriginCamera;

    public void DragCameraToXROriginCamera()
    {
        transform.position = XROriginCamera.transform.position;
        transform.rotation = XROriginCamera.transform.rotation;
    }
}
