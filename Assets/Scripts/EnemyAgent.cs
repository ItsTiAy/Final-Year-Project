using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyAgent : Agent
{
    private Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    private float reloadSpeed = 5f;
    private bool reloading = false;

    public GameObject targetPrefab;
    private GameObject target;

    public void Start()
    {
        rb = transform.GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        //LevelManager.instance.ReloadCurrentLevel();
        LevelManager.instance.LoadLevelTraining(10);

        int randX;
        int randY;

        do
        {
            randX = Random.Range(-11, 11);
            randY = Random.Range(-6, 7);
        }
        while (!Pathfinding.nodes[new Vector2Int(randX, randY)].IsWalkable());


        transform.position = new Vector3(randX + 0.5f, randY + 0.5f);
        GameController.instance.enemies[0].transform.position = new Vector3(-randX + 0.5f, -randY + 0.5f);


        //Debug.Log(GameController.instance.enemies.Count);
        //target.transform.GetComponent<Enemy>().GenerateNewPath();
        //targetTransform = GameObject.FindGameObjectsWithTag("Enemy")[0].transform;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.DiscreteActions[0];
        float moveY = actions.DiscreteActions[1];

        Vector2 movement = Vector2.zero;

        switch (moveX)
        {
            case 0: 
                movement.x = 0;
                break;
            case 1: 
                movement.x = -1;
                break;
            case 2: 
                movement.x = 1;
                break;
        }

        switch (moveY)
        {
            case 0:
                movement.y = 0;
                break;
            case 1:
                movement.y = -1;
                break;
            case 2:
                movement.y = 1;
                break;
        }

        float moveSpeed = 2f;
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);

        float rot = actions.DiscreteActions[2];
        
        int rotation = 0;

        switch (rot)
        {
            case 0:
                rotation = 0;
                break;
            case 1:
                rotation = -1;
                break;
            case 2:
                rotation = 1;
                break;
        }
        
        float rotationSpeed = 100f;

        turret.Rotate(Vector3.forward * rotationSpeed * Time.fixedDeltaTime * rotation);

        //Quaternion deltaRotation = Quaternion.Euler(rotation * Time.fixedDeltaTime);

        //turretRb.GetComponent<Rigidbody2D>().MoveRotation(turretRb.rotation * rot);

        bool fire = actions.DiscreteActions[3] == 1;

        if (fire && !reloading)
        {
            Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation/*, GameController.instance.bulletContainer*/);
            StartCoroutine(Reload(reloadSpeed));
        }

        //Debug.Log(actions.ContinuousActions[2]);

    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(GameController.instance.enemies[0].transform.position);
        sensor.AddObservation(reloading);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")))
        {
            case -1:
                discreteActions[0] = 1;
                break;

            case 0:
                discreteActions[0] = 0;
                break;
            case 1:
                discreteActions[0] = 2;
                break;
        }

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")))
        {
            case -1:
                discreteActions[1] = 1;
                break;

            case 0:
                discreteActions[1] = 0;
                break;
            case 1:
                discreteActions[1] = 2;
                break;
        }

        if (Input.GetKey(KeyCode.Comma))
        {
            discreteActions[2] = 2;
        }
        else if (Input.GetKey(KeyCode.Period))
        {
            discreteActions[2] = 1;
        }
        else
        {
            discreteActions[2] = 0;
        }


        discreteActions[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private IEnumerator Reload(float waitTime)
    {
        reloading = true;

        yield return new WaitForSeconds(waitTime);

        reloading = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            RestartEpisode(-1);
        }
    }

    public void RestartEpisode(float reward)
    {
        SetReward(reward);
        StopAllCoroutines();
        reloading = false;
        EndEpisode();
    }
}