using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class AudioTrainer : Agent
{
    [SerializeField] private MicrophoneInput mic;

    public override void OnEpisodeBegin()
    {
        Debug.Log("Reset");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (float f in mic.GetMicrophoneData())
        {
            sensor.AddObservation(f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log(actions.DiscreteActions[0]);
        if (Input.GetKey(KeyCode.A))
        {
            AddReward(actions.DiscreteActions[0] == 0 ? 0.2f : -0.1f);
        }else if (Input.GetKey(KeyCode.D))
        {
            AddReward(actions.DiscreteActions[0] == 1 ? 0.2f : -0.1f);
        }
        else
        {
            AddReward(actions.DiscreteActions[0] == 2 ? 0.2f : -0.1f);
        }
    }
}
