using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;
public class SaveAndLoad : MonoBehaviour
{
    [SerializeField] string levelToLoadDebug;
    Dictionary<Vector3Int, TileData> tileDataByPosition = new();
    public static SaveAndLoad instance;
    private LevelData levelData;
    public LevelData LevelData
    {
        get { return levelData; }
        set { levelData = value; }
    }
    string pathToLevel;
    Dictionary<string, TileBase> tilePrefabs;
    Dictionary<string, Tilemap> tilemaps;
    [SerializeField] Tilemap groundMap, triggerMap, startingTilemap;
    [SerializeField] Camera thumbnailCamera;
    [SerializeField] RenderTexture thumbnailRT;
    [SerializeField] GridRenderer gridRenderer;
    [SerializeField] GameObject player;
    [SerializeField] PlayerController playerController;
    [SerializeField] LevelBuilder levelBuilder;

    void Awake()
    {
        instance = this;

        tilePrefabs = new();
        tilemaps = new()
        {
            ["Base Blocks"] = groundMap,
            ["Interactables"] = triggerMap,
        };

        LoadAllTilePrefabs("Tiles"); 

        string editOrPlay;
        if(SceneData.loadBehaviour == "Play") editOrPlay = "Play";
        else editOrPlay = "Edit";


        pathToLevel = Path.Combine(Application.persistentDataPath, "Levels", editOrPlay, SceneData.loadedLevelName + ".json");
        levelData = LevelFunctions.instance.GetJsonFromPath(pathToLevel);  
    }

    void Start()
    {
        switch(SceneData.loadBehaviour)
        {
            case "Edit":
                if(levelData == null) SetupEditor(false);
                else SetupEditor(true);
                break;
            case "Clear":
                levelBuilder.StartLevelClearing(true, true);
                SetupEditor(true);
                break;
            case "Play":
                levelBuilder.StartLevelClearing(true, false);
                SetupEditor(true);
                break;

        }
    }
    void SetupEditor(bool _isExistingLevel)
    {
        if(_isExistingLevel)
        {
            Debug.Log($"Level loaded: {levelData.levelName}");
            BuildLevel();
        }
        else
        {
            Debug.Log($"New level created: {SceneData.loadedLevelName}");
            levelData = new();

            foreach (Transform _child in startingTilemap.transform)
            {
                Vector3Int cellPosition = startingTilemap.WorldToCell(_child.position);
                levelBuilder.CreateTile(cellPosition, startingTilemap);
            }

            SaveLevel(SceneData.loadedLevelName);
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
        //Handle old data
        if(tileDataByPosition.ContainsKey(_position))
        {
            TileData dataToUpdate = tileDataByPosition[_position];
            if(dataToUpdate.tileMap != "StartingTiles")
            {
                Debug.Log($"Removing Tile from LevelData at {_position}");
                if(dataToUpdate.tileMap != _tileMap) tilemaps[dataToUpdate.tileMap].SetTile(_position, null);
                tileDataByPosition.Remove(_position);
                levelData.tiles.Remove(dataToUpdate);
                if(_tileBase == "null") return;
            }
        }

        //Create new tile data
        if(_tileBase != "null")
        {
            TileData newTileData = new()
            {
                tileType = _tileBase,
                position = _position,
                tileMap = _tileMap
            };

            levelData.tiles.Add(newTileData);
            tileDataByPosition.Add(_position, newTileData);
        }
    }

    public void SaveLevel(string _levelName)
    {
        if(SceneData.loadedLevelName == "") return;

        levelData.levelName = _levelName;   
        levelData.playerStartX = player.transform.position.x;
        levelData.playerStartX = player.transform.position.y;

        string path = Path.Combine(Application.persistentDataPath, "Levels", "Edit", _levelName + ".json");

        LevelData oldLevelData = null;
        if (File.Exists(path))
        {
            oldLevelData = LevelFunctions.instance.GetJsonFromPath(path);
            Debug.LogWarning("File already exists: " + path);
        }

        if(oldLevelData == levelData) return;

        levelData.isCleared = false;
        string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);

        File.WriteAllText(path, json);
        Debug.Log("Level saved to " + path);

        gridRenderer.gameObject.SetActive(false);
        Texture2D thumbnail = CaptureThumbnail(thumbnailCamera, thumbnailRT);
        byte[] bytes = thumbnail.EncodeToPNG();

        string thumbnailPath = Path.Combine(Application.persistentDataPath, "Levels", "Edit", "Thumbnails", _levelName + ".png");
        File.WriteAllBytes(thumbnailPath, bytes);
        gridRenderer.gameObject.SetActive(true);
    }

    public void BuildLevel()
    {
        if(levelData == null) return;
        tileDataByPosition = new();
        foreach(var kvp in tilemaps)
        {
            kvp.Value.ClearAllTiles();
        }

        foreach (TileData tile in levelData.tiles)
        {
            if (tilePrefabs.TryGetValue(tile.tileType, out TileBase _tileBase))
            {
                if(tile.tileType != "null") 
                {
                    tilemaps[tile.tileMap].SetTile(tile.position, _tileBase);
                    tileDataByPosition.Add(tile.position, tile);
                }
                else tilemaps[tile.tileMap].SetTile(tile.position, null);
            }
            else if(tile.tileType == "null")
            {
                tilemaps[tile.tileMap].SetTile(tile.position, null);
            }
            else Debug.LogWarning("Unknown tile type: " + tile.tileType);
        }
        player.transform.position = new Vector2(levelData.playerStartX, levelData.playerStartY);

        UIManager.instance.DisplayHealthFromInt(levelData.maxPlayerHealth, true);
        UIManager.instance.GetTextElementFromDict("TimerText").text = $"{levelData.timeToComplete}";
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
