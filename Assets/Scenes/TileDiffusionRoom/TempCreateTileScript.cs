using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class TempCreateTileScript : MonoBehaviour
{
    public GameObject diffusables;
    public Transform NewParent;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject GO in GameManager.getInstance().diffusionList)
        {
            Transform t = GO.transform;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(NewParent);
            ApplyTo(cube.transform, t);
            cube.AddComponent<XRSimpleInteractable>();

        }        
    }

    public void ApplyTo(Transform transform, Transform transformTo)
    {
        transform.localPosition = transformTo.localPosition;
        transform.localEulerAngles = transformTo.localEulerAngles;
        transform.localScale = transformTo.localScale;
    }
}
