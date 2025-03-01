using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static GeneralGameLibraries;

/// <summary>
/// Image Gadget Mechanism for creating an "Outpainting screen", 
/// a screen of tiles onwhich an initial image is extended by the addition of outpainted images on all sides of this image.
/// The new image is created by picking a DiffusableObject with an embedded text prompt, 
/// and the additional extended area is created accordingly.
/// </summary>
public class OutpaintGadgetMechanism : GadgetMechanism
{
    // Outpainting screen that is filled
    public OutpaintingScreenScr outpaintingScreen;

    public override string mechanismText => "Outpainting";

    // Diffusable Object that is being held up
    private GameObject _grabbedObject;

    // These are used to represent the tiles that are under progress
    private Texture2D[] NoiseTextures;

    private void Awake()
    {
        // TODO: check if this is needed
        NoiseTextures = Resources.LoadAll<Texture2D>("Textures/Noise");
    }

    /// <summary>
    /// Helper function for the Outpainting Mechanism script that checks whether a interactable object should be interacted with further.
    /// </summary>
    /// <param name="args">Interactable Object args to check</param>
    /// <returns>True if should be interacted with</returns>
    private bool ValidInteractableObject(BaseInteractionEventArgs args, bool grabbable)
    {
        if (args == null || args.interactableObject == null) return false;
        if (GameManager.getInstance() == null) return false;

        Transform curTrans = args.interactableObject.transform;

        // When you use a GrabInteractable, it moves the transform in the hierarchy, thus not being in the Diffusables anymore while being grabbed.
        if (!grabbable)
        {
            if (!GameManager.getInstance().DiffusionList.Contains(curTrans.gameObject)) return false;
        }
        if (curTrans.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (!DO.Model3D) return false;
        }

        return true;
    }


    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (_grabbedObject == null) return;
        if (!ValidInteractableObject(args, false)) return;

        if (args.interactableObject.transform.gameObject.TryGetComponent<OutpaintingTile>(out OutpaintingTile OPT))
        {
            if (!OPT.paintable || OPT.painted) return;

            // Creates pre-selection outline
            GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
        }  
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!ValidInteractableObject(args, false)) return;

        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }


    public override void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (!ValidInteractableObject(args, true)) return;

        if (args.interactableObject.transform.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (DO.Model3D)
            {
                _grabbedObject = args.interactableObject.transform.gameObject;
            }
        }        
    }

    public override void DiffusableUnGrabbed(SelectExitEventArgs args)
    { 
        if (!ValidInteractableObject(args, true)) return;
        if (args.interactableObject.transform.gameObject != _grabbedObject) return;
        _grabbedObject = null;
    }


    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Outpainting Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        //newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.diffusionModel = diffusionModels.JuggernautXLInpaint;
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Outpainting;
        newDiffusionRequest.addToTextureTotal = false;

        return newDiffusionRequest;
    }


    /// <summary>
    /// Helper function for the selection function. 
    /// Allows to determine if the adjacent tile that is given, could be used in the outpainting process as an input. 
    /// </summary>
    /// <param name="offset">Determines which tile is checked of the surrounding tiles</param>
    /// <param name="diffusionRequest">DiffusionRequest to change</param>
    /// <param name="OPT">Input tile for which we check its adjacents</param>
    /// <param name="offsetTileName">Adjacent tile name</param>
    /// <param name="mainTileName">Input tile name</param>
    /// <returns> true if the adjacent tile that is given, could be used in the outpainting process as an input, 
    /// false otherwise</returns>
    private bool CheckAdjacentTile(Vector2Int offset, DiffusionRequest diffusionRequest, OutpaintingTile OPT, string mainTileName, string offsetTileName)
    {
        GameObject curOffsetTile = outpaintingScreen.tiles[OPT.tilePosition.x + offset.x, OPT.tilePosition.y + offset.y];
        if (curOffsetTile == null) return false;
        OutpaintingTile curTileComp = curOffsetTile.GetComponent<OutpaintingTile>();
        if (curTileComp == null) return false;

        if (curTileComp.painted == true)
        {
            Texture2D curTexture = null;
            if (curOffsetTile.TryGetComponent<TextureTransition>(out TextureTransition TT))
            {
                if (TT.textures.Count <= 0)
                {
                    Debug.LogError("There is no texture in the painted tile " + curOffsetTile.name);
                    return false;
                }

                Texture curTextureToConvert = TT.textures[0];
                curTexture = GeneralGameLibraries.TextureManipulationLibrary.toTexture2D(curTextureToConvert);
            }
            else
            {
                curTexture = TextureManipulationLibrary.toTexture2D(curOffsetTile.GetComponent<Renderer>().material.mainTexture);
            }

            diffusionRequest.uploadTextures.Add(curTexture);
            curTexture.name = mainTileName + "_" + offsetTileName + ".png";
            diffusionRequest.SpecialInput = offsetTileName;

            return true;
        }

        return false;
    }

    
    /// <summary>
    /// Helper function that gets a TextureTransition and adds a "in-progress" effect with a few random textures
    /// </summary>
    private void CreateInProgressDiffusionTileEffect(TextureTransition TT)
    {
        List<Texture2D> listTextures = new List<Texture2D>(NoiseTextures);
        listTextures = GeneralGameLibraries.GetRandomElements(listTextures, 2);
        if (listTextures.Count <= 1)
        {
            Debug.LogError("Add more noise textures to the Resources/Textures/Noise folder");
        }
        TT.TransitionTextures(new List<Texture> { listTextures[0], listTextures[1] }, -1, 0, 1);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (_grabbedObject == null) return;
        if (!ValidInteractableObject(args, false)) return;

        string curPositivePrompt = "";
        if (_grabbedObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (!DO.Model3D) return;
            curPositivePrompt = DO.keyword;
        }

        DiffusionRequest newDiffusionRequest = CreateDiffusionRequest();
        newDiffusionRequest.positivePrompt = curPositivePrompt;

        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        TextureTransition TT = args.interactableObject.transform.gameObject.GetComponent<TextureTransition>();

        if (OPT == null || TT == null) return;

        // Object that is interacted with is an OutpaintingTile
        if (!(OPT.paintable && !OPT.painted)) return;

        // Finding a texture to be the original to be outpainted from.
        bool topTileOutpaint = false;
        bool leftTileOutpaint = false;
        bool rightTileOutpaint = false;

        string uniqueName = GameManager.getInstance().ComfyOrgan.UniqueImageName();

        // Top tile outpainting
        if (OPT.tilePosition.y < outpaintingScreen.tileMatrixSize.y - 1)
        {
            topTileOutpaint = CheckAdjacentTile(new Vector2Int(0, 1), newDiffusionRequest, OPT, uniqueName, "top");
        }

        // Left tile outpainting
        if (OPT.tilePosition.x > 0)
        {
            leftTileOutpaint = CheckAdjacentTile(new Vector2Int(-1, 0), newDiffusionRequest, OPT, uniqueName, "left");
        }

        // Right tile outpainting
        if (OPT.tilePosition.x < outpaintingScreen.tileMatrixSize.x - 1)
        {
            rightTileOutpaint = CheckAdjacentTile(new Vector2Int(1, 0), newDiffusionRequest, OPT, uniqueName, "right");
        }                      

        if (!topTileOutpaint && !leftTileOutpaint && !rightTileOutpaint)    return;

        // For diagonal image generation
        if (topTileOutpaint && leftTileOutpaint)
        {
            if (!CheckAdjacentTile(new Vector2Int(-1, 1), newDiffusionRequest, OPT, uniqueName, "bottomRight")) return;
            newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Grid4Outpainting;
        }
        else if (topTileOutpaint && rightTileOutpaint)
        {
            if (!CheckAdjacentTile(new Vector2Int(1, 1), newDiffusionRequest, OPT, uniqueName, "bottomLeft")) return;
            newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Grid4Outpainting;
        }

        CreateInProgressDiffusionTileEffect(TT);
        newDiffusionRequest.targets.Add(TT);        

        // Makes the picked up DiffusableObject to fly towards the tile that will change to the newly created extension image
        ObjectFlightToTile curFlight = _grabbedObject.AddComponent<ObjectFlightToTile>();
        curFlight.StartMovement(_grabbedObject.transform.position, args.interactableObject.transform.position);

        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);

        // Stops the picked up DiffusableObject from being a grabbable object,
        //  as it stops being relevant after it was chosen for image creation
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable xRGrabInteractable = _grabbedObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (xRGrabInteractable != null) xRGrabInteractable.enabled = false;
        _grabbedObject = null;

        if (gadget != null)
        {
            gadget.playSounds.PlaySound("SelectElement");
        }

        GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(newDiffusionRequest);
    }
}