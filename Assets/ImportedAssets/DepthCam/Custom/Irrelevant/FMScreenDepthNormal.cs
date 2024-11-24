using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FMScreenDepthNormal : MonoBehaviour
{
    public Camera Cam;

    public Material Mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Cam == null)
        {
            Cam = this.GetComponent<Camera>();
            Cam.depthTextureMode = DepthTextureMode.Depth;
        }

        if (Mat == null)
        {
            Mat = new Material(Shader.Find(""));
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }
}
