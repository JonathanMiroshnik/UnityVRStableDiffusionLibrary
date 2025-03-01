using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}

public class FileExistsChecker
{
    public bool fileExists = false;
}

public enum diffusionWorkflows
{
    Txt2Img,
    Txt2ImgLCM,
    Img2Img,
    Img2ImgLCM,
    CombineImages,

    // Assuming these workflows will not use low powered models and thus no need for LCM
    BaseCamera,
    DepthCamera,
    OpenPose,

    // A special two-type mechanism
    Outpainting,
    Grid4Outpainting,

    // Gadget AI representation
    AIAssistant,

    // Used specifically for creating 2D-3D objects with a partly transparent image
    FlatObject,

    // Used to create a cube where each side is different with a logical texture
    CubeObject,

    // Workflow used to load model into memory for as little cost as possible
    Empty
}

public enum diffusionModels
{
    Nano,
    Mini,
    Turbo,
    Turblxl,
    Ghostmix,
    ThinkdiffusionTest,
    JuggernautXLInpaint,
    JuggernautReborn
}


[System.Serializable]
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
