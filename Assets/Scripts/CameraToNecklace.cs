using System.Collections;
using UnityEngine;

/// <summary>
/// Moves a physical-virtual camera to the necklace attached to the player
/// Generally activated after the camera is dropped(as of 23.11.24)
/// </summary>
public class CameraToNecklace : MonoBehaviour
{
    // Speed of movement of the Camera
    public float speed = 0.05f;

    // Time before the movement starts after the action that started the movement
    public float delay = 1.0f;

    // Indicates that the Camera is currently moving towards the necklace
    public bool returning = false;

    // Necklace socket interactable, the final position of the Camera
    public GameObject socketInteractable;

    // Update is called once per frame
    void Update()
    {
        if (returning && socketInteractable != null)
        {
            // Duration of the movement
            Vector3 startPosition = transform.position;
            Vector3 endPosition = socketInteractable.transform.position;

            transform.position = Vector3.Lerp(startPosition, endPosition, speed);
        }

    }

    /// <summary>
    /// Stops the movement of the Camera
    /// </summary>
    public void StopSummoning()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.useGravity = true;
        }

        returning = false;
    }

    public void SummonBack()
    {
        StartCoroutine(DragToSocket());
    }

    /// <summary>
    /// Starts the movement of the Camera towards the necklace
    /// </summary>
    IEnumerator DragToSocket()
    {
        yield return new WaitForSeconds(delay);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null )
        {
            body.useGravity = false;
        }

        returning = true;

        yield return null;

    }
}
