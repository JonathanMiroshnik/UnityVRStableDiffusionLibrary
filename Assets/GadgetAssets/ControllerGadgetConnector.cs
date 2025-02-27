using UnityEngine;

public class ControllerGadgetConnector : MonoBehaviour
{
    public Gadget gadget;

    // Start is called before the first frame update
    void Start()
    {
        if (gadget == null) Debug.LogError($"Add Gadget to the { gameObject.name } Controller Gadget Connector");
    }
}
