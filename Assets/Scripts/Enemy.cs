using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
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

    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    public LayerMask layerMask;
    //public List<Player> players;

    private Bullet bulletClass;
    private float maxBulletDistance;
    private Quaternion newRotation;

    private bool reloading = false;
    //private bool waiting = false;
    //private bool pausing = false;

    private int currentNode = 0;
    private List<Pathfinding.Node> path;

    private Quaternion q = Quaternion.identity;


    //private Pathfinding pathfinding;

    private void Start()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;

        newRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        GeneratePath(ChooseNewPosition());
    }

    private void FixedUpdate()
    {
        Aim();
        FollowPath();

        if (moveSpeed > 0)
        {
            // Checks if the current positon is equal to the current path node's position
            if (rb.position == new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f))
            {
                // Generates new path when reaches the end of the path
                if (currentNode == path.Count - 1)
                {
                    currentNode = 0;
                    do
                    {
                        GeneratePath(ChooseNewPosition());
                    }
                    while (path == null);
                    //StartCoroutine(Pause());
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
                    Debug.DrawLine(new Vector2(path[i].x + 0.5f, path[i].y + 0.5f), new Vector2(path[i + 1].x + 0.5f, path[i + 1].y + 0.5f), Color.white);
                }
            }
        }

        Thinking();
    }

    private Vector2Int ChooseNewPosition()
    {
        Vector2Int newPos;
        bool validPos = false;

        // Loops until a valid position has been found
        do
        {
            Player closestPlayer = FindClosestPlayer();

            // Direction towards the closest player as a vector
            Vector2 direction = (closestPlayer.transform.position - transform.position);

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
            newPos = Vector2Int.FloorToInt(new Vector2(transform.position.x + Mathf.Cos(theta) * radius, transform.position.y + Mathf.Sin(theta) * radius));

            // Checks that the chosed position is within bounds and a walkable square
            if (newPos.x < 11 && newPos.x > -12 && newPos.y < 6 && newPos.y > -7 && Pathfinding.nodes[newPos].IsWalkable())
            {
                validPos = true;
            }
        }
        while (!validPos);

        return newPos;
    }

    private void GeneratePath(Vector2Int newPosition)
    {
        Vector2Int currentPos = Vector2Int.FloorToInt(transform.position);

        // Generates the path from the current position to the new position
        path = Pathfinding.FindPath(currentPos.x, currentPos.y, newPosition.x, newPosition.y);

    }

    private void FollowPath()
    {
        // Moves towards the position chosen
        rb.MovePosition(Vector2.MoveTowards(rb.position, new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f), moveSpeed * Time.deltaTime));
    }

    public void Thinking()
    {
        //MoveTurret();

        float currentRayDistance;
        //float currentTotalDistance = 0;
        float distanceRemaining = maxBulletDistance;
        //float rayDistance = maxBulletDistance;

        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);
        Color rayColour = Color.red;

        //Debug.Log(maxBulletDistance);

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

            //currentRayDistance = Mathf.Min(hit.distance, maxBulletDistance - currentTotalDistance);
            //currentTotalDistance += currentRayDistance;

            //hit = Physics2D.Raycast(ray.origin, ray.direction, currentRayDistance);

            Debug.DrawRay(ray.origin, ray.direction * currentRayDistance, rayColour);

            if (hit)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);
                    ray = new Ray2D(hit.point + (newDirection * 0.0001f), newDirection);
                }
                else if (hit.transform.CompareTag("Player"))
                {
                    if (!reloading && FireProbability())
                    {
                        Fire();
                    }

                    //ray = new Ray2D(hit.point, ray.direction);
                    //rayColour = Color.blue;
                }
            }

            //hit = Physics2D.Raycast(ray.origin, ray.direction, currentRayDistance);
        }
    }

    private void Fire()
    {
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
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
            Vector3 playerPosition = FindClosestPlayer().gameObject.transform.position;

            Vector3 targetDirection = playerPosition - turret.position;
            float theta = Mathf.Atan2(targetDirection.y, targetDirection.x);

            theta += Random.Range(-(turretRotateAngle / 2), turretRotateAngle / 2);

            theta *= Mathf.Rad2Deg;

            q = Quaternion.AngleAxis(theta, Vector3.forward);
        }

        turret.rotation = Quaternion.RotateTowards(turret.rotation, q, rotationSpeed);


        //Vector3 newDirection = Vector3.RotateTowards(turret.up, targetDirection, 1 * Time.deltaTime, 0.0f);
        //transform.LookAt
        //Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector2.up);


        //turret.rotation = Quaternion.RotateTowards(turret.rotation, angle, rotationSpeed);
    }

    /*
    private IEnumerator MoveTurret()
    {
        if (turret.rotation == (newRotation))
        {
            waiting = true;

            yield return new WaitForSeconds(Random.Range(0, 2));

            waiting = false;

            newRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
        else
        {
            turret.rotation = Quaternion.RotateTowards(turret.rotation, newRotation, rotationSpeed);
        }
    }
    */

    private IEnumerator Reload(float waitTime)
    {
        reloading = true;

        yield return new WaitForSeconds(waitTime);

        reloading = false;
    }

    /*private IEnumerator Pause()
    {
        pausing = true;

        yield return new WaitForSeconds(2);

        pausing = false;
    }
    */

    private Player FindClosestPlayer()
    {
        Dictionary<float, Player> playerDistances = new Dictionary<float, Player>();
        List<float> distances = new List<float>();

        // Loops for each player in the game
        foreach (Player player in GameController.instance.players)
        {
            // Adds the player's distance from the enemy to the list of distances
            distances.Add(Vector2.Distance(player.transform.position, transform.position));

            // Adds the player and their distance to the dictionary
            playerDistances.Add(distances.Last(), player);
        }

        Player closestPlayer = playerDistances[distances.Min()];

        return closestPlayer;
    }

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            Destroy(gameObject);

            GameController.instance.enemies.Remove(gameObject.GetComponent<Enemy>());

            if (GameController.instance.enemies.Count <= 0)
            {
                // End the current level and move on to the next
                GameController.instance.EndLevel();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentNode = 0;
            do
            {
                GeneratePath(ChooseNewPosition());
            }
            while (path == null);
        }
    }
}