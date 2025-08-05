namespace _GAME.Scripts.Tile
{
    using _GAME.Scripts.Grid;
    using DG.Tweening;
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class TileView : MonoBehaviour
    {
        public GameObject TileVisual;
        public GameObject TileRenderer;
        public TileType   Type         { get; set; }
        public Vector2Int GridPosition { get; set; }

        [SerializeField] private Animator   _animator;
        [SerializeField] private GameObject _highlight;

        private void OnEnable()
        {
            _highlight.SetActive(false);
            if (this.TileVisual != null)
            {
                this.TileVisual.transform.localScale = Vector3.one;
            }
        }

        public void SetHighlight(bool highlight)
        {
            _highlight.SetActive(highlight);
        }

        public void AnimateSpawn(float delay)
        {
            this.TileVisual.transform.localScale = Vector3.zero;

            DOTween.Sequence()
                .SetDelay(delay)
                .Append(this.TileVisual.transform.DOScale(1f,
                    GridManager.SPAWN_ANIMATION_DURATION).SetEase(Ease.OutBack))
                .Play();
        }
    }
}