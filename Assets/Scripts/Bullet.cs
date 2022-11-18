using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem trailParticles;
    public Rigidbody2D rb;

    private const int maxBounces = 1;
    private const float bulletLifeTime = 3f;
    private const float bulletSpeed = 5f;

    private int numBounces;

    private void Start()
    {
        rb.velocity = bulletSpeed * transform.right;
        StartCoroutine(DestroyBulletAfterLifetime());
    }

    IEnumerator DestroyBulletAfterLifetime()
    {
        yield return new WaitForSeconds(bulletLifeTime);

        transform.DetachChildren();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DestroyBullet();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            // Checks if the number of bounces already done is less than the max it can do
            if (numBounces < maxBounces)
            {
                // Gets array of contact points between the bullet and the wall
                // In this case that will always only be 1

                ContactPoint2D[] contacts = new ContactPoint2D[1];
                collision.GetContacts(contacts);

                // Sets new velocity to be the reflection of its previous velocity

                rb.velocity = Vector2.Reflect(rb.velocity, contacts[0].normal);
            }
            else
            {
                DestroyBullet();
            }
            numBounces++;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // Decreases enemy health by 1 and then destroys the bullet
            collision.gameObject.GetComponent<Enemy>().DecreaseHealth();
            DestroyBullet();
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Decreases the player's health by 1 and then destroys the bullet
            collision.gameObject.GetComponent<Player>().DecreaseHealth();
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
