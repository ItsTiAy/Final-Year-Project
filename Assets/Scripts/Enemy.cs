using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int health = 1;
    
    public void DecreaseHealth()
    {
        health--;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}