using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// TODO
/// </summary>
public class FlatObjectTextureChanger : DiffusionTextureChanger
{
    public RawImage image;

    private void Start()
    {
        if (image == null)
        {
            Debug.LogError("Add an image to the Flat Object Texture Changer");            
        }
    }

    /// <summary> TODO
    /// </summary>
    /// <returns></returns>
    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (diffusionRequest.textures == null) return false;

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            diff_Textures.Add(texture);
        }

        if (diff_Textures.Count == 1)
        {
            image.texture = diff_Textures[0]; // TODO what's the point of having multiple Textures EVER then?
        }
        else
        {
            Debug.LogError("You have generated something other than 1 image for this Texture Changer");
        }

        AddedTextureUnityEvent?.Invoke();

        return true;
    }

    public bool AddTexture(List<Texture2D> newDiffTextures)
    {
        if (newDiffTextures == null) return false;

        foreach (Texture2D texture in newDiffTextures)
        {
            diff_Textures.Add(texture);
        }

        if (diff_Textures.Count == 1)
        {
            image.texture = diff_Textures[0]; // TODO what's the point of having multiple Textures EVER then?
        }
        else
        {
            Debug.LogError("You have generated something other than 1 image for this Texture Changer");
        }

        AddedTextureUnityEvent?.Invoke();

        return true;
    }

    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        return AddTexture(newDiffTextures);
    }    

    /*/// <summary>
    /// Changes the texture of a GameObject to the given texture
    /// </summary>
    /// <param name="curGameObject">GameObject to change texture on</param>
    /// <param name="texture">Texture to change into</param>
    public virtual void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null) return;

        Renderer renderer = curGameObject.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
        renderer.material.SetTexture("_BaseMap", texture);
    }*/
}
