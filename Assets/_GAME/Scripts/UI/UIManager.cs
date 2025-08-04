namespace _GAME.Scripts.UI
{
    using System.Collections.Generic;
    using _GAME.Scripts.Extensions;
    using UnityEngine;

    public class UIManager : Singleton<UIManager>
    {
        public                   Dictionary<System.Type, UICanvas> activeCanvases = new Dictionary<System.Type, UICanvas>();
        public                   Dictionary<System.Type, UICanvas> canvasPrefabs  = new Dictionary<System.Type, UICanvas>();
        [SerializeField] private Transform                         _parentCanvas;

        private void Awake()
        {
            var prefabs = Resources.LoadAll<UICanvas>("UI");
            Debug.Log($"UIManager: Found {prefabs.Length} UI prefabs in Resources/UI.");
            foreach (var item in prefabs)
            {
                if (!this.canvasPrefabs.ContainsKey(item.GetType()))
                {
                    this.canvasPrefabs.Add(item.GetType(), item);
                    Debug.Log($"UIManager: Loaded {item.GetType().Name} prefab.");
                }
            }
        }

        public T Open<T>() where T : UICanvas
        {
            T canvas = GetUI<T>();
            canvas.SetUp();
            canvas.Open();
            return canvas;
        }

        public void CloseUI<T>(float time) where T : UICanvas
        {
            if (this.IsLoaded<T>())
            {
                activeCanvases[typeof(T)].Close(time);
            }
        }

        public void CloseDirectly<T>() where T : UICanvas
        {
            if (this.IsLoaded<T>())
            {
                this.activeCanvases[typeof(T)].CloseDirectly();
            }
        }

        public void CloseAll()
        {
            foreach (var canvas in this.activeCanvases)
            {
                if (canvas.Value != null && canvas.Value.gameObject.activeSelf)
                {
                    canvas.Value.Close(0);
                }
            }
        }

        public bool IsLoaded<T>() where T : UICanvas
        {
            return this.activeCanvases.ContainsKey(typeof(T));
        }

        public bool IsOpened<T>() where T : UICanvas
        {
            return this.IsLoaded<T>() && this.activeCanvases[typeof(T)].gameObject.activeSelf;
        }

        public T GetUI<T>() where T : UICanvas
        {
            if (!this.IsLoaded<T>())
            {
                T prefab = this.GetUIPrefab<T>();
                T canvas = Instantiate(prefab);
                this.activeCanvases[typeof(T)] = canvas;
            }
            return this.activeCanvases[typeof(T)] as T;
        }

        private T GetUIPrefab<T>() where T : UICanvas
        {
            return this.canvasPrefabs[typeof(T)] as T;
        }
    }
}