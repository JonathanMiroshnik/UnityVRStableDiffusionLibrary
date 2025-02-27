using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: documentation
public class PaintableCanvas : MonoBehaviour
{
    // The texture to paint on. If empty, the texture will be created when the canvas is initialized.
    public Texture2D canvasTexture;

    // TODO: why a mechanism? why not just a script to send a DiffusionRequest?
    // TODO: why not have this as a component instead of being specifically for diffusion? can be just for painting.
    public PaintbrushMechanism paintbrushMechanism;
    public DiffusionTextureChanger changer;

    void Start()
    {        
        InitializeTexture();
    }
    
    private void InitializeTexture()
    {
        if (canvasTexture == null) {
            // Create a new blank texture (e.g., white background)
            canvasTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            canvasTexture.name = "canvasTex_" + unixTime.ToString() + "_" + UnityEngine.Random.Range(1, 1000);

            for (int y = 0; y < canvasTexture.height; y++)
            {
                for (int x = 0; x < canvasTexture.width; x++)
                {
                    canvasTexture.SetPixel(x, y, Color.white);  // Initialize with white background
                }
            }
            canvasTexture.Apply();
        }        

        if (changer != null)
        {
            var texture = new List<Texture2D>();
            texture.Add(canvasTexture);

            changer.AddTexture(texture, false);
            changer.changeTextureOn(gameObject, canvasTexture);
        }
        else
        {
            // Assign it to the material
            GetComponent<Renderer>().material.mainTexture = canvasTexture;
            GetComponent<Renderer>().material.SetTexture("_BaseMap", canvasTexture);
        }
    }

    public void ActivateGeneration()
    {
        if (paintbrushMechanism == null) return;
        // Texture2D inTex = GeneralGameLibraries.TextureManipulationLibrary.toTexture2D(canvasTexture);
        // ChangeTextureDebug(); // TODO: what is this?
        paintbrushMechanism.ActivateGeneration(canvasTexture);
    }
}

