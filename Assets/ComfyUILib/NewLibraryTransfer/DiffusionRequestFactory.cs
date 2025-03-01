using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
// using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System;

public class DiffusionRequestFactory : System.Object
{
    private Dictionary<diffusionWorkflows, string> _diffusionJsons;
    private const string JsonFolderPath = "JSONMain";

    void Awake()
    {
        _diffusionJsons = new Dictionary<diffusionWorkflows, string>();
    }

    // TODO: notice that this START must always come BEFORE(put the library before the organizer in the node properties)
    // TODO: cont. the ComfyOrganizer or else some things will not be ready for an instant diffusion request
    public void LoadFactory()
    {
        // Get all enum adjacent JSON workflows
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(JsonFolderPath);

        foreach (var file in jsonFiles)
        {
            string fileName = file.name;
            string fileContent = file.text;

            if (Enum.IsDefined(typeof(diffusionWorkflows), fileName))
            {
                diffusionWorkflows enumVal;
                Enum.TryParse<diffusionWorkflows>(fileName, out enumVal);
                _diffusionJsons.Add(enumVal, fileContent);
            }
            else
            {
                Debug.LogError($"Please add JSON workflow {fileName.ToString()} to the diffusionJsons enum");
            }
        }
    }

    /// <summary>
    /// Gets the JSON workflow corresponding to the Diffusion Workflow
    /// </summary>
    /// <param name="enumValName">Diffusion Workflow to get JSON of</param>
    private string getWorkflowJSON(diffusionWorkflows enumValName)
    {
        string ret_str = "";
        if (_diffusionJsons.ContainsKey(enumValName))
        {
            ret_str = _diffusionJsons[enumValName];
        }

        return ret_str;
    }

    // TODO: move UploadImage to the ComfySceneLibrary entirely, notice it is the SAME sentence everywhere.
    // TODO: remove ComfySceneLibrary from the parameters of this function.

    /// <summary>
    /// Returns an appropriate JSON text in accordance to the given DiffusionRequest. Factory design pattern.
    /// </summary>
    /// <param name="diffReq">given DiffusionRequest to create the JSON text from.</param>
    public async Task<string> DiffusionJSONFactory(DiffusionRequest diffReq, ComfySceneLibrary library)
    {
        string curDiffModel = "";
        Vector2Int curImageSize = Vector2Int.zero;
        switch (diffReq.diffusionModel)
        {
            case diffusionModels.Nano:
                curDiffModel = "stable-diffusion-nano-2-1.ckpt";
                curImageSize = new Vector2Int(128, 128);
                break;
            case diffusionModels.Mini:
                curDiffModel = "miniSD.ckpt";
                curImageSize = new Vector2Int(256, 256);
                break;
            case diffusionModels.Turbo:
                curDiffModel = "sdTurbo_v10.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
            case diffusionModels.Turblxl:
                // TODO: add this model
                curDiffModel = "??";
                curImageSize = new Vector2Int(1024, 1024);
                break;
            case diffusionModels.Ghostmix:
                curDiffModel = "ghostmix_v20Bakedvae.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
            case diffusionModels.ThinkdiffusionTest:
                curDiffModel = "01_ThinkDiffusionXL.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
            case diffusionModels.JuggernautXLInpaint:
                curDiffModel = "juggernautXL_versionXInpaint.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
            case diffusionModels.JuggernautReborn:
                curDiffModel = "juggernaut_reborn.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;                
            default:
                Debug.LogError("Pick a valid model from the list");
                break;
        }
        if (curDiffModel == null || curDiffModel == "" || curImageSize == Vector2Int.zero)
        {
            Debug.LogError("You must choose a useable Diffusion model");
            return null;
        }

        string guid = Guid.NewGuid().ToString();
        string promptText = $@"
        {{
            ""id"": ""{guid}"",
            ""prompt"": {getWorkflowJSON(diffReq.diffusionJsonType)}
        }}";

        Debug.Log("Current Diffusion Request type: " + diffReq.diffusionJsonType.ToString());

        JObject json = JObject.Parse(promptText); // promptText

        string randomSeed = UnityEngine.Random.Range(1, 10000).ToString();
        //TODO: add all cases according to diffusionWorkflows ENUM
        switch (diffReq.diffusionJsonType)
        {
            case diffusionWorkflows.Empty:
                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                diffReq.uploadFileChecker.fileExists = true;
                diffReq.finishedRequest = true;
                diffReq.sentDownloadRequest = true;
                break;

            case diffusionWorkflows.Txt2ImgLCM:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;

                diffReq.uploadFileChecker.fileExists = true;                
                break;

            case diffusionWorkflows.Img2Img:
                await library.UploadImage(diffReq, curImageSize);

                json["prompt"]["10"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                Debug.Log(diffReq.uploadTextures[0].name);

                json["prompt"]["3"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["3"]["inputs"]["steps"] = 20;

                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                break;

            case diffusionWorkflows.Img2ImgLCM:
                if (diffReq.uploadTextures == null)
                {
                    Debug.LogError("Upload some existing textures");
                    return null;
                }
                if (diffReq.uploadTextures.Count <= 0)
                {
                    Debug.LogError("Upload enough textures for the workflow");
                    return null;
                }

                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["3"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;
                json["prompt"]["15"]["inputs"]["amount"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                
                await library.UploadImage(diffReq, curImageSize);

                json["prompt"]["11"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                break;

            case diffusionWorkflows.Txt2Img:
                // TODO: create this one
                /*json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;*/

                diffReq.uploadFileChecker.fileExists = true;
                break;

            case diffusionWorkflows.CombineImages:
                if (diffReq.uploadTextures == null)
                {
                    Debug.LogError("Upload some existing textures");
                    return null;
                }
                if (diffReq.uploadTextures.Count <= 1)
                {
                    Debug.LogError("Upload enough textures for the workflow");
                    return null;
                }

                json["prompt"]["1"]["inputs"]["ckpt_name"] = curDiffModel;
                json["prompt"]["2"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["3"]["inputs"]["text"] += diffReq.negativePrompt;

                json["prompt"]["50"]["inputs"]["amount"] = diffReq.numOfVariations;
                json["prompt"]["51"]["inputs"]["amount"] = diffReq.numOfVariations;

                await library.UploadImage(diffReq, curImageSize);

                // Input Image:
                json["prompt"]["12"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                // Style is extracted from this Image:
                json["prompt"]["41"]["inputs"]["image"] = diffReq.uploadTextures[1].name;

                json["prompt"]["21"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["21"]["inputs"]["steps"] = 10;
                break;

            case diffusionWorkflows.AIAssistant:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                
                curImageSize.x = 512;
                curImageSize.y = 768;
                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;

                diffReq.uploadFileChecker.fileExists = true;
                break;
            case diffusionWorkflows.FlatObject:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                diffReq.uploadFileChecker.fileExists = true;
                break;
            case diffusionWorkflows.CubeObject:
                json["prompt"]["212"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["291"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["316"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["350"]["inputs"]["seed"] = randomSeed;

                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                json["prompt"]["25"]["inputs"]["ckpt_name"] = curDiffModel;

                diffReq.uploadFileChecker.fileExists = true;
                break;
            case diffusionWorkflows.Grid4Outpainting:
            case diffusionWorkflows.Outpainting:       
                switch (diffReq.SpecialInput)
                {
                    // Regular cases.
                    case "left":
                        await library.UploadImage(diffReq, curImageSize);

                        // Inputing the correct upload image name
                        json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                        // Outpainting increase size of original upload image
                        json["prompt"]["11"]["inputs"]["left"] = 512;

                        // For the final quarter-of-the-grid output
                        json["prompt"]["110"]["inputs"]["y"] = 512;

                        // Compositing the small image to the grid
                        json["prompt"]["156"]["inputs"]["x"] = 128;
                        json["prompt"]["156"]["inputs"]["y"] = 640; // 512 + 128 - for small image of 256x256

                        // Prompt for creating the small image to be placed in the grid
                        json["prompt"]["152"]["inputs"]["text"] = diffReq.positivePrompt;

                        // Prompt for the whole grid, taking into account the small image that was created and placed in it
                        json["prompt"]["6"]["inputs"]["text"] += ", " + diffReq.positivePrompt + " on left";
                        json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                        // Random seeds for both diffusions of the single workflow request(one for the small image, the other for the whole grid)
                        json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                        json["prompt"]["150"]["inputs"]["seed"] = randomSeed;
                        break;    
                        
                    case "right":
                        await library.UploadImage(diffReq, curImageSize);

                        // Inputing the correct upload image name
                        json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                        // Outpainting increase size of original upload image
                        json["prompt"]["11"]["inputs"]["right"] = 512;

                        // For the final quarter-of-the-grid output
                        json["prompt"]["110"]["inputs"]["y"] = 512;
                        json["prompt"]["110"]["inputs"]["x"] = 512;

                        // Compositing the small image to the grid
                        json["prompt"]["156"]["inputs"]["x"] = 640;
                        json["prompt"]["156"]["inputs"]["y"] = 640;

                        // Prompt for creating the small image to be placed in the grid
                        json["prompt"]["152"]["inputs"]["text"] = diffReq.positivePrompt;

                        // Prompt for the whole grid, taking into account the small image that was created and placed in it
                        json["prompt"]["6"]["inputs"]["text"] += ", " + diffReq.positivePrompt + " on right";
                        json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                        // Random seeds for both diffusions of the single workflow request(one for the small image, the other for the whole grid)
                        json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                        json["prompt"]["150"]["inputs"]["seed"] = randomSeed;
                        break;

                    case "top":
                        await library.UploadImage(diffReq, curImageSize);

                        // Inputing the correct upload image name
                        json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                        // Outpainting increase size of original upload image
                        json["prompt"]["11"]["inputs"]["right"] = 512;

                        // Compositing the small image to the grid
                        json["prompt"]["156"]["inputs"]["x"] = 128;
                        json["prompt"]["156"]["inputs"]["y"] = 128;

                        // Prompt for creating the small image to be placed in the grid
                        json["prompt"]["152"]["inputs"]["text"] = diffReq.positivePrompt;

                        // Prompt for the whole grid, taking into account the small image that was created and placed in it
                        json["prompt"]["6"]["inputs"]["text"] += ", " + diffReq.positivePrompt + " on top";
                        json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                        // Random seeds for both diffusions of the single workflow request(one for the small image, the other for the whole grid)
                        json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                        json["prompt"]["150"]["inputs"]["seed"] = randomSeed;
                        break;

                    case "bottomRight":
                        if (diffReq.uploadTextures == null)
                        {
                            Debug.LogError("Upload textures for worklow");
                            return null;
                        }
                        if (diffReq.uploadTextures.Count <= 2)
                        {
                            Debug.LogError("Upload enough textures for worklow");
                            return null;
                        }

                        promptText = $@"
                            {{
                                ""id"": ""{guid}"",
                                ""prompt"": {getWorkflowJSON(diffusionWorkflows.Grid4Outpainting)}
                            }}";

                        await library.UploadImage(diffReq, curImageSize);                        

                        // Inputing the correct upload image name
                        json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                        json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[1].name;
                        json["prompt"]["90"]["inputs"]["image"] = diffReq.uploadTextures[2].name;

                        // Outpainting increase size of original upload image
                        json["prompt"]["11"]["inputs"]["left"] = 512;

                        // For the final quarter-of-the-grid output - no need to do anything in this case

                        // Compositing the small image to the grid
                        json["prompt"]["126"]["inputs"]["x"] = 128;
                        json["prompt"]["126"]["inputs"]["y"] = 128;

                        // Prompt for creating the small image to be placed in the grid
                        json["prompt"]["123"]["inputs"]["text"] = diffReq.positivePrompt;

                        // Prompt for the whole grid, taking into account the small image that was created and placed in it
                        json["prompt"]["6"]["inputs"]["text"] += ", " + diffReq.positivePrompt + " on top left";
                        json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                        // Random seeds for both diffusions of the single workflow request(one for the small image, the other for the whole grid)
                        json["prompt"]["21"]["inputs"]["seed"] += randomSeed;
                        json["prompt"]["121"]["inputs"]["seed"] += randomSeed;
                        break;

                    case "bottomLeft":
                        if (diffReq.uploadTextures == null)
                        {
                            Debug.LogError("Upload textures for worklow");
                            return null;
                        }
                        if (diffReq.uploadTextures.Count <= 2)
                        {
                            Debug.LogError("Upload enough textures for worklow");
                            return null;
                        }

                        promptText = $@"
                            {{
                                ""id"": ""{guid}"",
                                ""prompt"": {getWorkflowJSON(diffusionWorkflows.Grid4Outpainting)}
                            }}";

                        await library.UploadImage(diffReq, curImageSize);

                        // Inputing the correct upload image name
                        json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[2].name;
                        json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[1].name;
                        json["prompt"]["90"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                        // Outpainting increase size of original upload image
                        json["prompt"]["11"]["inputs"]["right"] = 512;

                        // For the final quarter-of-the-grid output
                        json["prompt"]["110"]["inputs"]["x"] = 512;

                        // Compositing the small image to the grid
                        json["prompt"]["126"]["inputs"]["x"] = 640;
                        json["prompt"]["126"]["inputs"]["y"] = 128;

                        // Prompt for creating the small image to be placed in the grid
                        json["prompt"]["123"]["inputs"]["text"] = diffReq.positivePrompt;

                        // Prompt for the whole grid, taking into account the small image that was created and placed in it
                        json["prompt"]["6"]["inputs"]["text"] += ", " + diffReq.positivePrompt + " on top right";
                        json["prompt"]["7"]["inputs"]["text"] += diffReq.negativePrompt;

                        // Random seeds for both diffusions of the single workflow request(one for the small image, the other for the whole grid)
                        json["prompt"]["21"]["inputs"]["seed"] += randomSeed;
                        json["prompt"]["121"]["inputs"]["seed"] += randomSeed;
                        break;

                    // Default switch sub-statement will run if it does not enter the previous cases
                    default:
                        Debug.LogError("Pick a valid direction for Outpainting");
                        break;
                }                                                       
                break;

            default:
                Debug.LogError("Please choose a useable Diffusion workflow");
                return null;
        }
        return json.ToString();
    }
}
