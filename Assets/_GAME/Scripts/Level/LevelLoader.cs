namespace _GAME.Scripts.Level
{
    using UnityEngine;

    public class LevelLoader
    {
        public static LevelData LoadLevel(int levelNumber)
        {
            var path = $"Levels/level_{levelNumber}";

            var jsonFile = Resources.Load<TextAsset>(path);

            if (jsonFile == null)
            {
                return null;
            }
            var loadedData = JsonUtility.FromJson<LevelData>(jsonFile.text);

            return loadedData;
        }
    }
}