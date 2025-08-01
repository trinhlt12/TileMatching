namespace _GAME.Scripts.Grid
{
    using _GAME.Scripts.Tile;
    using UnityEngine;

    [System.Serializable]
    public class GridCell
    {
        public TileType   tileType;
        public bool       isActive;
        public Vector2Int gridPos;  // Logic position (row, col)
        public Vector3    worldPos; // World position for rendering

        public GridCell(int row, int col)
        {
            gridPos  = new Vector2Int(col, row); // Note: x=col, y=row
            isActive = false;
            tileType = 0;
        }
    }

    public class GridData
    {
        public GridCell[,] cells;
        public int         rows, cols;

        public GridData(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            cells     = new GridCell[rows, cols];

            // Initialize all cells
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    cells[row, col] = new GridCell(row, col);
                }
            }
        }

        // Helper methods
        public bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }

        public GridCell GetCell(int row, int col)
        {
            if (IsValidPosition(row, col)) return cells[row, col];
            return null;
        }
    }
}