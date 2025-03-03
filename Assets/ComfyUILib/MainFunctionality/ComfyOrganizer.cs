using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;

/// <summary>
/// This class is used to keep track of all the DiffusionRequests that have been made in the duration of the Game.
/// </summary>
public class ComfyOrganizer : System.Object
{
    // Holds all the DiffusionRequests that have been made in the duration of the Game
    // We do not remove any DiffusionRequests from this dictionary, 
    //because we want to keep track of all the textures that have been generated.
    public Dictionary<int, DiffusionRequest> DiffuseDictionary;

    // When comes to 0, invokes a UnityEvent
    public int EndSceneTextureNum = 0;

    // Event that is invoked when the number of textures in the EndSceneTextureNum comes to 0, 
    //indicating that the scene is finished and the textures can be sent to the recipient
    public UnityEvent EndSceneUnityEvent;

    // Counter for the DiffusionRequests
    private static int _currentRequestNum = 0;

    // TODO: in ComfySceneLibrary I added HashSet of incoming image names,
    // TODO: but this is used for outgoing/input image names, should be connected into one DB?
    // TODO: it doesn't even seem that I'm really using this List for anything
    private List<string> _allTextureNames;

    // Counter for Images to differentiate them
    private static int _currentTextureNameNumber = 0;

    public ComfyOrganizer()
    {
        DiffuseDictionary = new Dictionary<int, DiffusionRequest>();
        _allTextureNames = new List<string>();
    }

    /// <summary>
    /// Returns a unique name for an Image and adds it to the total list of texture names
    /// </summary>
    public string UniqueImageName()
    {
        string newTextureName = "DiffImage_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + '_' + _currentTextureNameNumber.ToString();
        _allTextureNames.Add(newTextureName);
        _currentTextureNameNumber++;

        return newTextureName;
    }

    /// <summary>
    /// Returns a deep copy of the given Diffusion Request
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request to deep copy</param>
    public DiffusionRequest copyDiffusionRequest(DiffusionRequest diffusionRequest)
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        foreach (var target in diffusionRequest.targets)
        {
            newDiffusionRequest.targets.Add(target);
        }

        newDiffusionRequest.addToTextureTotal = diffusionRequest.addToTextureTotal;
        newDiffusionRequest.numOfVariations = diffusionRequest.numOfVariations;
        newDiffusionRequest.positivePrompt = diffusionRequest.positivePrompt;
        newDiffusionRequest.negativePrompt = diffusionRequest.negativePrompt;

        newDiffusionRequest.denoise = diffusionRequest.denoise;
        newDiffusionRequest.requestNum = diffusionRequest.requestNum;

        newDiffusionRequest.diffusionModel = diffusionRequest.diffusionModel;
        newDiffusionRequest.diffusionJsonType = diffusionRequest.diffusionJsonType;

        newDiffusionRequest.collision = diffusionRequest.collision;
        newDiffusionRequest.diffusableObject = diffusionRequest.diffusableObject;

        // Texture2D deep copying --------------------------------------------------------------------
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            Texture2D copyTexture = GeneralGameLibraries.TextureManipulationLibrary.CopyTexture(texture);
            newDiffusionRequest.textures.Add(copyTexture);
        }
        
        foreach (Texture2D texture in diffusionRequest.uploadTextures)
        {
            Texture2D copyTexture = GeneralGameLibraries.TextureManipulationLibrary.CopyTexture(texture);
            newDiffusionRequest.uploadTextures.Add(copyTexture);
        }
        // Texture2D deep copying --------------------------------------------------------------------

        newDiffusionRequest.finishedRequest = diffusionRequest.finishedRequest;
        newDiffusionRequest.diffImgName = diffusionRequest.diffImgName;
        newDiffusionRequest.prompt_id = diffusionRequest.prompt_id;

        newDiffusionRequest.SpecialInput = diffusionRequest.SpecialInput;

        return newDiffusionRequest;
    }


    /// <summary>
    /// Used to send a Diffusion Request that does not have any targets.
    /// Used primarily for testing purposes.
    /// </summary>
    /// <param name="diffusionWorkflow">Workflow of the Diffusion Request</param>
    public void SendNonTargetWorkflowDiffusionRequest(string diffusionWorkflow)
    {
        diffusionWorkflows enumVal;
        if (Enum.IsDefined(typeof(diffusionWorkflows), diffusionWorkflow))
        {
            Enum.TryParse<diffusionWorkflows>(diffusionWorkflow, out enumVal);
        }
        else
        {
            Debug.LogError("Tried to send an empty Diffusion Request of false workflow " + diffusionWorkflow.ToString());
            return;
        }

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionJsonType = enumVal;

        newDiffusionRequest.positivePrompt = "";
        newDiffusionRequest.negativePrompt = "";

        Texture[] emptyTextures = Resources.LoadAll<Texture>("Textures/EmptyDiffuseTexture");
        Texture2D defEmptyTexture = GeneralGameLibraries.TextureManipulationLibrary.toTexture2D(emptyTextures[0]);

        switch (enumVal)
        {
            case diffusionWorkflows.CombineImages:
                if (emptyTextures.Length == 0) Debug.LogError("Add an empty default texture");
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);

                //newDiffusionRequest.diffusionModel = diffusionModels.Ghostmix;
                newDiffusionRequest.diffusionModel = diffusionModels.JuggernautReborn;
                newDiffusionRequest.numOfVariations = 1;
                break;
            case diffusionWorkflows.Outpainting:
                if (emptyTextures.Length == 0) Debug.LogError("Add an empty default texture");
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);

                newDiffusionRequest.diffusionModel = diffusionModels.JuggernautXLInpaint;
                newDiffusionRequest.SpecialInput = "right";
                newDiffusionRequest.numOfVariations = 1;
                break;
            case diffusionWorkflows.Txt2ImgLCM:
                newDiffusionRequest.diffusionModel = diffusionModels.Nano;
                newDiffusionRequest.numOfVariations = 1;
                break;
        }              

        SendDiffusionRequest(newDiffusionRequest);
    }


    // TODO: problem with empty, sometimes not everything is loaded, maybe better to not have special workflow, but just have an empty version of each workflow? see above

    /// <summary>
    /// Used to send an empty DiffusionRequest to the server to load a model preemptively to the RAM.
    /// </summary>
    /// <param name="diffusionModel">Model to load to RAM</param>
    public void SendEmptyDiffusionRequest(diffusionModels diffusionModel)
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.Empty;
        newDiffusionRequest.diffusionModel = diffusionModel;
        
        SendDiffusionRequest(newDiffusionRequest);
    }

    /// <summary>
    /// Used to send an empty DiffusionRequest to the server to load a model preemptively to the RAM.
    /// </summary>
    /// <param name="modelName">Model to load to RAM</param>
    public void SendEmptyDiffusionRequest(string modelName)
    {
        diffusionModels enumVal;
        if (Enum.IsDefined(typeof(diffusionModels), modelName))
        {
            Enum.TryParse<diffusionModels>(modelName, out enumVal);
        }
        else
        {
            Debug.LogError("Tried to send an empty Diffusion Request of false model " + modelName.ToString());
            return;
        }
        
        SendEmptyDiffusionRequest(enumVal);
    }

    /// <summary>
    /// Sends an image generation request to the ComfySceneLibrary. The Images that are created are then added to the targets list of the diffusionRequest.
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request that is sent to the ComfySceneLibrary</param>
    public void SendDiffusionRequest(DiffusionRequest diffusionRequest)
    {
        DiffusionRequest newDiffusionRequest = copyDiffusionRequest(diffusionRequest);
        newDiffusionRequest.requestNum = _currentRequestNum;
        newDiffusionRequest.diffImgName = "Generated_" + newDiffusionRequest.requestNum;
        DiffuseDictionary.Add(_currentRequestNum, newDiffusionRequest);
        _currentRequestNum++;

        int promptTrials = 5;
        GameManager.getInstance().ComfySceneLib.QueuePrompt(newDiffusionRequest, promptTrials);
    }

    /// <summary>
    /// Gets a list of the Diffusion Requests that have still not been downloaded.
    /// </summary>
    public List<DiffusionRequest> GetUndownloadedRequestPrompts()
    {
        List<DiffusionRequest> relevantKeys = new List<DiffusionRequest>();
        List<DiffusionRequest> sentKeys = new List<DiffusionRequest>();

        foreach (var diffReqID in DiffuseDictionary)
        {
            if (!diffReqID.Value.sentDownloadRequest && !diffReqID.Value.finishedRequest)
            {
                relevantKeys.Add(diffReqID.Value);
            }
            else
            {
                sentKeys.Add(diffReqID.Value);
            }
        }

        return relevantKeys;
    }


    // TODO: can be partly broken up into a function in the DiffusionRequest class
    /// <summary>
    /// Adds a texture that was generated for a Diffusion Request to that Diffusion Request.
    /// </summary>
    /// <param name="texture">Texture that was created according to and added to the Diffusion Request</param>
    /// <param name="diffusionRequest">Current Diffusion Request forwhich the texture was created</param>
    public void AddImage(Texture2D texture, DiffusionRequest diffusionRequest)
    {
        if (texture == null || diffusionRequest == null)
        {
            Debug.Log("Texture or Diffusion Request is null");
            return;
        }
        
        int requestNum = diffusionRequest.requestNum;
        if (!DiffuseDictionary.ContainsKey(requestNum))
        {
            Debug.Log("Diffusion Request not found");
            return;
        }
        if (DiffuseDictionary[requestNum].finishedRequest)
        {
            Debug.Log("Diffusion Request finished");
            return;
        }

        if (DiffuseDictionary[requestNum].numOfVariations > DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].textures.Add(texture);
            EndSceneTextureNum--;
        }
        if (DiffuseDictionary[requestNum].numOfVariations <= DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].finishedRequest = true;
            DiffuseDictionary[requestNum].sentDownloadRequest = true;
            DiffuseDictionary[requestNum].SendTexturesToRecipient();
        }

        // When the needed number of textures in a scene has been generated, invokes the given EndSceneUnityEvent
        if (EndSceneTextureNum <= 0)
        {
            EndSceneUnityEvent?.Invoke();
        }
    }

    /// <summary>
    /// Gets a list of all the textures that have been generated for a Diffusion Request.
    /// </summary>
    public List<Texture2D> GetAllTextures()
    {
        List<Texture2D> allTextures = new List<Texture2D>();
        foreach (DiffusionRequest dr in DiffuseDictionary.Values)
        {
            foreach (Texture2D texture in dr.textures)
            {
                allTextures.Add(texture);
            }
        }

        return allTextures;
    }
}
