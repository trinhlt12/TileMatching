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

        public bool IsMatchValid(TileView tile1, TileView tile2)
        {
            // Rule 0: Tiles must be of the same type.
            if (tile1.Type != tile2.Type)
            {
                return false;
            }

            // Convert real grid coordinates to our virtual "padded grid" coordinates.
            // The tile at (0,0) in the real grid is at (1,1) in the padded grid.
            var pos1 = new Vector2Int(tile1.GridPosition.x + 1, tile1.GridPosition.y + 1);
            var pos2 = new Vector2Int(tile2.GridPosition.x + 1, tile2.GridPosition.y + 1);

            // Rule 1: Check for a direct line connection (0 turns).
            if (CheckLineMatch(pos1, pos2))
            {
                return true;
            }

            // TODO: We will add the other checks here later.
            // if (CheckL_ShapeMatch(pos1, pos2)) { return true; }
            // if (CheckZ_U_ShapeMatch(pos1, pos2)) { return true; }

            return false;
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

        private bool CheckLineMatch(Vector2Int pos1, Vector2Int pos2)
        {
            // Check for same column
            if (pos1.x == pos2.x)
            {
                int col = pos1.x;
                // Determine the start and end of the path to check.
                int minY = Mathf.Min(pos1.y, pos2.y);
                int maxY = Mathf.Max(pos1.y, pos2.y);

                for (int row = minY + 1; row < maxY; row++)
                {
                    if (!IsCellEmpty(new Vector2Int(col, row)))
                    {
                        return false; // Found an obstacle.
                    }
                }
                return true; // Path is clear.
            }

            // Check for same row
            if (pos1.y == pos2.y)
            {
                int row  = pos1.y;
                int minX = Mathf.Min(pos1.x, pos2.x);
                int maxX = Mathf.Max(pos1.x, pos2.x);

                for (int col = minX + 1; col < maxX; col++)
                {
                    if (!IsCellEmpty(new Vector2Int(col, row)))
                    {
                        return false; // Found an obstacle.
                    }
                }
                return true; // Path is clear.
            }

            return false; // Not on the same row or column.
        }

        #endregion
    }
}