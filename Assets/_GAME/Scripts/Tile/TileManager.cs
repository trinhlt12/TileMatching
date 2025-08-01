namespace _GAME.Scripts.Tile
{
    using System;
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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var          ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Tiles"));
                if (hit.collider != null)
                {
                    TileView chosenTile = hit.collider.GetComponent<TileView>();
                    if (chosenTile != null)
                    {
                        Debug.Log($"Clicked on a tile! Type: {chosenTile.Type}, Position: {chosenTile.GridPosition}");
                        //TODO: abcxyz
                    }
                }
            }
        }
    }
}