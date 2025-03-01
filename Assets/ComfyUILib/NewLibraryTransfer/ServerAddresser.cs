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
    public string ServerAddress = "127.0.0.1:8188";
    public HTTPPrefix HTTPPrefix;
    
    public const string ThinkDiffusionPrefix = "jonathanmiroshnik-";
    public const string ThinkDiffusionPostfix = ".thinkdiffusion.xyz";

    public Dictionary<HTTPPrefix, string> HTTPPrefixes = new Dictionary<HTTPPrefix, string>()
    {
        {HTTPPrefix.HTTP, "http://"},
        {HTTPPrefix.HTTPS, "https://"}
    };

    private void Awake()
    {
        LoadSpecialServerAddress(ServerAddress);
    }

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

    public string GetServerAddress()
    {
        // Debug.Log("Getting server address: " + HTTPPrefixes[HTTPPrefix] + ServerAddress);
        return HTTPPrefixes[HTTPPrefix] + ServerAddress;
    }
}
