namespace _GAME.Scripts.Grid
{
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Extensions;
    using _GAME.Scripts.Level;
    using _GAME.Scripts.Tile;
    using UnityEngine;

    public class GridManager : MonoBehaviour
    {
        #region FIELDS

        [Header("Grid Settings")] [SerializeField] private GameObject cellPrefab;
        [SerializeField]                           private Transform  gridParent;

        private       int       rows;
        private       int       cols;
        private       LevelData currentLevelData;
        private const float     CELL_SIZE = 1f;

        [Header("Dependencies")] [SerializeField] private LineDrawer lineDrawer;

        public static GridManager                                Instance { get; private set; }
        private       GridData                                   gridData;
        private       GameObject[,]                              cellObjects;
        private       List<TileDB>                               allTileData => TileManager.Instance.tileDataList;
        private       Dictionary<TileType, ObjectPool<TileView>> _tilePools = new Dictionary<TileType, ObjectPool<TileView>>();

        #endregion

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

        [Header("Testing")]
        [SerializeField] private int levelToTest = 1;
        private void Start()
        {
            this.OnInit();
            LevelData testLevelData = LevelLoader.LoadLevel(levelToTest);
            if (testLevelData != null)
            {
                SetupGridFromData(testLevelData);
            }
            else
            {
                Debug.LogError($"Failed to load level {levelToTest}. Please check the level data.");
            }
            GameManager.OnGameStateChanged += HandleGameStateChange;

            /*SetupGrid();*/
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChange;
        }

        #endregion

        #region PUBLIC-METHODS

        public void StartLevel()
        {
            // ClearOldGrid();

            OnInit();
            GameManager.Instance.UpdateGameState(GameState.Playing);
        }

        public void SetupGridFromData(LevelData levelData)
        {
            if (levelData == null)
            {
                return;
            }

            this.currentLevelData = levelData;
            this.rows             = levelData.gridSize.rows;
            this.cols             = levelData.gridSize.cols;

            InitializeGrid();
        }

        public void ClearMatch(TileView tile1, TileView tile2)
        {
            var cell1 = gridData.GetCell(tile1.GridPosition.y, tile1.GridPosition.x);
            if (cell1 != null)
            {
                cell1.isActive          = false;
                cell1.tileViewReference = null;
            }

            var cell2 = gridData.GetCell(tile2.GridPosition.y, tile2.GridPosition.x);
            if (cell2 != null)
            {
                cell2.isActive          = false;
                cell2.tileViewReference = null;
            }

            if (gridData.IsValidPosition(tile1.GridPosition.y, tile1.GridPosition.x))
            {
                gridData.cells[tile1.GridPosition.y, tile1.GridPosition.x].isActive = false;
            }
            if (gridData.IsValidPosition(tile2.GridPosition.y, tile2.GridPosition.x))
            {
                gridData.cells[tile2.GridPosition.y, tile2.GridPosition.x].isActive = false;
            }
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

            path = CheckZ_U_ShapeMatch(pos1, pos2);
            if (path != null) return path;

            return null; // No path found
        }

        public Vector3 GetWorldPositionForPaddedGrid(Vector2Int paddedPos)
        {
            // Convert from padded (1-based) to real (0-based)
            int realRow = paddedPos.y - 1;
            int realCol = paddedPos.x - 1;

            if (realRow >= 0 && realRow < this.rows && realCol >= 0 && realCol < this.cols)
            {
                return gridData.cells[realRow, realCol].worldPos;
            }
            int     clampedRow      = Mathf.Clamp(realRow, 0, this.rows - 1);
            int     clampedCol      = Mathf.Clamp(realCol, 0, this.cols - 1);
            Vector3 adjacentCellPos = gridData.cells[clampedRow, clampedCol].worldPos;
            float   offsetX         = (realCol - clampedCol) * CELL_SIZE;
            float   offsetY         = -(realRow - clampedRow) * CELL_SIZE;
            return adjacentCellPos + new Vector3(offsetX, offsetY, 0);
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

        public List<TileView> GetAllActiveTiles()
        {
            var activeTiles = new List<TileView>();
            for (int r = 0; r < this.rows; r++)
            {
                for (int c = 0; c < this.cols; c++)
                {
                    if (gridData.cells[r, c].isActive && gridData.cells[r, c].tileViewReference != null)
                    {
                        activeTiles.Add(gridData.cells[r, c].tileViewReference);
                    }
                }
            }
            return activeTiles;
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

        private void HandleGameStateChange(GameState newState)
        {
            if (newState == GameState.LevelSetup)
            {
                Debug.Log("Game state changed to LevelSetup. Reinitializing grid.");
                StartLevel();
            }
        }

        private void InitializeGrid()
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
            GridCalculator.CalculateWorldPositions(gridData, CELL_SIZE, Camera.main);
            cellObjects = new GameObject[newRows, newCols];

            Debug.Log($"Grid initialized: {newRows}x{newCols} cells, Cell size: {CELL_SIZE}");

            SpawnCellsFromLayout();

            GenerateAndPlaceTiles();
        }

        private void SpawnCellsFromLayout()
        {
            for (int row = 0; row < this.rows; row++)
            {
                for (int col = 0; col < this.cols; col++)
                {
                    if (this.currentLevelData.layout[row][col] == '1')
                    {
                        Vector3    worldPos = gridData.cells[row, col].worldPos;
                        GameObject cellObj  = Instantiate(cellPrefab, worldPos, Quaternion.identity, gridParent);
                        cellObj.name          = $"Cell_{row}_{col}";
                        cellObjects[row, col] = cellObj;
                    }
                    else
                    {
                        cellObjects[row, col]             = null;
                        gridData.cells[row, col].isActive = false;
                    }
                }
            }
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

            if (this.currentLevelData == null)
            {
                return;
            }

            var validCellPositions = new List<Vector2Int>();
            for (int row = 0; row < this.rows; row++)
            {
                for (int col = 0; col < this.cols; col++)
                {
                    if (this.currentLevelData.layout[row][col] == '1')
                    {
                        validCellPositions.Add(new Vector2Int(col, row));
                    }
                }
            }

            int totalCells = validCellPositions.Count;

            if (totalCells == 0 || totalCells % 2 != 0)
            {
                return;
            }

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

            if (numTypesToUse == 0)
            {
                return;
            }

            var selectedTileTypes = availableTileTypes.OrderBy(x => Random.value).Take(numTypesToUse).ToList();

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
            for (int i = 0; i < tilesToPlace.Count - 1; i++)
            {
                int randomIndex = Random.Range(i, tilesToPlace.Count);
                (tilesToPlace[i], tilesToPlace[randomIndex]) = (tilesToPlace[randomIndex], tilesToPlace[i]); // C# tuple swap
            }
            for (int i = 0; i < totalCells; i++)
            {
                Vector2Int positionToPlace = validCellPositions[i];
                TileType   tileToPlace     = tilesToPlace[i];

                SpawnTileAt(positionToPlace.y, positionToPlace.x, tileToPlace);
            }
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
                tileView.Type                              = tileType;
                tileView.GridPosition                      = new Vector2Int(col, row); // Note: x=col, y=row
                gridData.cells[row, col].tileViewReference = tileView;
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
            for (int x = pos1.x + 1; x < this.cols + 2; x++)
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
            for (int y = pos1.y + 1; y < this.rows + 2; y++)
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