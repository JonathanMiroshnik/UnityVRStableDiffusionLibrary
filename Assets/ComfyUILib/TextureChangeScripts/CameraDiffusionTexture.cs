using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// TODO: to be used for the DEPTH camera
/// </summary>
public class CameraDiffusionTexture : DiffusionTextureChanger
{
    new private Queue<List<Texture2D>> _diffTextures = new Queue<List<Texture2D>>();
}
