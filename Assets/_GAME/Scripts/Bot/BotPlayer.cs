namespace _GAME.Scripts.Bot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Grid;
    using _GAME.Scripts.Services;
    using _GAME.Scripts.Tile;
    using UnityEngine;

    public class BotPlayer : MonoBehaviour
    {
        [Header("Bot Settings")] [SerializeField] private bool  isEnabled = true;
        [SerializeField]                          private float moveDelay = 1.0f;

        private Coroutine   _botCoroutine;
        private GridManager _gridManager;
        private GameManager _gameManager;

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChange;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChange;
        }

        private void Start()
        {
            this._gridManager = ServiceLocator.Get<GridManager>();
            this._gameManager = ServiceLocator.Get<GameManager>();
            if (isEnabled && this._gameManager.CurrentState == GameState.Playing)
            {
                StartCoroutine(PlayGameCoroutine());
            }
        }

        private void HandleGameStateChange(GameState newState)
        {
            if (!isEnabled) return;

            if (newState == GameState.Playing && _botCoroutine == null)
            {
                Debug.Log("BOT: Game state is 'Playing'. Starting bot coroutine...");
                _botCoroutine = StartCoroutine(PlayGameCoroutine());
            }
            else if (newState != GameState.Playing && _botCoroutine != null)
            {
                Debug.Log("BOT: Game state is no longer 'Playing'. Stopping bot coroutine...");
                StopCoroutine(_botCoroutine);
                _botCoroutine = null;
            }
        }

        private IEnumerator PlayGameCoroutine()
        {
            yield return new WaitForSeconds(2.0f);

            while (true)
            {
                var move = FindValidMove();

                if (move != null)
                {
                    Debug.Log($"BOT: Found a match! {move.Value.tile1.Type} at {move.Value.tile1.GridPosition} and {move.Value.tile2.GridPosition}");

                    _gridManager.DrawPath(move.Value.path);
                    yield return new WaitForSeconds(moveDelay / 2);

                    _gridManager.HidePath();
                    _gridManager.ClearMatch(move.Value.tile1, move.Value.tile2);

                    yield return new WaitForSeconds(moveDelay / 2);
                }
                else
                {
                    Debug.Log("BOT: No more valid moves found. Bot stopping.");
                    break;
                }
            }
            _botCoroutine = null;

        }

        private (TileView tile1, TileView tile2, List<Vector2Int> path)? FindValidMove()
        {
            var activeTiles = _gridManager.GetAllActiveTiles();

            if (activeTiles.Count < 2) return null;

            // brute-force O(n^2)
            for (int i = 0; i < activeTiles.Count; i++)
            {
                for (int j = i + 1; j < activeTiles.Count; j++)
                {
                    TileView tile1 = activeTiles[i];
                    TileView tile2 = activeTiles[j];

                    var path = _gridManager.IsMatchValid(tile1, tile2);

                    if (path != null)
                    {
                        return (tile1, tile2, path);
                    }
                }
            }

            return null;
        }
    }
}