using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

/// <summary>
/// Used to load the needed parameters and variables onto the GameManager
/// </summary>
public class ComfySceneParametersNew : MonoBehaviour
{
    // ComfyUI Library scripts
    public ComfyOrganizerNew ComfyOrganizer;
    public ComfySceneLibraryNew ComfyLibrary;

    // If false, won't load scene parameters onto the GameManager
    public bool LoadComfyParametrs = true;

    // These events will be invoked after the GameManager is fully loaded
    public UnityEvent unityEventAfterLoading;

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
            if (ComfyOrganizer == null)
            {
                ComfyOrganizer = gameObject.GetComponent<ComfyOrganizerNew>();
            }
            if (ComfyLibrary == null)
            {
                ComfyLibrary = gameObject.GetComponent<ComfySceneLibraryNew>();
            }

            // Sending the Beginning DiffusionRequest onwards
            // GameManager.getInstance().InitiateSceneParameters(ComfyOrganizer, ComfyLibrary);
        }

        await Task.Delay(1000); // 1 second delay

        Debug.Log("Loading Comfy Scene Library!");

        ComfyLibrary.StartComfySceneLibrary();
        unityEventAfterLoading?.Invoke();

        Debug.Log("Loaded scene parameters!");
    }
}
