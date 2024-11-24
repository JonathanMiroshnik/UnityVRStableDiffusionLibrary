using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBrushImageRequestor : MonoBehaviour
{
    [SerializeField] private PaintBrushImageRequestSystem _paintBrushImageRequestSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RequestImage()
    {
        GetComponent<Renderer>().material.mainTexture = _paintBrushImageRequestSystem.RequestImage();
    }
    
    
}
