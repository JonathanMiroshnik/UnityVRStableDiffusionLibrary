using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Manager for the project.
/// Responsible for changing scenes and keeping the communication going with the Diffusion Server.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Non scene-local objects
    private static GameManager instance = null;
    
    public string IP = ""; // jonathanmiroshnik-networks-24172136.thinkdiffusion.xyz

    [NonSerialized]
    public List<GameObject> diffusionList;

    // Scene-local objects
    [NonSerialized]
    public ComfyOrganizer comfyOrganizer;
    [NonSerialized]
    public ComfySceneLibrary comfySceneLibrary;

    public static bool fullyLoaded = false;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    /// <summary>
    /// Returns the singular instance of GameManager
    /// </summary>
    public static GameManager getInstance()
    {
        if (instance == null) Debug.Log("Awake the GameManager");

        return instance;
    }
        
    public void LoadNextScene(string thisScene, string nextScene)
    {
        fullyLoaded = false;
        StartCoroutine(LoadScene(thisScene, nextScene));
    }

    /// <summary>
    /// Used to enter the relevant scene-specific Scene parameters
    /// </summary>
    /// <param name="_comfyOrganizer"></param>
    /// <param name="_comfySceneLibrary"></param>
    public void InitiateSceneParameters(ComfyOrganizer _comfyOrganizer, ComfySceneLibrary _comfySceneLibrary)
    {
        comfyOrganizer = _comfyOrganizer;
        comfySceneLibrary = _comfySceneLibrary;

        if (comfyOrganizer == null)
        {
            Debug.LogError("Please add a Comfy Organizer to the GameManager");
        }
        if (comfySceneLibrary == null)
        {
            Debug.LogError("Please add a Comfy Scene Library to the GameManager");
        }

        FillDiffusablesList();

        fullyLoaded = true;
    }

    // TODO Use a courotine so u dont freeze the ui
    public IEnumerator LoadScene(string thisScene, string nextScene)
    {
        // Only when the scene is loaded we can unload the orginally active screen
        var asyncUnload = SceneManager.UnloadSceneAsync(thisScene);

        while (!asyncUnload.isDone)
        {
            if (asyncUnload.progress >= 0.9f)
            {
                Debug.Log("Unloading...");
                break;
            }

            yield return null;
        }


        // Load a scene in additive mode, meaning it wont unload the currently loaded scene if there is one
        var loadScene = SceneManager.LoadSceneAsync(
        nextScene,
        LoadSceneMode.Additive
        );

        loadScene.allowSceneActivation = false;

        // wait for the scene to load
        while (!loadScene.isDone)
        {
            if (loadScene.progress >= 0.9f) break;

            yield return null;
        }

        loadScene.allowSceneActivation = true;

        // HERE IS WHERE ALOT OF PEOPLE FAIL.
        // You need to wait for the scene to be loaded before you can unload the another scene.
        // This is because UNITY WILL NOT UNLOAD A scene if its the only one  currently active
        // If you look up, we allow scene activate only at 90% of the loading(for reasons im not sure but whatever), 
        // so now we have to wait for the rest of the 10% to load
        // yield return null;
        yield return new WaitForSeconds(1f);

        bool d = SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextScene));
        Debug.Log("New Scene: " + SceneManager.GetActiveScene().name);
    }

    public static IEnumerator CallWhenGameManagerLoaded(Action action)
    {
        while(!fullyLoaded)
        {
            yield return new WaitForSeconds(0.1f);
        }

        action.Invoke();
    }

    /// <summary>
    /// Should be used at the beginning of each scene to fill the relevant Diffusables into the list and use the various mechanisms
    /// </summary>
    private void FillDiffusablesList()
    {
        diffusionList = new List<GameObject>();

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.activeInHierarchy)
            {
                diffusionList.Add(go);
            }
        }
    }
}
