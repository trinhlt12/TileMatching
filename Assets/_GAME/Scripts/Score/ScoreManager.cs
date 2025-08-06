namespace _GAME.Scripts.Score
{
    using System;
    using _GAME.Scripts.Services;
    using UnityEngine;

    public class ScoreManager : MonoBehaviour
    {
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;
        public int               CurrentScore { get; private set; }
        public int               HighScore    { get; private set; }

        private const string HighScoreKey = "HighScore";

        private const int DefaultScore = 10;

        public int DefaultScoreValue => DefaultScore;

        private void Awake()
        {
            ServiceLocator.Register(this);
            LoadHighScore();
        }

        public void AddScore(int pointsToAdd)
        {
            CurrentScore += pointsToAdd;
            Debug.Log($"ScoreManager: Added {pointsToAdd} points. Current Score: {CurrentScore}");
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
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