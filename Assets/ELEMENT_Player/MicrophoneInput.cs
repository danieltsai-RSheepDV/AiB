using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MicrophoneInput : MonoBehaviour
{
    public int sampleCount = 64;
    
    private AudioClip _micClip;
    private RawLowPassFilter lowPassFilter;
    private RawHighPassFilter highPassFilter;
    
    // Start is called before the first frame update
    void Start()
    {
        _micClip = Microphone.Start(null, true, 10, 44100);
        
        lowPassFilter = new RawLowPassFilter(1000.0, AudioSettings.outputSampleRate);
        highPassFilter = new RawHighPassFilter(1200.0f, AudioSettings.outputSampleRate);  // 100 Hz cutoff frequency

        normalCutoff = PlayerPrefs.GetFloat("NormalCutoff");
        highPassCutoff = PlayerPrefs.GetFloat("HighPassCutoff");
        
        Debug.Log(normalCutoff);
        Debug.Log(highPassCutoff);
    }

    public float normalCutoff;
    public float highPassCutoff;

    public float lerpVelocity = 0;
    public float breathValue = 0;
    
    // Update is called once per frame
    void Update()
    {
        float[] micData = lowPassFilter.ApplyFilter(GetMicrophoneData());
        float[] highPassData = highPassFilter.ApplyFilter(GetMicrophoneData());
        if (Average(micData) > normalCutoff)
        {
            if (ZCR(highPassData) > highPassCutoff)
            {
                lerpVelocity += Time.deltaTime * 4f;
            }
            else
            {
                lerpVelocity -= Time.deltaTime;
            }

            breathValue += lerpVelocity * Time.deltaTime;
            if (breathValue > 1 || breathValue < 0)
            {
                lerpVelocity = 0;
            }
            breathValue = Mathf.Clamp(breathValue, 0, 1);
        }

        lerpVelocity *= 0.99f;
    }

    private float Average(float[] array)
    {
        float sum = 0;
        foreach (float f in array)
        {
            sum += Mathf.Abs(f);
        }
        sum /= array.Length;
        
        return sum;
    }

    private int ZCR(float[] array)
    {
        int counter = 0;
        for (int i = 0; i < array.Length - 1; i++)
        {
            if (!Mathf.Approximately(Mathf.Sign(array[i]), Mathf.Sign(array[i + 1])))
            {
                counter++;
            }
        }

        return counter;
    }

    public float[] GetMicrophoneData()
    {
        float[] waveData = new float[sampleCount];
        
        int startPosition = Microphone.GetPosition(Microphone.devices[0]) - sampleCount;
        
        if (startPosition < 0)
        {
            return waveData;
        }
        
        _micClip.GetData(waveData, startPosition);
        
        return waveData;
    }
}
