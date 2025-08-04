namespace _GAME.Scripts.Extensions
{
    using UnityEngine;

    public class Singleton<T>: MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        _instance = new GameObject(nameof(T)).AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }
}