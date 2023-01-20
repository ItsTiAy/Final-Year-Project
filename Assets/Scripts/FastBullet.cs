using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastBullet : Bullet
{
    public FastBullet()
    {
        maxBounces = 1;
        bulletLifeTime = 3f;
        bulletSpeed = 5f;
    }
}
