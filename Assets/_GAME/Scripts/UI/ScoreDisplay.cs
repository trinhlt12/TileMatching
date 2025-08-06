namespace _GAME.Scripts.UI
{
    using System;
    using _GAME.Scripts.Score;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;

    public class ScoreDisplay : MonoBehaviour
    {
        [Header("Dependencies")] [SerializeField] private TMP_Text scoreText;

        [Header("Animation Settings")] [SerializeField] private float countDuration = 0.5f;
        [SerializeField]                                private float punchScale    = 1.2f;
        [SerializeField]                                private Ease  easeType      = Ease.OutCubic;

        private ScoreManager _scoreManager;
        private Tween        _textTween;
        private Tween        _panelTween;
        private int          _displayedScore = 0;

        private void OnEnable()
        {
            if (this._scoreManager == null)
            {
                this._scoreManager = ServiceLocator.Get<ScoreManager>();
            }

            this._scoreManager.OnScoreChanged += HandleScoreChanged;

            UpdateScoreText(_scoreManager.CurrentScore, true);
        }

        private void OnDisable()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged -= HandleScoreChanged;
            }

            _textTween?.Kill();
            _panelTween?.Kill();
        }

        private void HandleScoreChanged(int newTotalScore)
        {
            UpdateScoreText(newTotalScore, false);
            AnimatePanelPunch();
        }

        private void UpdateScoreText(int newScore, bool immediate = false)
        {
            _textTween?.Kill();

            if (immediate)
            {
                scoreText.text  = newScore.ToString("N0");
                _displayedScore = newScore;
            }
            else
            {
                _textTween = DOTween.To(() => _displayedScore,
                        x => _displayedScore = x,
                        newScore,
                        countDuration)
                    .SetEase(easeType)
                    .OnUpdate(() =>
                    {
                        scoreText.text = _displayedScore.ToString("N0");
                    });
            }
        }

        private void AnimatePanelPunch()
        {
            _panelTween?.Kill();

            transform.localScale = Vector3.one;

            _panelTween = transform.DOPunchScale(Vector3.one * (punchScale - 1),
                countDuration,
                10,
                1);
        }
    }
}