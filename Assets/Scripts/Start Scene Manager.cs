using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text display;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private string thisScene, nextScene;

    public GameObject XROrigin;

    public void TryTransferScene()
    {
        if (GameManager.getInstance() == null) return;

        Debug.Log("Called TryTransferScene!");
        if (input.text == "")
        {
            display.text = "IP cannot be empty!";
            return;
        }

        XROrigin.SetActive(false);
        GameManager.getInstance().IP = input.text;
        GameManager.getInstance().LoadNextScene(thisScene, nextScene);
        
    }
}
