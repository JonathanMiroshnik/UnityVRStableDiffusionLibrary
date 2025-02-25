using System.Collections.Generic;
using UnityEngine;

public class PopUpOnEnable : MonoBehaviour
{

    [SerializeField] private UIDiffusionTexture _diffusion;
    [SerializeField] private List<Texture2D> _textures;
    [SerializeField] private Gadget _gadget;

    private void OnEnable()
    {
        _diffusion.CreateAIPopup(_textures);
        Debug.Log("Pop up!");
    }
}
