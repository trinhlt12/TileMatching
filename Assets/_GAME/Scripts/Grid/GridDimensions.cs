namespace _GAME.Scripts.Grid
{
    using UnityEngine;

    public class GridDimensions
    {
        public int   rows;
        public int   cols;
        public float tileSize = 1f;

        public GridDimensions(int rows, int cols, float tileSize)
        {
            this.rows = rows;
            this.cols = cols;
            this.tileSize = tileSize;
        }

        public int TotalCells => rows * cols;
        public Vector2 GridSize => new Vector2(cols * tileSize, rows * tileSize);

        public override string ToString()
        {
            return $"Grid: {rows}x{cols}, TileSize: {tileSize}";
        }
    }
}