namespace _GAME.Scripts.Bot
{
    using System.Collections;
    using System.Collections.Generic;
    using _GAME.Scripts.Grid;
    using _GAME.Scripts.Tile;
    using UnityEngine;

    public class BotPlayer : MonoBehaviour
    {
        [Header("Bot Settings")] [SerializeField] private bool  isEnabled = true;
        [SerializeField]                          private float moveDelay = 1.0f;

        [Header("Dependencies")] [SerializeField] private GridManager gridManager;

        private void Start()
        {
            if (isEnabled && gridManager != null)
            {
                StartCoroutine(PlayGameCoroutine());
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

                    gridManager.DrawPath(move.Value.path);
                    yield return new WaitForSeconds(moveDelay / 2);

                    gridManager.HidePath();
                    gridManager.ClearMatch(move.Value.tile1, move.Value.tile2);

                    yield return new WaitForSeconds(moveDelay / 2);
                }
                else
                {
                    Debug.Log("BOT: No more valid moves found. Bot stopping.");
                    break;
                }
            }
        }

        private (TileView tile1, TileView tile2, List<Vector2Int> path)? FindValidMove()
        {
            var activeTiles = gridManager.GetAllActiveTiles();

            if (activeTiles.Count < 2) return null;

            // brute-force O(n^2)
            for (int i = 0; i < activeTiles.Count; i++)
            {
                for (int j = i + 1; j < activeTiles.Count; j++)
                {
                    TileView tile1 = activeTiles[i];
                    TileView tile2 = activeTiles[j];

                    var path = gridManager.IsMatchValid(tile1, tile2);

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