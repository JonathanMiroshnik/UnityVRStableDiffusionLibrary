using UnityEngine;
using System.Collections;  // Required for IEnumerator

public class CameraGroundCollision : MonoBehaviour
{
    public GameObject socketInteractable; // Reference to the socket on the necklace
    private bool isDragging = false;

    /// <summary>
    /// Camera starts dragging to the socket when it collides with the ground.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Tag the ground as "Ground"
        {
            ActivateSocket();
        }
    }

    void ActivateSocket()
    {
        if (!isDragging)
        {
            socketInteractable.SetActive(true); // Enable the socket interactable
            isDragging = true;
            StartCoroutine(DragToSocket());
        }
    }

    /// <summary>
    /// Moves the camera to the socket interactable, on the player.
    /// </summary>
    IEnumerator DragToSocket()
    {
        yield return new WaitForSeconds(1.0f);
        isDragging = true;

        float timeToMove = 2f; // Duration of the movement
        Vector3 startPosition = transform.position;
        Vector3 endPosition = socketInteractable.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition; // Snap to the final position
        isDragging = false; // Reset for future interactions
    }
}
