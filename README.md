# UnityVRStableDiffusionLibrary
A library and code base filled with all kinds of scripts that work with the ComfyUI API

As of ==26.11.2024==, this project is being actively worked on(it is based on a previous project which is already complete and includes much of this code, but here it is refactored and made much more useable for the general Unity developer), and this README section will be expanded and will include as many details as I can fit, if you have any questions, feel free to send me an e-mail.

---

**The following are instructions to use the More Walls game/libraries to create images in Unity.** 

Please notice that this code and the instructions that have been fit to it were made for the game "Walls beyond Walls" for the XR Future Realities course in HUJI 2023-2024. Even though the use case was quite limited, I have modified the code to be more general purpose for the case of others who might be interested in creating images and downloading them into their Unity game during play time.

Our system works by sending JSON "API workflows" to a ComfyUI API server and downloading the resultant images when they finish generating.

The document will first detail the minimum requirements to get things working and later-on will delve into the more specific cases of the game "Walls Beyond Walls" and general recommendations we wish to give from our experiences.

# Basic Set up:
You will need to set up a ComfyUI instance. To do this, you must either download it and set it up yourself(on your computer, for example) or you _could_ pay a service that provides you with an instance.

_Assuming_ we have decided on the first option, you will have to install the instance by following the regular instructions here:
https://github.com/comfyanonymous/ComfyUI

After you have installed ComfyUI and managed to get it to generate images, we turn our attention to the needed changes you need to perform on ComfyUI. You will have to download several models and plugins to access all the features, but this step is fluid especially for developers who want to understand the code. Different models/workflows are used for different things, some of them are good for quick(but relatively low quality) generations, other produce more than just images. The code is set up in such a way that to add a new workflow/model to the mix should be quite easy.

## Models to Install:
- Mini SD - for 256x256 images: https://huggingface.co/justinpinkney/miniSD/blob/main/miniSD.ckpt
- Nano SD - for 128x128 images: https://huggingface.co/NikolayKozloff/stable-diffusion-nano-2-1-ckpt/resolve/main/stable-diffusion-nano-2-1.ckpt
- Turbo SD - for 512x512 images: https://civitai.com/api/download/models/248781?type=Model&format=SafeTensor&size=full&fp=fp32
- GhostMix - High quality, slow, 512x512 images: https://civitai.com/api/download/models/76907?type=Model&format=SafeTensor&size=pruned&fp=fp16

#To-Do explanations on these:
- LCM LoRA - Allows SD 1.5 to speed up significantly by lowering the number of required steps to get a good quality generation from 20-30 to 3-5: https://civitai.com/api/download/models/223551?type=Model&format=SafeTensor
- CLIP VISION: https://huggingface.co/h94/IP-Adapter/tree/main/models/image_encoder
- IPADAPTER+IPADAPTER PLUS: https://huggingface.co/h94/IP-Adapter/tree/main/models

## Plugin to Install: #To-Do 

#To-Do thinkdiffusion
#To-Do explanations for small/quick models vs big/quality models and various graphics cards thoughts.
