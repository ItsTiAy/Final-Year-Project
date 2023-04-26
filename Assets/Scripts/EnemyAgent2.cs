using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class EnemyAgent2 : Agent
{
    public Rigidbody2D rb;
    public Rigidbody2D bullet;
    public Rigidbody2D target;

    public Transform bulletSpawn;
    public Transform turret;
    public Transform bulletContainer;

    public float moveSpeed;
    public float reloadSpeed;
    public float existential;

    public bool reloading = false;

    public AgentManager agentManager;
    public BufferSensorComponent m_BufferSensor;


    public override void Initialize()
    {
        // Sets the existential value 
        existential = 1 / MaxStep;
    }

    public void ControlAgent(ActionSegment<int> actions)
    {
        Vector3 rotateDir = Vector3.zero;

        float moveX = actions[0];
        float moveY = actions[1];
        float rotate = actions[2];
        bool fire = actions[3] == 1;

        Vector2 movement = Vector2.zero;

        switch (moveX)
        {
            case 1:
                movement.x = -1;
                break;
            case 2:
                movement.x = 1;
                break;
        }

        switch (moveY)
        {
            case 1:
                movement.y = -1;
                break;
            case 2:
                movement.y = 1;
                break;
        }

        switch (rotate)
        {
            case 1:
                rotateDir = transform.forward * -1f;
                break;
            case 2:
                rotateDir = transform.forward * 1f;
                break;
        }

        // Rotates the turret based on the actions received
        turret.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
        // Moves the agent based on the actions received
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);
        // Fires based on the actions received
        if (fire && !reloading)
        {
            Rigidbody2D b = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, bulletContainer);
            b.GetComponent<Bullet>().bulletId = GetComponent<BehaviorParameters>().TeamId;
            StartCoroutine(Reload(reloadSpeed));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 directionToTarget = (target.transform.localPosition - transform.localPosition).normalized;

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.localPosition);
        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.y);
        sensor.AddObservation(reloading ? 1 : 0);
        sensor.AddObservation(turret.rotation.normalized);

        var bullets = bulletContainer.GetComponentsInChildren<Bullet>();

        int numBulletAdded = 0;

        // Observations for the bullets currently in the scene
        foreach (Bullet b in bullets)
        {
            // Max of 10 bullets added to the observations at a time
            if (numBulletAdded >= 10)
            {
                break;
            }

            // Gets the direction the bullet is in from the agent's current position and the direction the bullet is going in
            float[] bulletObservation = new float[]
            {
                (b.transform.localPosition - transform.localPosition).normalized.x,
                (b.transform.localPosition - transform.localPosition).normalized.y,
                b.transform.forward.x,
                b.transform.forward.z
            };
            numBulletAdded += 1;

            m_BufferSensor.AppendObservation(bulletObservation);
        };
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Small negative reward over time
        AddReward(-existential);
        ControlAgent(actions.DiscreteActions);
    }

    // Used to test that the actions work properly
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 2;
        }

        if (Input.GetKey(KeyCode.S))
        {
            discreteActions[1] = 1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[1] = 2;
        }

        if (Input.GetKey(KeyCode.Period))
        {
            discreteActions[2] = 1;
        }

        if (Input.GetKey(KeyCode.Comma))
        {
            discreteActions[2] = 2;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[3] = 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            agentManager.TargetHit(1);
        }
    }

    private IEnumerator Reload(float waitTime)
    {
        reloading = true;

        yield return new WaitForSeconds(waitTime);

        reloading = false;
    }
}
