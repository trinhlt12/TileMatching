namespace _GAME.Scripts.Tile
{
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class TileView : MonoBehaviour
    {
        public TileType   Type         { get; set; }
        public Vector2Int GridPosition { get; set; }

        public void SetHighlight(bool highlight){ }

    }
}