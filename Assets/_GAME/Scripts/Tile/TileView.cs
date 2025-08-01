namespace _GAME.Scripts.Tile
{
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class TileView : MonoBehaviour
    {
        public TileType   Type         { get; set; }
        public Vector2Int GridPosition { get; set; }

        [SerializeField] private Animator animator;

        private void OnEnable()
        {
            TileManager.OnTileClicked += HandleTileClicked;
        }

        private void OnDisable()
        {
            TileManager.OnTileClicked -= HandleTileClicked;
        }

        private void HandleTileClicked(TileView chosenTile)
        {
            if (chosenTile == this)
            {
                this.animator.Play("Clicked");
            }
        }
        public void SetHighlight(bool highlight){ }



    }
}