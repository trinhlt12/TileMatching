namespace _GAME.Scripts.UI
{
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public class GamePlayCanvas : UICanvas
    {
        [SerializeField]                               private Image timeBarFillImage;
        [Header("Timer FX Settings")] [SerializeField] private Color normalTimeColor = Color.white;
        [SerializeField]                               private Color lowTimeColor    = new Color(1f, 0.53f, 0.53f); // #FE8888
        [SerializeField]                               private float blinkDuration   = 0.5f;

        [Header("Buttons")] [SerializeField] private Button pauseButton;

        private TimeManager _timeManager;
        private Tween       _blinkingTween;

        public override void SetUp()
        {
            base.SetUp();
            _timeManager = ServiceLocator.Get<TimeManager>();

            if (this._timeManager != null)
            {
                this._timeManager.OnTimeUpdated    += UpdateTimerUI;
                this._timeManager.OnTimeRunningLow += StartBlinkingEffect;
            }
            StopBlinkingEffect();

            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        private void OnPauseButtonClicked()
        {
            Debug.Log("Pause button clicked!");
            _gameManager.PauseGame();
        }

        public void UpdateTimerUI(float currentTime, float maxTime)
        {
            if (timeBarFillImage != null)
            {
                timeBarFillImage.fillAmount = currentTime / maxTime;
            }
        }

        public override void CloseDirectly()
        {
            if (this._timeManager != null)
            {
                this._timeManager.OnTimeUpdated -= UpdateTimerUI;
            }
            StopBlinkingEffect();
            base.CloseDirectly();
        }

        public override void Close(float time)
        {
            if (this._timeManager != null)
            {
                this._timeManager.OnTimeUpdated -= UpdateTimerUI;
            }
            StopBlinkingEffect();
            base.Close(time);
        }

        private void StartBlinkingEffect()
        {
            Debug.Log("Time is running low! Starting blink effect.");

            _blinkingTween?.Kill();

            _blinkingTween = timeBarFillImage.DOColor(lowTimeColor, blinkDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopBlinkingEffect()
        {
            _blinkingTween?.Kill();
            _blinkingTween = null;

            timeBarFillImage.color = normalTimeColor;
        }
    }
}