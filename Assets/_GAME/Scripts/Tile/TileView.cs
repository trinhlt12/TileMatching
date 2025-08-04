namespace _GAME.Scripts.Tile
{
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class TileView : MonoBehaviour
    {
        public TileType   Type         { get; set; }
        public Vector2Int GridPosition { get; set; }

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _highlight;

        private void OnEnable()
        {
            TileManager.OnTileClicked += HandleTileClicked;
            _highlight.SetActive(false);
        }

        private void OnDisable()
        {
            TileManager.OnTileClicked -= HandleTileClicked;
        }

        private void HandleTileClicked(TileView chosenTile)
        {
            if (chosenTile == this)
            {
                this._animator.Play("Clicked");
            }
        }

        public void SetHighlight(bool highlight)
        {
           _highlight.SetActive(highlight);
        }
    }
}