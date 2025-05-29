using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] Tilemap currentTilemap;
    [SerializeField] TileBase currentTile;
    List<Vector3Int> touchedCellPositions = new();
    bool leftMouseDown, rightMouseDown; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                CreateTile(cellPosition);
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

    public void PickNewTile(TileBase _newTile, Sprite _newSprite)
    {
        currentTile = _newTile;
        UIManager.instance.UpdateSelectedTile(_newSprite);
    }

    void CreateTile(Vector3Int _cellPosition)
    {
        currentTilemap.SetTile(_cellPosition, currentTile);
        SaveAndLoad.instance.SaveTileData(currentTile.name, _cellPosition, currentTilemap.name);
    }

    void RemoveTile(Vector3Int _cellPosition)
    {
        currentTilemap.SetTile(_cellPosition, null);
        SaveAndLoad.instance.SaveTileData("null", _cellPosition, "null");
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
