using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Image Gadget Mechanism for painting a canvas with a paintbrush and creating images through the painting.
/// </summary>
public class PaintbrushMechanism : GadgetMechanism
{
    public List<DiffusionTextureChanger> MechanismTextureChangers = new List<DiffusionTextureChanger>();
    public override string mechanismText => "Painting";

    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionModel = diffusionModels.Ghostmix;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        //newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Img2Img;
        newDiffusionRequest.positivePrompt += "smile"; // TODO: add some sort of prompt from outside, not predefined
        newDiffusionRequest.numOfVariations = 1;
        newDiffusionRequest.addToTextureTotal = false;

        newDiffusionRequest.denoise = 0.6f;

        return newDiffusionRequest;
    }

    public void ActivateGeneration(Texture2D canvasTexture)
    {
        // Add .png extension if not present
        if (canvasTexture != null && !canvasTexture.name.EndsWith(".png"))
        {
            canvasTexture.name += ".png";
        }

        DiffusionRequest diffusionRequest = CreateDiffusionRequest(MechanismTextureChangers);
        diffusionRequest.uploadTextures.Add(canvasTexture);

        ResetMechanism();
        GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(diffusionRequest);
    }
}