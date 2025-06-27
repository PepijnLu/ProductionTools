using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
[System.Serializable]
public class TileEvent
{
    public TileBase tile;
    public UnityEvent action;
}

public class TileLogic : MonoBehaviour
{
    public static TileLogic instance;
    [SerializeField] List<TileEvent> tileEvents;
    private Dictionary<TileBase, UnityEvent> eventLookup;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        eventLookup = new();
        foreach (var entry in tileEvents) eventLookup.Add(entry.tile, entry.action);
        
    }

    public void InvokeTileAction(TileBase _tile, GameObject _invoker)
    {
        UnityEvent action = eventLookup[_tile];
        action.Invoke();
    }
}
