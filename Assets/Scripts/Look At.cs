using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private GameObject _mainCamera;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_mainCamera.transform.position);
    }
}
