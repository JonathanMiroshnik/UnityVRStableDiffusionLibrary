using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Image Gadget Mechanism for painting a canvas with a paintbrush and creating images through the painting.
/// </summary>
public class PaintbrushMechanism : GadgetMechanism
{
    public List<DiffusionTextureChanger> mechanismTextureChangers = new List<DiffusionTextureChanger>();

    private void Awake()
    {
        mechanismText = "Painting";
    }

    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        //newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.img2img;
        newDiffusionRequest.positivePrompt = "smile"; // TODO add some sort of prompt from outside, not predefined
        newDiffusionRequest.numOfVariations = 1;

        newDiffusionRequest.denoise = 0.6f;

        return newDiffusionRequest;
    }

    public void ActivateGeneration(Texture2D canvasTexture)
    {
        DiffusionRequest diffusionRequest = CreateDiffusionRequest(mechanismTextureChangers);
        diffusionRequest.uploadTextures.Add(canvasTexture);

        ResetMechanism();
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }
}