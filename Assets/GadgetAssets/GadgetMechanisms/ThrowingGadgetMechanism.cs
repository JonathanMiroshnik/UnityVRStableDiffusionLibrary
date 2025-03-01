using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Image Gadget Mechanism for picking up a DiffusableObject with an embedded text prompt, and creating images with it.
/// After the images are created, the object lights up, is thrown, and when it impacts blocks with DiffusionTextureChangers, 
/// it adds the textures to these. The point of this mechanism is to be quick and created low fidelity imagery to achieve this.
/// </summary>
public class ThrowingGadgetMechanism : GadgetMechanism
{
    public override string mechanismText => "Throwing";

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Throwing Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionModel = diffusionModels.Nano;

        foreach(DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Txt2ImgLCM;
        newDiffusionRequest.numOfVariations = 5;

        return newDiffusionRequest;
    }

    public override void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if(args.interactableObject == null) return;
        
        DiffusionRequest diffusionRequest = CreateDiffusionRequest(new List<DiffusionTextureChanger> { gadget.radiusDiffusionTexture });

        GameObject interactorObject = args.interactableObject.transform.gameObject;
        diffusionRequest.diffusableObject = interactorObject.GetComponent<DiffusableObject>();
        diffusionRequest.positivePrompt = diffusionRequest.diffusableObject.keyword;

        GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(diffusionRequest);        
    }
    public override void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (args.interactableObject == null) return;

        // Lighting up the picked up DiffusableObject forwhich the images were created
        if (args.interactableObject.transform.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}