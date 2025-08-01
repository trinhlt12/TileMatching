namespace _GAME.Scripts.Grid
{
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Tile;
    using UnityEngine;

    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")] [SerializeField] private GameObject cellPrefab;
        [SerializeField]                           private Transform  gridParent;

        [Header("Runtime Grid Info")] [SerializeField] private int   rows;
        [SerializeField]                               private int   cols;
        [SerializeField]                               private float cellSize = 1f;

        private GridData      gridData;
        private GameObject[,] cellObjects;
        private List<TileDB>  allTileData => TileManager.Instance.tileDataList;

        private void Start()
        {
            SetupGrid();
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

            List<TileType> availableTileTypes = allTileData
                .Where(t => t.TileType != TileType.None)
                .Select(t => t.TileType)
                .ToList();

            if (availableTileTypes.Count == 0)
            {
                return;
            }

            int maxPossibleTypes = totalCells / 2;
            int numTypesToUse    = Random.Range(1, Mathf.Min(availableTileTypes.Count, maxPossibleTypes) + 1);

            List<TileType> selectedTileTypes = availableTileTypes.OrderBy(x => Random.value).Take(numTypesToUse).ToList();
            Debug.Log($"Sẽ sử dụng {numTypesToUse} loại tile: " + string.Join(", ", selectedTileTypes));

            Dictionary<TileType, int> tileCounts = new Dictionary<TileType, int>();

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

            List<TileType> tilesToPlace = new List<TileType>(totalCells);
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

            var tileObj = Instantiate(tileData.TilePrefab, cellObj.transform);
            tileObj.transform.localPosition = Vector3.zero;

            gridData.cells[row, col].tileType = tileType;
            gridData.cells[row, col].isActive = true;
            tileObj.name                      = $"Tile_{tileType}_{row}_{col}";
        }
    }
}