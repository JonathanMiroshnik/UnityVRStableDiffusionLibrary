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
    public GameObject SelectedTextObject = null;

    public List<DiffusionTextureChanger> MechanismTextureChangers = new List<DiffusionTextureChanger>();
    public override string mechanismText => "Cube Generator";

    // There are 6 sides to a cube
    private const int NumSides = 6;

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

        if (!validInteractableObject(args)) return;
        if (SelectedTextObject == args.interactableObject.transform.gameObject) return;
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;
        if (SelectedTextObject == args.interactableObject.transform.gameObject) return;
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;

        if (!validInteractableObject(args)) return;

        GameObject curInteractable = args.interactableObject.transform.gameObject;
        DiffusableObject curDO = curInteractable.GetComponent<DiffusableObject>();

        if (curDO == null) return;
        SelectedTextObject = args.interactableObject.transform.gameObject;

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
        if (list.Count != NumSides) return null;

        const string TopTextureName = "top";
        const string BottomTextureName = "bottom";
        const string RightTextureName = "right";
        const string LeftTextureName = "left";
        const string FrontTextureName = "front";
        const string BackTextureName = "back";

        Dictionary<string, Texture2D> trans_dict = new Dictionary<string, Texture2D>();        

        foreach (Texture2D tex in list)
        {
            if (tex.name.Contains(TopTextureName))
            {
                trans_dict[TopTextureName] = tex;
            }
            else if (tex.name.Contains(BottomTextureName))
            {
                trans_dict[BottomTextureName] = tex;
            }
            else if (tex.name.Contains(RightTextureName))
            {
                trans_dict[RightTextureName] = tex;
            }
            else if (tex.name.Contains(LeftTextureName))
            {
                trans_dict[LeftTextureName] = tex;
            }
            else if (tex.name.Contains(FrontTextureName))
            {
                trans_dict[FrontTextureName] = tex;
            }
            else if (tex.name.Contains(BackTextureName))
            {
                trans_dict[BackTextureName] = tex;
            }
            else
            {
                Debug.LogError("Texture does not contain proper substring: " + tex.name);
                return null;
            }
        }

        if (trans_dict.Count != NumSides) return null;

        List<Texture2D> ret_textures = new List<Texture2D>();
        ret_textures.Add(trans_dict[TopTextureName]);
        ret_textures.Add(trans_dict[BottomTextureName]);
        ret_textures.Add(trans_dict[RightTextureName]);
        ret_textures.Add(trans_dict[LeftTextureName]);
        ret_textures.Add(trans_dict[FrontTextureName]);
        ret_textures.Add(trans_dict[BackTextureName]);

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

        if (MechanismTextureChangers.Count <= 0) return; // TODO: dont like this code what if first is not the correct texturechanger?

        if (MechanismTextureChangers[0] is not MultiTextureChanger) return;
        MultiTextureChanger MTC = (MultiTextureChanger)MechanismTextureChangers[0]; // TODO: hate downcasting see above

        var textures = MTC.GetTexturesAndReset(); 
        if (textures == null) return;

        // Instantiate the prefab at the target location
        GameObject prefabGO = Instantiate(sidesCubePrefab, targetLocation, Quaternion.identity);
        SidesCubeProperties SCP = prefabGO.GetComponent<SidesCubeProperties>(); // TODO: dont like how specific this is see above

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
        newDiffusionRequest.diffusionModel = diffusionModels.Ghostmix;

        foreach (DiffusionTextureChanger DTC in diffusionTextureChangers)
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        foreach (DiffusionTextureChanger DTC in MechanismTextureChangers) // TODO: added this need explanation?
        {
            newDiffusionRequest.targets.Add(DTC);
        }

        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.CubeObject;
        newDiffusionRequest.numOfVariations = NumSides;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;
        if (SelectedTextObject == null) return;

        string positivePrompt = SelectedTextObject.GetComponent<DiffusableObject>().keyword;

        DiffusionRequest diffusionRequest = CreateDiffusionRequest(diffusionTextureChangers);
        diffusionRequest.positivePrompt = positivePrompt;

        if (gadget != null)
        {
            gadget.playSounds.PlaySound("ImagePlacement");
        }

        ResetMechanism();
        GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(diffusionRequest);
    }


    public override void ResetMechanism()
    {
        if (SelectedTextObject != null)
        {
            // GameObjectManipulationLibrary.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
            SelectedTextObject = null;
        }
    }
}