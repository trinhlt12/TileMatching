namespace _GAME.Scripts.UI
{
    using System;
    using _GAME.Scripts.Services;
    using UnityEngine;

    public class UICanvas : MonoBehaviour
    {
        [SerializeField] bool isDestroyOnClose = false;

        protected GameManager _gameManager;

        private void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            float         ratio         = (float)Screen.width / Screen.height;
            if (ratio > 2.1f)
            {
                var leftBottom = rectTransform.offsetMin;
                var rightTop   = rectTransform.offsetMax;
                leftBottom.y = 0f;
                rightTop.y   = -100f;

                rectTransform.offsetMin = leftBottom;
                rectTransform.offsetMax = rightTop;
            }
        }

        private void Start()
        {
            this._gameManager = ServiceLocator.Get<GameManager>();
        }

        public virtual void SetUp()
        {
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close(float time)
        {
            Invoke(nameof(CloseDirectly), time);
        }

        public virtual void CloseDirectly()
        {
            if (this.isDestroyOnClose)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}