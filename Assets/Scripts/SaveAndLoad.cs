using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
public static class LevelLoaderData
{
    public static string loadedLevelName = "";
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

        if(LevelLoaderData.loadedLevelName != null)
        {
            LoadAndBuild(LevelLoaderData.loadedLevelName + ".json");
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
        if(tileDataByPosition.ContainsKey(_position))
        {
            TileData dataToUpdate = tileDataByPosition[_position];

            Debug.Log($"Passed tilemap check");
            if(_tileBase == "null")
            {
                Debug.Log($"Removing Tile from LevelData at {_position}");
                tileDataByPosition.Remove(_position);
                levelData.tiles.Remove(dataToUpdate);
                return;
            }
            else
            {
                tileDataByPosition.Remove(_position);
                levelData.tiles.Remove(dataToUpdate);
            }
        
        }

        TileData newTileData = new()
        {
            tileType = _tileBase,
            position = _position,
            tileMap = _tileMap
        };

        levelData.tiles.Add(newTileData);
        tileDataByPosition.Add(_position, newTileData);
    }

    public void SaveLevel(string _levelName)
    {
        if(LevelLoaderData.loadedLevelName == "")
        {

        }

        string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, _levelName + ".json");

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
        string path = Path.Combine(Application.persistentDataPath, fileName);
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
        if(level == null) return;

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
