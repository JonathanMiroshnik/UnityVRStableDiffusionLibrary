using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static GeneralGameLibraries;
using System;

/// <summary>
/// Create a 2D image and place it into 3D space. The 2D image will have a transparent background.
/// To create an image, select a DiffusableObject to get its prompt text, and then activate the generation process.
/// After the image is created, place it anywhere, it will create a 2D image in 3D space.
/// You can pick up the 2D image, and when you hold it with two hands, it allows modifying its size.
/// </summary>
public class FlatObjectGadgetMechanism : GadgetMechanism
{
    // DiffusableObject with embedded text prompt used for the generation of additional 2D texture represantations
    [NonSerialized]
    public GameObject selectedTextObject = null;

    public override string mechanismText => "Flat\nObject\nGenerator"; 

    private bool validInteractableObject(BaseInteractionEventArgs args)
    {
        if (GameManager.getInstance() == null) return false;
        if (args == null || args.interactableObject == null) return false;

        Transform curTransform = args.interactableObject.transform;

        if (!GameManager.getInstance().diffusionList.Contains(curTransform.gameObject)) return false;
        if (curTransform.GetComponent<DiffusableObject>() == null &&
            curTransform.GetComponent<Renderer>().material.mainTexture == null) return false;

        return true;
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;

        if (!validInteractableObject(args)) return;
        if (selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Creates pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;
        if (selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Remove pre-selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;        
        if (!validInteractableObject(args)) return;

        GameObject curInteractable = args.interactableObject.transform.gameObject;
        DiffusableObject curDO = curInteractable.GetComponent<DiffusableObject>();

        if (curDO == null) return;
        selectedTextObject = args.interactableObject.transform.gameObject;

        if (gadget != null)
        {
            gadget.playSounds.PlaySound("SelectElement");
        }

        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    public GameObject flatObjectPrefab; // The prefab to instantiate
    public float maxRayDistance = 10f; // Maximum distance if no hit occurs

    public override void PlaceTextureInput(GameObject GO)
    {
        // Shoot a ray from the GameObject's position in its forward direction
        Ray ray = new Ray(GO.transform.position, GO.transform.forward);
        RaycastHit hit;

        Vector3 targetLocation;

        // Check if the ray hits anything
        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            // If hit, use the hit point as the target location
            targetLocation = hit.point;
        }
        else
        {
            // If no hit, set the target location at maxRayDistance
            targetLocation = ray.GetPoint(maxRayDistance);
        }

        // Instantiate the prefab at the target location
        GameObject prefabGO = Instantiate(flatObjectPrefab, targetLocation, Quaternion.identity);
        var FOTC = prefabGO.GetComponent<FlatObjectTextureChanger>();
        if (FOTC != null)
        {
            var textures = gadget.uiDiffusionTexture.GetTextures();
            if (textures == null) return;

            var texture = textures[0];
            if (texture != null)
            {
                List<Texture2D> cur_list = new List<Texture2D>();
                cur_list.Add(texture); // TODO: bad code IMO

                FOTC.AddTexture(cur_list);
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
        newDiffusionRequest.diffusionModel = diffusionModels.Nano;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.FlatObject;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;
        if (selectedTextObject == null)  return;

        string positivePrompt = selectedTextObject.GetComponent<DiffusableObject>().keyword;

        DiffusionRequest diffusionRequest = CreateDiffusionRequest(diffusionTextureChangers);
        diffusionRequest.positivePrompt = positivePrompt;

        if (gadget != null)
        {
            gadget.playSounds.PlaySound("ImagePlacement");
        }

        ResetMechanism();
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }


    public override void ResetMechanism()
    {
        if (selectedTextObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
            selectedTextObject = null;
        }
    }
}