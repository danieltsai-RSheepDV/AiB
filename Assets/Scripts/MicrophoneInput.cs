using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip _micClip;
    
    // Start is called before the first frame update
    void Start()
    {
        MicrophoneToAudioClip();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("Correct");
        }else if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("sss");
        }
        else
        {
            Debug.Log(Time.timeScale);
        }
    }

    private void MicrophoneToAudioClip()
    {
        string micName = Microphone.devices[0];
        _micClip = Microphone.Start(micName, true, 20, AudioSettings.outputSampleRate);
    }

    public float[] GetMicrophoneData()
    {
        float[] waveData = new float[sampleWindow];
        
        int startPosition = Microphone.GetPosition(Microphone.devices[0]) - sampleWindow;
        if (startPosition < 0)
        {
            return waveData;
        }
        
        _micClip.GetData(waveData, startPosition);
        // Debug.Log(waveData[21]);

        return waveData;
    }
}
