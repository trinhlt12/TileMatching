// File: GameManager.cs
using System;
using _GAME.Scripts.Core;
using _GAME.Scripts.Grid;
using _GAME.Scripts.Level;
using _GAME.Scripts.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    private GameState _currentState;

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
    public void UpdateGameState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Debug.Log("Game State: Main Menu");
                UIManager.Instance.CloseAll();
                UIManager.Instance.Open<MainMenuCanvas>();
                break;
            case GameState.LevelSetup:
                Debug.Log("Game State: Level Setup");
                UIManager.Instance.CloseDirectly<MainMenuCanvas>();
                var levelData = LevelLoader.LoadLevel(_currentLevel);
                if (levelData != null)
                {
                    GridManager.Instance.SetupGridFromData(levelData);
                    UpdateGameState(GameState.Playing);
                }
                else
                {
                    UpdateGameState(GameState.MainMenu);
                }
                break;
            case GameState.Playing:
                Debug.Log("Game State: Playing");
                UIManager.Instance.CloseAll();
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Debug.Log("Game State: Paused");
                Time.timeScale = 0f;
                break;
            case GameState.LevelComplete:
                Debug.Log("Game State: Level Complete");
                break;
            case GameState.GameOver:
                Debug.Log("Game State: Game Over");
                break;
        }
    }
}