using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
// using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


public class ComfySceneLibraryNew : UnityEngine.Object
{
    public string ServerAddress = "";
    public ComfyOrganizerNew comfyOrg;

    [NonSerialized]
    public static bool loadedAddress = false;

    public const string ImageFolderName = "Assets/"; // TODO: if the other consts exist, this one can be const aswell

    private ClientWebSocket _ws;
    private bool _startedGenerations = false;
    private Dictionary<diffusionWorkflows, string> _diffusionJsons;
    private bool _readyForDiffusion = false;

    private static string HTTPPrefix = "https://";  // https://  ------ When using online API service | http:// ------ When using offline server
    // TODO:: in ComfyOrganizer I added List of outgioing image names
    private static HashSet<string> _incomingImageNames;

    private const int MaxNetworkingRetries = 1000;
    private const string JsonFolderPath = "JSONMain";
    private const string ThinkDiffusionPrefix = "jonathanmiroshnik-";
    private const string ThinkDiffusionPostfix = ".thinkdiffusion.xyz";

    public ComfySceneLibraryNew()
    {
        _ws = new ClientWebSocket();
        _diffusionJsons = new Dictionary<diffusionWorkflows, string>();

        if (_incomingImageNames == null) _incomingImageNames = new HashSet<string>();
    }

    public void StartComfySceneLibrary()
    {
        StartComfySceneLibrary(null);
    }

    public void LoadSpecialServerAddress(string initialIP)
    {
        if (loadedAddress) return;
        if (GameManager.getInstance() == null) return;

        if (initialIP == "" || initialIP == "127.0.0.1:8188")
        {
            GameManager.getInstance().IP = "127.0.0.1:8188";
            HTTPPrefix = "http://";

            Debug.Log("No unique server IP set, setting default: " + GameManager.getInstance().IP.ToString());

            
        }
        else
        {
            GameManager.getInstance().IP = ThinkDiffusionPrefix + initialIP + ThinkDiffusionPostfix;
            HTTPPrefix = "https://";

            Debug.Log("Set the final server IP as: " + GameManager.getInstance().IP.ToString());
        }

        loadedAddress = true;
    }

    // TODO: notice that this START must always come BEFORE(put the library before the organizer in the node properties)
    // TODO: cont. the ComfyOrganizer or else some things will not be ready for an instant diffusion request
    public void StartComfySceneLibrary(DiffusionRequest beginningDiffusionRequest)
    {
        /*// TODO: bad if statement because alread exists inside the function, remove serveraddress as it is
        if (serverAddress != "" && serverAddress != "127.0.0.1:8188")
        {
            LoadSpecialServerAddress(serverAddress);
        }*/
        LoadSpecialServerAddress(ServerAddress);

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

        _readyForDiffusion = true;

        DownloadCycle();

        if (beginningDiffusionRequest != null) comfyOrg.SendDiffusionRequest(beginningDiffusionRequest);
    }

    /// <summary>
    /// Goes over the yet undownloaded images of the DiffusionRequests and attempts to download these.    
    /// </summary>
    private async void DownloadCycle()
    {
        // Infinite loop as the download cycle must continue until the user quits the application
        while(true)
        {
            await Task.Delay(100); // 0.1f seconds

            if (comfyOrg == null) Debug.Log("hell");
            List<DiffusionRequest> allDiffReqs = comfyOrg.GetUndownloadedRequestPrompts();
            if (allDiffReqs == null || allDiffReqs.Count == 0) continue;
            foreach (DiffusionRequest diffReq in allDiffReqs)
            {
                if (!diffReq.sentDownloadRequest)
                {
                    RequestFileNameRoutine(diffReq);                    
                }                
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

    /// <summary>
    /// Returns an appropriate JSON text in accordance to the given DiffusionRequest. Factory design pattern.
    /// </summary>
    /// <param name="diffReq">given DiffusionRequest to create the JSON text from.</param>
    private async Task<string> DiffusionJSONFactory(DiffusionRequest diffReq)
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
                await UploadImage(diffReq, curImageSize);

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
                
                await UploadImage(diffReq, curImageSize);

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

                await UploadImage(diffReq, curImageSize);

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
                        await UploadImage(diffReq, curImageSize);

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
                        await UploadImage(diffReq, curImageSize);

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
                        await UploadImage(diffReq, curImageSize);

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

                        await UploadImage(diffReq, curImageSize);                        

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

                        await UploadImage(diffReq, curImageSize);

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

    /// <summary>
    /// Sends a Diffusion Image generation request to the server.
    /// </summary>
    /// <param name="diffReq">DiffusionRequest to send to the server.</param>
    public async void QueuePrompt(DiffusionRequest diffReq, int trials)
    {
        if (trials <= 0 || GameManager.getInstance() == null) return;

        // Waits for the Library to be started
        int INNER_DIFFUSION_TRIALS = 10;
        if (!_readyForDiffusion)
        {
            if (INNER_DIFFUSION_TRIALS <= 0) return;
            INNER_DIFFUSION_TRIALS--;
            await Task.Delay(1000); // 1 second
        }

        string url = HTTPPrefix + GameManager.getInstance().IP + "/prompt";
        Debug.Log("URL: " + url);
        
        // Creates the prompt string to be send to the server
        string promptText = await DiffusionJSONFactory(diffReq);
        if (promptText == null || promptText.Length <= 0) return;

        // Waits for all the relevant images for the workflow to upload to the server
        int MAX_RETRIES = MaxNetworkingRetries;
        while (!diffReq.uploadFileChecker.fileExists && MAX_RETRIES > 0)
        {
            await Task.Delay(200); // 0.2 seconds
            MAX_RETRIES--;
        }
        if (MAX_RETRIES <= 0) return;        
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error + " Trial Number: " + trials.ToString());
                await Task.Delay(200); // 0.2 seconds
                trials--;
                QueuePrompt(diffReq, trials); // TODO: recursive call?
            }
            else
            {
                // This is the only use of ResponseData, but it is needed for proper downloading of the prompt
                ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                diffReq.prompt_id = data.prompt_id;
                return;
            }
        }
    }


    void OnDestroy()
    {
        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }

    /// <summary>
    /// Checks whether a file exists in the server
    /// </summary>
    /// <param name="imageName">Name of the file we want to check exists</param>
    /// <param name="fileChecker">Used to keep track of the final result of the check</param>
    /// <param name="subfolder">Subfolder on the server the file should be found in</param>
    private async Task CheckIfFileExists(string imageName, FileExistsChecker fileChecker, string subfolder)
    {
        if (GameManager.getInstance() == null) return;
        string url = HTTPPrefix + GameManager.getInstance().IP + "/view?filename=" + imageName + "&type=" + subfolder;

        using (var unityWebRequest = UnityWebRequest.Head(url))
        {
            var operation = unityWebRequest.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {                
                Debug.Log("File " + imageName + " still not in " + subfolder);
            }
            else
            {
                fileChecker.fileExists = true;
            }
        }
    }

    /// <summary>
    /// Requests and downloads the images created for a given DiffusionRequest
    /// </summary>
    /// <param name="diffReq">given DiffusionRequest to download the images created for it</param>
    private async void RequestFileNameRoutine(DiffusionRequest diffReq)
    {
        if (GameManager.getInstance() == null) return;
        string url = HTTPPrefix + GameManager.getInstance().IP + "/history/" + diffReq.prompt_id;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {            
             // Request and wait for the desired page.
            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield(); // This replaces 'yield return' to work with frame timing
            }

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    if (_startedGenerations)
                    {
                        Debug.LogError(": HTTP Error: " + webRequest.error);
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    // Jonathan - added the for loop to coincide with the changes to the ExtractFilename function becoming a batch-size dependant downloader
                    // Jonathan - another change, download all the images FIRST, then display, to test display speed
                    string[] filenames = ExtractFilename(webRequest.downloadHandler.text);

                    // If there are no filenames, the prompt has not yet finished generating
                    if (filenames.Length <= 0)
                    {
                        break;
                    }

                    // Downloading each image of the prompt
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        if (!_incomingImageNames.Contains(filenames[i])) {
                            DownloadImage(filenames[i], diffReq);
                        }
                    }

                    diffReq.sentDownloadRequest = true;
                    break;
            }
        }
    }

    string[] ExtractFilename(string jsonString)
    {
        // Jonathan - Changed this from returning a single filename to all the filenames in the output - with the for loop
        string keyToLookFor = "\"filename\":";
        int total_files = Regex.Matches(jsonString, keyToLookFor).Count;

        string[] filenames = new string[total_files];
        int prevIndex = -1;

        for (int i = 0; i < total_files; i++)
        {
            // Step 1: Identify the part of the string that contains the "filename" key
            int startIndex = jsonString.IndexOf(keyToLookFor, prevIndex + 1);
            prevIndex = startIndex;

            if (startIndex == -1)
            {
                return null;
            }

            // Adjusting startIndex to get the position right after the keyToLookFor
            startIndex += keyToLookFor.Length;

            // Step 2: Extract the substring starting from the "filename" key
            string fromFileName = jsonString.Substring(startIndex);

            // Assuming that filename value is followed by a comma (,)
            int endIndex = fromFileName.IndexOf(',');

            // Extracting the filename value (assuming it's wrapped in quotes)
            string filenameWithQuotes = fromFileName.Substring(0, endIndex).Trim();

            // Removing leading and trailing quotes from the extracted value
            filenames[i] = filenameWithQuotes.Trim('"');

        }

        return filenames;
    }

    /// <summary>
    /// Downloads a single image according to the given image name and adds it to the DiffusionRequest
    /// </summary>
    /// <param name="filename">Image name to download</param>
    /// <param name="diffReq">DiffusionRequest to add downloaded image to</param>
    private async Task DownloadImage(string filename, DiffusionRequest diffReq)
    {
        if (GameManager.getInstance() == null) return;
        int MAX_RETRIES = MaxNetworkingRetries;

        FileExistsChecker fileCheck = new FileExistsChecker();
        while (!fileCheck.fileExists && MAX_RETRIES > 0)
        {
            await Task.Delay(200); // 0.2f seconds
            await CheckIfFileExists(filename, fileCheck, "output");
            MAX_RETRIES--;
        }
        if (MAX_RETRIES <= 0) return;

        int MAX_RETRIES_DOWNLOAD = MaxNetworkingRetries;

        while(MAX_RETRIES_DOWNLOAD > 0)
        {
            // Checks if image already downloaded
            if (_incomingImageNames.Contains(filename)) return;

            string imageURL = HTTPPrefix + GameManager.getInstance().IP + "/view?filename=" + filename;

            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageURL))
            {
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Get the downloaded texture
                    Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                    // Checks if image already downloaded
                    if (_incomingImageNames.Contains(filename)) return;
                    // Adds Image to downloaded Images HashSet
                    _incomingImageNames.Add(filename);

                    // Adding the texture to the texture queue
                    texture.name = filename;
                    comfyOrg.AddImage(texture, diffReq);

                    return;
                }
                else
                {
                    Debug.LogError("Image download failed: " + webRequest.error);
                    MAX_RETRIES_DOWNLOAD--;
                }
            }
        }
    }

    /// <summary>
    /// Uploads given Textures to the server.
    /// </summary>
    /// <param name="diffReq">Diffusion Request containting Textures to upload to the server.</param>
    private async Task UploadImage(DiffusionRequest diffReq, Vector2Int imageSize)
    {
        if (GameManager.getInstance() == null) return;
        if (diffReq == null) return;
        List<Texture2D> curTextures = diffReq.uploadTextures;

        if (curTextures == null) return;
        if (curTextures.Count == 0) return;

        int curUploadedTextures = 0;
        
        for (int i = 0; i < curTextures.Count; i++)
        {
            Texture2D curTexture = curTextures[i];

            // Resizing the image to a default size for fast Diffusion
            curTexture = GeneralGameLibraries.TextureManipulationLibrary.Resize(curTexture, imageSize.x, imageSize.y);

            string url = HTTPPrefix + GameManager.getInstance().IP + "/upload/image";

            WWWForm form = new WWWForm();

            form.AddBinaryData("image", curTexture.EncodeToPNG(), curTexture.name, "image/png");
            form.AddField("type", "input");
            form.AddField("overwrite", "false");

            using (var unityWebRequest = UnityWebRequest.Post(url, form))
            {
                var operation = unityWebRequest.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (unityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(unityWebRequest.error);
                }
                else
                {
                    FileExistsChecker fileCheck = new FileExistsChecker();
                    int retries = MaxNetworkingRetries;

                    while (!fileCheck.fileExists && retries > 0)
                    {
                        await Task.Delay(200); // 0.2f seconds
                        await CheckIfFileExists(curTexture.name, fileCheck, "input");
                        retries--;
                    }
                    if (retries <= 0) return;

                    curUploadedTextures++;
                    if (curUploadedTextures == diffReq.uploadTextures.Count)
                    {
                        diffReq.uploadFileChecker.fileExists = true;
                    }
                }
            }
        }
    }
}
