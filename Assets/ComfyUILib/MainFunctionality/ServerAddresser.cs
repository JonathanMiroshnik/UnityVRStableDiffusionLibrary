using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://  ------ When using online API service | http:// ------ When using offline server
public enum HTTPPrefix
{
    HTTP,
    HTTPS
}

// TODO:: in ComfyOrganizer I added List of outgioing image names

/// <summary>
/// Responsible for loading and holding a server address
/// </summary>
public class ServerAddresser : MonoBehaviour
{
    // Address of the server
    public string ServerAddress = "127.0.0.1:8188";
    // Prefix of the server address
    public HTTPPrefix HTTPPrefix;
    // Dictionary of the prefixes of the server address
    public Dictionary<HTTPPrefix, string> HTTPPrefixes = new Dictionary<HTTPPrefix, string>()
    {
        {HTTPPrefix.HTTP, "http://"},
        {HTTPPrefix.HTTPS, "https://"}
    };

    // Prefix of the server address, when connecting to ThinkDiffusion
    public const string ThinkDiffusionPrefix = "jonathanmiroshnik-";
    // Postfix of the server address, when connecting to ThinkDiffusion
    public const string ThinkDiffusionPostfix = ".thinkdiffusion.xyz";

    private void Awake()
    {
        LoadSpecialServerAddress(ServerAddress);
    }

    /// <summary>
    /// Loads the special server address, when connecting to ThinkDiffusion/locally
    /// </summary>
    /// <param name="initialIP">The initial IP address</param>
    public void LoadSpecialServerAddress(string initialIP)
    {
        if (initialIP == "" || initialIP == "127.0.0.1:8188")
        {
            ServerAddress = "127.0.0.1:8188";
            Debug.Log("No unique server IP set, setting default: " + ServerAddress);
        }
        else
        {
            ServerAddress = ThinkDiffusionPrefix + initialIP + ThinkDiffusionPostfix;
            Debug.Log("Set the final server IP as: " + ServerAddress);
        }
    }

    /// <summary>
    /// Gets the server address
    /// </summary>
    /// <returns>The server address</returns>
    public string GetServerAddress()
    {
        return HTTPPrefixes[HTTPPrefix] + ServerAddress;
    }
}
