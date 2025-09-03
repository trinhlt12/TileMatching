namespace _GAME.Scripts.UI
{
    using _GAME.Scripts.Core;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public class GamePlayCanvas : UICanvas
    {
        [SerializeField]                               private Image timeBarFillImage;
        [Header("Timer FX Settings")] [SerializeField] private Color normalTimeColor = Color.white;
        [SerializeField]                               private Color lowTimeColor    = new Color(1f, 0.53f, 0.53f); // #FE8888
        [SerializeField]                               private float blinkDuration   = 0.5f;

        private TimerController _timerController;
        private Tween           _blinkingTween;

        public override void SetUp()
        {
            base.SetUp();
            _timerController = GameManager.Instance.GetComponent<TimerController>();

            if (_timerController != null)
            {
                _timerController.OnTimeUpdated    += UpdateTimerUI;
                _timerController.OnTimeRunningLow += StartBlinkingEffect;
            }
            StopBlinkingEffect();
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
            if (_timerController != null)
            {
                _timerController.OnTimeUpdated -= UpdateTimerUI;
            }
            StopBlinkingEffect();
            base.CloseDirectly();
        }

        public override void Close(float time)
        {
            if (_timerController != null)
            {
                _timerController.OnTimeUpdated -= UpdateTimerUI;
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