namespace _GAME.Scripts.Grid
{
    using UnityEngine;

    public class GridCalculator
    {
        /*public static GridDimensions CalculateOptimalGrid(Camera camera)
        {
            var screenHeight = camera.orthographicSize * 2;
            var screenWidth = screenHeight * camera.aspect;

            var usableHeight = screenHeight * 0.7f; //
            var usableWidth = screenWidth * 0.8f; //

            var baseTileSize = 1f;

            //calculate how many tiles can fit:
            var maxCols = Mathf.FloorToInt(usableWidth / baseTileSize);
            var maxRows = Mathf.FloorToInt(usableHeight / baseTileSize);

            var finalCols = Mathf.Clamp(maxCols, 2, 10);
            var finalRows = Mathf.Clamp(maxRows, 2, 10);

            return new GridDimensions(finalRows, finalCols, baseTileSize);
        }*/

        public static void CalculateWorldPositions(GridData gridData, float tileSize, Camera camera)
        {
            // Screen center in world coordinates
            Vector3 screenCenter = camera.transform.position;

            // Calculate grid's total size
            float gridWidth  = gridData.cols * tileSize;
            float gridHeight = gridData.rows * tileSize;

            // Calculate starting position (top-left of grid)
            float startX = screenCenter.x - (gridWidth * 0.5f) + (tileSize * 0.5f);
            float startY = screenCenter.y + (gridHeight * 0.5f) - (tileSize * 0.5f);

            for (int row = 0; row < gridData.rows; row++)
            {
                for (int col = 0; col < gridData.cols; col++)
                {
                    float worldX = startX + (col * tileSize);
                    float worldY = startY - (row * tileSize);

                    gridData.cells[row, col].worldPos = new Vector3(worldX, worldY, 0);
                }
            }
        }
    }
}