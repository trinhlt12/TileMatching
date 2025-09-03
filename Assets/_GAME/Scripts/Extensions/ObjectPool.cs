namespace _GAME.Scripts.Extensions
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectPool<T> where T : Component
    {
        private Queue<T> _objectPool = new();
        private T        _objectPrefab;

        public ObjectPool(T prefab, int initialSize = 10)
        {
            this._objectPrefab = prefab;

            for (var i = 0; i < initialSize; i++)
            {
                T instance = Object.Instantiate(this._objectPrefab);
                instance.gameObject.SetActive(false);
                this._objectPool.Enqueue(instance);
            }
        }

        public T Spawn(Vector3 position, Quaternion rotation)
        {
            T instance = null;

            // Keep trying to get a valid instance from pool
            while (_objectPool.Count > 0)
            {
                var pooledInstance = _objectPool.Dequeue();
                if (pooledInstance != null) // Check if not destroyed
                {
                    instance = pooledInstance;
                    break;
                }
                // If destroyed, continue to next item in pool
            }

            // If no valid instance found, create new one
            if (instance == null)
            {
                instance = Object.Instantiate(_objectPrefab);
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public void ReturnToPool(T instance)
        {
            if (instance != null) // Add null check
            {
                instance.gameObject.SetActive(false);
                _objectPool.Enqueue(instance);
            }
        }
    }
}