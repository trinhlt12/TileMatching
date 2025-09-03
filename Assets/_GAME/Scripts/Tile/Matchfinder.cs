namespace _GAME.Scripts.Tile
{
    using System.Collections.Generic;
    using System.Linq;
    using _GAME.Scripts.Grid;
    using _GAME.Scripts.Tile;
    using _GAME.Scripts.Services;

    public class MatchFinder
    {
        private GridManager GridManager { get { return ServiceLocator.Get<GridManager>(); } }

        public (TileView tile1, TileView tile2)? FindValidMove(List<TileView> allActiveTiles)
        {
            if (allActiveTiles.Count < 2) return null;

            var groupedTiles = allActiveTiles.GroupBy(tile => tile.Type)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var group in groupedTiles.Values)
            {
                if (group.Count < 2) continue;

                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        var tile1 = group[i];
                        var tile2 = group[j];

                        if (GridManager.IsMatchValid(tile1, tile2) != null)
                        {
                            return (tile1, tile2);
                        }
                    }
                }
            }

            return null;
        }
    }
}