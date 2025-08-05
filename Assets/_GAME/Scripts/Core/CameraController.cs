namespace _GAME.Scripts.Core
{
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float duration = 0.75f;

        private Camera _camera;
        private Tween  _cameraTween;

        private void Awake()
        {
            ServiceLocator.Register(this);
            _camera = GetComponent<Camera>();
        }

        public void AdjustCameraToFit(int gridWidth, int gridHeight, float cellSize, float padding)
        {
            float totalGridWidth  = gridWidth * cellSize;
            float totalGridHeight = gridHeight * cellSize;

            float paddedWidth  = totalGridWidth + padding;
            float paddedHeight = totalGridHeight + padding;

            float sizeForWidth = (paddedWidth / _camera.aspect) / 2f;

            float sizeForHeight = paddedHeight / 2f;

            float targetSize = Mathf.Max(sizeForWidth, sizeForHeight);
            _cameraTween?.Kill();

            _cameraTween = _camera.DOOrthoSize(targetSize, duration)
                .SetEase(Ease.OutCubic);
        }
    }
}