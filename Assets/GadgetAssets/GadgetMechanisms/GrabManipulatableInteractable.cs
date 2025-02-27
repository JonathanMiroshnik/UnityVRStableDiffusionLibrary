using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GrabManipulatableInteractable : MonoBehaviour
{
    private bool _multiSelect = false;
    private float _multiSelectStartDistance = 0f;
    private List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor> _interactorsSelecting;
    private Vector3 _currentGrabbedGOoriginalScale;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _interactorsSelecting = interactable.interactorsSelecting;
    }

    private void Update()
    {
        if (_interactorsSelecting.Count >= 2)
        {
            if (!_multiSelect)
            {
                _multiSelect = true;
                _multiSelectStartDistance = Vector3.Distance(_interactorsSelecting[0].transform.position, _interactorsSelecting[1].transform.position);
                _currentGrabbedGOoriginalScale = transform.localScale;
            }

            var curDistance = Vector3.Distance(_interactorsSelecting[0].transform.position, _interactorsSelecting[1].transform.position);
            transform.localScale = _currentGrabbedGOoriginalScale * (curDistance / _multiSelectStartDistance); // TODO: need to power of 2 this? distance vs scale

            Debug.Log("DISTANCE " + curDistance.ToString()); // TODO: delete
        }
        else
        {
            _multiSelect = false;
        }

    }
}
