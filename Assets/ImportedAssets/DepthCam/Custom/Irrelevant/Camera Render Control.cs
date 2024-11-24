using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
 
public class CameraRenderControl : MonoBehaviour
{
    public Texture depthTexture;
 
    public List<GameObject> thingsToHide = new List<GameObject>();
    private List<GameObject> hiddenThings = new List<GameObject>();
 
    [Obsolete]
    public void PreRender(ScriptableRenderContext _context, Camera _camera)
    {
        Debug.Log("Called Pre Render!");
        // Do stuff here before the render, i.e. you could hide things specifically from this camera
        foreach (GameObject _thingToHide in thingsToHide)
        {
            _thingToHide.SetActive(false);
            hiddenThings.Add(_thingToHide);
        }
 
        // Manual Render (This might actually not be necessary but good to know how to do)
        UniversalRenderPipeline.RenderSingleCamera(_context, _camera); // NOTE: This is the obsolete method
        // the one below is the new broken one, use this until Unity fixes it
 
        //UniversalRenderPipeline.SubmitRenderRequest<UniversalAdditionalCameraData>(_camera, _camera.GetUniversalAdditionalCameraData());
    }
 
    public void PostRender(ScriptableRenderContext _context, Camera _camera)
    {
        Debug.Log("Called Post Render!");
        // Get Camera depth texture (Must be rendering to a used display OR a render texture)
        Texture _camDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
        if (!depthTexture) depthTexture = new Texture2D(_camDepthTexture.width, _camDepthTexture.height, TextureFormat.RFloat, false);
        Graphics.CopyTexture(_camDepthTexture, depthTexture);
 
        // Reactivate the hidden things after the render
        foreach (GameObject _hiddenThing in hiddenThings)
        {
            _hiddenThing.SetActive(true);
        }
        hiddenThings.Clear();
    }
}