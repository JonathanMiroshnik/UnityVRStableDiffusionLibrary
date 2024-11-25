using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents an object that can create a Diffusion Request.
/// It is not required however that it is part of the diffusables, 
/// I.E. it doesn't have to be effected by textures being added, it _could_ just be used to create.
/// </summary>
public class DiffusableObject : MonoBehaviour
{
    // Used to add to the prompts in the DiffusionRequests
    public string keyword;

    // True if it is grabbable, needs to have a GrabInteractable component if that is the case
    public bool grabbable;

    // True if grabbed
    [NonSerialized]
    public bool grabbed;

    // todo maybe the grabbed and ungrabbed should be here as well? maybe another script of theirs instead of gadgetmechanismS?
    
    // If true, then the DiffusableObject is a complex 3D model and not a simple shape/image container
    public bool Model3D = false;

    // Latest/Current object that is interacting with the DiffusableObject
    private Gadget currentGadget;

    // Responsible for playing the sounds related to the DiffusableObject
    public PlaySounds playSounds;

    private void Start()
    {
        // todo should grabbable raise a bigger alert? do I even need grabbable?
        if (grabbable && GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            Debug.LogError("Add a GrabInteractable on");
        }
    }

    // TODO what about when not using throwing mechanism? only thing that is stopping this from activating is just grabbed and radiusDiffusionTexture?
    private void OnCollisionEnter(Collision collision)
    {
        if (grabbed) return;
        if (GameManager.getInstance() == null) return;

        playSounds.PlaySound("FallSound");

        if (currentGadget == null) return;
        currentGadget.radiusDiffusionTexture.DiffusableObjectCollided(collision);
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        grabbed = true;

        ControllerGadgetConnector controllerGadgetConnector = args.interactorObject.transform.gameObject.GetComponent<ControllerGadgetConnector>();
        if (controllerGadgetConnector == null) Debug.LogError("Gadget connector not found in " + args.interactorObject.transform.gameObject.name);

        Gadget gadget = controllerGadgetConnector.gadget;
        if (gadget == null) Debug.LogError("Gadget in Gadget connector not found in " + args.interactorObject.transform.gameObject.name);

        currentGadget = gadget;

        gadget.DiffusableGrabbed(args);
    }
    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        grabbed = false;

        ControllerGadgetConnector controllerGadgetConnector = args.interactorObject.transform.gameObject.GetComponent<ControllerGadgetConnector>();
        if (controllerGadgetConnector == null) Debug.LogError("Gadget connector not found in " + args.interactorObject.transform.gameObject.name);

        Gadget gadget = controllerGadgetConnector.gadget;
        if (gadget == null) Debug.LogError("Gadget in Gadget connector not found in " + args.interactorObject.transform.gameObject.name);

        gadget.DiffusableUnGrabbed(args);
    }
}
