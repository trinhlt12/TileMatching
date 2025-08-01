namespace _GAME.Scripts.Tile
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "TileDB", menuName = "ScriptableObjects/TileDB", order = 1)]
    public class TileDB : ScriptableObject
    {
        public GameObject TilePrefab;
        public TileType TileType;
        public Sprite TileImage;
    }
}