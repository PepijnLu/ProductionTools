using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public string levelName;
    public List<TileData> tiles = new();
    public bool isCleared, isBeaten;
    public int timeToComplete = 300;
    public int maxPlayerHealth = 6;
    public float fastestTime;
    public float playerStartX = 2.5f, playerStartY = 2.5f;
    public int coinsCollected;
}

public struct TileData
{
    public string tileMap;
    public string tileType;
    public Vector3Int position;
}
