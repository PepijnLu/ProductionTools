using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
public static class SceneData
{
    public static string loadedLevelName = "";
    public static string menuToLoad = "MainMenu";
    public static string levelsToLoad;
    public static string loadBehaviour;
}
public class LevelData
{
    public string levelName;
    public List<TileData> tiles = new();
    public bool isCleared, isBeaten;
    public float fastestTime;
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
    [SerializeField] Tilemap groundMap, triggerMap;
    [SerializeField] Camera thumbnailCamera;
    [SerializeField] RenderTexture thumbnailRT;
    [SerializeField] GridRenderer gridRenderer;
    [SerializeField] GameObject player;
    [SerializeField] Vector2 playerStartPosition;
    [SerializeField] PlayerController playerController;
    [SerializeField] LevelBuilder levelBuilder;

    void Awake()
    {
        instance = this;
        levelData = new();
        tilePrefabs = new();
        tilemaps = new()
        {
            ["Base Blocks"] = groundMap,
            ["Items"] = triggerMap
        };

        LoadAllTilePrefabs("Tiles");

        if(SceneData.loadedLevelName != "")
        {
            LoadAndBuild(SceneData.loadedLevelName + ".json");
        }
        else if(levelToLoadDebug != "")
        {
            LoadAndBuild(levelToLoadDebug + ".json");
        }
    }

    void Start()
    {
        switch(SceneData.loadBehaviour)
        {
            case "Edit":

                break;
            case "Clear":
                levelBuilder.StartLevelClearing(true);
                break;
            case "Play":

                break;

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
        playerStartPosition = new Vector2(player.transform.position.x, player.transform.position.y);
        if(SceneData.loadedLevelName == "") return;

        levelData.levelName = _levelName;   
        levelData.playerStartX = playerStartPosition.x;
        levelData.playerStartX = playerStartPosition.y;

        string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, "Levels", "Edit", _levelName + ".json");

        if (File.Exists(path))
        {
            Debug.LogWarning("File already exists: " + path);
        }

        File.WriteAllText(path, json);
        Debug.Log("Level saved to " + path);

        gridRenderer.gameObject.SetActive(false);
        Texture2D thumbnail = CaptureThumbnail(thumbnailCamera, thumbnailRT);
        byte[] bytes = thumbnail.EncodeToPNG();

        string thumbnailPath = Path.Combine(Application.persistentDataPath, "Levels", "Edit", "Thumbnails", _levelName + ".png");
        File.WriteAllBytes(thumbnailPath, bytes);
        gridRenderer.gameObject.SetActive(true);
    }

    public void ClearLevel()
    {
        playerController.KillMovement();
        playerController.enabled = false;
        string path;
        string boolToFlip;

        if(SceneData.loadBehaviour == "Play")
        {
            path = Path.Combine(Application.persistentDataPath, "Levels", "Play");
            boolToFlip = "isBeaten";
        }
        else if(SceneData.loadBehaviour == "Clear")
        {
            path = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
            boolToFlip = "isCleared";
        }
        else 
        {
            Debug.LogWarning($"Finish flag hit in: {SceneData.loadBehaviour}");
            return;
        }

        string levelName = SceneData.loadedLevelName;
        string pathToLevel = Path.Combine(path, levelName + ".json");
        string json = File.ReadAllText(pathToLevel);

        JObject obj = JObject.Parse(json);

        obj[boolToFlip] = true;

        File.WriteAllText(Path.Combine(path, levelName + ".json"), obj.ToString(Formatting.Indented));
        UIManager.instance.ToggleUIElement("LevelCleared", true);
        UIManager.instance.InstantiateLevelObject(UIManager.instance.GetUIElementFromDict("ClearedLevelTransform").transform, levelName, path);
    }

    public void UploadLevel(TMP_InputField _textInput)
    {
        string oldLevelName = SceneData.loadedLevelName;
        string newLevelName = _textInput.text;

        string sourcePath = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
        string destinationPath = Path.Combine(Application.persistentDataPath, "Levels", "Play");

        string sourceLevelPath = Path.Combine(sourcePath, oldLevelName + ".json");
        string destinationLevelPath = Path.Combine(destinationPath, newLevelName + ".json");

        string sourceThumbnailPath = Path.Combine(sourcePath, "Thumbnails", oldLevelName + ".png");
        string destinationThumbnailPath = Path.Combine(destinationPath, "Thumbnails", newLevelName + ".png");

        if (File.Exists(destinationLevelPath))
        {
            Debug.LogWarning("File already exists: " + destinationLevelPath);
            StartCoroutine(UIManager.instance.ShowTextForSeconds("UP_NameInUse", "name already in use!", 2f));
        }
        else
        {
            //Copy the level to "Play" folder
            File.Copy(sourceLevelPath, destinationLevelPath);

            //Update the level name
            string json = File.ReadAllText(destinationLevelPath);
            JObject obj = JObject.Parse(json);
            obj["levelName"] = newLevelName;
            File.WriteAllText(destinationLevelPath, obj.ToString(Formatting.Indented));

            //Duplicate the thumbnail
            File.Copy(sourceThumbnailPath, destinationThumbnailPath);

            SceneData.menuToLoad = "Play/Edit";
            SceneData.levelsToLoad = "Play";
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void LoadAndBuild(string fileName)
    {
        LevelData savedLevel = LoadLevel(fileName);
        BuildLevel(savedLevel);
    }

    public LevelData LoadLevel(string fileName)
    {
        string editOrPlay;
        if(SceneData.loadBehaviour == "Play") editOrPlay = "Play";
        else if(SceneData.loadBehaviour == "Edit" || SceneData.loadBehaviour == "Clear") editOrPlay = "Edit";
        else throw new Exception($"{SceneData.loadBehaviour} not valid load behaviour");
        string path = Path.Combine(Application.persistentDataPath, "Levels", editOrPlay, fileName);
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
