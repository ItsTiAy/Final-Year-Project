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

    private Bullet bulletClass;
    private float maxBulletDistance;
    private Quaternion newRotation;

    private bool reloading = false;
    private bool secondaryCooldown = false;
    //private bool waiting = false;
    //private bool pausing = false;

    private int currentNode = 0;
    private List<Pathfinding.Node> path;

    //private Quaternion newRot = Quaternion.identity;


    //private Pathfinding pathfinding;

    private void Start()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;

        newRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        GeneratePath(ChooseNewPosition());
        StartCoroutine(SecondaryReload(Random.Range(2, 5)));
    }

    private void FixedUpdate()
    {
        Aim();
        FollowPath();

        // Only calculates path if enemy can move
        if (moveSpeed > 0)
        {
            // Checks if the current positon is equal to the current path node's position
            if (rb.position == new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f))
            {
                // Generates new path when reaches the end of the path
                if (currentNode == path.Count - 1)
                {
                    // Checks if the enemy is not within the radius of a mine
                    if (!TooCloseToMine(transform.position))
                    {
                        bool validPath = false;
                        int counter = 0;

                        while (!validPath)
                        {
                            validPath = true;

                            do
                            {
                                GeneratePath(ChooseNewPosition());
                            }
                            while (path == null);

                            // Path is invalid if any node of the path goes within the radius of a mine
                            foreach (Pathfinding.Node node in path)
                            {
                                if (TooCloseToMine(new Vector2(node.x + 0.5f, node.y + 0.5f)))
                                {
                                    validPath = false;
                                }
                            }

                            counter++;

                            // Generates a new path without restrictions if one can't be found after 100 attempts
                            if (counter > 100)
                            {
                                do
                                {
                                    GeneratePath(ChooseNewPosition());
                                }
                                while (path == null);

                                validPath = true;
                            }
                        }
                    }
                    else
                    {
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
                    Debug.DrawLine(new Vector2(path[i].x + 0.5f, path[i].y + 0.5f), new Vector2(path[i + 1].x + 0.5f, path[i + 1].y + 0.5f), Color.white);
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
            GameObject closestPlayer = FindClosestPlayer();

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
        currentNode = 0;

        Vector2Int currentPos = Vector2Int.FloorToInt(transform.position);

        // Generates the path from the current position to the new position
        path = Pathfinding.FindPath(currentPos.x, currentPos.y, newPosition.x, newPosition.y);

    }

    private void FollowPath()
    {
        // Moves towards the position chosen
        rb.MovePosition(Vector2.MoveTowards(rb.position, new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f), moveSpeed * Time.deltaTime));
    }

    // Decides whether to shoot or not
    public void Thinking()
    {
        float currentRayDistance;
        float distanceRemaining = maxBulletDistance;

        // Casts a ray from the barrel of the gun
        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);
        Color rayColour = Color.red;

        // Checks the bullets trajectory
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
                // Ray reflects if it hits a wall
                if (hit.transform.CompareTag("Wall"))
                {
                    Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);
                    ray = new Ray2D(hit.point + (newDirection * 0.0001f), newDirection);
                }
                else if (hit.transform.CompareTag("Player"))
                {
                    // Random chance for the enemy to shoot if the ray hits a player
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
        // Creates a new mine object
        Instantiate(secondary, transform.position, Quaternion.identity, GameController.instance.mineContainer);
    }

    // Checks to see if the enemy is within the radius of a mine
    public bool TooCloseToMine(Vector2 enemyPos)
    {
        bool isTooClose = false;
        Transform closestMine;

        foreach (Transform mine in GameController.instance.mineContainer)
        {
            if (Vector2.Distance(mine.position, enemyPos) <= 4)
            {
                isTooClose = true;
                closestMine = mine;
            }
        }

        return isTooClose;
    }

    private void Fire()
    {
        // Creates a new bullet object
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, GameController.instance.bulletContainer);
        StartCoroutine(Reload(reloadSpeed));
    }

    private bool FireProbability()
    {
        // Checks to see if the enemy should fire based on the fire chance
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

    // Rotates the enemies turret
    private void Aim()
    {
        // If the the current rotation reaches the new rotation, choose new rotation
        if (turret.rotation == newRotation)
        {
            Vector3 playerPosition = FindClosestPlayer().gameObject.transform.position;

            Vector3 targetDirection = playerPosition - turret.position;
            float theta = Mathf.Atan2(targetDirection.y, targetDirection.x);

            theta += Random.Range(-(turretRotateAngle / 2), turretRotateAngle / 2);

            theta *= Mathf.Rad2Deg;

            newRotation = Quaternion.AngleAxis(theta, Vector3.forward);
        }

        // Rotate towards the new rotation
        turret.rotation = Quaternion.RotateTowards(turret.rotation, newRotation, rotationSpeed);
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

    private GameObject FindClosestPlayer()
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

        return closestPlayer.gameObject;
    }

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            ParticleSystem exp = Instantiate(explosion, transform.position, Quaternion.identity);

            Destroy(gameObject);

            if (SaveManager.instance.GetSaveData().endlessUnlocked)
            {
                GameController.instance.IncreaseEndlessScore();
            }

            GameController.instance.enemies.Remove(gameObject.GetComponent<Enemy>());

            if (GameController.instance.enemies.Count <= 0)
            {
                var main = exp.main;
                main.useUnscaledTime = true;

                // End the current level and move on to the next
                GameController.instance.EndLevel();
            }

            AudioManager.instance.Play("TankExplosion");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Recalculate path if bumped into anther enemy
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

    /*
    public void GenerateNewPath()
    {
        do
        {
            GeneratePath(ChooseNewPosition());
        }
        while (path == null);
    }

    public void ResetReloading()
    {
        reloading = false;
    }
    */
}