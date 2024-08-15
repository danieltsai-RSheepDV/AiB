using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MicrophoneSetup : MonoBehaviour
{
    public int sampleCount = 64;
    
    private AudioClip _micClip;
    private RawLowPassFilter lowPassFilter;
    private RawHighPassFilter highPassFilter;
    
    // Start is called before the first frame update
    void Awake()
    {
        inputActionSet = new AiBInput();
        jumpAction = inputActionSet.Player.Jump;
        
        _micClip = Microphone.Start(null, true, 10, 44100);
        
        lowPassFilter = new RawLowPassFilter(1000.0, AudioSettings.outputSampleRate);
        highPassFilter = new RawHighPassFilter(1200.0f, AudioSettings.outputSampleRate);  // 100 Hz cutoff frequency
    }

    private void OnEnable()
    {
        // Enable the PlayerInput actions
        inputActionSet.Enable();
    }

    private void OnDisable()
    {
        // Disable the PlayerInput actions
        inputActionSet.Disable();
    }
    
    public bool normalTriggered;
    public bool highPassTriggered;

    public float normalCutoff;
    public float highPassCutoff;
    public List<float> highPassCutoff1;
    public List<float> highPassCutoff2;
    
    private AiBInput inputActionSet;
    private InputAction jumpAction;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image circle;
    
    private float timer = 0;
    private int stage = 0;
    private int breathCount = 0;
    
    // Update is called once per frame
    void Update()
    {
        float[] micData = lowPassFilter.ApplyFilter(GetMicrophoneData());
        float[] highPassData = highPassFilter.ApplyFilter(GetMicrophoneData());
        timer -= Time.deltaTime;
        switch (stage)
        {
            case 0:
                if (jumpAction.WasPressedThisFrame())
                {
                    timer = 11;
                    stage = 1;
                    text.text = "Make no sound until calibration is done.";
                }
                break;
            case 1:
                if (timer > 0 && timer < 10)
                {
                    normalCutoff = Mathf.Max(normalCutoff, Average(micData));
                }else if (timer < 0)
                {
                    stage = 2;
                    text.text = "Calibration done! Next, Breathe in for 4 seconds whenever you see an O. Breathe out for 4 seconds whenever you see an X. Press Space to Continue.";
                }
                break;
            case 2:
                if (jumpAction.WasPressedThisFrame())
                {
                    timer = 4;
                    stage = 3;
                    breathCount = 0;
                    text.text = "O";
                }
                break;
            case 3:
                switch (breathCount)
                {
                    case 0:
                        if (timer > 0)
                        {
                            if (Average(micData) > normalCutoff)
                            {
                                highPassCutoff1.Add(ZCR(highPassData));
                            }
                        } else
                        {
                            breathCount = 1;
                            timer = 4f;
                            text.text = "X";
                        }
                        break;
                    case 1:
                        if (timer > 0)
                        {
                            if (Average(micData) > normalCutoff)
                            {
                                // highPassCutoff1.Add(Average(highPassData));
                            }
                        } else
                        {
                            breathCount = 2;
                            timer = 4f;
                            text.text = "O";
                        }
                        break;
                    case 2:
                        if (timer > 0)
                        {
                            if (Average(micData) > normalCutoff)
                            {
                                highPassCutoff1.Add(ZCR(highPassData));
                            }
                        } else
                        if (timer < 0)
                        {
                            breathCount = 3;
                            timer = 4f;
                            text.text = "X";
                        }
                        break;
                    case 3:
                        if (timer > 0)
                        {
                            if (Average(micData) > normalCutoff)
                            {
                                // highPassCutoff1.Add(Average(highPassData));
                            }
                        } else
                        {
                            stage = 4;
                            highPassCutoff = Average(highPassCutoff1.ToArray());
                            PlayerPrefs.SetFloat("NormalCutoff", normalCutoff);
                            PlayerPrefs.SetFloat("HighPassCutoff", highPassCutoff);
                            text.text =
                                "Calibration complete! Breathe to test the detection. You can press Space to start the game!";
                        }
                        break;
                }
                break;
            case 4:
                if (Average(micData) > normalCutoff)
                {
                    if (ZCR(highPassData) < highPassCutoff)
                    {
                        circle.transform.localScale += Vector3.one * Time.deltaTime;
                    }
                    else
                    {
                        circle.transform.localScale -= Vector3.one * Time.deltaTime;
                    }
                }
                break;
        }
        // float[] highPassData = highPassFilter.ApplyFilter(GetMicrophoneData());
        // else
        // {
        //     normalTriggered = Average(micData) > normalCutoff;
        //     highPassTriggered = Average(highPassData) > highPassCutoff;
        // }
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
