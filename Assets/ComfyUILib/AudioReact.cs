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
    private const int NumChannels = 512;

    // Audio channels gotten from the audio source
    [NonSerialized]
    public float[] samples = new float[NumChannels];

    // Keeps track whether the audio went above the given threshold at a certain moment
    [NonSerialized]
    public bool wentOverThreshold = false;

    // Audio reaction threshold
    public float threshold = 1.2f;

    [Range(0, NumChannels - 1)]
    public int MaximalChannel;
    [Range(0, NumChannels-1)]
    public int MinimalChannel;    

    [Range(0f, 1f)]
    public float rollingAvgAlpha = 0.5f;

    // TODO: continue documentation
    private float _avg = 0f;
    private float _rollingAvg = 0f;

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
        _avg = avgSamples.Average();

        _rollingAvg = rollingAvgAlpha * _rollingAvg + (1 - rollingAvgAlpha) * _avg;

        if (_rollingAvg > threshold * _avg)
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
