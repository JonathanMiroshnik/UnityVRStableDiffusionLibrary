using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

/// <summary>
/// TODO
/// </summary>

public class SidesCubeProperties : MonoBehaviour
{
    public GameObject TopSide;
    public GameObject BottomSide;
    public GameObject RightSide;
    public GameObject LeftSide;
    public GameObject FrontSide;
    public GameObject BackSide;


    public bool ChangeTextures(List<Texture2D> listTextures)
    {
        if (listTextures.Count < 6) return false;

        ChangeTextures(listTextures[0], listTextures[1], listTextures[2], listTextures[3], listTextures[4], listTextures[5]);
        return true;
    }

    public bool ChangeTextures(Texture2D TopTexture, Texture2D BottomTexture, Texture2D RightTexture, 
                               Texture2D LeftTexture, Texture2D FrontTexture, Texture2D BackTexture)
    {
        if (TopTexture != null)
        {
            // Debug.Log("TOP " + TopTexture.name);

            if (TopSide == null)
            {
                Debug.LogError("Top side of Sides cube does not exist to be added a texture");
                return false;
            }
            TopSide.GetComponent<Renderer>().material.mainTexture = TopTexture;
        }
        if (BottomTexture != null)
        {
            // Debug.Log("BOTTOM " + BottomTexture.name);
            if (BottomSide == null)
            {
                Debug.LogError("Bottom side of Sides cube does not exist to be added a texture");
                return false;
            }
            BottomSide.GetComponent<Renderer>().material.mainTexture = BottomTexture;
        }
        if (RightTexture != null)
        {
            // Debug.Log("RIGHT " + RightTexture.name);
            if (RightSide == null)
            {
                Debug.LogError("Right side of Sides cube does not exist to be added a texture");
                return false;
            }
            RightSide.GetComponent<Renderer>().material.mainTexture = RightTexture;
        }
        if (LeftTexture != null)
        {
            // Debug.Log("LEFT " + LeftTexture.name);
            if (LeftSide == null)
            {
                Debug.LogError("Left side of Sides cube does not exist to be added a texture");
                return false;
            }
            LeftSide.GetComponent<Renderer>().material.mainTexture = LeftTexture;
        }
        if (FrontTexture != null)
        {
            // Debug.Log("FRONT " + FrontTexture.name);
            if (FrontSide == null)
            {
                Debug.LogError("Front side of Sides cube does not exist to be added a texture");
                return false;
            }
            FrontSide.GetComponent<Renderer>().material.mainTexture = FrontTexture;
        }
        if (BackTexture != null)
        {
            // Debug.Log("BACK " + BackTexture.name);
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
