namespace _GAME.Scripts.Grid
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Pathfinder
    {
        private GridData _gridData;
        private int      _rows, _cols;

        public void SetGridData(GridData gridData)
        {
            _gridData = gridData;
            _rows     = gridData.rows;
            _cols     = gridData.cols;
        }

        public List<Vector2Int> FindPath(Vector2Int pos1Padded, Vector2Int pos2Padded)
        {
            if (_gridData == null)
            {
                Debug.LogError("Pathfinder has no GridData!");
                return null;
            }

            List<Vector2Int> path;

            path = CheckLineMatch(pos1Padded, pos2Padded);
            if (path != null) return path;

            path = CheckL_ShapeMatch(pos1Padded, pos2Padded);
            if (path != null) return path;

            path = CheckZ_U_ShapeMatch(pos1Padded, pos2Padded);
            if (path != null) return path;

            return null; // No path found
        }

        private bool IsCellEmpty(Vector2Int pos)
        {
            if (pos.y < 0 || pos.y >= _rows + 2 || pos.x < 0 || pos.x >= _cols + 2)
            {
                return false;
            }
            if (pos.y == 0 || pos.y == _rows + 1 || pos.x == 0 || pos.x == _cols + 1)
            {
                return true;
            }
            var realRow = pos.y - 1;
            var realCol = pos.x - 1;
            return !_gridData.cells[realRow, realCol].isActive;
        }

        #region CHECK-MATCHES

        private List<Vector2Int> CheckLineMatch(Vector2Int pos1, Vector2Int pos2)
        {
            var path = new List<Vector2Int>();

            // Check for same column
            if (pos1.x == pos2.x)
            {
                int col  = pos1.x;
                int minY = Mathf.Min(pos1.y, pos2.y);
                int maxY = Mathf.Max(pos1.y, pos2.y);

                for (int row = minY + 1; row < maxY; row++)
                {
                    if (!IsCellEmpty(new Vector2Int(col, row))) return null; // Obstacle found, return failure
                }

                path.Add(pos1);
                path.Add(pos2);
                return path;
            }

            // Check for same row
            if (pos1.y == pos2.y)
            {
                int row  = pos1.y;
                int minX = Mathf.Min(pos1.x, pos2.x);
                int maxX = Mathf.Max(pos1.x, pos2.x);

                for (int col = minX + 1; col < maxX; col++)
                {
                    if (!IsCellEmpty(new Vector2Int(col, row))) return null; // Obstacle found, return failure
                }

                path.Add(pos1);
                path.Add(pos2);
                return path;
            }

            return null; // Failure
        }

        private List<Vector2Int> CheckL_ShapeMatch(Vector2Int pos1, Vector2Int pos2)
        {
            var corner1 = new Vector2Int(pos1.x, pos2.y);
            var corner2 = new Vector2Int(pos2.x, pos1.y);

            if (IsCellEmpty(corner1))
            {
                var path1 = CheckLineMatch(pos1, corner1);
                var path2 = CheckLineMatch(corner1, pos2);

                if (path1 != null && path2 != null)
                {
                    return path1.Concat(path2.Skip(1)).ToList();
                }
            }

            if (IsCellEmpty(corner2))
            {
                // Check for path via corner2.
                var path1 = CheckLineMatch(pos1, corner2);
                var path2 = CheckLineMatch(corner2, pos2);

                if (path1 != null && path2 != null)
                {
                    return path1.Concat(path2.Skip(1)).ToList();
                }
            }

            return null; // Failure
        }

        private List<Vector2Int> CheckZ_U_ShapeMatch(Vector2Int pos1, Vector2Int pos2)
        {
            // --- Scan RIGHT from pos1 ---
            for (int x = pos1.x + 1; x < this._cols + 2; x++)
            {
                var currentPos = new Vector2Int(x, pos1.y);
                if (!IsCellEmpty(currentPos)) break; // Stop if we hit an obstacle

                // Try to find an L-path from this empty cell to the destination
                var lPath = CheckL_ShapeMatch(currentPos, pos2);
                if (lPath != null)
                {
                    // SUCCESS! We found a path. Now, construct the full path.
                    var fullPath = new List<Vector2Int> { pos1 };
                    fullPath.AddRange(lPath);
                    return fullPath;
                }
            }

            // --- Scan LEFT from pos1 ---
            for (int x = pos1.x - 1; x >= 0; x--)
            {
                var currentPos = new Vector2Int(x, pos1.y);
                if (!IsCellEmpty(currentPos)) break;

                var lPath = CheckL_ShapeMatch(currentPos, pos2);
                if (lPath != null)
                {
                    var fullPath = new List<Vector2Int> { pos1 };
                    fullPath.AddRange(lPath);
                    return fullPath;
                }
            }

            // --- Scan DOWN from pos1 ---
            for (int y = pos1.y + 1; y < this._rows + 2; y++)
            {
                var currentPos = new Vector2Int(pos1.x, y);
                if (!IsCellEmpty(currentPos)) break;

                var lPath = CheckL_ShapeMatch(currentPos, pos2);
                if (lPath != null)
                {
                    var fullPath = new List<Vector2Int> { pos1 };
                    fullPath.AddRange(lPath);
                    return fullPath;
                }
            }

            // --- Scan UP from pos1 ---
            for (int y = pos1.y - 1; y >= 0; y--)
            {
                var currentPos = new Vector2Int(pos1.x, y);
                if (!IsCellEmpty(currentPos)) break;

                var lPath = CheckL_ShapeMatch(currentPos, pos2);
                if (lPath != null)
                {
                    var fullPath = new List<Vector2Int> { pos1 };
                    fullPath.AddRange(lPath);
                    return fullPath;
                }
            }

            // No two-turn path was found in any direction
            return null;
        }

        #endregion
    }
}