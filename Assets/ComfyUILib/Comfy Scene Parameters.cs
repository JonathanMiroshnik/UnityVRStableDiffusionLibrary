using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Used to load the needed parameters and variables onto the GameManager
/// </summary>
public class ComfySceneParameters : MonoBehaviour
{
    // ComfyUI Library scripts
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    // Scene inwhich the GameManager will live
    private string GameManagerScene = "Empty Scene";

    // Used to keep track of the loading of the GameManager scene
    private static bool loadedGameManagerScene = false;


    // If false, won't load scene parameters onto the GameManager
    public bool LoadComfyParametrs = true;

    // These events will be invoked after the GameManager is fully loaded
    public UnityEvent unityEventAfterLoading;

    [NonSerialized]
    public bool LoadedConnectorParameters = false;

    private void Start()
    {
        StartCoroutine(LoadGameManagerScene());
    }

    public IEnumerator LoadGameManagerScene()
    {
        while (!LoadedConnectorParameters)
        {
            yield return new WaitForSeconds(1);
        }

        // Loading the GameManager scene
        if (SceneManager.sceneCount < 3 && !loadedGameManagerScene)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(GameManagerScene, LoadSceneMode.Additive);            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return new WaitForSeconds(0.2f);
            }
            //asyncLoad.allowSceneActivation = false;
            Debug.Log("Got to part of script after load scene!");
        }

        Debug.Log("Loaded GameManager!");

        // Finished loading GameManager scene
        loadedGameManagerScene = true;

        // Loading parameters onto the GameManager
        if (!loadedGameManagerScene || LoadComfyParametrs)
        {
            if (comfyOrganizer == null)
            {
                comfyOrganizer = gameObject.GetComponent<ComfyOrganizer>();
            }
            if (comfySceneLibrary == null)
            {
                comfySceneLibrary = gameObject.GetComponent<ComfySceneLibrary>();
            }

            // Sending the Beginning DiffusionRequest onwards
            GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary);
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("Loading Comfy Scene Library!");

        comfySceneLibrary.StartComfySceneLibrary();
        unityEventAfterLoading?.Invoke();

        Debug.Log("Loaded scene parameters!");

        yield break;
    }
}
