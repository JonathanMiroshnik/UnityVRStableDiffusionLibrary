using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _display;
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private string _thisScene, _nextScene;

    public GameObject XROrigin;

    public void TryTransferScene()
    {
        if (GameManager.getInstance() == null) return;

        Debug.Log("Called TryTransferScene!");
        if (_input.text == "")
        {
            _display.text = "IP cannot be empty!";
            return;
        }

        XROrigin.SetActive(false);
        GameManager.getInstance().IP = _input.text;
        GameManager.getInstance().LoadNextScene(_thisScene, _nextScene);
        
    }
}
