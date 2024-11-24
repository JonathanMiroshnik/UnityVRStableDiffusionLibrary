using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeToDepthTexture : MonoBehaviour
{
    [SerializeField] private CameraRenderControl _cameraRenderControl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().material.mainTexture = _cameraRenderControl.depthTexture;
    }
}
