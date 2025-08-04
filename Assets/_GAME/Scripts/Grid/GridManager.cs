namespace _GAME.Scripts.Grid
{
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Extensions;
    using _GAME.Scripts.Tile;
    using UnityEngine;

    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")] [SerializeField] private GameObject cellPrefab;
        [SerializeField]                           private Transform  gridParent;

        [Header("Runtime Grid Info")] [SerializeField] private int   rows;
        [SerializeField]                               private int   cols;
        [SerializeField]                               private float cellSize = 1f;

        [Header("Dependencies")] [SerializeField] private LineDrawer lineDrawer;

        public static GridManager                                Instance { get; private set; }
        private       GridData                                   gridData;
        private       GameObject[,]                              cellObjects;
        private       List<TileDB>                               allTileData => TileManager.Instance.tileDataList;
        private       Dictionary<TileType, ObjectPool<TileView>> _tilePools = new Dictionary<TileType, ObjectPool<TileView>>();

        #region UNITY-CALLBACKS

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            this.OnInit();

            SetupGrid();
        }

        #endregion

        #region PUBLIC-METHODS

        public void ClearMatch(TileView tile1, TileView tile2)
        {
            if (this._tilePools.ContainsKey(tile1.Type))
            {
                this._tilePools[tile1.Type].ReturnToPool(tile1);
            }
            if (this._tilePools.ContainsKey(tile2.Type))
            {
                this._tilePools[tile2.Type].ReturnToPool(tile2);
            }
        }

        public List<Vector2Int> IsMatchValid(TileView tile1, TileView tile2)
        {
            if (tile1.Type != tile2.Type) return null;

            Vector2Int pos1 = new Vector2Int(tile1.GridPosition.x + 1, tile1.GridPosition.y + 1);
            Vector2Int pos2 = new Vector2Int(tile2.GridPosition.x + 1, tile2.GridPosition.y + 1);

            // Try to find a path and store it.
            List<Vector2Int> path;

            path = CheckLineMatch(pos1, pos2);
            if (path != null) return path;

            path = CheckL_ShapeMatch(pos1, pos2);
            if (path != null) return path;

            // TODO: The Z/U shape check will go here.

            return null; // No path found
        }

        public Vector3 GetWorldPositionForPaddedGrid(Vector2Int paddedPos)
        {
            // Convert from padded (1-based) to real (0-based)
            int realRow = paddedPos.y - 1;
            int realCol = paddedPos.x - 1;

            // Check bounds (for points in the padding area)
            if (realRow < 0 || realRow >= this.rows || realCol < 0 || realCol >= this.cols)
            {
                // This part requires careful calculation based on your grid's centering logic.
                // A simple approximation:
                float x = (realCol + 0.5f - this.cols / 2f) * cellSize;
                float y = -(realRow + 0.5f - this.rows / 2f) * cellSize;
                return new Vector3(x, y, 0); // This will need tuning to match your grid layout
            }

            // For points inside the real grid, we can just get the cell's world position.
            return gridData.cells[realRow, realCol].worldPos;
        }

        public void DrawPath(List<Vector2Int> path)
        {
            if (path == null)
            {
                lineDrawer.Hide();
                return;
            }

            // Convert List<Vector2Int> (grid positions) to Vector3[] (world positions)
            Vector3[] worldPoints = path.Select(p => GetWorldPositionForPaddedGrid(p)).ToArray();
            lineDrawer.Draw(worldPoints);
        }

        public void HidePath()
        {
            lineDrawer.Hide();
        }

        #endregion

        #region PRIVATE-METHODS

        private void OnInit()
        {
            if (cellPrefab == null)
            {
                Debug.LogError("Cell Prefab is not assigned in GridManager!");
            }

            if (gridParent == null)
            {
                Debug.LogError("Grid Parent Transform is not assigned in GridManager!");
            }

            foreach (var tileData in allTileData)
            {
                if (tileData.TileType == TileType.None || tileData.TilePrefab == null) continue;
                var tilePrefab = tileData.TilePrefab.GetComponent<TileView>();
                if (tilePrefab != null)
                {
                    var tilePool = new ObjectPool<TileView>(tilePrefab, 20);
                    _tilePools.Add(tileData.TileType, tilePool);
                }
            }
            Debug.Log($"Initialized {allTileData.Count} tile types with their respective pools.");
        }

        private void SetupGrid()
        {
            var newRows = rows;
            var newCols = cols;

            if ((newRows * newCols) % 2 != 0)
            {
                if (newCols % 2 != 0)
                    newCols = Mathf.Max(2, newCols - 1);
                else
                    newRows = Mathf.Max(2, newRows - 1);
            }

            this.rows = newRows;
            this.cols = newCols;

            gridData = new GridData(newRows, newCols);
            GridCalculator.CalculateWorldPositions(gridData, cellSize, Camera.main);
            cellObjects = new GameObject[newRows, newCols];

            Debug.Log($"Grid initialized: {newRows}x{newCols} cells, Cell size: {cellSize}");

            SpawnEmptyCells(newRows, newCols);

            GenerateAndPlaceTiles();
        }

        private void SpawnEmptyCells(int newRows, int newCols)
        {
            for (int row = 0; row < newRows; row++)
            {
                for (int col = 0; col < newCols; col++)
                {
                    Vector3    worldPos = gridData.cells[row, col].worldPos;
                    GameObject cellObj  = Instantiate(cellPrefab, worldPos, Quaternion.identity, gridParent);
                    cellObj.name          = $"Cell_{row}_{col}";
                    cellObjects[row, col] = cellObj;
                }
            }
            Debug.Log($"Spawned {newRows * newCols} empty cells successfully!");
        }

        private void GenerateAndPlaceTiles()
        {
            int totalCells = this.rows * this.cols;

            var availableTileTypes = allTileData
                .Where(t => t.TileType != TileType.None)
                .Select(t => t.TileType)
                .ToList();

            if (availableTileTypes.Count == 0)
            {
                return;
            }

            int maxPossibleTypes = totalCells / 2;
            int numTypesToUse    = Mathf.Min(availableTileTypes.Count, maxPossibleTypes);

            var selectedTileTypes = availableTileTypes.OrderBy(x => Random.value).Take(numTypesToUse).ToList();
            Debug.Log($"Will use {numTypesToUse} of tile: " + string.Join(", ", selectedTileTypes));

            var tileCounts = new Dictionary<TileType, int>();

            foreach (var type in selectedTileTypes)
            {
                tileCounts[type] = 2;
            }

            int remainingPairs = (totalCells - (numTypesToUse * 2)) / 2;
            for (int i = 0; i < remainingPairs; i++)
            {
                int      randomIndex = Random.Range(0, selectedTileTypes.Count);
                TileType randomType  = selectedTileTypes[randomIndex];
                tileCounts[randomType] += 2;
            }

            var tilesToPlace = new List<TileType>(totalCells);
            foreach (var pair in tileCounts)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    tilesToPlace.Add(pair.Key);
                }
            }

            //Fisher-Yates
            for (int i = 0; i < tilesToPlace.Count - 1; i++)
            {
                int      randomIndex = Random.Range(i, tilesToPlace.Count);
                TileType temp        = tilesToPlace[i];
                tilesToPlace[i]           = tilesToPlace[randomIndex];
                tilesToPlace[randomIndex] = temp;
            }

            int tileIndex = 0;
            for (int row = 0; row < this.rows; row++)
            {
                for (int col = 0; col < this.cols; col++)
                {
                    SpawnTileAt(row, col, tilesToPlace[tileIndex]);
                    tileIndex++;
                }
            }

            Debug.Log("Placed all tiles successfully according to the algorithm.");
        }

        private void SpawnTileAt(int row, int col, TileType tileType)
        {
            if (!this.gridData.IsValidPosition(row, col))
            {
                return;
            }

            var cellObj = cellObjects[row, col];
            if (cellObj == null)
            {
                return;
            }

            var tileData = allTileData.Find(t => t.TileType == tileType);
            if (tileData == null)
            {
                return;
            }

            if (!this._tilePools.ContainsKey(tileType))
            {
                Debug.LogWarning($"No tile pool found for {tileType}. Skipping tile placement.");
                return;
            }

            var pool    = this._tilePools[tileType];
            var tileObj = pool.Spawn(cellObj.transform.position, cellObj.transform.rotation);
            tileObj.transform.SetParent(cellObj.transform);
            tileObj.transform.localPosition = Vector3.zero;

            var tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Type         = tileType;
                tileView.GridPosition = new Vector2Int(col, row); // Note: x=col, y=row
            }
            else
            {
                Debug.LogWarning($"Tile prefab for {tileType} does not have a TileView component!");
            }

            gridData.cells[row, col].tileType = tileType;
            gridData.cells[row, col].isActive = true;
            tileObj.name                      = $"Tile_{tileType}_{row}_{col}";
        }

        private bool IsCellEmpty(Vector2Int pos)
        {
            // Check if the coordinate is within the outer bounds of the padded grid.
            // Padded grid dimensions are (rows+2) x (cols+2).
            if (pos.y < 0 || pos.y >= this.rows + 2 || pos.x < 0 || pos.x >= this.cols + 2)
            {
                return false; // Path is trying to go out of bounds.
            }

            if (pos.y == 0 || pos.y == this.rows + 1 || pos.x == 0 || pos.x == this.cols + 1)
            {
                return true;
            }

            var realRow = pos.y - 1;
            var realCol = pos.x - 1;

            // The cell is "empty" if the tile at that position is NOT active.
            return !gridData.cells[realRow, realCol].isActive;
        }

        #endregion

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

                // Success! Build the path.
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

                // Success! Build the path.
                path.Add(pos1);
                path.Add(pos2);
                return path;
            }

            return null; // Failure
        }

        private List<Vector2Int> CheckL_ShapeMatch(Vector2Int pos1, Vector2Int pos2)
        {
            Vector2Int corner1 = new Vector2Int(pos1.x, pos2.y);
            Vector2Int corner2 = new Vector2Int(pos2.x, pos1.y);

            if (IsCellEmpty(corner1))
            {
                // Check for path via corner1. NOTE: The line check itself returns null on failure.
                var path1 = CheckLineMatch(pos1, corner1);
                var path2 = CheckLineMatch(corner1, pos2);

                if (path1 != null && path2 != null)
                {
                    // Success! Combine the paths. Use Skip(1) to avoid adding the corner twice.
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

        #endregion
    }
}