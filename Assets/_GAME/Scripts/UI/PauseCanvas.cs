using _GAME.Scripts.Core;
using _GAME.Scripts.Level;
using _GAME.Scripts.Services;
using _GAME.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class PauseCanvas : UICanvas
{
    [Header("Buttons")] [SerializeField] private Button resumeButton;
    [SerializeField]                     private Button restartButton;
    [SerializeField]                     private Button mainMenuButton;

    public override void SetUp()
    {
        base.SetUp();

        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(OnResumeButtonClicked);

        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    private void OnResumeButtonClicked()
    {
        _gameManager.ResumeGame();
    }

    private void OnRestartButtonClicked()
    {
        _gameManager.RestartLevel();
    }

    private void OnMainMenuButtonClicked()
    {
        _gameManager.QuitToMainMenu();
    }
}