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
    public List<TileData> tiles = new();
    public bool isCleared;
    public float playerStartX = 2.5f, playerStartY = 2.5f;
}

public struct TileData
{
    public string tileMap;
    public string tileType;
    public Vector3Int position;
}

public class SaveAndLoad : MonoBehaviour
{
    [SerializeField] string levelToLoadDebug;
    Dictionary<Vector3Int, TileData> tileDataByPosition = new();
    public static SaveAndLoad instance;
    LevelData levelData;
    Dictionary<string, TileBase> tilePrefabs;
    Dictionary<string, Tilemap> tilemaps;
    [SerializeField] Camera thumbnailCamera;
    [SerializeField] RenderTexture thumbnailRT;
    [SerializeField] GridRenderer gridRenderer;
    [SerializeField] GameObject player;
    [SerializeField] Vector2 playerStartPosition;

    void Awake()
    {
        instance = this;
        levelData = new();
        tilePrefabs = new();
        tilemaps = new();

        LoadAllTilePrefabs("Tiles");

        if(LevelLoaderData.loadedLevelName != "")
        {
            LoadAndBuild(LevelLoaderData.loadedLevelName + ".json");
        }
        else if(levelToLoadDebug != "")
        {
            LoadAndBuild(levelToLoadDebug + ".json");
        }
    }
    void Start()
    {
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

    public void SaveLevel(string _levelName, bool _isCleared = false)
    {
        playerStartPosition = new Vector2(player.transform.position.x, player.transform.position.y);
        if(LevelLoaderData.loadedLevelName == "") return;

        levelData.isCleared = _isCleared;
        levelData.playerStartX = playerStartPosition.x;
        levelData.playerStartX = playerStartPosition.y;

        string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, _levelName + ".json");

        if (File.Exists(path))
        {
            Debug.LogWarning("File already exists: " + path);
        }

        File.WriteAllText(path, json);
        Debug.Log("Level saved to " + path);

        gridRenderer.gameObject.SetActive(false);
        Texture2D thumbnail = CaptureThumbnail(thumbnailCamera, thumbnailRT);
        byte[] bytes = thumbnail.EncodeToPNG();

        string thumbnailPath = Path.Combine(Application.persistentDataPath, _levelName + ".png");
        File.WriteAllBytes(thumbnailPath, bytes);
        gridRenderer.gameObject.SetActive(true);
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
        player.transform.position = new Vector2(level.playerStartX, level.playerStartY);
    }

    public void StopClearingLevel()
    {
        player.transform.position = playerStartPosition;
    }

    public void ClearLevel()
    {
        SaveLevel(LevelLoaderData.loadedLevelName, true);
    }

    public Texture2D CaptureThumbnail(Camera _thumbnailCamera, RenderTexture _renderTexture)
    {
        RenderTexture currentRT = RenderTexture.active;

        _thumbnailCamera.targetTexture = _renderTexture;
        RenderTexture.active = _renderTexture;

        _thumbnailCamera.Render();

        Texture2D image = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        _thumbnailCamera.targetTexture = null;

        return image;
    }
}
