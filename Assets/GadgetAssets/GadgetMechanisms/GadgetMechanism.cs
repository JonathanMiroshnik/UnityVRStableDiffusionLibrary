using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

/// <summary>
/// Represents a Gadget(Diffusion images making device) Mechanism, using a Strategy design pattern.
/// Other Gadget Mechanisms inherit from this one.
/// </summary>
public class GadgetMechanism : MonoBehaviour
{
    /// <summary>
    /// Text that will be shown that represents and indicates the mechanism.
    /// </summary>
    // [NonSerialized]
    public virtual string mechanismText { get; protected set; } = "";
    // public string mechanismText;

    // Gadget that the mechanism belongs to
    public Gadget gadget;

    // Event that is called when the mechanism is activated, but can be used for other purposes
    public UnityEvent MechanismExtraEvent;

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
    public virtual void ResetMechanism() {}

    /// <summary>
    /// Left hand controller ray hover entered.
    /// </summary>
    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args) {}

    /// <summary>
    /// Left hand controller ray hover exited.
    /// </summary>
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args) {}

    /// <summary>
    /// Left hand controller ray select entered.
    /// </summary>
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args) {}

    /// <summary>
    /// Left hand controller ray select exited.
    /// </summary>
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args) {}

    public virtual void OnClick() {}

    // TODO: docu
    // TODO: do I even need these two?
    public virtual void OnActivate(ActivateEventArgs args) {}
    public virtual void OnDeActivate(DeactivateEventArgs args) {}

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Places a texture on a selected object.
    /// </summary>
    public virtual void PlaceTextureInput(GameObject GO) {}

    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="GO">GameObject sent to be par tof the generation</param>
    /// <param name="diffusionTextureChangers">Diffusion Texture Changers that are added to the targets of the resultant DiffusionRequest</param>
    public virtual void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers) {}

    /// <summary>
    /// Activates the Diffusion image generation.
    /// </summary>
    /// <param name="GO">GameObject sent to be par tof the generation</param>
    public virtual void ActivateGeneration(GameObject GO) {}

    // TODO: change the name of this one from TakeScreenshot to something like "TriggerButtonAction" to be more general and not for one specific mechanism
    /// <summary>
    /// Uses a camera to shoot an image.
    /// </summary>
    public virtual void TakeScreenshot(Texture2D screenshot, Camera camera) {}
    public virtual void GeneralActivation(DiffusionTextureChanger dtc) {}

    public virtual void DiffusableGrabbed(SelectEnterEventArgs args) {}
    public virtual void DiffusableUnGrabbed(SelectExitEventArgs args) {}

    // TODO: this is a hyper specific function for a hyper specific mechanism
    public virtual void GripProperty(GameObject GO, Pose curPose) {}
}