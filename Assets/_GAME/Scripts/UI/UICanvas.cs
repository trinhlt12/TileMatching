namespace _GAME.Scripts.UI
{
    using System;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using UnityEngine;

    public class UICanvas : MonoBehaviour
    {
        [Header("Animation Settings")] [SerializeField] private bool          useCustomAnimation = false;
        [SerializeField]                                private RectTransform container;
        [SerializeField]                                private float         fadeInDuration   = 0.5f;
        [SerializeField]                                private float         fadeOutDuration  = 0.3f;
        [SerializeField]                                private Ease          easeInType       = Ease.OutBack;
        [SerializeField]                                private Ease          easeOutType      = Ease.InBack;
        [Header("Base Settings")] [SerializeField]      private bool          isDestroyOnClose = false;

        protected GameManager _gameManager;
        private   Tween       _animationTween;

        private void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            float         ratio         = (float)Screen.width / Screen.height;
            if (ratio > 2.1f)
            {
                var leftBottom = rectTransform.offsetMin;
                var rightTop   = rectTransform.offsetMax;
                leftBottom.y = 0f;
                rightTop.y   = -100f;

                rectTransform.offsetMin = leftBottom;
                rectTransform.offsetMax = rightTop;
            }
        }

        private void Start()
        {
            this._gameManager = ServiceLocator.Get<GameManager>();
        }

        public virtual void SetUp()
        {
        }

        public virtual void Open()
        {
            if (useCustomAnimation)
            {
                gameObject.SetActive(true);
                return;
            }
            gameObject.SetActive(true);
            AnimateFadeIn();
        }

        public virtual void Close(float time)
        {
            _animationTween?.Kill();
            if (useCustomAnimation)
            {
                Invoke(nameof(CloseDirectly), time);
                return;
            }

            AnimateFadeOut();
        }

        public virtual void CloseDirectly()
        {
            _animationTween?.Kill();

            if (this.isDestroyOnClose)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #region DEFAULT ANIMATIONS

        private void AnimateFadeIn()
        {
            _animationTween?.Kill();

            if (container == null)
            {
                Debug.LogWarning($"Container for {name} is not set. Animation skipped.");
                return;
            }

            container.localScale = Vector3.one * 0.7f;

            var canvasGroup                      = container.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = container.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            _animationTween = container.DOScale(1f, fadeInDuration)
                .SetEase(easeInType);

            canvasGroup.DOFade(1f, fadeInDuration * 0.8f); // Fade nhanh hơn một chút cho đẹp
        }

        private void AnimateFadeOut()
        {
            _animationTween?.Kill();

            if (container == null)
            {
                Debug.LogWarning($"Container for {name} is not set. Animation skipped.");
                CloseDirectly();
                return;
            }

            var canvasGroup                      = container.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = container.gameObject.AddComponent<CanvasGroup>();

            _animationTween = container.DOScale(0.7f, fadeOutDuration)
                .SetEase(easeOutType);

            canvasGroup.DOFade(0f, fadeOutDuration)
                .OnComplete(() =>
                {
                    CloseDirectly();
                });
        }

        #endregion
    }
}