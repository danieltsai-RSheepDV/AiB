using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirTotem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    [SerializeField] private MicrophoneInput mic;
    private bool breathingPaused = false;
    private bool isInhaling = true;
    private float threshold = 0.2f;
    private float airMeter = 0;
    private float maxAir = 10f;
    private int stage = 0;
    
    private void BreathingTracker()
    {
        if (stage == 0)
        {
            Debug.Log("0");
            if (mic.breathValue < threshold)
            {
                airMeter = 4f;
                stage = 1;
                return;
            }
        }else if (stage == 1)
        {
            Debug.Log("1");
            if (mic.breathValue > 1 - threshold)
            {
                airMeter = 4f;
                stage = 2;
                return;
            }else if (airMeter < 0f)
            {
                stage = 0;
            }
        }else if (stage == 2)
        {
            Debug.Log("2");
            if (mic.breathValue > 1 - threshold)
            {
                if (airMeter < 0)
                {
                    airMeter = 4f;
                    stage = 3;
                    return;
                }
            }else
            {
                stage = 0;
            }
        }
        else
        {
            Debug.Log("3");
            if (mic.breathValue < threshold)
            {
                gameObject.SetActive(false);
                return;
            }else if (airMeter < 0f)
            {
                stage = 4;
            }
        }

        airMeter -= Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BreathingTracker();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            stage = 0;
        }
    }
}
