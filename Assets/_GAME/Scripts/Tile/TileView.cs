namespace _GAME.Scripts.Tile
{
    using DG.Tweening;
    using UnityEngine;

    [RequireComponent(typeof(Collider2D))]
    public class TileView : MonoBehaviour
    {
        public GameObject TileVisual;
        public TileType   Type         { get; set; }
        public Vector2Int GridPosition { get; set; }

        [SerializeField] private Animator   _animator;
        [SerializeField] private GameObject _highlight;

        private void OnEnable()
        {
            _highlight.SetActive(false);
            this.transform.localScale = Vector3.one;
            /*if (this.TileVisual != null)
            {
                this.TileVisual.transform.localScale = Vector3.one;
            }*/
        }

        public void SetHighlight(bool highlight)
        {
            _highlight.SetActive(highlight);
        }

        public void AnimateSpawn(float delay)
        {
            this.transform.localScale = Vector3.zero;

            DOTween.Sequence()
                .SetDelay(delay)
                .Append(this.transform.DOScale(1f,
                    0.5f).SetEase(Ease.OutBack))
                .Play();
        }
    }
}