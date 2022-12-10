using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Camera cam;
    public Rigidbody2D bullet;
    public LayerMask layerMask;

    private Vector2 movement;
    private Vector2 mousePos;
    private Bullet bulletClass;
    private int health = 1;
    //private float moveSpeed = 5f;
    private float maxBulletDistance;

    private void Start()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    private void FixedUpdate()
    {
        Move();
        Aim();

        // Debug laser pointer to show where the bullet will go

        float currentRayDistance;
        float currentTotalDistance = 0;

        Ray2D ray = new(bulletSpawn.position, bulletSpawn.right);

        for (int i = 0; i < bulletClass.MaxBounces + 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100, layerMask);

            currentRayDistance = Mathf.Min(hit.distance, maxBulletDistance - currentTotalDistance);
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

    // Moves the player in the input direction at the movement speed
    private void Move()
    {
        //rb.velocity = PixelPerfectClamp(moveSpeed * 16 * Time.fixedDeltaTime * movement); // Probably need some pixel perfect movement
        rb.MovePosition(rb.position + (movement / 16));
    }

    // Points the turret towards the mouse 
    private void Aim()
    {
        turret.right = mousePos - new Vector2(turret.position.x, turret.position.y);
    }

    private void Fire()
    {
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation /*Quaternion.Euler(0, 0, 0)*/);
    }

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
