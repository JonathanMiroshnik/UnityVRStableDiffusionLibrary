using System;
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
    public float[] Samples = new float[NumChannels];

    // Keeps track whether the audio went above the given threshold at a certain moment
    [NonSerialized]
    public bool WentOverThreshold = false;

    // Audio reaction threshold
    public float Threshold = 1.2f;

    // Maximal channel
    [Range(0, NumChannels - 1)]
    public int MaximalChannel;

    // Minimal channel
    [Range(0, NumChannels-1)]
    public int MinimalChannel;    

    // Rolling average alpha, the higher the value the less the average is affected by the current sample
    [Range(0f, 1f)]
    public float RollingAvgAlpha = 0.5f;

    // Current verage of the samples
    private float _avg = 0f;

    // Rolling average of the samples
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

        // Get the spectrum data from the audio source
        audioSource.GetSpectrumData(Samples, 0, FFTWindow.Blackman);

        // Get the average of the samples
        var avgSamples = Samples.Skip(MinimalChannel).Take(MaximalChannel - MinimalChannel).ToArray();
        _avg = avgSamples.Average();

        // Update the rolling average
        _rollingAvg = RollingAvgAlpha * _rollingAvg + (1 - RollingAvgAlpha) * _avg;

        // Check if the rolling average is greater than the threshold times the average
        if (_rollingAvg > Threshold * _avg)
        {
            WentOverThreshold = true;
        }
        else
        {
            WentOverThreshold = false;
        }
    }
}
