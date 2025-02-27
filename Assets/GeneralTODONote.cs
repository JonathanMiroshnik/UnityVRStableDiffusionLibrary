using UnityEngine;

// TIMELY NOTES(as of 23.11.24):
// CombineMechanism does not send textures to uiDiffusionTexture and doesn't allow placing textures.
// ^ Also not working for Base Camera Mechanism, the issue is the texture queue in the gadget.
// Object to Image Diffusion needs proper containers for 3D objects with DiffusableObject component,
//  also need images to take style and places to place images
// Add object grid for Throwing mechanism to see effects
// Camera Diffusion needs need images to take style and places to place images
// Sides Cube Mechanism add outline for prompt DiffusableObject, also make work


// GENERAL NOTES:
// TODO: make a unique ID for everything, downloaded images, uploaded images, request IDs etc
// TODO: maybe this is a wrong choice to make ANOTHER diffusion request because this one isn't really a diffusion request at all, just a transfer from one
// TODO: notice that these PlaceTextureInput methods are exactly the same for several mechanisms, make it one somewhere?
// TODO: do I even NEED the baseCamera workflow? diffusionWorkflows.baseCamera

public class GeneralTODONote : MonoBehaviour
{
}
