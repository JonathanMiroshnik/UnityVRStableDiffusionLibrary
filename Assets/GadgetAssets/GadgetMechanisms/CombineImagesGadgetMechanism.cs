using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static GeneralGameLibraries;
using UnityEngine.Events;
using System;

/// <summary>
/// Image Gadget Mechanism for combining several images together to one new image
/// As of 23.11.24, one image is used as the base to Diffuse further from(as the initial noise) and the other is used for its' style.
/// </summary>
public class CombineImagesGadgetMechanism : GadgetMechanism
{
    // Objects with textures that are chosen to be combined
    [NonSerialized]
    public Queue<GameObject> selectedObjects = new Queue<GameObject>();

    // Number of images to combine together(as of 23.11.24, it is only possible to do 2)
    [NonSerialized]
    public int MAX_QUEUED_OBJECTS = 2;
    
    public override string mechanismText => "Combine\nImages";

    /// <summary>
    /// Helper function for the Combine Mechanism script that checks whether a interactable object should be interacted with further.
    /// </summary>
    /// <param name="args">Interactable Object args to check</param>
    /// <returns>True if should be interacted with</returns>
    private bool ValidInteractableObject(BaseInteractionEventArgs args)
    {
        if (args == null || args.interactableObject == null) return false;
        if (GameManager.getInstance() == null) return false;

        Transform curTrans = args.interactableObject.transform;
        if (!GameManager.getInstance().DiffusionList.Contains(curTrans.gameObject)) return false;
        if (curTrans.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            // Textures from 3D models are not taken into consideration
            if (DO.Model3D) return false;
        }
        if (selectedObjects.Contains(curTrans.gameObject)) return false;
        if (curTrans.gameObject.TryGetComponent<Renderer>(out Renderer REN))
        {
            // A mainTexture needs to exist for the chosen object to be able to use it
            if (REN.material.mainTexture == null) return false;
        }
        else
        {
            return false;
        }

        return true;
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Creates pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Remove pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Adds to queue of selected objects
        if (selectedObjects.Count >= MAX_QUEUED_OBJECTS)
        {
            GameObject dequeObject = selectedObjects.Dequeue();
            GameObjectManipulationLibrary.ChangeOutline(dequeObject, GadgetSelection.unSelected);
        }

        selectedObjects.Enqueue(args.interactableObject.transform.gameObject);
        if (gadget != null)
        {
            gadget.playSounds.PlaySound("SelectElement");
        }

        // Creates selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Combine Images Mechanism
    /// </summary>
    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        //newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        //newDiffusionRequest.diffusionModel = diffusionModels.mini; // produces bad results
        newDiffusionRequest.diffusionModel = diffusionModels.JuggernautReborn;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        newDiffusionRequest.targets.Add(gadget.uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.CombineImages;
        newDiffusionRequest.numOfVariations = 1;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;
        if (selectedObjects.Count != MAX_QUEUED_OBJECTS) return;

        // Extracting the textures of the two Diffusable GameObjects
        GameObject firstGameObject = selectedObjects.Dequeue();
        GameObject secondGameObject = selectedObjects.Dequeue();

        GameObjectManipulationLibrary.ChangeOutline(firstGameObject, GadgetSelection.unSelected);
        GameObjectManipulationLibrary.ChangeOutline(secondGameObject, GadgetSelection.unSelected);

        Texture go1Text = firstGameObject.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = secondGameObject.GetComponent<Renderer>().material.mainTexture;        

        Texture2D copyTexture = TextureManipulationLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = TextureManipulationLibrary.toTexture2D(go2Text);

        string uniqueName = GameManager.getInstance().ComfyOrganizer.UniqueImageName();
        copyTexture.name = uniqueName + ".png";
        secondCopyTexture.name = uniqueName + "_2" + ".png";

        DiffusionRequest diffusionRequest = CreateDiffusionRequest(diffusionTextureChangers);

        diffusionRequest.uploadTextures.Add(copyTexture);
        diffusionRequest.uploadTextures.Add(secondCopyTexture);

        // Adding words to the positive prompt according to the chosen objects' embedded text
        diffusionRequest.positivePrompt = "";
        if (firstGameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DiffObj))
        {
            diffusionRequest.positivePrompt += DiffObj.keyword;
        }
        if (secondGameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DiffObjSec))
        {
            diffusionRequest.positivePrompt += DiffObjSec.keyword;
        }

        ResetMechanism();

        GameManager.getInstance().ComfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(GameObject GO)
    {
        if (GameManager.getInstance() == null) return;
        if (GO == null) return;
        if (gadget == null) return;

        Texture2D curTexture = gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.Log("Tried to add a textures from the Gadget camera without textures in the Queue");
            return;
        }

        // Perform the raycast
        Ray ray = new Ray(GO.transform.position, GO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject has the DiffusionTextureChanger component
            // In this code, one DiffusionTextureChanger to another DiffusionTextureChanger
            if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
            {
                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);

                // Sending broadcast to Game timeline script
                MechanismExtraEvent?.Invoke();
            }
        }
    }

    public override void ResetMechanism()
    {
        foreach(GameObject GO in selectedObjects)
        {
            GameObjectManipulationLibrary.ChangeOutline(GO, GadgetSelection.unSelected);
        }

        selectedObjects = new Queue<GameObject>();
    }
}