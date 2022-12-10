using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
    public ParticleSystem trailParticles;
    public Rigidbody2D rb;
    public LayerMask layerMask;

    private const int maxBounces = 1;
    private const float bulletLifeTime = 3f;
    private const float bulletSpeed = 5f;

    private Transform bulletOrigin;

    private int currentBounce = 0;
    private List<Vector2> bounces = new List<Vector2>();

    // Getters for attributes
    public int MaxBounces => maxBounces;
    public float BulletLifeTime => bulletLifeTime;
    public float BulletSpeed => bulletSpeed;

    private void Start()
    {
        bulletOrigin = rb.transform;

        //rb.velocity = bulletSpeed * transform.right;

        StartCoroutine(DestroyBulletAfterLifetime());

        CalculateTrajectory();
    }

    private void FixedUpdate()
    {
        MoveBullet();
    }

    private IEnumerator DestroyBulletAfterLifetime()
    {
        yield return new WaitForSeconds(bulletLifeTime);

        DestroyBullet();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DestroyBullet();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // Decreases enemy health by 1 and then destroys the bullet
            collision.gameObject.GetComponent<Enemy>().DecreaseHealth();
            DestroyBullet();
            Debug.Log("Hit Enemy");
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Decreases the player's health by 1 and then destroys the bullet
            collision.gameObject.GetComponent<Player>().DecreaseHealth();
            DestroyBullet();
            Debug.Log("Hit self");
        }
    }

    private void CalculateTrajectory()
    {
        // Draws ray from the bullet spawn forwards
        Ray2D ray = new(bulletOrigin.position, bulletOrigin.right);

        //Debug.DrawRay(ray.origin, ray.direction, Color.green, bulletLifeTime);

        // Loops for max number of bounces + 1 extra for the final destination
        for (int i = 0; i < maxBounces + 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100f, layerMask);

            // If ray hits a wall
            if (hit.transform.CompareTag("Wall"))
            {
                // Adds the hit position to the list of bounces
                bounces.Add(hit.point);

                // Calculates the new direction the ray needs to go in
                Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);

                // Updates the ray to a new ray in the new direction
                // Small offset is used to make sure the ray doesn't get stuck in a wall
                ray = new Ray2D(hit.point + (newDirection * 0.0001f), newDirection);
            }
        }
    }

    private void MoveBullet()
    {
        if (currentBounce < maxBounces + 1)
        {
            // Moves the bullet towards the next bounce position
            rb.position = Vector2.MoveTowards(rb.position, bounces[currentBounce], bulletSpeed * Time.deltaTime);

            // Changes the bounce position to go to once reached the old bounce position
            if (rb.position == bounces[currentBounce])
            {
                currentBounce++;
            }
        }

        // Destoys bullet once it reaches the final position in the list
        if (currentBounce >= maxBounces + 1)
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        // Stops particle system from producing more particles
        trailParticles.Stop();

        // Detatches from the bullet gameobject so that it is not destroyed when the bullet is
        // Allows particles to linger for a bit after the bullet has been destroyed
        transform.DetachChildren();
        Destroy(gameObject);
    }
}
