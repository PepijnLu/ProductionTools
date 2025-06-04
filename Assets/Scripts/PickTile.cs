using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PickTile : MonoBehaviour
{
    LevelBuilder levelBuilder;
    public Image borderImage, tileImage;
    public TileBase _tileToPick;
    public Sprite _newSprite;
    public string tilemap;

    public void ChooseNewTile()
    {
        if(levelBuilder == null) levelBuilder = FindFirstObjectByType<LevelBuilder>();
        levelBuilder.PickNewTile(_tileToPick, _newSprite, tilemap);
    }
}
