using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform turret;
    public Transform bulletSpawn;
    public Rigidbody2D bullet;
    public LayerMask layerMask;
    public SecondaryItem secondaryItem;
    public ParticleSystem explosion;

    private Vector2 movement;
    private Vector2 mousePos;
    private Bullet bulletClass;
    private int health = 1;
    private float moveSpeed = 3f;
    private float maxBulletDistance;
    private int ammo;
    private Camera cam;

    private bool reloading = false;
    private bool secondaryCooldown = false;

    private KeyCode left = KeyCode.A;
    private KeyCode right = KeyCode.D;
    private KeyCode up = KeyCode.W;
    private KeyCode down = KeyCode.S;

    private void Start()
    {
        // Assigns the bullet and secondary item from the save data
        bulletClass = GameController.instance.bulletTypes[SaveManager.instance.GetSaveData().primaryWeaponIndex];
        secondaryItem = GameController.instance.secondaryItems[SaveManager.instance.GetSaveData().secondaryWeaponIndex];

        ammo = bulletClass.AmmoCapacity;

        GameController.instance.UpdateAmmoUI(ammo);

        bullet = bulletClass.rb;
        maxBulletDistance = bulletClass.BulletSpeed * bulletClass.BulletLifeTime;

        cam = Camera.main;
    }

    void Update()
    {
        if (!GameController.isPaused)
        {
            // Gets the keys down used for movement
            movement.x = Convert.ToInt32(Input.GetKey(right)) - Convert.ToInt32(Input.GetKey(left));
            movement.y = Convert.ToInt32(Input.GetKey(up)) - Convert.ToInt32(Input.GetKey(down));

            // Gets the mouse position
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            // Fires a bullet on mouse click if not reloading
            if (Input.GetMouseButtonDown(0))
            {
                if (!reloading)
                {
                    Fire();
                }
                else
                {
                    AudioManager.instance.Play("FireFail");
                }
            }

            // Lays a mine on space bar press if not reloading secondary item
            if(Input.GetKeyDown(KeyCode.Space) && !secondaryCooldown)
            {
                SecondaryItem();
            }
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

    private void Move()
    {
        // Moves the player in the input direction at the movement speed
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

    private void Aim()
    {
        // Points the turret towards the mouse 
        turret.right = mousePos - new Vector2(turret.position.x, turret.position.y);
    }

    private void Fire()
    {
        // Creates a new bullet object
        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation, GameController.instance.bulletContainer);
        ammo--;

        GameController.instance.UpdateAmmoUI(ammo);

        // Reloads if ammo runs out
        if (ammo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void SecondaryItem()
    {
        if (secondaryItem != null)
        {
            // Creates a new mine object
            Instantiate(secondaryItem, transform.position, Quaternion.identity, GameController.instance.mineContainer);
            StartCoroutine(Cooldown());
        }
    }

    // Starts timer for reloading
    private IEnumerator Reload()
    {
        reloading = true;

        GameController.instance.AnimateBulletReload(bulletClass.ReloadSpeed);

        yield return new WaitForSeconds(bulletClass.ReloadSpeed);

        ammo = bulletClass.AmmoCapacity;

        GameController.instance.UpdateAmmoUI(ammo);

        reloading = false;

        AudioManager.instance.Play("FinishReloading");
    }

    // Starts timer for secondary reloading
    private IEnumerator Cooldown()
    {
        secondaryCooldown = true;

        GameController.instance.AnimateSecondaryReload(5);

        yield return new WaitForSeconds(5);

        secondaryCooldown = false;
    }

    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            // Plays particle effect on death
            ParticleSystem exp = Instantiate(explosion, transform.position, Quaternion.identity);
            var main = exp.main;
            main.useUnscaledTime = true;

            AudioManager.instance.Play("TankExplosion");
            DestroyPlayer();
            GameController.instance.RestartLevel();
        }
    }

    public void DestroyPlayer()
    {
        GameController.instance.players.Remove(this);
        Destroy(gameObject);
    }

    public void ResetBulletClass()
    {
        bulletClass = bullet.GetComponent<Bullet>();
        ammo = bulletClass.AmmoCapacity;
        GameController.instance.UpdateAmmoUI(ammo);
    }
}
