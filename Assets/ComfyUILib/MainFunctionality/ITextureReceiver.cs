using UnityEngine;

/// <summary>
/// Interface for objects that can receive textures from DiffusionRequests
/// </summary>
public interface ITextureReceiver
{
    /// <summary>
    /// Receives a texture from a DiffusionRequest
    /// </summary>
    /// <param name="texture">The texture to receive</param>
    /// <returns>True if the texture was successfully received and processed</returns>
    public bool ReceiveTexture(Texture2D texture);

    /// <summary>
    /// Receives textures from a DiffusionRequest
    /// </summary>
    /// <param name="diffusionRequest">The DiffusionRequest containing the textures</param>
    /// <returns>True if the textures were successfully received and processed</returns>
    public bool ReceiveTexturesFromDiffusionRequest(DiffusionRequest diffusionRequest);
}
