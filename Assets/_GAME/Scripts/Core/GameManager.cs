// File: GameManager.cs

using System;
using _GAME.Scripts.Core;
using _GAME.Scripts.Grid;
using _GAME.Scripts.Level;
using _GAME.Scripts.Services;
using _GAME.Scripts.UI;
using UnityEngine;

[RequireComponent(typeof(TimeManager))]
public class GameManager : MonoBehaviour
{
    public static event Action<GameState> OnGameStateChanged;

    private GameState _currentState;

    private TimeManager timeManager;

    private LevelManager _levelManager;

    private UIManager _uiManager;

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
        this.timeManager.OnTimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        this.timeManager.OnTimeUp -= HandleTimeUp;
    }

    private void Awake()
    {
        ServiceLocator.Register(this);
        this.timeManager = GetComponent<TimeManager>();
    }

    private void Start()
    {
        _levelManager = ServiceLocator.Get<LevelManager>();
        _uiManager    = ServiceLocator.Get<UIManager>();
        UpdateGameState(GameState.MainMenu);
    }

    public void StartLevel(int levelNumber)
    {
        UpdateGameState(GameState.LevelSetup);
        this._levelManager.LoadLevel(levelNumber);
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
            case GameState.Shuffling:      this.HandleShuffling(); break;
        }
    }

    #region GAMESTATE-HANDLERS

    private void HandleMainMenu()
    {
        Debug.Log("Game State: Main Menu");
        this._uiManager.CloseAll();
        this._uiManager.Open<MainMenuCanvas>();
    }

    private void HandleLevelSetup()
    {
        this._uiManager.CloseDirectly<MainMenuCanvas>();
    }

    private void HandlePlaying()
    {
        this._uiManager.Open<GamePlayCanvas>();

        var timeLimit = this._levelManager.CurrentLevelData.timeLimit;
        this.timeManager.StartTimer(timeLimit);
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
        this.timeManager.StopTimer();
    }

    private void HandleGameOver()
    {
        Debug.Log("Game State: Game Over");
        this.timeManager.StopTimer();
    }

    private void HandleShuffling()
    {
        Debug.Log("Game State: Shuffling... Player input is locked.");
    }

    #endregion
}