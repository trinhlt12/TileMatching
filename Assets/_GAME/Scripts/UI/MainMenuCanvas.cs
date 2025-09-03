namespace _GAME.Scripts.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MainMenuCanvas : UICanvas
    {
        [SerializeField] private Button playButton;

        public override void SetUp()
        {
            base.SetUp();

            playButton.onClick.RemoveAllListeners();

            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("MainMenu: Play Button Clicked! Notifying GameManager.");

            GameManager.Instance.StartLevel(1);
        }
    }
}