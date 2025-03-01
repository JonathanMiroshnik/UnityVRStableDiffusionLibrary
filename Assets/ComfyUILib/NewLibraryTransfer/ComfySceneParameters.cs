using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

/// <summary>
/// Used to load the needed parameters and variables onto the GameManager
/// </summary>
public class ComfySceneParameters : MonoBehaviour
{
    // If false, won't load scene parameters onto the GameManager
    public bool LoadComfyParametrs = true;
    
    public ServerAddresser ServerAddress;

    // These events will be invoked after the GameManager is fully loaded
    public UnityEvent unityEventAfterLoading;

    // ComfyUI Library scripts
    [NonSerialized]
    public ComfyOrganizer ComfyOrgan;
    [NonSerialized]
    public ComfySceneLibrary ComfyLib;

    [NonSerialized]
    public bool LoadedConnectorParameters = false;

    // Scene inwhich the GameManager will live
    private string _gameManagerScene = "Empty Scene";

    // Used to keep track of the loading of the GameManager scene
    private static bool _loadedGameManagerScene = false;

    private void Start()
    {
        LoadGameManagerScene();
    }

    public async void LoadGameManagerScene()
    {
        while (!LoadedConnectorParameters)
        {
            await Task.Delay(1000); // 1 second delay instead of WaitForSeconds(1)
        }

        ComfyOrgan = new ComfyOrganizer();
        ComfyLib = new ComfySceneLibrary(); 

        // Loading the GameManager scene
        if (SceneManager.sceneCount < 3 && !_loadedGameManagerScene) // TODO: magic numbers
        {
            var asyncLoad = SceneManager.LoadSceneAsync(_gameManagerScene, LoadSceneMode.Additive);            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                await Task.Delay(200); // 0.2 second delay
            }
            //asyncLoad.allowSceneActivation = false;
            Debug.Log("Got to part of script after load scene!");
        }

        Debug.Log("Loaded GameManager!");

        // Finished loading GameManager scene
        _loadedGameManagerScene = true;

        // Loading parameters onto the GameManager
        if (!_loadedGameManagerScene || LoadComfyParametrs)
        {
            if (ComfyOrgan == null)
            {
                Debug.LogError("ComfyOrgan is null");
            }
            if (ComfyLib == null)
            {
                Debug.LogError("ComfyLib is null");
            }
            
            // Sending the Beginning DiffusionRequest onwards
            GameManager.getInstance().InitiateSceneParameters(ComfyOrgan, ComfyLib, ServerAddress);
        }

        await Task.Delay(1000); // 1 second delay

        Debug.Log("Loading Comfy Scene Library!");

        ComfyLib.StartComfySceneLibrary();
        unityEventAfterLoading?.Invoke();

        Debug.Log("Loaded scene parameters!");
    }
}
