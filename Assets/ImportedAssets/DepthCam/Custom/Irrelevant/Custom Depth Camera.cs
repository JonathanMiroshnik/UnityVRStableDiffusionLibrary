using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDepthCamera : MonoBehaviour
{
    [SerializeField] private Renderer camRenderer;

    private Camera depthDamera;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
        //Debug.Log(GetComponent<Renderer>().materials.ToString());
        
        Texture camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        Debug.Log(camDepthTexture.ToString());

        camRenderer.material.SetTexture("_BaseMap", camDepthTexture);
        //renderer.material.mainTexture = camDepthTexture;
    }
}
