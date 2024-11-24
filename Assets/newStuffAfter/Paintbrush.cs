using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO
/// </summary>
public class Paintbrush : MonoBehaviour
{
    public Color brushColor = Color.black;  // Color of the paintbrush
    public int brushSize = 5;  // Size of the paintbrush stroke
    public Transform brushTourchPos;
    private PaintableCanvas canvas;

    void OnTriggerStay(Collider other)
    {
        canvas = other.GetComponent<PaintableCanvas>();
        if (canvas != null )
        {
            Paint(other);
        }
    }

    private Color GetColorFromString(string colorName)
    {
        Color color;
        // Try to parse the color name using the HTML format (supports names like "red", "green", etc.)
        if (ColorUtility.TryParseHtmlString(colorName, out color))
        {
            return color;
        }
        else
        {
            Debug.LogWarning("Invalid color name: " + colorName);
            return Color.white; // Default color if parsing fails
        }
    }

    public void ChangeColor(string newBrushColor)
    {
        brushColor = GetColorFromString(newBrushColor);
    }

    void Paint(Collider canvasCollider)
    {
        // Get the point of contact in world space
        RaycastHit hit;

        LayerMask layerMask = LayerMask.GetMask("Canvas");

        if (Physics.Raycast(brushTourchPos.position, transform.forward, out hit, 5f, layerMask))
        {            
            if (hit.collider == canvasCollider)
            {
                // Get UV coordinates (0 to 1 range) from the hit point on the canvas
                Vector2 uv;
                Renderer renderer = canvasCollider.GetComponent<Renderer>();
                uv = hit.textureCoord;

                // Convert UV to pixel coordinates on the texture
                int x = (int)(uv.x * canvas.canvasTexture.width);
                int y = (int)(uv.y * canvas.canvasTexture.height);

                // Paint on the texture
                PaintOnTexture(x, y, canvas);
            }
        }
    }

    void PaintOnTexture(int x, int y, PaintableCanvas canvas)
    {
        // Paint a simple square for the brush size
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                // Ensure the brush stays within the texture bounds
                if (pixelX >= 0 && pixelX < canvas.canvasTexture.width && pixelY >= 0 && pixelY < canvas.canvasTexture.height)
                {
                    canvas.canvasTexture.SetPixel(pixelX, pixelY, brushColor);
                }
            }
        }
        canvas.canvasTexture.Apply();  // Apply the changes to the texture
    }
}