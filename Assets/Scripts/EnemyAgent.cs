using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using static LevelManager;
using System.IO;

public class EnemyAgent : Agent
{
    private Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    private float reloadSpeed = 5f;
    private bool reloading = false;

    //public GameObject targetPrefab;
    public GameObject target;
    public GameObject levelContainer;
    public Transform bulletContainer;

    BufferSensorComponent m_BufferSensor;

    //private int maxSteps = 500000;

    // Switch stuff to local positions so that you can train more than 1 guy at a time

    public void Start()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        m_BufferSensor = GetComponent<BufferSensorComponent>();

        if (GetComponent<BehaviorParameters>().TeamId == 0)
        {
            Pathfinding.Initialize();

            string path = Application.streamingAssetsPath + "/Levels/level" + 10 + ".json";
            string jsonString = File.ReadAllText(path);
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonString);

            // Loops for each type of tile in the level
            for (int i = 0; i < levelData.tiles.Count; i++)
            {
                TileData tileData = levelData.tiles[i];

                for (int j = 0; j < tileData.tilePositionsX.Count; j++)
                {
                    Pathfinding.nodes[new Vector2Int(tileData.tilePositionsX[j], tileData.tilePositionsY[j])].SetIsWalkable(false);
                }
            }
            Pathfinding.nodes[Vector2Int.zero].SetIsWalkable(false);
        }
    }

    private void FixedUpdate()
    {
        float currentRayDistance;
        float currentTotalDistance = 0;

        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);

        for (int i = 0; i < 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100);

            currentRayDistance = Mathf.Min(hit.distance, (8 * 5) - currentTotalDistance);
            currentTotalDistance += currentRayDistance;

            Debug.DrawRay(ray.origin, ray.direction * currentRayDistance, Color.green);

            if (hit)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);
                    ray = new Ray2D(hit.point + (newDirection * 0.0001f), newDirection);
                }
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        //LevelManager.instance.ReloadCurrentLevel();
        //LevelManager.instance.LoadLevelTraining(10);
        
        MaxStep = 10000;

        if (GetComponent<BehaviorParameters>().TeamId == 0)
        {
            /*
            int randX;
            int randY;

            do
            {
                randX = Random.Range(-11, 11);
                randY = Random.Range(-6, 7);

                //randX = 10;
                //randY = -6;
            }
            while (!Pathfinding.nodes[new Vector2Int(randX, randY)].IsWalkable());
            */
            //float xPos = randX + 0.5f;
            //float yPos = randY + 0.5f;

            //transform.localPosition = new Vector3(randX + 0.5f, randY + 0.5f);
            //target.transform.localPosition = new Vector3(-(randX + 0.5f), -randY + 0.5f);

            transform.localPosition = new Vector3(Random.Range(-11, 11) + 0.5f, Random.Range(-6, 7) + 0.5f);
            target.transform.localPosition = new Vector3(Random.Range(-11, 11) + 0.5f, Random.Range(-6, 7) + 0.5f);

            //Debug.Log(transform.localPosition);
            //Debug.Log(target.transform.localPosition);
        }

        turret.rotation = Quaternion.identity;

        //GameController.instance.enemies[0].transform.localPosition = new Vector3(-randX + 0.5f, -randY + 0.5f);


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
            //Debug.Log(GetComponent<BehaviorParameters>().TeamId);
            Debug.Log("Fired bullet");

            var b = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, bulletContainer);
            b.GetComponent<Bullet>().bulletId = GetComponent<BehaviorParameters>().TeamId;
            StartCoroutine(Reload(reloadSpeed));
        }

        //Debug.Log(actions.ContinuousActions[2]);

        AddReward(-1f / MaxStep);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("wombat");

        Vector2 directionToTarget = (target.transform.localPosition - transform.localPosition).normalized;

        //sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.transform.localPosition);

        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.y); 

        sensor.AddObservation(reloading ? 1 : 0);

        sensor.AddObservation(turret.rotation);


        
        // Collect observation about the 20 closest Bullets
        var bullets = bulletContainer.GetComponentsInChildren<Bullet>();
        // Sort by closest :
        //System.Array.Sort(bullets, (a, b) => (Vector3.Distance(a.transform.position, transform.position)).CompareTo(Vector3.Distance(b.transform.position, transform.position)));
        int numBulletAdded = 0;

        // foreach (Bullet b in bullets)
        // {
        //     b.transform.localScale = new Vector3(1, 1, 1);
        // }

        foreach (Bullet b in bullets)
        {
            if (numBulletAdded >= 10)
            {
                break;
            }

            //Vector2 directionToBullet = (b.transform.localPosition - transform.localPosition).normalized;

            float[] bulletObservation = new float[]
            {
                b.transform.localPosition.x,
                b.transform.localPosition.y
            };
            numBulletAdded += 1;

            m_BufferSensor.AppendObservation(bulletObservation);

            // b.transform.localScale = new Vector3(2, 2, 2);
        };
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
            /*
            if (collision.gameObject.GetComponent<Bullet>().bulletId == GetComponent<BehaviorParameters>().TeamId)
            {
                Debug.Log("Hit self");
                target.GetComponent<EnemyAgent>().RestartEpisode(1f);
                RestartEpisode(-1f);
            }
            else
            {
                Debug.Log("Hit target");
                target.GetComponent<EnemyAgent>().RestartEpisode(1f);
                RestartEpisode(-1f);
            }
            */

            Debug.Log("Hit");

            //AddReward(1f);
            //RestartEpisode();
            //target.GetComponent<EnemyAgent>().SetReward(-1f);
            //target.GetComponent<EnemyAgent>().RestartEpisode();


            foreach (Transform bulletObject in bulletContainer)
            {
                Destroy(bulletObject.gameObject);
            }
        }
    }

    public void RestartEpisode()
    {
        StopAllCoroutines();
        reloading = false;
        EndEpisode();
    }

    /*
      self_play:
      window: 10
      play_against_latest_model_ratio: 0.5
      save_steps: 20000
      swap_steps: 10000
      team_change: 100000
     */
}