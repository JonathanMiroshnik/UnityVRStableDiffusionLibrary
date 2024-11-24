using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;


/// <summary>
/// Used to create audio reactive effects.
/// </summary>

[RequireComponent (typeof(AudioSource))]
public class AudioReact : MonoBehaviour
{
    // Audio source from on which the reaction occurs
    public AudioSource audioSource;

    // Number of audio channels that are extracted from the audio source
    private const int NUM_CHANNELS = 512;

    // Audio channels gotten from the audio source
    [NonSerialized]
    public float[] samples = new float[NUM_CHANNELS];

    // Keeps track whether the audio went above the given threshold at a certain moment
    [NonSerialized]
    public bool wentOverThreshold = false;

    // Audio reaction threshold
    public float threshold = 1.2f;

    // TODO continue documentation
    private float avg = 0f;
    private float rollingAvg = 0f;

    [Range(0, NUM_CHANNELS - 1)]
    public int MaximalChannel;
    [Range(0, NUM_CHANNELS-1)]
    public int MinimalChannel;    

    [Range(0f, 1f)]
    public float rollingAvgAlpha = 0.5f;

    private void OnValidate()
    {
        if (MinimalChannel > MaximalChannel)
        {
            Debug.LogError("The Minimal Channel cannot be higher than the Maximal Channel");
            MinimalChannel = MaximalChannel;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null) return;

        GetSpectrumAudioSource ();
        var avgSamples = samples.Skip(MinimalChannel).Take(MaximalChannel - MinimalChannel).ToArray();
        avg = avgSamples.Average();

        rollingAvg = rollingAvgAlpha * rollingAvg + (1 - rollingAvgAlpha) * avg;

        if (rollingAvg > threshold * avg)
        {
            wentOverThreshold = true;
        }
        else
        {
            wentOverThreshold = false;
        }
    }

    void GetSpectrumAudioSource()
    {
        if (audioSource == null) return;

        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }
}
