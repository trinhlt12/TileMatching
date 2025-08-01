namespace _GAME.Scripts.Tile
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TileManager : MonoBehaviour
    {
        public List<TileDB> tileDataList;

        public static TileManager Instance { get; private set; }

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
    }
}