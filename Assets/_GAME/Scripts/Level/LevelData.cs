namespace _GAME.Scripts.Level
{
    using System.Collections.Generic;

    [System.Serializable]
    public class GridSizeData
    {
        public int rows;
        public int cols;
    }

    [System.Serializable]
    public class LevelData
    {
        public int          levelNumber;
        public int          timeLimit;
        public GridSizeData gridSize;
        public List<string> layout;
    }
}