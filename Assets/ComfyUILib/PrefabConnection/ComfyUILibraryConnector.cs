using UnityEngine;

// TODO: documentation
public class ComfyUILibraryConnector : MonoBehaviour
{

    public ComfyXROriginConnector comfyXROriginConnector;
    public ComfySceneParameters curParameters;    

    const string ComfyUILibraryName = "ComfyUILib";
    
    // Start is called before the first frame update
    void Start()
    {
        if (comfyXROriginConnector == null)
        {
            Debug.LogError("Add the requirements to the Universal Comfy Object");
            return;
        }

        if (curParameters == null)
        {
            Debug.LogError("Add a Comfy Scene Parameters component to the ComfyUILib Object");
            return;
        }

        // Indicates to the parameters object that the connector has loaded all the relevant parameters into it
        curParameters.LoadedConnectorParameters = true;
    }
}
