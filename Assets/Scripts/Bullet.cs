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
            Debug.Log("Wall");

            if (numBounces < maxBounces)
            {
                // Gets array of contact points between the bullet and the wall
                // In this case that will always only be 1

                ContactPoint2D[] contacts = new ContactPoint2D[1];
                collision.GetContacts(contacts);

                // Sets new velocity to be the reflection of its previous velocity

                GetComponent<Rigidbody2D>().velocity = Vector2.Reflect(GetComponent<Rigidbody2D>().velocity, contacts[0].normal);
            }
            else
            {
                DestroyBullet();
            }

            numBounces++;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().DecreaseHealth();
            DestroyBullet();
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().DecreaseHealth();
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        trailParticles.Stop();
        transform.DetachChildren();
        Destroy(gameObject);
    }
}
