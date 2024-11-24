using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitSceneChangeTrigger : MonoBehaviour
{ 
    public string currentRoomSceneName = "TileRoom";
    public string nextRoomSceneName = "Camera Full Prototype Scene";
    public string altNextRoomSceneName = "GadgetPrefabTest";

    public Toggle toggle;

    // TODO do I need this collision detection thing even?
    private void OnTriggerEnter(Collider other)
    {
        ChangeScene();
    }

    // TODO create a global IENUMERATOR function that check whether gamemanager is already loaded, and goes into a loop until it is loaded, maybe an error message after 5 seconds of trying?
    // TODO this is for many types of scripts throughout the project that NEED the GameManager up and running with everything, so maybe a check like that will help
    // TODO Or maybe some indicator bool that indicated it is "fully loaded"??


    public void InputToIP(TMP_InputField txt)
    {
        if (GameManager.getInstance() == null) return;

        ComfySceneLibrary.loadedAddress = false;
        GameManager.getInstance().comfySceneLibrary.LoadSpecialServerAddress(txt.text);
    }

    public void ToggleBasedChangeScene()
    {
        if (toggle == null) return;
        if (GameManager.getInstance() == null) return;

        if (toggle.isOn)
        {
            GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
        }
        else
        {
            GameManager.getInstance().LoadNextScene(currentRoomSceneName, altNextRoomSceneName);
        }
    }

    public void ChangeScene()
    {
        if (GameManager.getInstance() == null) return;

        GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
    }
}
