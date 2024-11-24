using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using System.Net;
using UnityEngine.Rendering;
using UnityEngine.Events;


// TODO maybe remove requestNum from this class?

/// <summary>
/// Describes a single Diffusion Image generation Request which goes throughout the system, picking up the relevant
/// information for the Request, sends it to the generation, and sends the images to the proper targets, to be finished.
/// </summary>
[Serializable]
public class DiffusionRequest
{
    // Where the images that are generated are sent.
    public List<DiffusionTextureChanger> targets;

    // true if we add the images that generated to the previous image rotation of a target.
    public bool addToTextureTotal = false;

    // Number of image variations for one generation(same prompt, model, etc.)
    public int numOfVariations = 1;

    // Positive and negative text prompts
    public string positivePrompt;
    public string negativePrompt;

    // We upload up-to two images to the server
    public List<Texture2D> uploadTextures;

    // Denoising parameter for the input image
    public float denoise = 1.0f;

    // Checkpoint model to be used in the generation
    public diffusionModels diffusionModel;

    // Workflow type to be used in the generation.
    public diffusionWorkflows diffusionJsonType;

    // Used for sending any type of information that might be unique to a Request
    public string SpecialInput;

    // Textures that were generated for the request
    [System.NonSerialized]
    public List<Texture2D> textures;

    // True when the request finished, the images were generated, downloaded and sent onwards
    [System.NonSerialized]
    public bool finishedRequest = false;

    // True when the Image download requests have been sent
    [System.NonSerialized]
    public bool sentDownloadRequest = false;

    // Each request in the Game has a unique request number which differentiates them
    [System.NonSerialized]
    public int requestNum = -1;

    [System.NonSerialized]
    public string diffImgName;
    [System.NonSerialized]
    public string prompt_id;
    [System.NonSerialized]
    public Collision collision = null;
    [System.NonSerialized]
    public DiffusableObject diffusableObject = null;

    // Used to check whether the input Images have been uploaded successfully
    [System.NonSerialized]
    public FileExistsChecker uploadFileChecker;

    public DiffusionRequest()
    {
        targets = new List<DiffusionTextureChanger>();
        textures = new List<Texture2D>();
        uploadTextures = new List<Texture2D>();
        uploadFileChecker = new FileExistsChecker();
    }

    public DiffusionRequest(List<DiffusionTextureChanger> curTargets)
    {
        targets = curTargets;
        textures = new List<Texture2D>();
        uploadTextures = new List<Texture2D>();
        uploadFileChecker = new FileExistsChecker();
    }
}

public class ComfyOrganizer : MonoBehaviour
{
    // Holds all the DiffusionRequests that have been made in the duration of the Game
    public Dictionary<int, DiffusionRequest> DiffuseDictionary;

    public ComfySceneLibrary comfyLib;

    // Counter for the DiffusionRequests
    private static int currentRequestNum = 0;

    // TODO in ComfySceneLibrary I added HashSet of incoming image names,
    // TODO but this is used for outgoing/input image names, should be connected into one DB?
    // TODO it doesn't even seem that I'm really using this List for anything
    private List<string> allTextureNames;

    // An updating list of all textures that have been generated throughout the Game
    public List<Texture2D> allTextures;

    // Counter for Images to differentiate them
    private static int currentTextureNameNumber = 0;

    // When comes to 0, invokes a UnityEvent
    public int EndSceneTextureNum = 0;
    public UnityEvent EndSceneUnityEvent;

    private void Awake()
    {
        DiffuseDictionary = new Dictionary<int, DiffusionRequest>();
        allTextureNames = new List<string>();
    }

    /// <summary>
    /// Returns a unique name for an Image and adds it to the total list of texture names
    /// </summary>
    public string UniqueImageName()
    {
        string newTextureName = "DiffImage_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + '_' + currentTextureNameNumber.ToString();
        allTextureNames.Add(newTextureName);

        currentTextureNameNumber++;

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
            case diffusionWorkflows.combineImages:
                if (emptyTextures.Length == 0) Debug.LogError("Add an empty default texture");
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);

                //newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
                newDiffusionRequest.diffusionModel = diffusionModels.juggernautReborn;
                newDiffusionRequest.numOfVariations = 1;
                break;
            case diffusionWorkflows.outpainting:
                if (emptyTextures.Length == 0) Debug.LogError("Add an empty default texture");
                newDiffusionRequest.uploadTextures.Add(defEmptyTexture);

                newDiffusionRequest.diffusionModel = diffusionModels.juggernautXLInpaint;
                newDiffusionRequest.SpecialInput = "right";
                newDiffusionRequest.numOfVariations = 1;
                break;
            case diffusionWorkflows.txt2imgLCM:
                newDiffusionRequest.diffusionModel = diffusionModels.nano;
                newDiffusionRequest.numOfVariations = 1;
                break;
        }              


        SendDiffusionRequest(newDiffusionRequest);
    }


    // TODO problem with empty, sometimes not everything is loaded, maybe better to not have special workflow, but just have an empty version of each workflow? see above

    /// <summary>
    /// Used to send an empty DiffusionRequest to the server to load a model preemptively to the RAM.
    /// </summary>
    /// <param name="diffusionModel">Model to load to RAM</param>
    public void SendEmptyDiffusionRequest(diffusionModels diffusionModel)
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.empty;
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
        newDiffusionRequest.requestNum = currentRequestNum;
        newDiffusionRequest.diffImgName = "Generated_" + newDiffusionRequest.requestNum;
        DiffuseDictionary.Add(currentRequestNum, newDiffusionRequest);
        currentRequestNum++;


        int PROMPT_TRIAL_NUM = 5;
        StartCoroutine(comfyLib.QueuePromptCoroutine(newDiffusionRequest, PROMPT_TRIAL_NUM));
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
        // TODO should I even remove previous ones?
        /*foreach (var key in sentKeys)
        {
            DiffuseDictionary.Remove(key.requestNum);
        }*/

        return relevantKeys;
    }

    /// <summary>
    /// Adds a texture that was generated for a Diffusion Request to that Diffusion Request.
    /// </summary>
    /// <param name="texture">Texture that was created according to and added to the Diffusion Request</param>
    /// <param name="diffusionRequest">Current Diffusion Request forwhich the texture was created</param>
    public void AddImage(Texture2D texture, DiffusionRequest diffusionRequest)
    {
        if (texture == null || diffusionRequest == null)
        {
            return;
        }
        
        int requestNum = diffusionRequest.requestNum;
        if (!DiffuseDictionary.ContainsKey(requestNum))
        {
            return;
        }
        if (DiffuseDictionary[requestNum].finishedRequest)
        {
            return;
        }

        if (DiffuseDictionary[requestNum].numOfVariations > DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].textures.Add(texture);
            
            // Not efficient to hold a large List like this, but used for explosion in the end of the Game
            allTextures.Add(texture);

            EndSceneTextureNum--;
        }
        if (DiffuseDictionary[requestNum].numOfVariations <= DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].finishedRequest = true;
            DiffuseDictionary[requestNum].sentDownloadRequest = true;
            SendTexturesToRecipient(DiffuseDictionary[requestNum]);
        }

        // When the needed number of textures in a scene has been generated, invokes the given EndSceneUnityEvent
        if (EndSceneTextureNum <= 0)
        {
            EndSceneUnityEvent?.Invoke();
        }
    }

    /// <summary>
    /// Sends the textures of a Diffusion Request to its targets.
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request to send its textures to its targets</param>
    private void SendTexturesToRecipient(DiffusionRequest diffusionRequest)
    {
        if (!diffusionRequest.finishedRequest || diffusionRequest.targets == null)
        {
            Debug.LogError("Add target to send textures to");
            return;
        }

        if (diffusionRequest.targets.Count == 0)
        {
            return;
        }
        foreach(DiffusionTextureChanger changer in diffusionRequest.targets)
        {
            changer.AddTexture(diffusionRequest);
        }

        diffusionRequest.targets.Clear();
    }
}
