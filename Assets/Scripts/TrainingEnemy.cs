using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Copy of the enemy script adjusted for training the agent
public class TrainingEnemy : MonoBehaviour
{
    public const int maxRadius = 5;
    [Range(-10, 10)]
    public int aggression = 0;
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private int health = 1;
    [SerializeField]
    private float rotationSpeed = 1f;
    [SerializeField]
    private float reloadSpeed = 5f;
    [SerializeField]
    private float fireChance = 1f;
    [SerializeField]
    private float turretRotateAngle = 3f;
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private bool laysMines = false;

    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    public SecondaryItem secondary;
    public LayerMask layerMask;
    //public List<Player> players;

    public GameObject target;
    public Transform bulletContainer;
    public AgentManager agentManager;
    public Transform trainingLevel;

    private Bullet bulletClass;
    private float maxBulletDistance;
    private Quaternion newRotation;

    public bool reloading = false;
    private bool secondaryCooldown = false;

    private int currentNode = 0;
    private List<Pathfinding.Node> path;

    private Quaternion q = Quaternion.identity;

    private void Start()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;

        newRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        StartCoroutine(SecondaryReload(Random.Range(2, 5)));
    }

    private void FixedUpdate()
    {
        Aim();
        FollowPath();

        if (moveSpeed > 0)
        {
            // Checks if the current positon is equal to the current path node's position
            if ((Vector2) transform.localPosition == new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f))
            {
                // Generates new path when reaches the end of the path
                if (currentNode == path.Count - 1)
                {
                    bool validPath = false;

                    while (!validPath)
                    {
                        validPath = true;

                        do
                        {
                            GeneratePath(ChooseNewPosition());
                        }
                        while (path == null);
                    }
                }
                else
                {
                    currentNode++;
                }
            }

            // Draws a debug line for the current path that has been generated
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(new Vector2(path[i].x + 0.5f, path[i].y + 0.5f) + (Vector2)trainingLevel.position, new Vector2(path[i + 1].x + 0.5f, path[i + 1].y + 0.5f) + (Vector2)trainingLevel.position, Color.white);
                }
            }
        }

        Thinking();

        if (laysMines && !secondaryCooldown)
        {
            LayMine();

            StartCoroutine(SecondaryReload(Random.Range(5, 10)));
        }
    }

    private Vector2Int ChooseNewPosition()
    {
        Vector2Int newPos;
        bool validPos = false;

        // Loops until a valid position has been found
        do
        {
            GameObject closestPlayer = target;

            // Direction towards the closest player as a vector
            Vector2 direction = (closestPlayer.transform.localPosition - transform.localPosition);

            // The angle of the the direction vector
            float theta = Mathf.Atan2(direction.y, direction.x);

            int radius = Random.Range(1, maxRadius + 1);

            // Chooses whether to move towards or away from the player based on the enemies agression level
            if (Random.Range(-10, 11) < aggression)
            {
                theta += Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            }
            else
            {
                theta += Random.Range(-(Mathf.PI / 2), Mathf.PI / 2) + Mathf.PI;
            }

            // The position the enemy will move towards
            newPos = Vector2Int.FloorToInt(new Vector2(transform.localPosition.x + Mathf.Cos(theta) * radius, transform.localPosition.y + Mathf.Sin(theta) * radius));

            // Checks that the chosed position is within bounds and a walkable square
            if (newPos.x < 11 && newPos.x > -12 && newPos.y < 6 && newPos.y > -7)
            {
                validPos = true;
            }
        }
        while (!validPos);

        return newPos;
    }

    public void GeneratePath(Vector2Int newPosition)
    {
        currentNode = 0;

        Vector2Int currentPos = Vector2Int.FloorToInt(transform.localPosition);

        // Generates the path from the current position to the new position
        path = Pathfinding.FindPath(currentPos.x, currentPos.y, newPosition.x, newPosition.y);

    }

    private void FollowPath()
    {
        // Moves towards the position chosen
        rb.MovePosition(Vector2.MoveTowards(rb.position, new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f) + (Vector2) trainingLevel.position, moveSpeed * Time.deltaTime));
    }

    public void Thinking()
    {

        float currentRayDistance;
        float distanceRemaining = maxBulletDistance;

        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);
        Color rayColour = Color.red;


        for (int i = 0; i < bulletClass.MaxBounces + 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, distanceRemaining);

            if (hit.distance > 0)
            {
                currentRayDistance = Mathf.Min(hit.distance, distanceRemaining);
            }
            else
            {
                currentRayDistance = distanceRemaining;
            }

            distanceRemaining -= currentRayDistance;

            Debug.DrawRay(ray.origin, ray.direction * currentRayDistance, rayColour);

            if (hit)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);
                    ray = new Ray2D(hit.point + (newDirection * 0.0001f), newDirection);
                }
                else if (hit.transform.CompareTag("Agent"))
                {
                    if (!reloading && FireProbability())
                    {
                        Fire();
                    }
                }
            }
        }
    }

    public void LayMine()
    {
        Instantiate(secondary, transform.position, Quaternion.identity, GameController.instance.mineContainer);
    }

    private void Fire()
    {
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, bulletContainer);
        StartCoroutine(Reload(reloadSpeed));
    }

    private bool FireProbability()
    {
        if (Random.value <= fireChance)
        {
            return true;
        }
        else
        {
            StartCoroutine(Reload(2));
            return false;
        }
    }

    private void Aim()
    {
        if (turret.rotation == q)
        {
            Vector3 playerPosition = target.transform.position;

            Vector3 targetDirection = playerPosition - turret.position;
            float theta = Mathf.Atan2(targetDirection.y, targetDirection.x);

            theta += Random.Range(-(turretRotateAngle / 2), turretRotateAngle / 2);

            theta *= Mathf.Rad2Deg;

            q = Quaternion.AngleAxis(theta, Vector3.forward);
        }

        turret.rotation = Quaternion.RotateTowards(turret.rotation, q, rotationSpeed);
    }

    private IEnumerator Reload(float waitTime)
    {
        reloading = true;

        yield return new WaitForSeconds(waitTime);

        reloading = false;
    }

    private IEnumerator SecondaryReload(float waitTime)
    {
        secondaryCooldown = true;

        yield return new WaitForSeconds(waitTime);

        secondaryCooldown = false;
    }

    public void DecreaseHealth()
    {
        health--;

        agentManager.TargetHit(2);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DecreaseHealth();
        }
    }

    public void GenerateNewPath()
    {
        do
        {
            GeneratePath(ChooseNewPosition());
        }
        while (path == null);
    }
}