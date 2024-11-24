using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

/// <summary>
/// Gadget Mechanism wherein a Camera is used to create 3D cubes where all the texture of the cube extend one another properly.
/// </summary>
public class CubeGadgetMechanism : GadgetMechanism
{
    // DiffusableObject with embedded text prompt used for the generation of 3D cubes
    [NonSerialized]
    public GameObject selectedTextObject = null;

    public List<DiffusionTextureChanger> mechanismTextureChangers = new List<DiffusionTextureChanger>();

    // There are 6 sides to a cube
    const int NUM_SIDES = 6;

    private void Awake()
    {
        mechanismText = "Cube Generator";
    }

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
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;
        if (selectedTextObject == args.interactableObject.transform.gameObject) return;
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
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    public GameObject sidesCubePrefab; // The prefab to instantiate
    public float maxRayDistance = 10f; // Maximum distance if no hit occurs

    // Helper function that takes a list of textures with certain expected names and reorders them in the order:
    // Top, Bottom, Right, Left, Front, Back
    private List<Texture2D> ReorderTextures(List<Texture2D> list)
    {
        if (list == null) return null;
        if (list.Count != NUM_SIDES) return null;

        const string TOP_TEXTURE_NAME = "top";
        const string BOTTOM_TEXTURE_NAME = "bottom";
        const string RIGHT_TEXTURE_NAME = "right";
        const string LEFT_TEXTURE_NAME = "left";
        const string FRONT_TEXTURE_NAME = "front";
        const string BACK_TEXTURE_NAME = "back";

        Dictionary<string, Texture2D> trans_dict = new Dictionary<string, Texture2D>();        

        foreach (Texture2D tex in list)
        {
            if (tex.name.Contains(TOP_TEXTURE_NAME))
            {
                trans_dict[TOP_TEXTURE_NAME] = tex;
            }
            else if (tex.name.Contains(BOTTOM_TEXTURE_NAME))
            {
                trans_dict[BOTTOM_TEXTURE_NAME] = tex;
            }
            else if (tex.name.Contains(RIGHT_TEXTURE_NAME))
            {
                trans_dict[RIGHT_TEXTURE_NAME] = tex;
            }
            else if (tex.name.Contains(LEFT_TEXTURE_NAME))
            {
                trans_dict[LEFT_TEXTURE_NAME] = tex;
            }
            else if (tex.name.Contains(FRONT_TEXTURE_NAME))
            {
                trans_dict[FRONT_TEXTURE_NAME] = tex;
            }
            else if (tex.name.Contains(BACK_TEXTURE_NAME))
            {
                trans_dict[BACK_TEXTURE_NAME] = tex;
            }
            else
            {
                Debug.LogError("Texture does not contain proper substring: " + tex.name);
                return null;
            }
        }

        if (trans_dict.Count != NUM_SIDES) return null;

        List<Texture2D> ret_textures = new List<Texture2D>();
        ret_textures.Add(trans_dict[TOP_TEXTURE_NAME]);
        ret_textures.Add(trans_dict[BOTTOM_TEXTURE_NAME]);
        ret_textures.Add(trans_dict[RIGHT_TEXTURE_NAME]);
        ret_textures.Add(trans_dict[LEFT_TEXTURE_NAME]);
        ret_textures.Add(trans_dict[FRONT_TEXTURE_NAME]);
        ret_textures.Add(trans_dict[BACK_TEXTURE_NAME]);

        return ret_textures;
    }

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

        if (mechanismTextureChangers.Count <= 0) return; // TODO dont like this code what if first is not the correct texturechanger?

        if (mechanismTextureChangers[0] is not MultiTextureChanger) return;
        MultiTextureChanger MTC = (MultiTextureChanger)mechanismTextureChangers[0]; // TODO hate downcasting see above

        var textures = MTC.GetTexturesAndReset(); 
        if (textures == null) return;

        // Instantiate the prefab at the target location
        GameObject prefabGO = Instantiate(sidesCubePrefab, targetLocation, Quaternion.identity);
        SidesCubeProperties SCP = prefabGO.GetComponent<SidesCubeProperties>(); // TODO dont like how specific this is see above

        var sent_textures = ReorderTextures(textures);
        if (sent_textures == null) return;

        SCP.ChangeTextures(sent_textures);
    }    

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Diffusable Object Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest(List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        foreach (DiffusionTextureChanger DTC in mechanismTextureChangers) // TODO added this need explanation?
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.CubeObject;
        newDiffusionRequest.numOfVariations = NUM_SIDES;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;
        if (selectedTextObject == null) return;

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
            // GameObjectManipulationLibrary.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
            selectedTextObject = null;
        }
    }
}