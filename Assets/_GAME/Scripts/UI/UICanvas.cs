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

        [Tooltip("Check this if the UI should animate even when Time.timeScale is 0 (e.g., Pause Menu).")]
        [SerializeField] protected bool ignoreTimeScale = false;
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
            AnimateFadeIn(this.ignoreTimeScale);
        }

        public virtual void Close(float time)
        {
            _animationTween?.Kill();
            if (useCustomAnimation)
            {
                Invoke(nameof(CloseDirectly), time);
                return;
            }

            AnimateFadeOut(this.ignoreTimeScale);
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

        private void AnimateFadeIn(bool useUnscaledTime = false)
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
                .SetEase(easeInType)
                .SetUpdate(useUnscaledTime);

            canvasGroup.DOFade(1f, fadeInDuration * 0.8f)
                .SetUpdate(useUnscaledTime);
        }

        private void AnimateFadeOut(bool useUnscaledTime = false)
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
                .SetEase(easeOutType)
                .SetUpdate(useUnscaledTime);

            canvasGroup.DOFade(0f, fadeOutDuration)
                .SetUpdate(useUnscaledTime)
                .OnComplete(OnFadeOutComplete);
        }
        protected virtual void OnFadeOutComplete()
        {
            CloseDirectly();
        }
        #endregion
    }
}