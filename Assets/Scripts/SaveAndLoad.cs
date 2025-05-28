using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
public static class LevelLoaderData
{
    public static string levelName;
}
public class LevelData
{
    public string levelName;
    public Vector2Int levelSize;
    public List<TileData> tiles = new();
}

public struct TileData
{
    public string tileMap;
    public string tileType;
    public Vector3Int position;
}

public class SaveAndLoad : MonoBehaviour
{
    Dictionary<Vector3Int, TileData> tileDataByPosition = new();
    public static SaveAndLoad instance;
    LevelData levelData;
    Dictionary<string, TileBase> tilePrefabs;
    Dictionary<string, Tilemap> tilemaps;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        levelData = new();
        tilePrefabs = new();
        tilemaps = new();

        LoadAllTilePrefabs("Tiles");

        if(LevelLoaderData.levelName != null)
        {
            LoadAndBuild(LevelLoaderData.levelName);
            LevelLoaderData.levelName = null;
        }
    }

    void LoadAllTilePrefabs(string folder)
    {
        TileBase[] loadedTiles = Resources.LoadAll<TileBase>(folder);

        foreach (TileBase _tileBase in loadedTiles)
        {
            string tileID = _tileBase.name; 
            if (!tilePrefabs.ContainsKey(tileID)) tilePrefabs.Add(tileID, _tileBase);
        }

        //For if its empty
        //tilePrefabs.Add()

        Debug.Log($"Loaded {tilePrefabs.Count} tile prefabs from Resources/{folder}");

        Tilemap[] loadedTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (Tilemap _tileMap in loadedTilemaps)
        {
            string tileMapID = _tileMap.name; // Or get a script component with a custom ID
            if (!tilemaps.ContainsKey(tileMapID)) tilemaps.Add(tileMapID, _tileMap);
        }
    }

    public void SaveTileData(string _tileBase, Vector3Int _position, string _tileMap)
    {
        /*
        Creating new tile data works completely fine
        Updating or removing existing tile data doesnt always work
        Check if it always doesnt work or if it has something to do with starting from new vs. editing loaded level
        */
        if(tileDataByPosition.ContainsKey(_position))
        {
            TileData dataToUpdate = tileDataByPosition[_position];

            if(dataToUpdate.tileMap == _tileMap)
            {
                if(_tileBase == "null")
                {
                    tileDataByPosition.Remove(_position);
                    levelData.tiles.Remove(dataToUpdate);
                }
                else
                {
                    dataToUpdate.tileType = _tileBase;
                    dataToUpdate.position = _position;
                }
            }

            return;
        }

        TileData newTileData = new()
        {
            tileType = _tileBase,
            position = _position,
            tileMap = _tileMap
        };

        levelData.tiles.Add(newTileData);
        tileDataByPosition.Add(_position, newTileData);
        
        //Debug.Log($"Saving Tile Data: Tile: {_tileBase.name}, Position: {_position}, Tilemap: {_tileMap}");
    }

    public void SaveLevel(string fileName)
    {
        string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");

        if (File.Exists(path))
        {
            Debug.LogWarning("File already exists: " + path);
        }

        File.WriteAllText(path, json);
        Debug.Log("Level saved to " + path);
    }

    public void LoadAndBuild(string fileName)
    {
        LevelData savedLevel = LoadLevel(fileName);
        BuildLevel(savedLevel);
    }

    public LevelData LoadLevel(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("File not found: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<LevelData>(json);
    }

    public void BuildLevel(LevelData level)
    {
        foreach (TileData tile in level.tiles)
        {
            if (tilePrefabs.TryGetValue(tile.tileType, out TileBase _tileBase))
            {
                if(tile.tileType != "null") 
                {
                    tilemaps[tile.tileMap].SetTile(tile.position, _tileBase);
                    SaveTileData(_tileBase.name, tile.position, tile.tileMap);
                }
                else tilemaps[tile.tileMap].SetTile(tile.position, null);
            }
            else
            {
                Debug.LogWarning("Unknown tile type: " + tile.tileType);
            }
        }
    }
}
