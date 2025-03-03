using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

/// <summary>
/// Manages the textures of the sides of the cube. Allows for changing the textures of the sides of the cube.
/// </summary>

public class SidesCubeProperties : MonoBehaviour
{
    // Sides of the cube
    public GameObject TopSide;
    public GameObject BottomSide;
    public GameObject RightSide;
    public GameObject LeftSide;
    public GameObject FrontSide;
    public GameObject BackSide;

    /// <summary>
    /// Changes the textures of the sides of the cube.
    /// </summary>
    /// <param name="listTextures">List of textures to change the sides of the cube to.</param>
    /// <returns>True if the textures were changed successfully, false otherwise.</returns>
    public bool ChangeTextures(List<Texture2D> listTextures)
    {
        if (listTextures.Count < 6) return false;

        ChangeTextures(listTextures[0], listTextures[1], listTextures[2], listTextures[3], listTextures[4], listTextures[5]);
        return true;
    }
    
    /// <summary>
    /// Changes the textures of the sides of the cube.
    /// </summary>
    /// <param name="TopTexture">Texture to change the top side of the cube to.</param>
    /// <param name="BottomTexture">Texture to change the bottom side of the cube to.</param>
    /// <param name="RightTexture">Texture to change the right side of the cube to.</param>
    /// <param name="LeftTexture">Texture to change the left side of the cube to.</param>
    /// <param name="FrontTexture">Texture to change the front side of the cube to.</param>
    /// <param name="BackTexture">Texture to change the back side of the cube to.</param>
    public bool ChangeTextures(Texture2D TopTexture, Texture2D BottomTexture, Texture2D RightTexture, 
                               Texture2D LeftTexture, Texture2D FrontTexture, Texture2D BackTexture)
    {
        if (TopTexture != null)
        {
            if (TopSide == null)
            {
                Debug.LogError("Top side of Sides cube does not exist to be added a texture");
                return false;
            }
            TopSide.GetComponent<Renderer>().material.mainTexture = TopTexture;
        }
        if (BottomTexture != null)
        {
            if (BottomSide == null)
            {
                Debug.LogError("Bottom side of Sides cube does not exist to be added a texture");
                return false;
            }
            BottomSide.GetComponent<Renderer>().material.mainTexture = BottomTexture;
        }
        if (RightTexture != null)
        {
            if (RightSide == null)
            {
                Debug.LogError("Right side of Sides cube does not exist to be added a texture");
                return false;
            }
            RightSide.GetComponent<Renderer>().material.mainTexture = RightTexture;
        }
        if (LeftTexture != null)
        {
            if (LeftSide == null)
            {
                Debug.LogError("Left side of Sides cube does not exist to be added a texture");
                return false;
            }
            LeftSide.GetComponent<Renderer>().material.mainTexture = LeftTexture;
        }
        if (FrontTexture != null)
        {
            if (FrontSide == null)
            {
                Debug.LogError("Front side of Sides cube does not exist to be added a texture");
                return false;
            }
            FrontSide.GetComponent<Renderer>().material.mainTexture = FrontTexture;
        }
        if (BackTexture != null)
        {
            if (BackSide == null)
            {
                Debug.LogError("Back side of Sides cube does not exist to be added a texture");
                return false;
            }
            BackSide.GetComponent<Renderer>().material.mainTexture = BackTexture;
        }

        return true;
    }
}
