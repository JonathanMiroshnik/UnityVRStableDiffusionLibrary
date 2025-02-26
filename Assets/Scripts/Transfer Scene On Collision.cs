using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferSceneOnCollision : MonoBehaviour
{
    [SerializeField] private string _thisScene, _nextScene;
    
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.getInstance() == null) return;

        Debug.Log("Exited Scene!");
        GameManager.getInstance().LoadNextScene(_thisScene, _nextScene);
    }
}
