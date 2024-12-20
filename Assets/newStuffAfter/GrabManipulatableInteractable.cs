using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GrabManipulatableInteractable : MonoBehaviour
{
    bool multiSelect = false;
    float multiSelectStartDistance = 0f;
    List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> interactorsSelecting;
    Vector3 currentGrabbedGOoriginalScale;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        interactorsSelecting = interactable.interactorsSelecting;
    }

    private void Update()
    {
        if (interactorsSelecting.Count >= 2)
        {
            if (!multiSelect)
            {
                multiSelect = true;
                multiSelectStartDistance = Vector3.Distance(interactorsSelecting[0].transform.position, interactorsSelecting[1].transform.position);
                currentGrabbedGOoriginalScale = transform.localScale;
            }

            var curDistance = Vector3.Distance(interactorsSelecting[0].transform.position, interactorsSelecting[1].transform.position);
            transform.localScale = currentGrabbedGOoriginalScale * (curDistance / multiSelectStartDistance); // TODO need to power of 2 this? distance vs scale

            Debug.Log("DISTANCE " + curDistance.ToString()); // TODO delete
        }
        else
        {
            multiSelect = false;
        }

    }
}
