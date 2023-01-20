using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    public LayerMask layerMask;

    private Vector2 movement;
    private Vector2 mousePos;
    private Bullet bulletClass;
    private int health = 1;
    private float moveSpeed = 5f;
    private float maxBulletDistance;
    private Camera cam;

    private Quaternion newDirection;
    private Quaternion prevTargetRotation;

    private void Start()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;
        Debug.Log(bulletClass.BulletSpeed);
        Debug.Log(bulletClass.BulletLifeTime);
        cam = Camera.main;
        GameController.instance.players.Add(this);
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
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100);

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
        //rb.velocity = moveSpeed * Time.fixedDeltaTime * movement.normalized;
        //rb.MovePosition(rb.position + (movement / 16));

        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement.normalized);

        // Rotates the tank towards the directon it is going in

        if (movement != Vector2.zero)
        {
            Quaternion targetRotationPos = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * movement);
            Quaternion targetRotationNeg = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * -movement);

            float targetAnglePos = Quaternion.Angle(transform.rotation, targetRotationPos);
            float targetAngleNeg = Quaternion.Angle(transform.rotation, targetRotationNeg);

            Quaternion targetRotation = targetAnglePos < targetAngleNeg ? targetRotationPos : targetRotationNeg;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 1000f * Time.fixedDeltaTime);
        }

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
            DestroyPlayer();
            LevelManager.instance.ReloadCurrentLevel();
        }
    }

    public void DestroyPlayer()
    {
        GameController.instance.players.Remove(this);
        Destroy(gameObject);
    }
}
