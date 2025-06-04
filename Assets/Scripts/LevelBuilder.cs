using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] Tilemap currentTilemap;
    [SerializeField] Tilemap solidTileMap, triggerTilemap, startingTilemap;
    [SerializeField] TileBase currentTile;
    Dictionary<string, Tilemap> tilemaps;
    List<Vector3Int> touchedCellPositions = new();
    bool leftMouseDown, rightMouseDown; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTilemap = solidTileMap;

        tilemaps = new()
        {
            ["Base Blocks"] = solidTileMap,
            ["Items"] = triggerTilemap,
        };

        foreach (Transform _child in startingTilemap.transform)
        {
            Vector3Int cellPosition = startingTilemap.WorldToCell(_child.position);
            CreateTile(cellPosition, startingTilemap);
        }
    }

    // Update is called once per frame
    void Update()
    {
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

    void CreateTile(Vector3Int _cellPosition, Tilemap _tilemap)
    {
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
}
