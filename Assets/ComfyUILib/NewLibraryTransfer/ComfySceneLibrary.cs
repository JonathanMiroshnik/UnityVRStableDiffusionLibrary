using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

/// <summary>
/// Interacts with the ComfyUI server to download and upload images.
/// </summary>
public class ComfySceneLibrary : System.Object
{
    public const string ImageFolderName = "Assets/"; // TODO: if the other consts exist, this one can be const aswell

    private DiffusionRequestFactory _diffusionRequestFactory;
    private ClientWebSocket _ws;
    private bool _startedGenerations = false;
    private bool _readyForDiffusion = false;
    private static HashSet<string> _incomingImageNames;

    // Maximum number of retries for networking operations
    private const int MaxNetworkingRetries = 1000;

    public ComfySceneLibrary()
    {
        _ws = new ClientWebSocket();
        if (_incomingImageNames == null) _incomingImageNames = new HashSet<string>();
    }

    public void StartComfySceneLibrary()
    {
        StartComfySceneLibrary(null);
    }

    // TODO: notice that this START must always come BEFORE(put the library before the organizer in the node properties)
    // TODO: cont. the ComfyOrganizer or else some things will not be ready for an instant diffusion request
    public void StartComfySceneLibrary(DiffusionRequest beginningDiffusionRequest)
    {
        _diffusionRequestFactory = new DiffusionRequestFactory();
        _diffusionRequestFactory.LoadFactory();

        _readyForDiffusion = true;
        DownloadCycle();

        if (beginningDiffusionRequest != null) GameManager.getInstance().ComfyOrgan.SendDiffusionRequest(beginningDiffusionRequest);
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
            if (GameManager.getInstance() == null) continue;
            if (GameManager.getInstance().ComfyOrgan == null) continue;
            
            List<DiffusionRequest> allDiffReqs = GameManager.getInstance().ComfyOrgan.GetUndownloadedRequestPrompts();
            if (allDiffReqs.Count == 0) continue;
            
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
    /// Sends a Diffusion Image generation request to the server.
    /// </summary>
    /// <param name="diffReq">DiffusionRequest to send to the server.</param>
    public async void QueuePrompt(DiffusionRequest diffReq, int trials)
    {
        if (trials <= 0 || GameManager.getInstance() == null) return;

        // Waits for the Library to be started
        int innerDiffusionTrials = 10;
        if (!_readyForDiffusion)
        {
            if (innerDiffusionTrials <= 0) return;
            innerDiffusionTrials--;
            await Task.Delay(1000); // 1 second
        }

        string url = GameManager.getInstance().ServerAddress.GetServerAddress() + "/prompt";
        
        // Creates the prompt string to be send to the server
        string promptText = await _diffusionRequestFactory.DiffusionJSONFactory(diffReq, this);
        if (promptText == null || promptText.Length <= 0) return;

        // Waits for all the relevant images for the workflow to upload to the server
        int maxNetworkingRetries = MaxNetworkingRetries;
        while (!diffReq.uploadFileChecker.fileExists && maxNetworkingRetries > 0)
        {
            await Task.Delay(200); // 0.2 seconds
            maxNetworkingRetries--;
        }
        if (maxNetworkingRetries <= 0) return;        

        WWWForm form = new WWWForm();
        form.AddField("prompt", promptText); // TODO: what about bodyRaw below?

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
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
                QueuePrompt(diffReq, trials);
            }
            else
            {
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
        string url = GameManager.getInstance().ServerAddress.GetServerAddress() + "/view?filename=" + imageName + "&type=" + subfolder;

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
        string url = GameManager.getInstance().ServerAddress.GetServerAddress() + "/history/" + diffReq.prompt_id;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {            
             // Request and wait for the desired page.
            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield(); // This replaces 'yield return' to work with frame timing
            }

            // TODO: why not use this switch case in every function that sends a request?
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

                    // TODO: previous solution, check if the new one is better
                    // // Downloading each image of the prompt
                    // for (int i = 0; i < filenames.Length; i++)
                    // {
                    //     if (!_incomingImageNames.Contains(filenames[i])) {
                    //         DownloadImage(filenames[i], diffReq);
                    //     }
                    // }

                    // Create list of download tasks for all new images
                    var downloadTasks = new List<Task>();
                    foreach (string filename in filenames)
                    {
                        if (!_incomingImageNames.Contains(filename)) {
                            downloadTasks.Add(DownloadImage(filename, diffReq));
                        }
                    }

                    // Wait for all downloads to complete in parallel
                    if (downloadTasks.Count > 0)
                    {
                        await Task.WhenAll(downloadTasks);
                    }

                    diffReq.sentDownloadRequest = true;
                    break;
            }
        }
    }

    /// <summary>
    /// Extracts the filenames from the JSON string.
    /// </summary>
    /// <param name="jsonString">The JSON string to extract the filenames from.</param>
    /// <returns>An array of filenames.</returns>
    string[] ExtractFilename(string jsonString)
    {
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
        int maxNetworkingRetries = MaxNetworkingRetries;

        FileExistsChecker fileCheck = new FileExistsChecker();
        while (!fileCheck.fileExists && maxNetworkingRetries > 0)
        {
            await Task.Delay(200); // 0.2f seconds
            await CheckIfFileExists(filename, fileCheck, "output");
            maxNetworkingRetries--;
        }
        if (maxNetworkingRetries <= 0) return;

        int maxNetworkingRetriesDownload = MaxNetworkingRetries;
        while(maxNetworkingRetriesDownload > 0)
        {
            // Checks if image already downloaded
            if (_incomingImageNames.Contains(filename)) return;

            string imageURL = GameManager.getInstance().ServerAddress.GetServerAddress() + "/view?filename=" + filename;

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
                    Debug.Log("Adding image: " + filename);
                    GameManager.getInstance().ComfyOrgan.AddImage(texture, diffReq);

                    return;
                }
                else
                {
                    Debug.LogError("Image download failed: " + webRequest.error);
                    maxNetworkingRetriesDownload--;
                }
            }
        }
    }

    /// <summary>
    /// Uploads given Textures to the server.
    /// </summary>
    /// <param name="diffReq">Diffusion Request containting Textures to upload to the server.</param>
    public async Task UploadImage(DiffusionRequest diffReq, Vector2Int imageSize)
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

            string url = GameManager.getInstance().ServerAddress.GetServerAddress() + "/upload/image";

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
                    // We check if the file exists on the server after uploading it
                    FileExistsChecker fileCheck = new FileExistsChecker();
                    int retries = MaxNetworkingRetries;

                    while (!fileCheck.fileExists && retries > 0)
                    {
                        await Task.Delay(200); // 0.2f seconds
                        await CheckIfFileExists(curTexture.name, fileCheck, "input");
                        retries--;
                    }
                    if (retries <= 0) return;

                    // If all textures of the DiffusionRequest have been uploaded, we set the fileExists flag to true
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
