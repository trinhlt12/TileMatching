namespace _GAME.Scripts.Tile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Grid;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using UnityEngine;

    public class TileManager : MonoBehaviour
    {
        public List<TileDB>                  tileDataList;
        public static event Action<TileView> OnTileClicked;

        [SerializeField] private float    _checkDelay;
        private                  TileView _selectedTile1;
        private                  TileView _selectedTile2;

        [Header("Hint System Settings")] [SerializeField] private bool  isHintEnabled     = true;
        [SerializeField]                                  private float hintDelay         = 5f;
        [SerializeField]                                  private float hintPulseScale    = 1.15f;
        [SerializeField]                                  private float hintPulseDuration = 0.7f;

        private float     _hintTimer;
        private Coroutine _hintCoroutine;
        private TileView  _hintedTile1;
        private TileView  _hintedTile2;

        private GameManager _gameManager;
        private GridManager _gridManager;
        private MatchFinder _matchFinder;

        #region UNITY-CALLBACKS

        private void Awake()
        {
            ServiceLocator.Register(this);
            this._matchFinder = new MatchFinder();
        }

        private void Start()
        {
            this._gameManager = ServiceLocator.Get<GameManager>();
            this._gridManager = ServiceLocator.Get<GridManager>();
        }

        private void Update()
        {
            //Guard clause
            if (this._gameManager.CurrentState != GameState.Playing) return;

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

            HandleHintTimer();
        }

        #endregion

        #region PUBLIC-METHODS

        public bool IsDeadlocked()
        {
            return this.FindValidMove() == null;
        }

        #endregion

        #region PRIVATE-METHODS

        private void HandleTileSelection(TileView chosenTile)
        {
            this._hintTimer = 0f; // Reset hint timer when a tile is selected
            StopHint();
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

                var path = this._gridManager.IsMatchValid(_selectedTile1, _selectedTile2);

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

        private void HandleHintTimer()
        {
            if (!this.isHintEnabled) return;
            this._hintTimer += Time.deltaTime;
            if (this._hintTimer >= this.hintDelay)
            {
                ShowHint();
                this._hintTimer = 0f;
            }
        }

        private void ShowHint()
        {
            this.StopHint();
            var hintPair = this.FindValidMove();
            if (hintPair.HasValue)
            {
                this._hintedTile1   = hintPair.Value.tile1;
                this._hintedTile2   = hintPair.Value.tile2;
                this._hintCoroutine = StartCoroutine(AnimateHint(this._hintedTile1, this._hintedTile2));
            }
        }

        private void StopHint()
        {
            if (this._hintCoroutine != null)
            {
                StopCoroutine(this._hintCoroutine);
                _hintCoroutine = null;
            }
            if (this._hintedTile1 != null)
            {
                this._hintedTile1.TileVisual.transform.DOKill();
                this._hintedTile1.TileVisual.transform.localScale = Vector3.one;
                this._hintedTile1                                 = null;
            }
            if (this._hintedTile2 != null)
            {
                _hintedTile2.TileVisual.transform.DOKill();
                _hintedTile2.TileVisual.transform.localScale = Vector3.one;
                _hintedTile2                                 = null;
            }
        }

        private IEnumerator AnimateHint(TileView tile1, TileView tile2)
        {
            tile1.TileVisual.transform.DOKill();
            tile2.TileVisual.transform.DOKill();
            tile1.TileVisual.transform.DOScale(this.hintPulseScale, this.hintPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            tile2.TileVisual.transform.DOScale(this.hintPulseScale, this.hintPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            yield return null;
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
            this._gridManager.DrawPath(path);

            yield return new WaitForSeconds(0.3f);

            this._gridManager.HidePath();
            this._gridManager.ClearMatch(tile1, tile2);
            yield return null;
            this._gridManager.CheckDeadlockAndShuffleIfNeeded();
        }

        private (TileView tile1, TileView tile2)? FindValidMove()
        {
            var activeTiles     = _gridManager.GetAllActiveTiles();
            var searchableTiles = activeTiles.Where(t => !t.IsLocked).ToList();

            return _matchFinder.FindValidMove(searchableTiles);
        }

        #endregion
    }
}