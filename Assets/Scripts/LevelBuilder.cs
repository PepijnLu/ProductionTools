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
            CreateTile();
        }
    }

    public void PickNewTile(TileBase _newTile, Sprite _newSprite)
    {
        currentTile = _newTile;
        UIManager.instance.UpdateSelectedTile(_newSprite);
    }

    void CreateTile()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = currentTilemap.WorldToCell(mouseWorldPos);

        currentTilemap.SetTile(cellPosition, currentTile);
    }
}
