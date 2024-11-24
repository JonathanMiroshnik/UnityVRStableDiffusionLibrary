using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


public enum GadgetSelection
{
    unSelected,
    preSelected,
    selected
}

// TODO change name of class to GadgetScript or something
public class Gadget : MonoBehaviour
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

    // TODO documentation
    public UIDiffusionTexture uiDiffusionTexture;
    public RadiusDiffusionTexture radiusDiffusionTexture;

    // XR Origin Main Camera
    public Camera xrCamera;

    // Strategy Design Pattern
    // Gadget Mechanisms list to be cycled through.
    [NonSerialized]
    public List<GadgetMechanism> GadgetMechanisms;

    // TODO make this Queue<List<Texture2D>>, talk to NADAV on gadget representation of special textures
    [NonSerialized]
    public Queue<Texture2D> textureQueue;

    // Controllers, for symmetric input
    public GameObject LeftHandController;
    public GameObject RightHandController;
    private string LeftHandBuildControllerInputName = "OculusTouchControllerLeft";
    private string RightHandBuildControllerInputName = "OculusTouchControllerRight";
    private string LeftHandLinkControllerInputName = "OculusTouchControllerOpenXR";
    private string RightHandLinkControllerInputName = "OculusTouchControllerOpenXR1";
    private string LeftHandSimulatedControllerInputName = "XRSimulatedController";
    private string RightHandSimulatedControllerInputName = "XRSimulatedController1";

    // Represents the current used Gadget Mechanism.
    private int gadgetMechanismIndex = 0;

    private void Awake()
    {
        textureQueue = new Queue<Texture2D>();
    }

    private void Start()
    {
        //gadgetCamera == null || 
        if (xrCamera == null || playSounds == null || gadgetImagePanel == null || aiGadgetAssistant == null ||
            uiDiffusionTexture == null || LeftHandController == null || RightHandController == null)
        {
            Debug.LogError("Add all requirements of Gadget");
            return;
        }

        if (GadgetMechanisms == null) GadgetMechanisms = new List<GadgetMechanism>();
        if (GadgetMechanisms.Count > 0 )
        {
            MechanismText.text = GadgetMechanisms[0].mechanismText;
        }
    }

    // Passing along the various Controller interactions onto the Mechanisms.
    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        playSounds.PlaySound("HoverOverElements");
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverEntered(args);
    }
    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverExited(args);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;        
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectEntered(args);
    }
    public void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectExited(args);
    }

    // TODO THESE 2 move this out of gadget, bad design need it in camera mechanism with correct input system of game.
    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[gadgetMechanismIndex].DiffusableGrabbed(args);
    }
    public void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        GadgetMechanisms[gadgetMechanismIndex].DiffusableUnGrabbed(args);
    }

    // For managing the current Diffusion Mechanism
    public void ChangeToNextMechanic()
    {
        if (GadgetMechanisms.Count <= 0) return;
        if (gadgetMechanismIndex >= GadgetMechanisms.Count) gadgetMechanismIndex %= GadgetMechanisms.Count;

        GadgetMechanisms[gadgetMechanismIndex].ResetMechanism();

        gadgetMechanismIndex++;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }

    // TODO do I need this one?
    public void ChangeToPreviousMechanic()
    {
        if (GadgetMechanisms.Count <= 0) return;
        if (gadgetMechanismIndex >= GadgetMechanisms.Count) gadgetMechanismIndex %= GadgetMechanisms.Count;

        GadgetMechanisms[gadgetMechanismIndex].ResetMechanism();

        gadgetMechanismIndex--;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }
    public void ChangeToMechanic(int index)
    {
        if (GadgetMechanisms.Count <= 0) return;
        
        gadgetMechanismIndex = index;
        MechanismText.text = GadgetMechanisms[index].mechanismText;
    }

    public Texture2D getGeneratedTexture()
    {
        if (GameManager.getInstance() == null) return null;
        if (textureQueue.Count == 0) return null;

        Texture2D current = textureQueue.Dequeue();
        
        if (uiDiffusionTexture != null)
        {
            uiDiffusionTexture.CreateImagesInside(textureQueue.Take(1).ToList(), displayMainTextureGadget, true);
            uiDiffusionTexture.CreateImagesInside(textureQueue.Take(9).Skip(1).ToList(), displayTexturesGadget, true);
        }
            
        return current;
    }

    public bool AddTexturesToQueue(List<Texture2D> textures)
    {
        if (GameManager.getInstance() == null) return false;

        if (textures == null) return false;

        foreach (Texture2D texture in textures)
        {
            textureQueue.Enqueue(texture);
        }

        if (uiDiffusionTexture != null)
        {
            uiDiffusionTexture.CreateImagesInside(textureQueue.Take(1).ToList(), displayMainTextureGadget, true);
            uiDiffusionTexture.CreateImagesInside(textureQueue.Take(9).Skip(1).ToList(), displayTexturesGadget, true);
        }        

        return true;
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


    // TODO should all these input functions below not send context onward? is context necessary for the specific mechanisms? should they deal with it, or Gadget?

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
 
            GadgetMechanisms[gadgetMechanismIndex].PlaceTextureInput(curController);   
            // TODO move these debugsLogs inside the specific mechanisms
            Debug.Log("Placing Texture");
        }
    }

    // TODO PlaySounds is in these functions, but they are only relevant for CERTAIN mechanisms, maybe play the sounds inside the mechanisms instead of here(this script?)
    public void ActivateGeneration(InputAction.CallbackContext context)
    {        
        if (GadgetMechanisms.Count <= 0) return;

        if (context.performed)
        {
            List<DiffusionTextureChanger> cur_list = new List<DiffusionTextureChanger>();
            cur_list.Add(uiDiffusionTexture); // TODO don't like these lines + why uiDiffusionTexture for all different mechanisms?

            GadgetMechanisms[gadgetMechanismIndex].ActivateGeneration(null, cur_list);
            Debug.Log("Generating Texture");
        }
    }

    // TODO change name of this function look at GadgetMechanism TODO note
    public void TakeScreenshot(Texture2D screenshot, Camera camera)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].TakeScreenshot(screenshot, camera);
        Debug.Log("Taking Screenshot");
    }
    public void GeneralActivation(DiffusionTextureChanger dtc)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].GeneralActivation(dtc);
        Debug.Log("Activating General Generation");
        return;
    }

    // TODO very different form other mechanisms because it isnt a diffusion mechanism, just a gadget one
    private bool isTracking = false;
    private Dictionary<GameObject, Transform> beginningGripDict = new Dictionary<GameObject, Transform>();
    public void GripProperty(InputAction.CallbackContext context)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GameObject curController = GetActionController(context);

        // Detect the start of the click
        if (context.started)
        {
            // Get the first position when the button is pressed down
            Transform curTrans = Transform.Instantiate(curController.transform);
            //curTrans.position = curController.transform.position;
            if (!beginningGripDict.ContainsKey(curController))
            {
                beginningGripDict.Add(curController, curTrans);
            }

            isTracking = true;
        }

        // Detect when the button is released
        if (context.canceled)
        {
            beginningGripDict.Remove(curController);
            isTracking = false;
        }
    }
    // TODO using this update exclusively for the above grip mechanism, not good,
    // need to figure out continuous button pressing operations that are constantly called on
    private void Update()
    {
        if (isTracking)
        {
            foreach (var key in beginningGripDict.Keys)
            {
                GadgetMechanisms[gadgetMechanismIndex].GripProperty(key, beginningGripDict[key]);
            }
        }
    }
}
