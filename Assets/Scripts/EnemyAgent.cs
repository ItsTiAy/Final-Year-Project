using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyAgent : Agent
{
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log(actions.DiscreteActions[0]);
        //base.OnActionReceived(actions);
    }
    /*
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }
    */
}