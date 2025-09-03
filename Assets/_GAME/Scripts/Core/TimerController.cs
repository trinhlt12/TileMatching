namespace _GAME.Scripts.Core
{
    // File: TimerController.cs
    using System;
    using UnityEngine;

    public class TimerController : MonoBehaviour
    {
        public event Action<float, float> OnTimeUpdated;

        public event Action OnTimeUp;
        public event Action OnTimeRunningLow;

        private float lowTimeThreshold => this._maxTime/ 5f;
        private bool  _isTimeLow       = false;

        private float _maxTime;
        private float _remainingTime;
        private bool  _isRunning = false;

        private void Update()
        {
            if (!_isRunning) return;

            if (_remainingTime > 0)
            {
                _remainingTime -= Time.deltaTime;

                OnTimeUpdated?.Invoke(_remainingTime, _maxTime);
                if (!_isTimeLow && _remainingTime <= lowTimeThreshold)
                {
                    _isTimeLow = true;
                    OnTimeRunningLow?.Invoke();
                }
            }
            else
            {
                _remainingTime = 0;
                _isRunning     = false;
                OnTimeUpdated?.Invoke(_remainingTime, _maxTime);
                OnTimeUp?.Invoke();
            }
        }

        public void StartTimer(float maxTimeInSeconds)
        {
            _maxTime       = maxTimeInSeconds;
            _remainingTime = _maxTime;
            _isRunning     = true;
            _isTimeLow     = false;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }
    }
}