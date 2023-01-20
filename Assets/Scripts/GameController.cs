using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<Player> players;
    public int enemiesRemaining;
    public const int totalNumLevels = 3;

    private void Awake()
    {
        // Checks to make sure there is only 1 instance of the game controller
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        players = new List<Player>();
    }
}
