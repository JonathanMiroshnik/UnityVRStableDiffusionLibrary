using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpOnEnable : MonoBehaviour
{

    [SerializeField] private UIDiffusionTexture diffusion;
    [SerializeField] private List<Texture2D> textures;
    [SerializeField] private Gadget gadget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        // gadget.AddTexturesToQueue(textures);
        
        // diffusion.CreatePopup(textures);
        
        diffusion.CreateAIPopup(textures);
        
        Debug.Log("Pop up!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
