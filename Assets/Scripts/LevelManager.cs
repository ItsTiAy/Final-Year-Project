using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
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
    private LevelData levelData;

    private void Awake()
    {
        // Checks to make sure there is only 1 instance of the level manager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //Destroy(gameObject);
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

        //StartCoroutine(GameController.instance.TransitionToNextLevel());
        //LoadLevel(currentLevel);
        //LoadLevel(1);
        //GenerateLevel();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S)) SaveLevel();
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L)) SaveChunk();
        if (Input.GetKeyDown(KeyCode.Alpha2)) SaveChunk("LCorner");
        if (Input.GetKeyDown(KeyCode.Alpha4)) SaveChunk("RCorner");
        if (Input.GetKeyDown(KeyCode.Alpha3)) SaveChunk("TMiddle");
        if (Input.GetKeyDown(KeyCode.Alpha1)) SaveChunk("LMiddle");
        if (Input.GetKeyDown(KeyCode.Alpha5)) SaveChunk("RMiddle");
        if (Input.GetKeyDown(KeyCode.Alpha6)) SaveChunk("Middle");
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

    public void SaveChunk(string chunkType)
    {
        Debug.Log("Save Chunk");

        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Chunks");
        int len = info.GetFiles(chunkType + "*.json").Length;

        ChunkData levelData = new ChunkData();

        // Loops for each type of tile in the chunk
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

        // Writes the chunk data to a json file
        string json = JsonUtility.ToJson(levelData, true);
        string chunk = chunkType + (len + 1);
        File.WriteAllText(Application.streamingAssetsPath + "/Chunks/" + chunk + ".json", json);
    }

    public void LoadLevel(int levelNumber)
    {
        Debug.Log("Load Level");
        StartCoroutine(LoadLevelCoroutine(levelNumber));
    }

    public IEnumerator LoadLevelCoroutine(int levelNumber)
    {
        Pathfinding.Initialize();

        currentLevel = levelNumber;

        ClearLevel();

        // Reads the level data from the json file
        //string json = File.ReadAllText(Application.dataPath + "/Levels/level" + levelNumber + ".json");
        //levelData = JsonUtility.FromJson<LevelData>(json);

        string path = Application.streamingAssetsPath + "/Levels/level" + levelNumber + ".json";
        UnityWebRequest uwr = UnityWebRequest.Get(path);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            levelData = JsonUtility.FromJson<LevelData>(uwr.downloadHandler.text);

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
            GameController.instance.enemies.Clear();
            GameController.instance.players.Clear();

            GameController.instance.players.Add(Instantiate(player, levelData.player.playerSpawn, Quaternion.identity).GetComponent<Player>());


            foreach (EnemyData enemy in levelData.enemies)
            {
                GameController.instance.enemies.Add(Instantiate(enemiesDict[enemy.enemyName], enemy.enemyPosition, Quaternion.identity).GetComponent<Enemy>());
                //Debug.Log(GameController.instance.enemies.Count);
                //GameController.instance.enemiesRemaining++;
            }

        }
    }

    /*
    public void LoadLevelTraining(int levelNumber)
    {
        Pathfinding.Initialize();

        currentLevel = levelNumber;

        ClearLevel();

        // Reads the level data from the json file
        //string json = File.ReadAllText(Application.dataPath + "/Levels/level" + levelNumber + ".json");
        //levelData = JsonUtility.FromJson<LevelData>(json);

        string path = Application.streamingAssetsPath + "/Levels/level" + levelNumber + ".json";
        string jsonString = File.ReadAllText(path);
        levelData = JsonUtility.FromJson<LevelData>(jsonString);

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
        GameController.instance.enemies.Clear();

        if (!GameController.instance.IsTraining())
        {
            //GameController.instance.enemies.Clear();

            GameController.instance.players.Clear();

            GameController.instance.players.Add(Instantiate(player, levelData.player.playerSpawn, Quaternion.identity).GetComponent<Player>());

        }

        foreach (EnemyData enemy in levelData.enemies)
        {
            GameController.instance.enemies.Add(Instantiate(enemiesDict[enemy.enemyName], enemy.enemyPosition, Quaternion.identity).GetComponent<Enemy>());
            //Debug.Log(GameController.instance.enemies.Count);
            //GameController.instance.enemiesRemaining++;
        }
    }

    */

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
        GameObject[] mineObjects = GameObject.FindGameObjectsWithTag("Mine");

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

        foreach (GameObject mineObject in mineObjects)
        {
            Destroy(mineObject);
        }
    }

    public void GenerateLevel()
    {
        StartCoroutine(GenerateLevelCoroutine());
    }

    private IEnumerator GenerateLevelCoroutine()
    {
        ClearLevel();

        levelData = new LevelData();

        Pathfinding.Initialize();

        string[] chunkNames = { "LCorner", "TMiddle", "TMiddle", "RCorner", "LMiddle", "Middle", "Middle", "RMiddle" };
        List<Vector2> spawnPositions = new List<Vector2>();

        int startX;
        int startY = 8;
        int counter = 0;

        foreach(Tilemap tilemap in tilemaps)
        {
            tilemap.ClearAllTiles();
        }

        for (int i = 0; i < chunkNames.Length / 4; i++)
        {
            startX = -12;
            startY -= 4;

            for (int l = 0; l < chunkNames.Length / 2; l++)
            {
                //DirectoryInfo info = new DirectoryInfo(Application.streamingAssetsPath + "/Chunks");
                //int len = info.GetFiles(chunkNames[counter] + "*.json").Length;

                int len = 2;

                Debug.Log("length " + len);

                //string json = File.ReadAllText(Application.streamingAssetsPath + "/Chunks/" + chunkNames[counter] + Random.Range(1, len + 1) + ".json");
                //ChunkData chunkData = JsonUtility.FromJson<ChunkData>(json);

                string path = Application.streamingAssetsPath + "/Chunks/" + chunkNames[counter] + Random.Range(1, len + 1) + ".json";
                UnityWebRequest uwr = UnityWebRequest.Get(path);
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    ChunkData chunkData = JsonUtility.FromJson<ChunkData>(uwr.downloadHandler.text);

                    // Loops for each type of tile in the chunk
                    for (int j = 0; j < chunkData.tiles.Count; j++)
                    {
                        TileData tileData = chunkData.tiles[j];
                        levelData.tiles.Add(tileData);

                        // Loops for each indiviual tile of the current tile type
                        for (int k = 0; k < tileData.tilePositionsX.Count; k++)
                        {
                            // Sets the tile at the coordinates with the current tile type in the current tilemap
                            tilemaps[j].SetTile(new Vector3Int(tileData.tilePositionsX[k] + startX, tileData.tilePositionsY[k] + startY), tiles[j]);
                            Pathfinding.nodes[new Vector2Int(tileData.tilePositionsX[k] + startX, tileData.tilePositionsY[k] + startY)].SetIsWalkable(false);

                            // Sets the mirrored and flipped tiles
                            tilemaps[j].SetTile(new Vector3Int(-(tileData.tilePositionsX[k] + startX) - 1, -(tileData.tilePositionsY[k] + startY)), tiles[j]);
                            Pathfinding.nodes[new Vector2Int(-(tileData.tilePositionsX[k] + startX) - 1, -(tileData.tilePositionsY[k] + startY))].SetIsWalkable(false);

                        }
                    }

                    spawnPositions.Add(chunkData.spawnPosition + new Vector2(startX, startY));
                    spawnPositions.Add(new Vector2(-(chunkData.spawnPosition.x + startX) - 1, -(chunkData.spawnPosition.y + startY)));

                    startX += 6;
                    counter++;
                }
            }
        }

        GameController.instance.enemies.Clear();
        GameController.instance.players.Clear();

        int randNum = Random.Range(1, 6);

        for (int i = 0; i < randNum; i++)
        {
            int spawnNum = Random.Range(0, spawnPositions.Count);
            GameController.instance.enemies.Add(Instantiate(enemies[Random.Range(0, enemies.Count)], spawnPositions[spawnNum], Quaternion.identity).GetComponent<Enemy>());
            spawnPositions.Remove(spawnPositions[spawnNum]);
        }

        GameController.instance.players.Add(Instantiate(player, spawnPositions[Random.Range(0, spawnPositions.Count)], Quaternion.identity).GetComponent<Player>());
    }

    public LevelData GetLevelData()
    {
        return levelData;
    }

    [System.Serializable]
    public class ChunkData
    {
        public List<TileData> tiles = new List<TileData>();
        public Vector2 spawnPosition = new Vector2(1.5f, 1.5f);
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