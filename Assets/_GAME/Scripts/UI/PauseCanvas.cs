using _GAME.Scripts.Core;
using _GAME.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class PauseCanvas : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;

    public override void SetUp()
    {
        base.SetUp();

        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnResumeButtonClicked()
    {
        this.Close(0);
    }

    public override void CloseDirectly()
    {
        base.CloseDirectly();
        if (_gameManager != null && _gameManager.CurrentState == GameState.Paused)
        {
            _gameManager.ResumeGame();
        }
    }
}