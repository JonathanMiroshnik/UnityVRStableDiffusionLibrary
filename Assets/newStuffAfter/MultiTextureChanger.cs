using System;
using System.Collections.Generic;
using UnityEngine;

// TODO check if the SidesCube works with this script, and why does only this mechanism even work like this?

/// <summary>
/// DiffusionTextureChanger used for the SidesCubeGadgetMechanism to add the correct textures to the SidesCube
/// </summary>
public class MultiTextureChanger : DiffusionTextureChanger
{
    // Activates the event after the number of downloaded textures has been met
    public int TextureThreshold = 1;    

    [NonSerialized]
    public bool filledThreshold = false; // TODO do I need this parameter?

    /// <summary>
    /// Adds the textures that are in the DiffusionRequest to the diff_Textures list
    /// </summary>
    /// <returns>True if the addition of textures was successful</returns>
    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (diffusionRequest.textures == null) return false;

        if (!diffusionRequest.addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures = new List<Texture2D>();
            diff_Textures.Clear();

            filledThreshold = false;
        }

        int currentTextureNum = diff_Textures.Count;
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            if (currentTextureNum >= TextureThreshold) break;
            currentTextureNum++;

            diff_Textures.Add(texture);
        }

        if (currentTextureNum >= diff_Textures.Count)
        {
            AddedTextureUnityEvent?.Invoke();
            filledThreshold = true;
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
            curTextureIndex = 0;
            diff_Textures = new List<Texture2D>();
            diff_Textures.Clear();

            filledThreshold = false;
        }

        int currentTextureNum = diff_Textures.Count;
        foreach (Texture2D texture in newDiffTextures)
        {
            if (currentTextureNum >= TextureThreshold) break;
            currentTextureNum++;

            diff_Textures.Add(texture);
        }

        if (currentTextureNum >= diff_Textures.Count)
        {
            AddedTextureUnityEvent?.Invoke();
            filledThreshold = true;
        }

        return true;
    }

    public List<Texture2D> GetTexturesAndReset()
    {
        List<Texture2D> ret_textures = null;

        if (filledThreshold)
        {
            ret_textures = new List<Texture2D>();
            for (int i = 0; i < diff_Textures.Count; i++)
            {
                ret_textures.Add(diff_Textures[i]);
            }

            diff_Textures = new List<Texture2D> ();
            filledThreshold = false;            
        }

        return ret_textures;
    }
}
