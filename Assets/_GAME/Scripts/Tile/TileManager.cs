namespace _GAME.Scripts.Tile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using _GAME.Scripts.Grid;
    using UnityEngine;

    public class TileManager : MonoBehaviour
    {
        public List<TileDB> tileDataList;

        public static TileManager Instance { get; private set; }

        public static event Action<TileView> OnTileClicked;

        [SerializeField] private float    _checkDelay = 1.5f;
        private                  TileView _selectedTile1;
        private                  TileView _selectedTile2;
        private                  bool     _isChecking = false;

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
            if (this._isChecking) return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, LayerMask.GetMask("Tiles"));
                if (hit.collider != null)
                {
                    TileView chosenTile = hit.collider.GetComponent<TileView>();
                    if (chosenTile != null)
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
            //case 1
            if (this._selectedTile1 == chosenTile)
            {
                Debug.Log("Clicked on the same tile again, deselecting.");
                _selectedTile1.SetHighlight(false);
                _selectedTile1 = null;
                return;
            }

            //case 2
            if (this._selectedTile1 == null)
            {
                this._selectedTile1 = chosenTile;
                this._selectedTile1.SetHighlight(true);
            }
            //case 3
            else
            {
                this._selectedTile2 = chosenTile;
                this._selectedTile2.SetHighlight(true);

                //start checking process:
                this._isChecking = true;
                //TODO: checking with coroutine
                StartCoroutine(CheckMatch());
            }
        }

        private IEnumerator CheckMatch()
        {
            yield return new WaitForSeconds(this._checkDelay);
            //TODO: path-finding
            if (GridManager.Instance.IsMatchValid(_selectedTile1, _selectedTile2))
            {
                Debug.Log("MATCH FOUND (Line Match)!");
                GridManager.Instance.ClearMatch(_selectedTile1, _selectedTile2);
            }
            else
            {
                Debug.Log("NOT A MATCH.");
                // If not a valid match, deselect them.
                _selectedTile1.SetHighlight(false);
                _selectedTile2.SetHighlight(false);
            }

            this._selectedTile1 = null;
            this._selectedTile2 = null;
            this._isChecking    = false;
        }
    }
}