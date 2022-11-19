using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Camera cam;
    //public ParticleSystem gun;
    public Rigidbody2D bullet;
    //public Transform bulletTransform;

    private Vector2 movement;
    private Vector2 mousePos;
    //private Rigidbody2D bulletInstance;
    //private float bulletLifeTime = 3f;
    //private float bulletSpeed = 300f;
    private int health = 1;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            //gun.Play();
            Fire();
        }
        /*
        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);

        Debug.DrawRay(ray.origin, ray.direction, Color.green);


        for (int i = 0; i < 2; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    Vector2 newDirection = Vector2.Reflect(ray.direction, hit.normal);

                    Debug.DrawRay(hit.point + (newDirection * 0.01f), newDirection, Color.magenta);
                    ray = new Ray2D(hit.point + (newDirection * 0.01f), newDirection);
                }
            }
        }
        */
    }

    private void FixedUpdate()
    {
        // Moves the player in the input direction at the movement speed

        //rb.velocity = PixelPerfectClamp(moveSpeed * 16 * Time.fixedDeltaTime * movement); // Probably need some pixel perfect movement
        rb.MovePosition(rb.position + (movement / 16));
        // Points the turret towards the mouse 
        turret.right = mousePos - new Vector2(turret.position.x, turret.position.y);
    }

    private void Fire()
    {
        //Subtracts one from the current ammo

        //currentAmmo--;

        // Creates an instance of a bullet with correct position and rotation

        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation /*Quaternion.Euler(0, 0, 0)*/);

        //bulletInstance.GetComponent<Rigidbody2D>().velocity = bulletSpeed * Time.fixedDeltaTime * bulletSpawn.right;
        //bulletInstance.GetComponent<Rigidbody2D>().AddForce bulletSpeed * Time.fixedDeltaTime * ;

        // Detroys bullet after the bullets lifetime

        //StartCoroutine(DestroyBulletAfterLifetime(bulletInstance));
    }

    /*
    IEnumerator DestroyBulletAfterLifetime(Rigidbody2D bulletInstance)
    {
        yield return new WaitForSeconds(bulletLifeTime);

        // Check that bullet has not been destroyed already

        if (bulletInstance != null)
        {
            bulletInstance.transform.DetachChildren();
            Destroy(bulletInstance.gameObject);
        }
    }
    */

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    /*
    private Vector2 PixelPerfectClamp(Vector2 moveVector)
    {
        Vector2 vectorInPixels = new Vector2(Mathf.RoundToInt(moveVector.x * 16), Mathf.RoundToInt(moveVector.y * 16));
        return vectorInPixels / 16;
    }
    */
}
