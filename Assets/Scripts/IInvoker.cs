using UnityEngine;
using UnityEngine.Tilemaps;

public interface IInvoker
{
    public void CollectCoin(Tilemap _tilemap, Vector3Int _cellPos);
    public void TakeDamage(int _value);
    public void Heal(int _value, Tilemap _tilemap, Vector3Int _cellPos);
    public bool IsPlayer();
}
