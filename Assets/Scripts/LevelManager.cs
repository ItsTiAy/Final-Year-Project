using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<TileBase> tiles;
    public List<Tilemap> tilemaps;

    public GameObject player;
    public Transform playerSpawn;

    public List<GameObject> enemies;
    public GameObject enemySpawns;

    public int currentLevel = 1;

    private Dictionary<string, GameObject> enemiesDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // Checks to make sure there is only 1 instance of the level manager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (GameObject enemy in enemies)
        {
            enemiesDict.Add(enemy.name, enemy);
        }

        try
        {
            currentLevel = SaveManager.instance.GetSaveData().maxLevelNum;
        }
        catch 
        {
            Debug.Log("No loaded save data");  
        }

        GameController.instance.LoadInterLevelScreen();
        LoadLevel(currentLevel);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S)) SaveLevel();
        if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(3);
    }

    public void SaveLevel()
    {
        Debug.Log("Save Level");

        LevelData levelData = new LevelData();

        // Loops for each type of tile in the level
        for (int i = 0; i < tiles.Count; i++)
        {
            TileData tileData = new TileData();
            tileData.tileName = tiles[i].name;

            // Gets the bounds of the tilemap to loop through
            BoundsInt bounds = tilemaps[i].cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    TileBase currentTile = tilemaps[i].GetTile(new Vector3Int(x, y, 0));

                    // If the current tile is not null, save the x and y positions
                    if (currentTile != null)
                    {
                        tileData.tilePositionsX.Add(x);
                        tileData.tilePositionsY.Add(y);
                    }
                }
            }

            levelData.tiles.Add(tileData);
        }

        // Creates the enemy data for each of the spawn positions and adds each to the level data
        foreach(Transform spawn in enemySpawns.transform)
        {
            EnemyData enemyData = new EnemyData();
            enemyData.enemyName = spawn.name;
            enemyData.enemyPosition = spawn.position;

            levelData.enemies.Add(enemyData);
        }

        PlayerData playerData = new PlayerData();
        playerData.playerSpawn = playerSpawn.position;

        levelData.player = playerData;

        // Writes the level data to a json file
        string json = JsonUtility.ToJson(levelData, true);
        string level = "level" + (GameController.instance.totalNumLevels + 1);
        File.WriteAllText(Application.dataPath + "/Levels/" + level + ".json", json);
    }

    public void LoadLevel(int levelNumber)
    {
        Debug.Log("Load Level");

        Pathfinding.Initialize();

        if (levelNumber > GameController.instance.totalNumLevels)
        {
            Debug.Log("Probably game win");
            return;
        }

        currentLevel = levelNumber;

        ClearLevel();

        // Reads the level data from the json file
        string json = File.ReadAllText(Application.dataPath + "/Levels/level" + levelNumber + ".json");
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // Loops for each type of tile in the level
        for (int i = 0; i < levelData.tiles.Count; i++)
        {
            // Clears the current tilemap
            tilemaps[i].ClearAllTiles();
            TileData tileData = levelData.tiles[i];

            // Loops for each indiviual tile of the current tile type
            for (int j = 0; j < tileData.tilePositionsX.Count; j++)
            {
                // Sets the tile at the coordinates with the current tile type in the current tilemap
                tilemaps[i].SetTile(new Vector3Int(tileData.tilePositionsX[j], tileData.tilePositionsY[j]), tiles[i]);

                Pathfinding.nodes[new Vector2Int(tileData.tilePositionsX[j], tileData.tilePositionsY[j])].SetIsWalkable(false);
                //Debug.Log(Pathfinding.nodes[new Vector2Int(tileData.tilePositionsX[j], tileData.tilePositionsY[j])].IsWalkable());
            }
        }

        GameController.instance.enemiesRemaining = 0;

        GameController.instance.players.Add(Instantiate(player, levelData.player.playerSpawn, Quaternion.identity).GetComponent<Player>());

        foreach (EnemyData enemy in levelData.enemies)
        {
            Instantiate(enemiesDict[enemy.enemyName], enemy.enemyPosition, Quaternion.identity);
            GameController.instance.enemiesRemaining++;
        }
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevel + 1);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevel);
    }

    private void ClearLevel()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bulletObjects = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] particleObjects = GameObject.FindGameObjectsWithTag("Particle");

        foreach (GameObject playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                playerObject.GetComponent<Player>().DestroyPlayer();
            }
        }

        foreach (GameObject enemyObject in enemyObjects)
        {
            Destroy(enemyObject);
        }

        foreach (GameObject bulletObject in bulletObjects)
        {
            Destroy(bulletObject);
        }

        foreach (GameObject particleObject in particleObjects)
        {
            Destroy(particleObject);
        }
    }

    [System.Serializable]
    public class LevelData
    {
        public List<TileData> tiles = new List<TileData>();
        public PlayerData player;
        public List<EnemyData> enemies = new List<EnemyData>();
    }

    [System.Serializable]
    public class TileData
    {
        public string tileName;
        
        public List<int> tilePositionsX = new List<int>();
        public List<int> tilePositionsY = new List<int>();
    }

    [System.Serializable]
    public class PlayerData
    {
        public Vector2 playerSpawn;
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;

        public Vector2 enemyPosition;
    }
}