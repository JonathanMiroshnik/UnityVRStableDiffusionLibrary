using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static GeneralGameLibraries;
using System;


/// <summary>
/// Image Gadget Mechanism for taking the prompt of a DiffussableObject and 
/// combining it with the style of a chosen texture of another DiffusableObject.
/// </summary>
public class DiffusableObjectGadgetMechanism : GadgetMechanism
{
    [NonSerialized]
    public GameObject selectedStyleObject = null;
    [NonSerialized]
    public GameObject selectedTextObject = null;

    public override string mechanismText => "Object\nto\nImage";

    private bool validInteractableObject(BaseInteractionEventArgs args)
    {
        if (GameManager.getInstance() == null) return false;
        if (args == null || args.interactableObject == null) return false;

        Transform curTransform = args.interactableObject.transform;

        if (!GameManager.getInstance().DiffusionList.Contains(curTransform.gameObject)) return false;
        if (curTransform.GetComponent<DiffusableObject>() == null &&
            curTransform.GetComponent<Renderer>().material.mainTexture == null) return false;

        return true;
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (!validInteractableObject(args))
        {
            Debug.Log(args.interactableObject.transform.gameObject.name);
            return;
        }
        if (selectedStyleObject == args.interactableObject.transform.gameObject || 
            selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Creates pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;
        if (selectedStyleObject == args.interactableObject.transform.gameObject || 
            selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Remove pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (!validInteractableObject(args)) return;

        GameObject curInteractable = args.interactableObject.transform.gameObject;
        DiffusableObject curDO = curInteractable.GetComponent<DiffusableObject>();

        // The given text object is a 3D model, while the style object is not.
        if (curDO != null)
        {
            if (curDO.Model3D)
            {
                if (selectedTextObject != null)
                {
                    GameObjectManipulationLibrary.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
                }
                selectedTextObject = args.interactableObject.transform.gameObject;
            }
            else
            {
                if (selectedStyleObject != null)
                {
                    GameObjectManipulationLibrary.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
                }
                selectedStyleObject = args.interactableObject.transform.gameObject;
            }
        }
        else 
        {
            if (curInteractable.GetComponent<Renderer>().material.mainTexture == null) return;
            selectedStyleObject = args.interactableObject.transform.gameObject;
        }

        if (gadget != null)
        {
            gadget.playSounds.PlaySound("SelectElement");
        }

        // Creates selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }    

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(GameObject GO)
    {
        if (GameManager.getInstance() == null) return;
        if (GO == null) return;
        if (gadget == null) return;

        // Perform the raycast
        Ray ray = new Ray(GO.transform.position, GO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject has the DiffusionTextureChanger component
            // In this code, one DiffusionTextureChanger to another DiffusionTextureChanger
            if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
            {
                Texture2D curTexture = gadget.getGeneratedTexture();
                if (curTexture == null)
                {
                    Debug.Log("Tried to add a textures from the Gadget camera without textures in the Queue");
                    return;
                }

                gadget.playSounds.PlaySound("ImagePlacement");
                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);
            }
        }
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Diffusable Object Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        //newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.diffusionModel = diffusionModels.JuggernautReborn;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }
        newDiffusionRequest.targets.Add(gadget);

        //newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Img2ImgLCM;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;
        if (selectedTextObject == null || selectedStyleObject == null)  return;
        if (gadget != null) return;

        // Extracting the texture of the chosen DiffusableObject and the prompt from the chosed 3D model - DiffusableObject

        Texture styleTexture = selectedStyleObject.GetComponent<Renderer>().material.mainTexture;
        string positivePrompt = selectedTextObject.GetComponent<DiffusableObject>().keyword;

        Texture2D copyStyleTexture = TextureManipulationLibrary.toTexture2D(styleTexture);

        string uniqueName = GameManager.getInstance().ComfyOrgan.UniqueImageName();
        copyStyleTexture.name = uniqueName + ".png";

        DiffusionRequest diffusionRequest = CreateDiffusionRequest(diffusionTextureChangers);

        diffusionRequest.uploadTextures.Add(copyStyleTexture);
        diffusionRequest.positivePrompt = positivePrompt;

        gadget.playSounds.PlaySound("ImagePlacement");

        ResetMechanism();

        GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(diffusionRequest);
    }

    public override void ResetMechanism()
    {
        if (selectedTextObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
            selectedTextObject = null;
        }

        if (selectedStyleObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }
    }
}