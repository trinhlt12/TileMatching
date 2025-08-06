namespace _GAME.Scripts.Score
{
    using System;
    using System.Collections;
    using _GAME.Scripts.Services;
    using UnityEngine;

    public class ScoreManager : MonoBehaviour
    {
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        public event Action<int> OnComboChanged;
        public event Action      OnComboBreak;
        public int               CurrentScore { get; private set; }
        public int               HighScore    { get; private set; }

        public        int    CurrentCombo { get; private set; }
        private const string HighScoreKey = "HighScore";

        private const float ComboResetTime = 2.5f;

        private const int DefaultScore = 10;

        private Coroutine _comboResetCoroutine;
        public int DefaultScoreValue => DefaultScore;

        private void Awake()
        {
            ServiceLocator.Register(this);
            LoadHighScore();
        }

        public void AddScore(int basePoints)
        {
            if (_comboResetCoroutine != null)
            {
                StopCoroutine(_comboResetCoroutine);
            }

            CurrentCombo++;

            int comboMultiplier = Mathf.Max(1, CurrentCombo -1);
            int finalScore      = basePoints * comboMultiplier;

            CurrentScore += finalScore;

            Debug.Log($"COMBO x{CurrentCombo}! Added {finalScore} points. Total Score: {CurrentScore}");

            OnScoreChanged?.Invoke(CurrentScore);
            if (CurrentCombo > 1)
            {
                OnComboChanged?.Invoke(CurrentCombo);
            }

            _comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());
        }

        public void ResetCombo()
        {
            if (_comboResetCoroutine != null)
            {
                StopCoroutine(_comboResetCoroutine);
            }

            CurrentCombo = 0;
            OnComboBreak?.Invoke();
        }
        private IEnumerator ResetComboAfterDelay()
        {
            yield return new WaitForSeconds(ComboResetTime);

            Debug.Log("Combo broken!");
            CurrentCombo = 0;
            OnComboBreak?.Invoke();
        }
        public void ResetScore()
        {
            CurrentScore = 0;

            if (_comboResetCoroutine != null)
            {
                StopCoroutine(_comboResetCoroutine);
            }
            CurrentCombo = 0;

            OnScoreChanged?.Invoke(CurrentScore);
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void SubmitAndTrySetNewHighScore()
        {
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();

                OnHighScoreChanged?.Invoke(HighScore);
            }
        }
    }
}