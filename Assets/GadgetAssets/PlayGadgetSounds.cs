using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO remove all "using" that aren't being used in a script

// TODO maybe instead of a special gadgetsounds library, we need a unified library for ALL sounds in the game?

public class PlayGadgetSounds : MonoBehaviour
{
    public string GadgetAudioClipFolder = "Sounds/SFX/GadgetSounds";
    private GeneralGameLibraries.AudioClipsLibrary AudioClipsLibrary;

    public AudioSource audioSource;

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

        if (AudioClipsLibrary.AudioClips.ContainsKey(sound)) {
            audioSource.PlayOneShot(AudioClipsLibrary.AudioClips[sound]);
        }        
    }
}
