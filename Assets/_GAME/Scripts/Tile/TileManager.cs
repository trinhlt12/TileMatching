namespace _GAME.Scripts.Tile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Grid;
    using UnityEngine;

    public class TileManager : MonoBehaviour
    {
        public List<TileDB> tileDataList;

        public static TileManager Instance { get; private set; }

        public static event Action<TileView> OnTileClicked;

        [SerializeField] private float    _checkDelay;
        private                  TileView _selectedTile1;
        private                  TileView _selectedTile2;
        /*
        private                  bool     _isChecking = false;
        */

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
            //Guard clause
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Tiles"));
                if (hit.collider != null)
                {
                    TileView chosenTile = hit.collider.GetComponent<TileView>();
                    if (chosenTile != null && !chosenTile.IsLocked)
                    {
                        Debug.Log($"Clicked on a tile! Type: {chosenTile.Type}, Position: {chosenTile.GridPosition}");
                        //TODO: abcxyz
                        OnTileClicked?.Invoke(chosenTile);
                        HandleTileSelection(chosenTile);
                    }
                }
            }
        }

        private void HandleTileSelection(TileView chosenTile)
        {
            if (_selectedTile1 == chosenTile)
            {
                _selectedTile1.SetHighlight(false);
                _selectedTile1 = null;
                return;
            }

            if (_selectedTile1 == null)
            {
                _selectedTile1 = chosenTile;
                _selectedTile1.SetHighlight(true);
            }
            else
            {
                _selectedTile2 = chosenTile;
                _selectedTile2.SetHighlight(true);

                var path = GridManager.Instance.IsMatchValid(_selectedTile1, _selectedTile2);

                if (path != null)
                {
                    _selectedTile1.IsLocked = true;
                    _selectedTile2.IsLocked = true;

                    StartCoroutine(ProcessMatchAnimation(_selectedTile1, _selectedTile2, path));
                }
                else
                {
                    StartCoroutine(ProcessInvalidMatch(_selectedTile1, _selectedTile2));
                }
                _selectedTile1 = null;
                _selectedTile2 = null;
            }
        }

        private IEnumerator ProcessInvalidMatch(TileView tile1, TileView tile2)
        {
            yield return new WaitForSeconds(0.5f);

            if (tile1 != null && tile1 != _selectedTile1)
            {
                tile1.SetHighlight(false);
            }
            if (tile2 != null && tile2 != _selectedTile1)
            {
                tile2.SetHighlight(false);
            }
        }

        private IEnumerator ProcessMatchAnimation(TileView tile1, TileView tile2, List<Vector2Int> path)
        {
            /*tile1.SetHighlight(false);
            tile2.SetHighlight(false);*/

            GridManager.Instance.DrawPath(path);

            yield return new WaitForSeconds(0.3f);

            GridManager.Instance.HidePath();
            GridManager.Instance.ClearMatch(tile1, tile2);
        }

    }
}