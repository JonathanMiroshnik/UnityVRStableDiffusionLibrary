using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: check if the SidesCube works with this script, and why does only this mechanism even work like this?

/// <summary>
/// DiffusionTextureChanger used for the SidesCubeGadgetMechanism to add the correct textures to the SidesCube
/// </summary>
public class MultiTextureChanger : DiffusionTextureChanger
{
    // Activates the event after the number of downloaded textures has been met
    public int TextureThreshold = 1;    

    [NonSerialized]
    public bool FilledThreshold = false; // TODO: do I need this parameter?

    // TODO: lots of repeating code here, can be refactored

    /// <summary>
    /// Adds the textures that are in the DiffusionRequest to the diff_Textures list
    /// </summary>
    /// <returns>True if the addition of textures was successful</returns>
    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (diffusionRequest.textures == null) return false;

        if (!diffusionRequest.addToTextureTotal)
        {
            _curTextureIndex = 0;
            _diffTextures = new List<Texture2D>();
            _diffTextures.Clear();

            FilledThreshold = false;
        }

        int currentTextureNum = _diffTextures.Count;
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            if (currentTextureNum >= TextureThreshold) break;
            currentTextureNum++;

            _diffTextures.Add(texture);
        }

        if (currentTextureNum >= _diffTextures.Count)
        {
            AddedTextureUnityEvent?.Invoke();
            FilledThreshold = true;
        }       

        return true;
    }

    /// <summary>
    /// Adds the textures that are in the DiffusionRequest to the diff_Textures list
    /// </summary>
    /// <param name="newDiffTextures">Textures to add to the list</param>
    /// <param name="addToTextureTotal">True when the textures are added to the already existing list, false if the list is dropped for the given new textures</param>
    /// <returns>True if the addition of textures was successful</returns>
    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        if (newDiffTextures == null) return false;

        if (!addToTextureTotal)
        {
            _curTextureIndex = 0;
            _diffTextures = new List<Texture2D>();
            _diffTextures.Clear();

            FilledThreshold = false;
        }

        int currentTextureNum = _diffTextures.Count;
        foreach (Texture2D texture in newDiffTextures)
        {
            if (currentTextureNum >= TextureThreshold) break;
            currentTextureNum++;

            _diffTextures.Add(texture);
        }

        if (currentTextureNum >= _diffTextures.Count)
        {
            AddedTextureUnityEvent?.Invoke();
            FilledThreshold = true;
        }

        return true;
    }

    public List<Texture2D> GetTexturesAndReset()
    {
        List<Texture2D> ret_textures = null;

        if (FilledThreshold)
        {
            ret_textures = new List<Texture2D>();
            for (int i = 0; i < _diffTextures.Count; i++)
            {
                ret_textures.Add(_diffTextures[i]);
            }

            _diffTextures = new List<Texture2D> ();
            FilledThreshold = false;            
        }

        return ret_textures;
    }
}
