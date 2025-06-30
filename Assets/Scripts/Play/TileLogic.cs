using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
[System.Serializable]
public class TileInfo
{
    public TileBase tile;
    public string actionToInvoke;
    public int relevantValue;
}

public class TileLogic : MonoBehaviour
{
    public static TileLogic instance;
    [SerializeField] List<TileInfo> tileInfo;
    bool isSpikeOnCooldown;
    public delegate void TileActionHandler(TileBase tile, Tilemap tilemap, Vector3Int cellPos, IInvoker invoker, float value);
    private Dictionary<TileBase, TileInfo> tileInfoLookup = new();
    private Dictionary<string, TileActionHandler> delegateLookup = new();
    public static event Action OnClearLevel;
    [SerializeField] float spikeCooldown;
    void Awake()
    {
        instance = this;
        InstantiateTileActions();
    }
    void Start()
    {
        foreach (TileInfo _info in tileInfo) tileInfoLookup.Add(_info.tile, _info);

    }

    public void InvokeTileAction(TileBase _tile, Tilemap _tilemap, Vector3Int _cellPos, IInvoker _invoker)
    {
        Debug.Log($"Tile: {_tile.name} on {_tilemap} at {_cellPos} was triggered by {_invoker}");
        TileInfo tileInfo = tileInfoLookup[_tile];  
        TileActionHandler actionToInvoke = delegateLookup[tileInfo.actionToInvoke];
        actionToInvoke.Invoke(_tile, _tilemap, _cellPos, _invoker, tileInfo.relevantValue);
    }

    void InstantiateTileActions()
    {
        delegateLookup["Finish"] = (tile, tilemap, cellPos, invoker, value) =>
        {
            if (invoker.IsPlayer()) OnClearLevel.Invoke();
        };
        
        delegateLookup["CollectCoin"] = (tile, tilemap, cellPos, invoker, value) =>
        {
            invoker.CollectCoin(tilemap, cellPos);
        };

        delegateLookup["TakeDamage"] = (tile, tilemap, cellPos, invoker, value) =>
        {
            if(!isSpikeOnCooldown) 
            {
                invoker.TakeDamage((int)value);
                isSpikeOnCooldown = true;
                StartCoroutine(SpikeCooldown());
            }
        };
    }

    IEnumerator SpikeCooldown()
    {
        yield return new WaitForSeconds(spikeCooldown);
        isSpikeOnCooldown = false;
    }
}
