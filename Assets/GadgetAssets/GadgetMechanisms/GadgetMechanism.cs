using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

/// <summary>
/// Represents a Gadget(Diffusion images making device) Mechanism, using a Strategy design pattern.
/// Other Gadget Mechanisms inherit from this one.
/// </summary>
public class GadgetMechanism : MonoBehaviour
{
    public static string MECHANISM_PRETEXT = "Mechanism:\n";

    /// <summary>
    /// Text that will be shown that represents and indicates the mechanism.
    /// </summary>
    [NonSerialized]
    public string mechanismText;

    // TODO: documentation
    public Gadget gadget;

    public GadgetMechanism()
    {
        this.mechanismText = "";
    }

    private void Start()
    {
        if (gadget == null) Debug.LogError("Add Gadget to the Mechanism: " + name);
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for a given mechanism.
    /// </summary>
    /// <param name="diffusionTextureChangers">Diffusion Texture Changers that are added to the targets of the DiffusionRequest</param>
    /// <returns></returns>
    protected virtual DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        return null;
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for a given mechanism.
    /// </summary>
    /// <returns></returns>
    protected virtual DiffusionRequest CreateDiffusionRequest()
    {
        return null;
    }

    // TODO: implement in the mechanisms - NOTE: should this be implemented in the throwing mechanism? ALSO what happens with uiDiffusionTexture?
    /// <summary>
    /// Resets the Mechanism, removing everything it has selected.
    /// </summary>
    public virtual void ResetMechanism()
    {
        return;
    }

    /// <summary>
    /// Left hand controller ray hover entered.
    /// </summary>
    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray hover exited.
    /// </summary>
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select entered.
    /// </summary>
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select exited.
    /// </summary>
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        return;
    }

    public virtual void OnClick()
    {
        return;
    }

    // TODO: do I even need these two?
    public virtual void OnActivate(ActivateEventArgs args)
    {
        return;
    }
    public virtual void OnDeActivate(DeactivateEventArgs args)
    {
        return;
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Places a texture on a selected object.
    /// </summary>
    public virtual void PlaceTextureInput(GameObject GO)
    {
        return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="GO">GameObject sent to be par tof the generation</param>
    /// <param name="diffusionTextureChangers">Diffusion Texture Changers that are added to the targets of the resultant DiffusionRequest</param>
    public virtual void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        return;
    }

    /// <summary>
    /// Activates the Diffusion image generation.
    /// </summary>
    /// <param name="GO">GameObject sent to be par tof the generation</param>
    public virtual void ActivateGeneration(GameObject GO)
    {
        return;
    }

    // TODO: change the name of this one from TakeScreenshot to something like "TriggerButtonAction" to be more general and not for one specific mechanism
    /// <summary>
    /// Uses a camera to shoot an image.
    /// </summary>
    public virtual void TakeScreenshot(Texture2D screenshot, Camera camera)
    {
        return;
    }
    public virtual void GeneralActivation(DiffusionTextureChanger dtc)
    {
        return;
    }

    public virtual void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        return;
    }
    public virtual void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        return;
    }

    // TODO: this is a hyper specific function for a hyper specific mechanism
    public virtual void GripProperty(GameObject GO, Transform curTransform)
    {
        return;
    }
}