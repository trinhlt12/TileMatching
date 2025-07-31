namespace _GAME.Scripts.Grid
{
    using UnityEngine;

    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")] [SerializeField] GameObject cellPrefab; // Cell prefab (empty container)
        [SerializeField]                           Transform  gridParent; // Parent object to organize cells

        [Header("Runtime Grid Info")] [SerializeField] int   rows     = 12;
        [SerializeField]                               int   cols     = 8;
        [SerializeField]                               float cellSize = 1f;

        // Data structures
        private GridData      gridData;
        private GameObject[,] cellObjects; // Visual cell GameObjects only

        void Start()
        {
            SetupGrid();
            SpawnCells();
        }

        void SetupGrid()
        {
            // Calculate optimal dimensions for portrait
            /*var dimensions = GridCalculator.CalculateOptimalGrid(Camera.main);
            rows     = dimensions.rows;
            cols     = dimensions.cols;
            cellSize = dimensions.tileSize; // Actually cell size now*/

            // Create logical grid data
            gridData = new GridData(rows, cols);

            // Calculate world positions for each cell
            GridCalculator.CalculateWorldPositions(gridData, cellSize, Camera.main);

            // Initialize arrays
            cellObjects = new GameObject[rows, cols];

            Debug.Log($"Grid initialized: {rows}x{cols} cells, Cell size: {cellSize}");
        }

        void SpawnCells()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Get world position from grid data
                    Vector3 worldPos = gridData.cells[row, col].worldPos;

                    // Spawn cell GameObject
                    GameObject cellObj = Instantiate(cellPrefab, worldPos, Quaternion.identity, gridParent);
                    cellObj.name = $"Cell_{row}_{col}"; // Nice naming for hierarchy

                    // Store reference
                    cellObjects[row, col] = cellObj;
                }
            }

            Debug.Log($"Spawned {rows * cols} cells successfully!");
        }
    }
}