using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Parent class for all Diffusion texture changers. Has a list of textures and an index indicating the current one of these textures.
/// Allows easy adding/changing of textures
/// </summary>
public class DiffusionTextureChanger : MonoBehaviour, ITextureReceiver
{
    // Event that is invoked when a texture(or several) is added
    public UnityEvent AddedTextureUnityEvent;
    // Textures that the DiffusionTextureChanger holds
    protected List<Texture2D> _diffTextures;
    // Index of the current texture
    protected int _curTextureIndex = 0;

    protected virtual void Awake()
    {
        _diffTextures = new List<Texture2D>();
    }

    /// <summary>
    /// Adds the textures that are in the DiffusionRequest to the diff_Textures list
    /// </summary>
    /// <returns>True if the addition of textures was successful</returns>
    public virtual bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (diffusionRequest.textures == null) return false;

        if (!diffusionRequest.addToTextureTotal)
        {
            _curTextureIndex = 0;
            _diffTextures = new List<Texture2D>();
            _diffTextures.Clear();
        }

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            _diffTextures.Add(texture);
        }

        AddedTextureUnityEvent?.Invoke();

        return true;
    }

    // TODO: older script of doing this, decided to make the diffusionrequest go all the way through to the end

    /// <summary>
    /// Adds the textures that are in the DiffusionRequest to the diff_Textures list
    /// </summary>
    /// <param name="newDiffTextures">Textures to add to the list</param>
    /// <param name="addToTextureTotal">True when the textures are added to the already existing list, false if the list is dropped for the given new textures</param>
    /// <returns>True if the addition of textures was successful</returns>
    public virtual bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        if (newDiffTextures == null) return false;

        if (!addToTextureTotal)
        {
            _curTextureIndex = 0;
            _diffTextures = new List<Texture2D>();
            _diffTextures.Clear();
        }

        foreach (Texture2D texture in newDiffTextures)
        {
            _diffTextures.Add(texture);
        }

        AddedTextureUnityEvent?.Invoke();

        return true;
    }

    /// <summary>
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
    }

    /// <returns>Current held textures</returns>
    public List<Texture2D> GetTextures()
    {
        return _diffTextures;
    }

    // Returns the index of the current texture
    public int GetTextureIndex()
    {
        return _curTextureIndex;
    }

    // Sets the index of the current texture
    public void SetTextureIndex(int newIndex)
    {
        if (newIndex < 0)
        {
            newIndex = 0;
        }
        _curTextureIndex = newIndex % _diffTextures.Count;     
    }

    // Implemented from ITextureReceiver
    public bool ReceiveTexture(Texture2D texture)
    {
        if (texture == null) return false;
        _diffTextures.Add(texture);
        return true;
    }

    // Implemented from ITextureReceiver
    public bool ReceiveTexturesFromDiffusionRequest(DiffusionRequest diffusionRequest) {        
        if (diffusionRequest == null) return false;
        if (diffusionRequest.textures == null) return false;

        if (!diffusionRequest.addToTextureTotal)
        {
            _curTextureIndex = 0;
            _diffTextures = new List<Texture2D>();
            _diffTextures.Clear();
        }

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            ReceiveTexture(texture);
        }

        AddedTextureUnityEvent?.Invoke();

        return true;
    }
}
