using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDepthCamera : MonoBehaviour
{
    [SerializeField] private Renderer _camRenderer;

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
        //Debug.Log(GetComponent<Renderer>().materials.ToString());
        
        Texture camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        Debug.Log(camDepthTexture.ToString());

        _camRenderer.material.SetTexture("_BaseMap", camDepthTexture);
        //renderer.material.mainTexture = camDepthTexture;
    }
}
