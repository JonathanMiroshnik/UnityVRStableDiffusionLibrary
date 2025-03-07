using System;
using System.Collections.Generic;
using UnityEngine;


// TODO: end of transition is too rapid, need smoothness when exchanging 2 textures
// TODO: stop with the m_ variables?

/// <summary>
/// Used on GameObject in the world for which we want to transition between textures in a smooth way.
/// Used in conjunction with the TextureTransitionShader.
/// </summary>
public class TextureTransition : DiffusionTextureChanger
{
    // Textures to be cycled through
    public List<Texture> textures;

    // Simple optimization to stop retexturing a single texture
    private bool _singleTexture = false;

    // True when Textures are endlessly and automatically transitioned from one to another.
    // False when one single transition, from texture to texture, will occur, and then stop instead of continuing onto the next transition.
    public bool constantTransition = true;

    public float transitionSpeed = 1f;
    [Range(0, 1)]
    public float noiseIntensity = 1f;
    private float m_noiseIntensity = 1f;
    [Range(0.01f, 1f)]
    public float smoothness = 0.5f;
    private float m_smoothness = 0.5f;

    [NonSerialized]
    public float transition = 0f;

    public string InitialTextureName = "_CurrentTex";
    public string AdditionalTextureName = "_NextTex";
    public string TransitionValueName = "_Transition";

    // Counter for the textures that are being cycled through
    // We use the default curTextureIndex for the other texture
    private int _nextTextureIndex = 1;

    private Material _transitionMaterial = null;    
    private Renderer _curRenderer = null;

    private void Start()
    {
        _curRenderer = GetComponent<Renderer>();
        if (_curRenderer == null) return;
        if (_curRenderer.material == null) return;

        // Checks if the current shader is appropriate as per its properties
        if (!_curRenderer.material.HasProperty(InitialTextureName) ||
            !_curRenderer.material.HasProperty(AdditionalTextureName)    ||
            !_curRenderer.material.HasProperty(TransitionValueName))
        {
            Debug.Log("Add correct shader to Game Object " + name);
            return;
        }

        _transitionMaterial = _curRenderer.material;
    }

    void Update()
    {
        if (textures == null || _transitionMaterial == null || _curRenderer == null) return;
        if (textures.Count <= 0) return;

        if (m_noiseIntensity != noiseIntensity)
        {
            if (!_curRenderer.material.HasProperty("_NoiseIntensity")) return;

            m_noiseIntensity = noiseIntensity;
            _transitionMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        }
        if (m_smoothness != smoothness)
        {
            if (!_curRenderer.material.HasProperty("_Smoothness")) return;

            m_smoothness = smoothness;
            _transitionMaterial.SetFloat("_Smoothness", smoothness);
        }

        if (textures.Count == 1)
        {
            if (_singleTexture) return;
            _transitionMaterial.SetTexture(AdditionalTextureName, textures[0]);
            _transitionMaterial.SetFloat(TransitionValueName, 1f);
            _singleTexture = true;
            return;
        }        

        _singleTexture = false;        

        // Animate the transition value
        transition += Time.deltaTime * transitionSpeed;
        if (transition > 1.0f)
        {
            // Stops the automatic step to the next transition in the list
            if (!constantTransition) return;

            TriggerNextTexture();
        }

        // Sends the needed parameters to the shader to look appropriate in accordance with the transition
        _transitionMaterial.SetFloat(TransitionValueName, transition);       
    }

    /// <summary>
    /// Triggers the next transition in the list
    /// </summary>
    public void TriggerNextTexture()
    {
        if (textures == null || _transitionMaterial == null) return;
        if (textures.Count <= 1) return;

        // TODO: should I keep this? smoother audioReaction?
        if (!constantTransition)
        {
            if (transition < 1.0f) return;
        }

        transition = 0f;

        // TODO: why is there two indexes and is this modulo correct?
        _curTextureIndex = _nextTextureIndex;
        _nextTextureIndex = (_curTextureIndex + 1) % textures.Count;

        if (_curTextureIndex >= textures.Count || _nextTextureIndex >= textures.Count) return;
        // Update the textures in the shader
        _transitionMaterial.SetTexture(InitialTextureName, textures[_curTextureIndex]);
        _transitionMaterial.SetTexture(AdditionalTextureName, textures[_nextTextureIndex]);
    }

    public void ResetTransition()
    {
        if (_transitionMaterial == null) return;

        textures = new List<Texture>();
        transition = 0;
        _curTextureIndex = 0;
        _nextTextureIndex = 1;

        _transitionMaterial.SetTexture(InitialTextureName, null);
        _transitionMaterial.SetTexture(AdditionalTextureName, null);
    }

    /// <summary>
    /// Changes all the relevant parameters in the Texture Transition and restarts it with these parameters.
    /// </summary>
    /// <param name="curTextures">New Textures of the TransitionTexture</param>
    /// <param name="curTransition">Transition value, if out of range, will not change the current value</param>
    /// <param name="curNoiseIntensity">Noise Intensity value, if out of range, will not change the current value</param>
    /// <param name="curSmoothness">Smoothness value, if out of range, will not change the current value</param>
    public void TransitionTextures(List<Texture> curTextures, float curTransition, float curNoiseIntensity, float curSmoothness)
    {
        if (curTransition >= 0 && curTransition <= 1)
        {
            transition = curTransition;
        }
        if (curNoiseIntensity >= 0.01f && curNoiseIntensity <= 1)
        {
            noiseIntensity = curNoiseIntensity;
        }
        if (curSmoothness >= 0 && curSmoothness <= 1)
        {
            smoothness = curSmoothness;
        }

        ResetTransition();

        textures = curTextures;
        
        TriggerNextTexture();
    }

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (base.AddTexture(diffusionRequest))
        {
            textures = new List<Texture>(_diffTextures);
            return true;
        }

        return false;
    }

    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        if (base.AddTexture(newDiffTextures, addToTextureTotal))
        {
            textures = new List<Texture>(_diffTextures);
            return true;
        }

        return false;
    }

    public override void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (texture == null) return;

        Texture2D copiedTexture = GeneralGameLibraries.TextureManipulationLibrary.CopyTexture(texture);
        textures = new List<Texture>() { copiedTexture };
    }

    public void changeTextureOn(Texture2D texture)
    {
        changeTextureOn(null, texture);
    }
}
