namespace _GAME.Scripts.UI
{
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Score;
    using _GAME.Scripts.Services;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;

    [RequireComponent(typeof(TMP_Text))]
    public class ComboDisplay : MonoBehaviour
    {
        [Header("Animation Settings")] [SerializeField] private float fadeInDuration  = 0.2f;
        [SerializeField]                                private float fadeOutDuration = 0.5f;
        [SerializeField]                                private float punchScale      = 1.5f;

        private TMP_Text     _comboText;
        private ScoreManager _scoreManager;
        private Sequence     _animationSequence;

        private void Awake()
        {
            _comboText       = GetComponent<TMP_Text>();
            _comboText.alpha = 0;
        }

        private void OnEnable()
        {
            _scoreManager                =  ServiceLocator.Get<ScoreManager>();
            _scoreManager.OnComboChanged += HandleComboChanged;
            _scoreManager.OnComboBreak   += HandleComboBreak;
        }

        private void OnDisable()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnComboChanged -= HandleComboChanged;
                _scoreManager.OnComboBreak   -= HandleComboBreak;
            }
            _animationSequence?.Kill();
        }

        private void HandleComboChanged(int comboCount)
        {
            _comboText.text = $"COMBO x{comboCount}";

            AnimateComboPopup();
        }

        private void HandleComboBreak()
        {
            AnimateFadeOut();
        }

        private void AnimateComboPopup()
        {
            _animationSequence?.Kill();

            transform.localScale = Vector3.one * 0.8f;
            _comboText.alpha     = 0f;

            _animationSequence = DOTween.Sequence();

            _animationSequence.Append(_comboText.DOFade(1f, fadeInDuration));                     // Mờ dần hiện ra
            _animationSequence.Join(transform.DOScale(1f, fadeInDuration).SetEase(Ease.OutBack)); // Phóng to ra

            _animationSequence.Append(transform.DOPunchScale(Vector3.one * (punchScale - 1), 0.3f, 8, 1));
        }

        private void AnimateFadeOut()
        {
            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            _animationSequence.Append(_comboText.DOFade(0f, fadeOutDuration));
            _animationSequence.Join(transform.DOScale(0.8f, fadeOutDuration).SetEase(Ease.InBack));
        }
    }
}