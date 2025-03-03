using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Gadget Selection State, used to determine if an object is selected, pre-selected, or not selected by the Gadget.
/// </summary>
public enum GadgetSelection
{
    unSelected,
    preSelected,
    selected
}

// TODO: add tooltip and header and other unity things
// TODO: change name of class to GadgetScript or something

public class Gadget : MonoBehaviour, ITextureReceiver
{
    // Texture display for the images the Gadget is holding onto
    public GameObject displayTexturesGadget;
    
    // Texture display for the current top of the queue Image
    public GameObject displayMainTextureGadget;

    // TextMeshPro object that shows the text that represents the current Mechanism.
    public TextMeshProUGUI MechanismText;
    public GameObject gadgetImagePanel;

    // Plays the sounds associated with the Gadget's actions.
    public PlaySounds playSounds;

    // Manages the AI Gadget Assistant.
    public AIGadgetAssistant aiGadgetAssistant;

    // TODO: documentation
    public UIDiffusionTexture uiDiffusionTexture;
    public RadiusDiffusionTexture radiusDiffusionTexture;

    // XR Origin Main Camera
    public Camera xrCamera;

    // Strategy Design Pattern
    // Gadget Mechanisms list to be cycled through.
    [NonSerialized]
    public List<GadgetMechanism> GadgetMechanisms;

    // TODO: make this Queue<List<Texture2D>>, talk to NADAV on gadget representation of special textures
    [NonSerialized]
    public Queue<Texture2D> TextureQueue;

    // Controllers, for symmetric input
    public GameObject LeftHandController;
    public GameObject RightHandController;

    // Controller Input Names depending on the platform
    private const string LeftHandBuildControllerInputName = "OculusTouchControllerLeft";
    private const string RightHandBuildControllerInputName = "OculusTouchControllerRight";
    private const string LeftHandLinkControllerInputName = "OculusTouchControllerOpenXR";
    private const string RightHandLinkControllerInputName = "OculusTouchControllerOpenXR1";
    private const string LeftHandSimulatedControllerInputName = "XRSimulatedController";
    private const string RightHandSimulatedControllerInputName = "XRSimulatedController1";

    // Represents the current used Gadget Mechanism.
    private int _gadgetMechanismIndex = 0;

    private void Awake()
    {
        TextureQueue = new Queue<Texture2D>(); // TODO: do I need this?
    }

    private void Start()
    {
        if (xrCamera == null || playSounds == null || gadgetImagePanel == null || aiGadgetAssistant == null ||
            uiDiffusionTexture == null || LeftHandController == null || RightHandController == null)
        {
            Debug.LogError("Add all requirements of Gadget");
            return;
        }

        if (GadgetMechanisms == null) GadgetMechanisms = new List<GadgetMechanism>();
        if (GadgetMechanisms.Count > 0 )
        {
            ChangeToMechanic(0);
        }
    }

    // Passing along the various Controller interactions onto the Mechanisms.
    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        playSounds.PlaySound("HoverOverElements");
        GadgetMechanisms[_gadgetMechanismIndex].OnGameObjectHoverEntered(args);
    }
    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[_gadgetMechanismIndex].OnGameObjectHoverExited(args);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;        
        GadgetMechanisms[_gadgetMechanismIndex].onGameObjectSelectEntered(args);
    }
    public void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[_gadgetMechanismIndex].onGameObjectSelectExited(args);
    }

    // TODO: THESE 2 move this out of gadget, bad design need it in camera mechanism with correct input system of game.
    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[_gadgetMechanismIndex].DiffusableGrabbed(args);
    }
    public void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[_gadgetMechanismIndex].DiffusableUnGrabbed(args);
    }

    // For managing the current Diffusion Mechanism
    public void ChangeToNextMechanic()
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[_gadgetMechanismIndex].ResetMechanism();

        _gadgetMechanismIndex++;
        _gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(_gadgetMechanismIndex);
    }

    public void ChangeToMechanic(int index)
    {
        // TODO: need to make "ExecuteIfMechanismExists" with => lambda thing
        // EXAMPLE: ExecuteIfMechanismsExist(() => {
            //     playSounds.PlaySound("HoverOverElements");
            //     GadgetMechanisms[_gadgetMechanismIndex].OnGameObjectHoverEntered(args);
            // });
        // TODO: do the same in mechanisms too, there are a lot of these types of codes.
        if (GadgetMechanisms.Count <= 0) return;
        
        _gadgetMechanismIndex = index;
        MechanismText.text = GadgetMechanisms[index].mechanismText;
    }

    public Texture2D getGeneratedTexture()
    {
        if (GameManager.getInstance() == null) return null;
        if (TextureQueue.Count == 0) return null;

        Texture2D current = TextureQueue.Dequeue();
        
        if (uiDiffusionTexture != null)
        {
            uiDiffusionTexture.CreateImagesInside(TextureQueue.Take(1).ToList(), displayMainTextureGadget, true);
            uiDiffusionTexture.CreateImagesInside(TextureQueue.Take(9).Skip(1).ToList(), displayTexturesGadget, true);
        }
            
        return current;
    }



    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Function that finds the controller that performed an action given the action's context
    /// </summary>
    /// <returns>GameObject representing the Controller</returns>
    public GameObject GetActionController(InputAction.CallbackContext context)
    {
        var device = context.control.device;

        if (device.name == LeftHandBuildControllerInputName || device.name == LeftHandLinkControllerInputName ||  
            device.name == LeftHandSimulatedControllerInputName)
        {
            return LeftHandController;
        }
        else if (device.name == RightHandBuildControllerInputName || device.name == RightHandLinkControllerInputName || 
            device.name == RightHandSimulatedControllerInputName)
        {
            return RightHandController;
        }

        return null;
    }


    // TODO: should all these input functions below not send context onward? is context necessary for the specific mechanisms? should they deal with it, or Gadget?

    public void ChangeMechanicInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ChangeToNextMechanic();
        }
    }
    public void PlaceTextureInput(InputAction.CallbackContext context)
    {
        if (GadgetMechanisms.Count <= 0) return;
        
        if (context.performed)
        {
            GameObject curController = GetActionController(context);
 
            GadgetMechanisms[_gadgetMechanismIndex].PlaceTextureInput(curController);   
            // TODO: move these debugsLogs inside the specific mechanisms
            Debug.Log("Placing Texture");
        }
    }

    // TODO: PlaySounds is in these functions, but they are only relevant for CERTAIN mechanisms, maybe play the sounds inside the mechanisms instead of here(this script?)
    public void ActivateGeneration(InputAction.CallbackContext context)
    {        
        if (GadgetMechanisms.Count <= 0) return;

        if (context.performed)
        {
            List<DiffusionTextureChanger> cur_list = new List<DiffusionTextureChanger>();
            cur_list.Add(uiDiffusionTexture); // TODO: don't like these lines + why uiDiffusionTexture for all different mechanisms?

            GadgetMechanisms[_gadgetMechanismIndex].ActivateGeneration(null, cur_list);
            Debug.Log("Generating Texture");
        }
    }

    // TODO: change name of this function look at GadgetMechanism TODO: note
    public void TakeScreenshot(Texture2D screenshot, Camera camera)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[_gadgetMechanismIndex].TakeScreenshot(screenshot, camera);
        Debug.Log("Taking Screenshot");
    }
    public void GeneralActivation(DiffusionTextureChanger dtc)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[_gadgetMechanismIndex].GeneralActivation(dtc);
        Debug.Log("Activating General Generation");
        return;
    }

    // TODO: very different form other mechanisms because it isnt a diffusion mechanism, just a gadget one
    private bool isTracking = false;

    // Tracks the position of the controllers that are currently gripping // TODO: should be a mechanism? like above TODO: note
    private Dictionary<GameObject, Pose> beginningGripDict = new Dictionary<GameObject, Pose>();

    public void GripProperty(InputAction.CallbackContext context)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GameObject curController = GetActionController(context);

        // Detect the start of the click
        if (context.started)
        {
            // Get the first position when the button is pressed down
            Pose curPose = new Pose(curController.transform.position, curController.transform.rotation);
            
            //curTrans.position = curController.transform.position;
            if (!beginningGripDict.ContainsKey(curController))
            {
                beginningGripDict.Add(curController, curPose);
            }

            isTracking = true;
        }

        // Detect when the button is released
        if (context.canceled)
        {
            // TODO: possible memory problem with Pose not being deleted? garbage collection?
            beginningGripDict.Remove(curController);
            isTracking = false;
        }
    }
    // TODO: using this update exclusively for the above grip mechanism, not good,
    // need to figure out continuous button pressing operations that are constantly called on
    private void Update()
    {
        if (isTracking)
        {
            foreach (var key in beginningGripDict.Keys)
            {
                GadgetMechanisms[_gadgetMechanismIndex].GripProperty(key, beginningGripDict[key]);
            }
        }
    }

    // Implemented from ITextureReceiver
    public bool ReceiveTexture(Texture2D texture)
    {
        if (GameManager.getInstance() == null) return false; // TODO: repeating this check in every function?
        if (texture == null) return false;

        TextureQueue.Enqueue(texture);

        if (uiDiffusionTexture != null)
        {
            uiDiffusionTexture.CreateImagesInside(TextureQueue.Take(1).ToList(), displayMainTextureGadget, true);
            uiDiffusionTexture.CreateImagesInside(TextureQueue.Take(9).Skip(1).ToList(), displayTexturesGadget, true);
        }        

        return true;
    }

    // Implemented from ITextureReceiver
    public bool ReceiveTexturesFromDiffusionRequest(DiffusionRequest diffusionRequest) {
        if (diffusionRequest == null) return false;
        if (diffusionRequest.textures == null) return false;

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            ReceiveTexture(texture);
        }

        return true;
    }
}
