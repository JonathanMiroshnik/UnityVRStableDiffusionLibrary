using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySounds : MonoBehaviour
{
    public string GadgetAudioClipFolder = "Sounds/SFX/GadgetSounds";
    public AudioSource audioSource;

    private GeneralGameLibraries.AudioClipsLibrary AudioClipsLibrary;

    private void Awake()
    {
        AudioClipsLibrary = new GeneralGameLibraries.AudioClipsLibrary(GadgetAudioClipFolder);
    }

    public void PlaySound(string sound)
    {
        if (audioSource == null)
        {
            Debug.Log("Add a Audio Source to Gadget Sounds");
            return;
        }

        if (AudioClipsLibrary.AudioClips.ContainsKey(sound))
        {
            audioSource.PlayOneShot(AudioClipsLibrary.AudioClips[sound]);
        }
    }
}
