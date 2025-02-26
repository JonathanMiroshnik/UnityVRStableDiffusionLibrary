using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBrushImageRequestor : MonoBehaviour
{
    [SerializeField] private PaintBrushImageRequestSystem _paintBrushImageRequestSystem;

    public void RequestImage()
    {
        GetComponent<Renderer>().material.mainTexture = _paintBrushImageRequestSystem.RequestImage();
    }
    
    
}
