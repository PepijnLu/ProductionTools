using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] Tilemap currentTilemap;
    [SerializeField] TileBase currentTile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) 
        {
            Vector3Int cellPosition = GetCellPositionMouse();
            CreateTile(cellPosition);
        }
        if (Input.GetMouseButton(1)) 
        {
            Vector3Int cellPosition = GetCellPositionMouse();
            RemoveTile(cellPosition);
        }
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
}
