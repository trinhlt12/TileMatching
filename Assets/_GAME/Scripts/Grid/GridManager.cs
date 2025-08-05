namespace _GAME.Scripts.Grid
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Extensions;
    using _GAME.Scripts.Level;
    using _GAME.Scripts.Services;
    using _GAME.Scripts.Tile;
    using DG.Tweening;
    using UnityEngine;

    public class GridManager : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform  gridParent;

        [Header("VFX")] [SerializeField] private GameObject matchVFXPrefab;

        public const  float SPAWN_ANIMATION_DURATION = 0.5f;
        public const  float SPAWN_STAGGER_PER_TILE   = 0.03f;
        private const float CAMERA_PADDING           = 2f;

        private       int       rows;
        private       int       cols;
        private       LevelData currentLevelData;
        private const float     CELL_SIZE = 1f;

        [Header("Dependencies")] [SerializeField] private LineDrawer lineDrawer;

        private GridData      gridData;
        private GameObject[,] cellObjects;

        private List<TileDB>   allTileData => this._tileManager.tileDataList;
        private List<TileView> _activeTiles = new List<TileView>();

        private ObjectPool<TileView>       _tilePool;
        private ObjectPool<ParticleSystem> _vfxPool;

        private GameManager _gameManager;
        private TileManager _tileManager;
        private Pathfinder  _pathfinder;
        private CameraController _cameraController;

        #endregion

        #region UNITY-CALLBACKS

        private void Awake()
        {
            ServiceLocator.Register(this);
            this._pathfinder = new Pathfinder();

            this.OnInit();
        }

        private void Start()
        {
            GameManager.OnGameStateChanged += HandleGameStateChange;

            _gameManager = ServiceLocator.Get<GameManager>();
            _tileManager = ServiceLocator.Get<TileManager>();
            _cameraController = ServiceLocator.Get<CameraController>();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GridManager>();

            GameManager.OnGameStateChanged -= HandleGameStateChange;
        }

        #endregion

        #region PUBLIC-METHODS

        public float SetupGridFromData(LevelData levelData)
        {
            if (levelData == null)
            {
                return 0f;
            }

            ClearOldGrid();

            this.currentLevelData = levelData;
            this.rows             = levelData.gridSize.rows;
            this.cols             = levelData.gridSize.cols;

            InitializeGrid();

            float totalAnimationTime = GenerateAndPlaceTiles();
            while (this._tileManager.IsDeadlocked())
            {
                Debug.LogWarning("Initial board state is deadlocked. Reshuffling data instantly.");
                ShuffleTileData(GetAllActiveTiles());
            }
            return totalAnimationTime;
        }

        public void ClearMatch(TileView tile1, TileView tile2)
        {
            PlayVFXAt(tile1.transform.position);
            PlayVFXAt(tile2.transform.position);

            var cell1 = gridData.GetCell(tile1.GridPosition.y, tile1.GridPosition.x);
            if (cell1 != null)
            {
                cell1.isActive = false;
                _activeTiles.Remove(tile1);
                cell1.tileViewReference = null;
            }

            var cell2 = gridData.GetCell(tile2.GridPosition.y, tile2.GridPosition.x);
            if (cell2 != null)
            {
                cell2.isActive = false;
                this._activeTiles.Remove(tile2);
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
            if (_tilePool != null)
            {
                tile1.gameObject.SetActive(false);
                tile2.gameObject.SetActive(false);

                _tilePool.ReturnToPool(tile1);
                _tilePool.ReturnToPool(tile2);
            }
            else
            {
                Destroy(tile1.gameObject);
                Destroy(tile2.gameObject);
            }
        }

        public void CheckDeadlockAndShuffleIfNeeded()
        {
            if (this._tileManager.IsDeadlocked())
            {
                Debug.LogWarning("DEADLOCK DETECTED! No more valid moves. Initiating auto-shuffle.");
                StartCoroutine(ShuffleAnimationRoutine());
            }
        }

        public IEnumerator ShuffleAnimationRoutine()
        {
            this._gameManager.UpdateGameState(GameState.Shuffling);

            var activeTiles = GetAllActiveTiles();
            if (activeTiles.Count <= 1)
            {
                this._gameManager.UpdateGameState(GameState.Playing);
                yield break;
            }

            float   animationDuration = 0.5f;
            Vector3 centerPoint       = gridParent.position;

            Sequence flyInSequence = DOTween.Sequence();
            foreach (var tile in activeTiles)
            {
                tile.IsLocked = true;
                flyInSequence.Join(tile.transform.DOMove(centerPoint, animationDuration).SetEase(Ease.InBack));
                flyInSequence.Join(tile.TileVisual.transform.DOScale(0f, animationDuration));
            }
            yield return flyInSequence.WaitForCompletion();

            ShuffleTileData(activeTiles);

            Sequence flyOutSequence = DOTween.Sequence();
            foreach (var tile in activeTiles)
            {
                flyOutSequence.Join(tile.transform.DOLocalMove(Vector3.zero, animationDuration).SetEase(Ease.OutBack));
                flyOutSequence.Join(tile.TileVisual.transform.DOScale(1f, animationDuration));
            }
            yield return flyOutSequence.WaitForCompletion();

            foreach (var tile in activeTiles)
            {
                tile.IsLocked = false;
            }

            this._gameManager.UpdateGameState(GameState.Playing);
        }

        private void ShuffleTileData(List<TileView> tilesToShuffle)
        {
            List<TileType> currentTypes = tilesToShuffle.Select(t => t.Type).ToList();

            for (int i = 0; i < currentTypes.Count - 1; i++)
            {
                int randomIndex = Random.Range(i, currentTypes.Count);
                (currentTypes[i], currentTypes[randomIndex]) = (currentTypes[randomIndex], currentTypes[i]); // Swap
            }

            for (int i = 0; i < tilesToShuffle.Count; i++)
            {
                TileView tileView = tilesToShuffle[i];
                TileType newType  = currentTypes[i];

                tileView.Type = newType;

                var tileData = allTileData.Find(t => t.TileType == newType);
                if (tileData != null)
                {
                    var spriteRenderer = tileView.TileVisual.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = tileData.TileImage;
                    }
                }
                tileView.name = $"Tile_{newType}_{tileView.GridPosition.y}_{tileView.GridPosition.x}";
            }

            Debug.Log($"Shuffled {tilesToShuffle.Count} tiles successfully.");
        }

        public void PlayVFXAt(Vector3 position)
        {
            if (this._vfxPool == null) return;
            var vfxInstance = this._vfxPool.Spawn(position, Quaternion.identity);
        }

        public List<Vector2Int> IsMatchValid(TileView tile1, TileView tile2)
        {
            if (tile1.Type != tile2.Type) return null;

            Vector2Int pos1 = new Vector2Int(tile1.GridPosition.x + 1, tile1.GridPosition.y + 1);
            Vector2Int pos2 = new Vector2Int(tile2.GridPosition.x + 1, tile2.GridPosition.y + 1);

            return _pathfinder.FindPath(pos1, pos2);
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
            /*var activeTiles = new List<TileView>();
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
            return activeTiles;*/
            return this._activeTiles;
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

            if (tilePrefab == null)
            {
                Debug.LogError("Tile Prefab is not assigned in GridManager!");
            }
            if (gridParent == null)
            {
                Debug.LogError("Grid Parent Transform is not assigned in GridManager!");
            }

            var tileView = this.tilePrefab.GetComponent<TileView>();
            if (tileView == null)
            {
                Debug.LogError("Tile Prefab does not have a TileView component!");
            }
            this._tilePool = new ObjectPool<TileView>(tileView, 100);
            if (this.matchVFXPrefab == null)
            {
                Debug.LogError("Match VFX Prefab is not assigned in GridManager!");
            }
            else
            {
                var particleSystem = this.matchVFXPrefab.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    this._vfxPool = new ObjectPool<ParticleSystem>(particleSystem, 20);
                }
                else
                {
                    Debug.LogError("Match VFX Prefab does not have a ParticleSystem component!");
                }
            }
        }

        public void ClearOldGrid()
        {
            if (cellObjects != null)
            {
                for (int r = 0; r < cellObjects.GetLength(0); r++)
                {
                    for (int c = 0; c < cellObjects.GetLength(1); c++)
                    {
                        if (cellObjects[r, c] != null)
                        {
                            Destroy(cellObjects[r, c]);
                        }
                    }
                }
            }
            _activeTiles.Clear();
            gridData         = null;
            cellObjects      = null;
            currentLevelData = null;
        }

        private void HandleGameStateChange(GameState newState)
        {
            if (newState == GameState.LevelSetup)
            {
                Debug.Log("Game state changed to LevelSetup. Reinitializing grid.");
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
            if (_cameraController != null)
            {
                _cameraController.AdjustCameraToFit(newCols, newRows, CELL_SIZE, CAMERA_PADDING);
            }
            else
            {
                Debug.LogWarning("CameraController not found. Camera will not be adjusted.");
            }
            this.rows = newRows;
            this.cols = newCols;

            gridData = new GridData(newRows, newCols);
            _pathfinder.SetGridData(gridData);

            GridCalculator.CalculateWorldPositions(gridData, CELL_SIZE, Camera.main);
            cellObjects = new GameObject[newRows, newCols];

            Debug.Log($"Grid initialized: {newRows}x{newCols} cells, Cell size: {CELL_SIZE}");

            SpawnCellsFromLayout();
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

        private float GenerateAndPlaceTiles()
        {
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
                return 0f;
            }

            var availableTileTypes = allTileData
                .Where(t => t.TileType != TileType.None)
                .Select(t => t.TileType)
                .ToList();

            if (availableTileTypes.Count == 0)
            {
                return 0f;
            }

            int maxPossibleTypes = totalCells / 2;
            int numTypesToUse    = Mathf.Min(availableTileTypes.Count, maxPossibleTypes);

            if (numTypesToUse == 0)
            {
                return 0f;
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

                SpawnTileAt(positionToPlace.y, positionToPlace.x, tileToPlace, i);
            }
            float lastTileDelay      = (totalCells - 1) * SPAWN_STAGGER_PER_TILE;
            float totalAnimationTime = lastTileDelay + SPAWN_ANIMATION_DURATION;
            return totalAnimationTime;
        }

        private void SpawnTileAt(int row, int col, TileType tileType, int staggerIndex)
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

            var tileObj = this._tilePool.Spawn(cellObj.transform.position, cellObj.transform.rotation);
            tileObj.transform.SetParent(cellObj.transform);
            tileObj.transform.localPosition = Vector3.zero;

            var tileData = allTileData.Find(t => t.TileType == tileType);

            if (tileData == null)
            {
                Debug.LogError($"Tile data for {tileType} not found!");
                this._tilePool.ReturnToPool(tileObj);
                return;
            }

            var spriteRenderer = tileObj.TileRenderer.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = tileData.TileImage;
            }

            var tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Type                              = tileType;
                tileView.GridPosition                      = new Vector2Int(col, row); // Note: x=col, y=row
                gridData.cells[row, col].tileViewReference = tileView;

                _activeTiles.Add(tileView);
                float staggerDelay = staggerIndex * SPAWN_STAGGER_PER_TILE;
                tileView.AnimateSpawn(staggerDelay);
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

            return !gridData.cells[realRow, realCol].isActive;
        }

        #endregion
    }
}