using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComfyXROriginConnector : MonoBehaviour
{
    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    public Gadget gadget;
    public List<GadgetMechanism> gadgetMechanisms;

    // Start is called before the first frame update
    void Start()
    {
        if (gadgetMechanisms != null) {
            gadget.GadgetMechanisms = gadgetMechanisms;
        }
    }

    public void AddMechanism(GadgetMechanism mechanism)
    {
        if (gadgetMechanisms != null && mechanism != null)
        {
            gadgetMechanisms.Add(mechanism);
            gadget.GadgetMechanisms = gadgetMechanisms;
        }
    }
}
