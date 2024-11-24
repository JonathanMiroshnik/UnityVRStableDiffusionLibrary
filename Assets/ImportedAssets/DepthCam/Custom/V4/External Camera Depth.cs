using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ExternalCameraDepth : MonoBehaviour
{
    [NotNull] [SerializeField] public RenderTexture _renderTexture;
    [NotNull] [SerializeField] private GameObject externalCamera;
    [NotNull] [SerializeField] private GameObject mainCamera;
    [NotNull] [SerializeField] private float swtichTime = 0.2f;
    private bool switchEnabled = false;

    public Texture2D _texture2D;

    [SerializeField] private Renderer project;
    // Start is called before the first frame update


    private void OnEnable()
    {
        //Invoke("StartSwitch", 2f);
    }
    

    // Update is called once per frame
    void Update()
    {
        if (switchEnabled)
        {
            externalCamera.SetActive(true);
            mainCamera.SetActive(false);
        }
        else
        {
            mainCamera.SetActive(true);
            externalCamera.SetActive(false);
        }
    }

    public void StartSwitch()
    {
        switchEnabled = true;
        Invoke("SaveTexture", swtichTime );
    }

    public void SaveTexture()
    {
        
        _texture2D = toTexture2D(_renderTexture);
        
        if (project != null)
        {
            project.material.mainTexture = _texture2D;
        }
        switchEnabled = false;
    }
    
    public Texture2D GetTexture()
    {

        return _texture2D;
    }
    
    
    public Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }
}
