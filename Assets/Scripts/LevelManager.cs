using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static LevelManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<TileBase> tiles;
    public List<Tilemap> tilemaps;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) SaveLevel();
        if (Input.GetKeyDown(KeyCode.L)) LoadLevel();
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

        // Writes the level data to a json file
        string json = JsonUtility.ToJson(levelData);
        string level = "level";
        File.WriteAllText(Application.dataPath + "/" + level + ".json", json);
    }

    public void LoadLevel()
    {
        Debug.Log("Load Level");

        // Reads the level data from the json file
        string level = "level";
        string json = File.ReadAllText(Application.dataPath + "/" + level + ".json");
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
            }
        }
        
    }

    [System.Serializable]
    public class LevelData
    {
        public List<TileData> tiles = new List<TileData>();
        //public List<EnemyData> enemies = new List<EnemyData>();
        //public Vector2Int playerSpawn;
    }

    [System.Serializable]
    public class TileData
    {
        public string tileName;
        
        public List<int> tilePositionsX = new List<int>();
        public List<int> tilePositionsY = new List<int>();
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;

        public List<int> enemyPositionsX = new List<int>();
        public List<int> enemyPositionsY = new List<int>();
    }
}