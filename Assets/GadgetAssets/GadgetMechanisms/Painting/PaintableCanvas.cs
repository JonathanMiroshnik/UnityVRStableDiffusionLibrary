using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: documentation
public class PaintableCanvas : MonoBehaviour
{
    public Texture2D canvasTexture;  // The texture to paint on
    public PaintbrushMechanism paintbrushMechanism;
    public DiffusionTextureChanger changer;

    // TODO: delete this
    public GameObject displayDebug;

    void Start()
    {        
        InitializeTexture();
    }

    public Texture2D GetCanvasTexture()
    {
        return canvasTexture;
    }

    public void InitializeTexture()
    {
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
        ChangeTextureDebug();
        paintbrushMechanism.ActivateGeneration(canvasTexture);
    }

    // TODO: delete this func
    public void ChangeTextureDebug()
    {
        displayDebug.GetComponent<Renderer>().material.mainTexture = canvasTexture;
    }
}

