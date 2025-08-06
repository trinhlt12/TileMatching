namespace _GAME.Scripts.Core
{
    using System.Collections;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float duration = 0.75f;

        private Camera    _camera;
        private Tween     _cameraTween;
        private Coroutine _adjustCoroutine;

        private void Awake()
        {
            ServiceLocator.Register(this);
            _camera = GetComponent<Camera>();
        }

        public void AdjustCameraToFit(int gridWidth, int gridHeight, float cellSize, float padding)
        {
            if (_adjustCoroutine != null)
            {
                StopCoroutine(_adjustCoroutine);
            }
            _adjustCoroutine = StartCoroutine(AdjustCameraCoroutine(gridWidth, gridHeight, cellSize, padding));
        }
        private IEnumerator AdjustCameraCoroutine(int gridWidth, int gridHeight, float cellSize, float padding)
        {
            yield return new WaitForEndOfFrame();

            float targetWorldWidth = (gridWidth * cellSize) + padding;
            float targetOrthoSize  = (targetWorldWidth / _camera.aspect) / 2f;

            _cameraTween?.Kill();
            _cameraTween = _camera.DOOrthoSize(targetOrthoSize, duration)
                .SetEase(Ease.OutCubic);

            Debug.Log($"Camera adjustment completed. Final aspect: {_camera.aspect}, Target size: {targetOrthoSize}");
        }
    }
}