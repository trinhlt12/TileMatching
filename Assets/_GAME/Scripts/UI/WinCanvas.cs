namespace _GAME.Scripts.UI
{
    using _GAME.Scripts.UI;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public class WinCanvas : UICanvas
    {
        [Header("Buttons")] [SerializeField]            private Button        restartButton;
        [SerializeField]                                private Button        nextLevelButton;
        [SerializeField]                                private Button        mainMenuButton;
        [Header("Animation Elements")] [SerializeField] private Image         glowImage;
        [SerializeField]                                private RectTransform bunnyImage;
        [Header("Animation Settings")] [SerializeField] private float         bunnyDropDuration = 0.8f;
        [SerializeField]                                private float         glowRotateSpeed   = 20f;
        [SerializeField]                                private float         glowPulseSpeed    = 1.5f;

        private Vector3 _initialBunnyPosition;
        private Tween   _glowTween;
        private Tween   _bunnyTween;
        protected override void Awake()
        {
            base.Awake();
            if (bunnyImage != null)
            {
                _initialBunnyPosition = bunnyImage.anchoredPosition;
            }
        }
        public override void SetUp()
        {
            base.SetUp();

            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);

            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);

            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }

        public override void Open()
        {
            base.Open();
            CleanUpAnimations();

            AnimateGlow();
            AnimateBunny();
        }
        public override void CloseDirectly()
        {
            CleanUpAnimations();
            base.CloseDirectly();
        }

        private void AnimateGlow()
        {
            _glowTween = DOTween.Sequence()
                .Append(glowImage.transform.DORotate(new Vector3(0, 0, -360), glowRotateSpeed, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear))
                .Join(glowImage.DOFade(0.5f, glowPulseSpeed).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void AnimateBunny()
        {
            bunnyImage.anchoredPosition = _initialBunnyPosition + new Vector3(0, 300, 0);

            _bunnyTween = bunnyImage.DOAnchorPos(_initialBunnyPosition, bunnyDropDuration)
                .SetEase(Ease.OutBounce)
                .SetDelay(0.2f);
        }

        private void CleanUpAnimations()
        {
            _glowTween?.Kill();
            _bunnyTween?.Kill();

            if (bunnyImage != null)
            {
                bunnyImage.anchoredPosition = _initialBunnyPosition;
            }
            if (glowImage != null)
            {
                glowImage.transform.rotation = Quaternion.identity;
                glowImage.color              = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, 1f);
            }
        }
        private void OnRestartButtonClicked()
        {
            Debug.Log("WinCanvas: Restart button clicked.");
            this.Close(0f);
            _gameManager.RestartLevel();
        }

        private void OnNextLevelButtonClicked()
        {
            Debug.Log("WinCanvas: Next Level button clicked.");
            this.Close(0f);
            _gameManager.LoadNextLevel();
        }

        private void OnMainMenuButtonClicked()
        {
            Debug.Log("WinCanvas: Main Menu button clicked.");
            _gameManager.QuitToMainMenu();
        }
    }
}