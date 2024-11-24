using System.Collections;
using UnityEngine;

// TODO documentation

public class OpenDoorScript : MonoBehaviour
{
    public Animator doorOpenAnimation;
    
    public AudioSource audioSource;

    private bool doorOpened = false;
    
    
    
    
    public void OpenDoor()
    {
        if (doorOpened)
        {
            return;
        }
        
        if (audioSource == null)
        {
            Debug.Log("Add a Audio Source to open door");
            return;
        }

        if (GameManager.getInstance() == null)
        {
            Debug.Log("Game manager null!");
            return;
        }

        GetComponent<BoxCollider>().enabled = false;
        doorOpenAnimation.enabled = true;
        doorOpenAnimation.Play("Door Open Clip 3");
        audioSource.Play();

        doorOpened = true;
    }
    
}
