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

        turret.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);

        if (fire && !reloading)
        {
            Rigidbody2D b = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, bulletContainer);
            b.GetComponent<Bullet>().bulletId = GetComponent<BehaviorParameters>().TeamId;
            StartCoroutine(Reload(reloadSpeed));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("wombat");

        Vector2 directionToTarget = (target.transform.localPosition - transform.localPosition).normalized;

        //Color rayColour = Color.red;
        //Debug.DrawRay(transform.position, directionToTarget * 10, Color.blue);


        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.localPosition);
        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.y);
        sensor.AddObservation(reloading ? 1 : 0);
        sensor.AddObservation(turret.rotation.normalized);

        var bullets = bulletContainer.GetComponentsInChildren<Bullet>();

        int numBulletAdded = 0;

        foreach (Bullet b in bullets)
        {
            if (numBulletAdded >= 10)
            {
                break;
            }

            //Vector2 directionToBullet = (b.transform.localPosition - transform.localPosition).normalized;

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
        AddReward(-existential);
        ControlAgent(actions.DiscreteActions);
    }

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
