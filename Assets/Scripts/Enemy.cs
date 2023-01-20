using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour
{
    public const int maxRadius = 5;
    [Range(-10, 10)]
    public int aggression = 0;

    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    public LayerMask layerMask;
    //public List<Player> players;

    private Bullet bulletClass;
    private float maxBulletDistance;
    private float moveSpeed = 2f;
    private int health = 1;
    private float rotationSpeed = 1;
    private Quaternion newRotation;

    private bool reloading = false;
    private bool waiting = false;
    private bool pausing = false;

    private int currentNode = 0;
    private List<Pathfinding.Node> path;


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
        FollowPath();

        // Checks if the current positon is equal to the current path node's position
        if (rb.position == new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f))
        {
            // Generates new path when reaches the end of the path
            if (currentNode == path.Count - 1)
            {
                currentNode = 0;
                GeneratePath(ChooseNewPosition());
                //StartCoroutine(Pause());
            }
            else
            {
                currentNode++;
            }
        }
        

        // Debug line for the pathfinding
        // Draws the current path
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(new Vector2(path[i].x + 0.5f, path[i].y + 0.5f), new Vector2(path[i + 1].x + 0.5f, path[i + 1].y + 0.5f), Color.white);
            }
        }

        //Thinking();
    }

    private Vector2Int ChooseNewPosition()
    {
        float timeStamp1 = Time.frameCount;

        Vector2Int newPos;
        bool validPos = false;

        do
        {
            Dictionary<float, Player> playerDistances = new Dictionary<float, Player>();
            List<float> distances = new List<float>();

            foreach (Player player in GameController.instance.players)
            {
                distances.Add(Vector2.Distance(player.transform.position, transform.position));
                playerDistances.Add(distances.Last(), player);
            }

            Player closestPlayer = playerDistances[distances.Min()];

            Vector2 direction = (closestPlayer.transform.position - transform.position);
            //Debug.Log(direction);

            float theta = Mathf.Atan2(direction.y, direction.x);
            //Debug.Log(theta);

            int radius = Random.Range(1, maxRadius + 1);

            if (Random.Range(-10, 11) < aggression)
            {
                theta += Random.Range(-(Mathf.PI / 2), Mathf.PI / 2);
            }
            else
            {
                theta += Random.Range(-(Mathf.PI / 2), Mathf.PI / 2) + Mathf.PI;
            }

            newPos = Vector2Int.FloorToInt(new Vector2(transform.position.x + Mathf.Cos(theta) * radius, transform.position.y + Mathf.Sin(theta) * radius));

            if (newPos.x < 11 && newPos.x > -12 && newPos.y < 6 && newPos.y > -7 && Pathfinding.nodes[newPos].IsWalkable())
            {
                validPos = true;
            }
        }
        while (!validPos);

        float timeStamp2 = Time.frameCount;

        //Debug.Log("Frames: " + (timeStamp2 - timeStamp1));

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
        rb.position = Vector2.MoveTowards(rb.position, new Vector2(path[currentNode].x + 0.5f, path[currentNode].y + 0.5f), moveSpeed * Time.deltaTime);
    }

    public void Thinking()
    {
        if(!waiting)
        {
            StartCoroutine(MoveTurret());
        }

        //MoveTurret();

        float currentRayDistance;
        float currentTotalDistance = 0;

        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);
        Color rayColour = Color.red;

        for (int i = 0; i < bulletClass.MaxBounces + 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100);

            currentRayDistance = Mathf.Min(hit.distance, maxBulletDistance - currentTotalDistance);
            currentTotalDistance += currentRayDistance;

            hit = Physics2D.Raycast(ray.origin, ray.direction, currentRayDistance);

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
                    if (!reloading)
                    {
                        Fire();
                    }

                    //ray = new Ray2D(hit.point, ray.direction);
                    //rayColour = Color.blue;
                }
            }
        }
    }

    private void Fire()
    {
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
        StartCoroutine(Reload(5));
    }

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

    private IEnumerator Reload(float waitTime)
    {
        reloading = true;

        yield return new WaitForSeconds(waitTime);

        reloading = false;
    }

    private IEnumerator Pause()
    {
        pausing = true;

        yield return new WaitForSeconds(2);

        pausing = false;
    }

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            Destroy(gameObject);

            GameController.instance.enemiesRemaining--;

            if (GameController.instance.enemiesRemaining <= 0)
            {
                // End the current level and move on to the next
                LevelManager.instance.LoadNextLevel();
            }
        }
    }
}