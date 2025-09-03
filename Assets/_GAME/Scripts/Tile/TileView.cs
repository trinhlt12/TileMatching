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
        public bool IsLocked { get; set; }

        [SerializeField] private Animator   _animator;
        [SerializeField] private GameObject _highlight;

        [Header("Highlight Effect Settings")]
        [SerializeField] private Vector3 punchScale      = new Vector3(0.2f, 0.2f, 0f);
        [SerializeField] private float punchDuration   = 0.4f;
        [SerializeField] private int   punchVibrato    = 5;
        [SerializeField] private float punchElasticity = 0.5f;
        private                  Tween _highlightTween;

        private void OnEnable()
        {
            _highlight.SetActive(false);
            if (this.TileVisual != null)
            {
                this.TileVisual.transform.localScale = Vector3.one;
            }
            IsLocked = false;
        }

        public void SetHighlight(bool highlight)
        {
            _highlightTween?.Kill();
            if (highlight)
            {
                _highlight.SetActive(true);

                this._highlightTween = this.TileVisual.transform.DOPunchScale(
                    this.punchScale,
                    this.punchDuration,
                    this.punchVibrato,
                    this.punchElasticity);

            }
            else
            {
                _highlight.SetActive(false);
                TileVisual.transform.localScale = Vector3.one;
            }
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