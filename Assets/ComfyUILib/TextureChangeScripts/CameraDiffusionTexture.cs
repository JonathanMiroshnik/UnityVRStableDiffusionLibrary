using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraDiffusionTexture : DiffusionTextureChanger
{
    new private Queue<List<Texture2D>> diff_Textures = new Queue<List<Texture2D>>();
}
