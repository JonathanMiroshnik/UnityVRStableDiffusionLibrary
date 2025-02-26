using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeToDepthTexture : MonoBehaviour
{
    [SerializeField] private CameraRenderControl _cameraRenderControl;

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().material.mainTexture = _cameraRenderControl.depthTexture;
    }
}
