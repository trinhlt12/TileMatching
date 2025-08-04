// File: GameManager.cs

using System;
using _GAME.Scripts.Core;
using _GAME.Scripts.Grid;
using _GAME.Scripts.Level;
using _GAME.Scripts.UI;
using UnityEngine;

[RequireComponent(typeof(TimerController))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    private GameState _currentState;

    private TimerController _timerController;

    private int _currentLevel = 1;

    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                OnGameStateChanged?.Invoke(_currentState);
            }
        }
    }

    private void OnEnable()
    {
        _timerController.OnTimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        _timerController.OnTimeUp -= HandleTimeUp;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        _timerController = GetComponent<TimerController>();
    }

    private void Start()
    {
        UpdateGameState(GameState.MainMenu);
    }

    public void StartLevel(int levelNumber)
    {
        this._currentLevel = levelNumber;

        UpdateGameState(GameState.LevelSetup);
    }

    private void HandleTimeUp()
    {
        Debug.Log("GAME MANAGER: Time's up! Game Over.");
        UpdateGameState(GameState.GameOver);
    }

    public void UpdateGameState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:       this.HandleMainMenu(); break;
            case GameState.LevelSetup:     this.HandleLevelSetup(); break;
            case GameState.Playing:        this.HandlePlaying(); break;
            case GameState.Paused:         this.HandlePaused(); break;
            case GameState.LevelCompleted: this.HandleLevelCompleted(); break;
            case GameState.GameOver:       this.HandleGameOver(); break;
        }
    }

    #region GAMESTATE-HANDLERS

    private void HandleMainMenu()
    {
        Debug.Log("Game State: Main Menu");
        UIManager.Instance.CloseAll();
        UIManager.Instance.Open<MainMenuCanvas>();
    }

    private void HandleLevelSetup()
    {
        Debug.Log("Game State: Level Setup");
        UIManager.Instance.CloseDirectly<MainMenuCanvas>();
        var levelData = LevelLoader.LoadLevel(_currentLevel);
        if (levelData != null)
        {
            // _timerController.StartTimer(levelData.timeLimit);
            GridManager.Instance.SetupGridFromData(levelData);
            UpdateGameState(GameState.Playing);
        }
        else
        {
            UpdateGameState(GameState.MainMenu);
        }
    }

    private void HandlePlaying()
    {
        UIManager.Instance.Open<GamePlayCanvas>();

        _timerController.StartTimer(LevelLoader.LoadLevel(_currentLevel).timeLimit);
        Time.timeScale = 1f;
    }

    private void HandlePaused()
    {
        Debug.Log("Game State: Paused");
        Time.timeScale = 0f;
    }

    private void HandleLevelCompleted()
    {
        Debug.Log("Game State: Level Complete");
        _timerController.StopTimer();
    }

    private void HandleGameOver()
    {
        Debug.Log("Game State: Game Over");
        _timerController.StopTimer();
    }

    #endregion
}