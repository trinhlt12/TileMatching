namespace _GAME.Scripts.Level
{
    using System;
    using System.Collections;
    using _GAME.Scripts.Core;
    using _GAME.Scripts.Grid;
    using _GAME.Scripts.Services;
    using UnityEngine;

    public class LevelManager : MonoBehaviour
    {
        #region FIELDS
        public LevelData CurrentLevelData { get; private set; }

        public int CurrentLevelIndex { get; private set; }

        [Header("Level Configuration")] [SerializeField] private int _maxLevelCount;

        private GameManager _gameManager;
        private GridManager _gridManager;

        #endregion

        #region UNITY-CALLBACKS

        private void Awake()
        {
            ServiceLocator.Register(this);

        }

        private void Start()
        {
            _gameManager      = ServiceLocator.Get<GameManager>();
            this._gridManager = ServiceLocator.Get<GridManager>();
        }

        #endregion

        public void OnInit()
        {
            CurrentLevelIndex = 0;
        }

        public void LoadLevel(int levelNumber)
        {
            if (levelNumber <= 0 || levelNumber > this._maxLevelCount)
            {
                Debug.LogError($"Invalid level number: {levelNumber}. Must be between 1 and {this._maxLevelCount}.");
                this._gameManager.UpdateGameState(GameState.MainMenu);
                return;
            }

            StartCoroutine(LoadLevelCoroutine(levelNumber));
        }

        public void LoadNextLevel()
        {
            var nextLevel = CurrentLevelIndex + 1;
            if (nextLevel > this._maxLevelCount)
            {
                //TODO: Handle the case when all levels are completed
                Debug.Log("All levels completed! Returning to main menu.");
                this._gameManager.UpdateGameState(GameState.MainMenu);
            }
            else
            {
                LoadLevel(nextLevel);
            }
        }

        public void RestartLevel()
        {
            if (CurrentLevelIndex > 0)
            {
                LoadLevel(CurrentLevelIndex);
            }
        }

        public void OnDespawn()
        {
            this._gridManager.ClearOldGrid();
            CurrentLevelData = null;
        }

        #region PRIVATE METHODS

        private IEnumerator LoadLevelCoroutine(int levelNumber)
        {
            OnDespawn();
            var path = $"Levels/level_{levelNumber}";
            ResourceRequest request = Resources.LoadAsync<TextAsset>(path);

            while (!request.isDone)
            {
                //TODO: Loading UI
                //loadingBar.fillAmount = request.progress;
                yield return null;
            }

            if (request.asset == null)
            {
                Debug.LogError($"Failed to load level data from path: {path}");
                this._gameManager.UpdateGameState(GameState.MainMenu);
                yield break;
            }
            TextAsset jsonFile = request.asset as TextAsset;
            CurrentLevelData  = JsonUtility.FromJson<LevelData>(jsonFile.text);
            CurrentLevelIndex = levelNumber;


            float setupAnimationTime = this._gridManager.SetupGridFromData(CurrentLevelData);

            if (setupAnimationTime > 0)
            {
                yield return new WaitForSeconds(setupAnimationTime);
            }

            if (CurrentLevelData == null)
            {
                Debug.LogError($"Failed to parse level data for level {levelNumber}.");
                this._gameManager.UpdateGameState(GameState.MainMenu);
                yield break;
            }
            this._gameManager.UpdateGameState(GameState.Playing);
        }

        #endregion

    }
}