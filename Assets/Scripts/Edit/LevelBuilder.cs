using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] Tilemap currentTilemap;
    [SerializeField] Tilemap solidTileMap, triggerTilemap, startingTilemap;
    [SerializeField] TileBase currentTile;
    [SerializeField] PlayerController playerController;
    Dictionary<string, Tilemap> tilemaps;
    List<Vector3Int> touchedCellPositions = new();
    bool leftMouseDown, rightMouseDown; 
    bool playingLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTilemap = solidTileMap;

        tilemaps = new()
        {
            ["Base Blocks"] = solidTileMap,
            ["Interactables"] = triggerTilemap,
        };

        TileLogic.OnClearLevel += ClearLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if(playingLevel) return;

        HandleMouseInputs();
        PlaceTiles();
    }

    void PlaceTiles()
    {
        if(UIManager.instance.inMenu) return;

        if (leftMouseDown) 
        {
            Vector3Int cellPosition = GetCellPositionMouse();
            if(!touchedCellPositions.Contains(cellPosition)) 
            {
                touchedCellPositions.Add(cellPosition);
                CreateTile(cellPosition, currentTilemap);
            }
        }
        else if (rightMouseDown)
        {
            Vector3Int cellPosition = GetCellPositionMouse();
            if(!touchedCellPositions.Contains(cellPosition)) 
            {
                touchedCellPositions.Add(cellPosition);
                RemoveTile(cellPosition);
            }
        }
    }

    void HandleMouseInputs()
    {
        if(Input.GetMouseButton(0)) 
        {
            if(!IsHoveringOverButton()) leftMouseDown = true;
        }
        else leftMouseDown = false;

        if(Input.GetMouseButton(1)) 
        {
            if(!IsHoveringOverButton()) rightMouseDown = true;
        }
        else rightMouseDown = false;

        if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) touchedCellPositions.Clear();

    }

    public void PickNewTile(TileBase _newTile, Sprite _newSprite, string _tilemap)
    {
        currentTile = _newTile;
        UIManager.instance.UpdateSelectedTile(_newSprite);
        currentTilemap = tilemaps[_tilemap];
    }

    public void CreateTile(Vector3Int _cellPosition, Tilemap _tilemap)
    {
        if(startingTilemap.GetTile(_cellPosition) != null) return;
    
        _tilemap.SetTile(_cellPosition, currentTile);
        SaveAndLoad.instance.SaveTileData(currentTile.name, _cellPosition, _tilemap.name);
    }

    void RemoveTile(Vector3Int _cellPosition)
    {
        foreach(var kvp in tilemaps)
        {
            kvp.Value.SetTile(_cellPosition, null);
            SaveAndLoad.instance.SaveTileData("null", _cellPosition, kvp.Value.gameObject.name);
        }
    }

    public void ChangeLevelVariable(string _variable, int _increment)
    {
        LevelData levelData = SaveAndLoad.instance.LevelData;
        switch(_variable)
        {
            case "health":
                Debug.Log($"Current max health: {levelData.maxPlayerHealth}");
                int newHealthValue = levelData.maxPlayerHealth + _increment;
                if((newHealthValue < 1) || (newHealthValue > 10)) return;

                levelData.maxPlayerHealth = newHealthValue;
                Debug.Log($"New max health: {levelData.maxPlayerHealth}");

                UIManager.instance.DisplayHealthFromInt(newHealthValue, true);
                break;
            case "timer":
                Debug.Log($"Current completion time: {levelData.timeToComplete}");
                int newTimerValue = levelData.timeToComplete + _increment;
                if((newTimerValue < 5) || (newTimerValue > 300)) return;

                levelData.timeToComplete = newTimerValue;
                Debug.Log($"New completion time: {levelData.timeToComplete}");

                UIManager.instance.GetTextElementFromDict("TimerText").text = $"{newTimerValue}";
                break;
        }
    }

    public Vector3Int GetCellPositionMouse()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currentTilemap.WorldToCell(mouseWorldPos);
    }

    public bool IsHoveringOverButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.GetComponent<RectTransform>() != null)
            {
                return true;
            }
        }

        return false;
    }

    public void StartLevelClearing(bool _start, bool _clearing)
    {
        bool hasFinishFlag = false;
        foreach(TileData _tileData in SaveAndLoad.instance.LevelData.tiles)
        {
            if(_tileData.tileType == "Finish") hasFinishFlag = true;
        }
        if(!hasFinishFlag) 
        {
            StartCoroutine(UIManager.instance.ShowTextForSeconds("NoFinishFlag", "add a finish flag first!", 2f));
            return;
        }


        UIManager.instance.ToggleUIElement("EscapeMenu", false);
        UIManager.instance.ToggleUIElement("InitialEscMenu", !_start);
        UIManager.instance.ToggleUIElement("SelectedBlockButton", !_start);
        UIManager.instance.ToggleUIElement("BlockSelect", false);
        UIManager.instance.ToggleUIElement("Coins", _start);
        UIManager.instance.ToggleUIElement("GridRenderer", !_start);

        UIManager.instance.ToggleUIElement("LivesButtons", !_start);
        UIManager.instance.ToggleUIElement("TimerButtons", !_start); 

        UIManager.instance.GetUIElementFromDict("ParallaxBackground").transform.localPosition = new Vector3(-7.48f, -0.62f, 60);
        UIManager.instance.GetUIElementFromDict("bg1").transform.localPosition = new Vector3(3.6f, 0, 0);
        UIManager.instance.GetUIElementFromDict("bg2").transform.localPosition = new Vector3(28.8999996f, 0, 0);

        if(_clearing) 
        {
            UIManager.instance.ToggleUIElement("ClearingEscMenu", _start);
            UIManager.instance.ToggleUIElement("ClearDeathScreen", false);
        }
        else 
        {
            UIManager.instance.ToggleUIElement("PlayingEscMenu", _start);
            UIManager.instance.ToggleUIElement("PlayDeathScreen", false);
        }

        if (_start) 
        {
            playerController.StartPlaying();
            UIManager.instance.GetUIElementFromDict("Timer").transform.position = UIManager.instance.GetUIElementFromDict("TimerClearTransform").transform.position;
            UIManager.instance.GetUIElementFromDict("Lives").transform.position = UIManager.instance.GetUIElementFromDict("LivesClearTransform").transform.position;
        }
        else 
        {
            playerController.StopPlaying();
            SaveAndLoad.instance.BuildLevel();
            UIManager.instance.inMenu = false;
            UIManager.instance.GetUIElementFromDict("Timer").transform.position = UIManager.instance.GetUIElementFromDict("TimerEditTransform").transform.position;
            UIManager.instance.GetUIElementFromDict("Lives").transform.position = UIManager.instance.GetUIElementFromDict("LivesEditTransform").transform.position;
        }
        
        playingLevel = _start;
    }

    public void ClearLevel()
    {
        //Disable player movement
        if(playerController != null)
        {
            if(playerController.enabled)
            {
                playerController.KillMovement();
                playerController.enabled = false;
            }
        }
        

        //Variables in json to update
        string path;
        string boolToFlip;
        //Depends on if youre clearing or playing
        string uiElementToActivate;
        string levelIconTransform;
        bool _edit;

        //Update correct things based on playing vs clearing
        if (SceneData.loadBehaviour == "Play")
        {
            path = Path.Combine(Application.persistentDataPath, "Levels", "Play");
            _edit = false;
            boolToFlip = "isBeaten";
            uiElementToActivate = "LevelBeaten";
            levelIconTransform = "BeatenLevelTransform";

            //Update coin and timer high scores in UI
            float completionTime = playerController.GetTime();
            string formattedCompletionTime = LevelFunctions.instance.GetFormattedTimeFromFloat(completionTime);
            UIManager.instance.GetTextElementFromDict("BeatTime").text = $"Time:\n{formattedCompletionTime}";
            int coinsCollected = playerController.GetCoins();
            UIManager.instance.GetTextElementFromDict("BeatCoins").text = $"Coins: {coinsCollected}";

            //Get json
            string _pathToLevel = Path.Combine(path, SceneData.loadedLevelName + ".json");
            LevelData _levelData = LevelFunctions.instance.GetJsonFromPath(_pathToLevel);

            //Update coin and timer high scores in json
            float prevCompletionTime = _levelData.fastestTime;
            int prevCoinsCollected = _levelData.coinsCollected;

            if ((prevCompletionTime == 0) || (completionTime < prevCompletionTime)) LevelFunctions.instance.UpdateFieldInJson(_pathToLevel, "fastestTime", "float", newFloat: completionTime);
            if (coinsCollected > prevCoinsCollected) LevelFunctions.instance.UpdateFieldInJson(_pathToLevel, "coinsCollected", "int", newFloat: coinsCollected);
        }
        else if (SceneData.loadBehaviour == "Clear")
        {
            path = Path.Combine(Application.persistentDataPath, "Levels", "Edit");
            boolToFlip = "isCleared";
            uiElementToActivate = "LevelCleared";
            levelIconTransform = "ClearedLevelTransform";
            _edit = true;
        }
        else
        {
            Debug.LogWarning($"Finish flag hit in: {SceneData.loadBehaviour}");
            return;
        }

        //Update variables in json
        string levelName = SceneData.loadedLevelName;
        string pathToLevel = Path.Combine(path, levelName + ".json");
        LevelFunctions.instance.UpdateFieldInJson(pathToLevel, boolToFlip, "bool", newBool: true);

        UIManager.instance.ToggleUIElement(uiElementToActivate, true);
        UIManager.instance.InstantiateLevelObject(UIManager.instance.GetUIElementFromDict(levelIconTransform).transform, levelName, path, _edit, true);
    }
}
