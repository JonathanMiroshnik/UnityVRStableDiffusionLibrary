using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static GeneralGameLibraries;
using UnityEngine.Events;

/// <summary>
/// Gadget Mechanism wherein a Camera is used to screenshot within the game world,
/// And that screenshot is used as an input in an img2img Diffusion workflow.
/// </summary>
public class CameraGadgetMechanism : GadgetMechanism
{ 
    // Texture chosen to be used as the style component in the combination with the content texture
    private Texture2D styleTexture;

    // Screenshot that was taken with the Camera
    private Texture2D contentTexture;

    // Limits when a picture can be taken    
    public bool takePicture = false;

    public UnityEvent TakeScreenshotUnityEvent;
    public UnityEvent ActivateGenerationUnityEvent;

    // Object that was selected for the texture to be used as the style base for the Camera's image.
    private GameObject selectedStyleObject = null;


    private void Awake()
    {
        mechanismText = "Base\nCamera";
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Places a texture on the object that was selected with the controllers' Trigger button. 
    /// The Object is selected with a ray that comes from the left hand.
    /// </summary>
    public override void PlaceTextureInput(GameObject GO)
    {
        if (GameManager.getInstance() == null) return;

        if (takePicture) return;
        if (GO == null) return;

        Texture2D curTexture = gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.LogError("Tried to add a textures from the Gadget camera without textures in the Queue");
            return;
        }

        // Perform the raycast
        // Ray ray = new Ray(GameManager.getInstance().gadget.transform.position, GameManager.getInstance().gadget.transform.right);
        Ray ray = new Ray(GO.transform.position, GO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject has the DiffusionTextureChanger component
            // In this code, one DiffusionTextureChanger to another DiffusionTextureChanger
            if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
            {
                gadget.playSounds.PlaySound("ImagePlacement");

                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);
            }
        }
    }

    /// <summary>
    /// Helper function used to validate further interaction with an object in accordance to the Camera Mechanism
    /// </summary>
    /// <param name="args">Interactable Object to be evaluated</param>
    /// <returns>True is further interaction is validated</returns>
    private bool ValidInteractableObject(BaseInteractionEventArgs args)
    {
        if (GameManager.getInstance() == null) return false;
        if (args == null || args.interactableObject == null) return false;

        Transform curTrans = args.interactableObject.transform;
        if (!GameManager.getInstance().diffusionList.Contains(curTrans.gameObject)) return false;
        if (curTrans.gameObject == selectedStyleObject) return false;
        if (!curTrans.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO)) return false;
        if (curTrans.gameObject.TryGetComponent<Renderer>(out Renderer REN))
        {
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

        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }
    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Removing previously selected object
        if (selectedStyleObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        // Creates selection outline
        GameObjectManipulationLibrary.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
        selectedStyleObject = args.interactableObject.transform.gameObject;
        Texture2D curTexture = TextureManipulationLibrary.toTexture2D(args.interactableObject.transform.gameObject.GetComponent<Renderer>().material.mainTexture);

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        curTexture.name = uniqueName + "_2.png";

        styleTexture = curTexture;

        if (gadget != null) {
            gadget.playSounds.PlaySound("SelectElement");
        }        
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Camera Mechanism
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

        //newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.CombineImages;

        return newDiffusionRequest;
    }


    /// <summary>
    /// Activates the Diffusion image generation using the right hand controller. 
    /// For the generation to begin, a camera shot and a style texture need to be picked to be sent to the generator.
    /// </summary>
    public override void ActivateGeneration(GameObject GO, List<DiffusionTextureChanger> diffusionTextureChangers)
    {
        if (GameManager.getInstance() == null) return;

        if (contentTexture == null || styleTexture == null)
        {
            Debug.LogError("Need to pick style and content textures for Camera mechanism");
            return;
        }

        if (selectedStyleObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }


        // Content texture is the first one in the uploadTextures List, Style texture is the second.
        DiffusionRequest newDiffusionRequest = CreateDiffusionRequest(diffusionTextureChangers);
        newDiffusionRequest.uploadTextures.Add(contentTexture);
        newDiffusionRequest.uploadTextures.Add(styleTexture);

        // Invoking voiceline
        ActivateGenerationUnityEvent?.Invoke();
        if (gadget != null) {
            gadget.playSounds.PlaySound("ImagePlacement");
        }

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(newDiffusionRequest);
        return;
    }

    // This function is called when the grabbed camera is activated(a screenshot is taken).
    /// <summary>
    /// Shoots an image with the Physical grabbable Camera.
    /// </summary>
    public override void TakeScreenshot(Texture2D screenShot, Camera camera)
    {
        if (GameManager.getInstance() == null) return;
        
        // NOTICE: we do not add prompts or additional data other than the chosen object(definitely not what we get from the screenshot process)
        camera.enabled = false;
        if (gadget != null)
        {
            gadget.xrCamera.enabled = true;
        }

        contentTexture = screenShot;

        // Invoking voiceline
        TakeScreenshotUnityEvent?.Invoke();

        if (selectedStyleObject != null)
        {
            GameObjectManipulationLibrary.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }
    }

    public void changeTakePicture(bool value)
    {
       takePicture = value;
    }
}
