namespace _GAME.Scripts.Extensions
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectPool<T> where T : MonoBehaviour
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
            var instance = this._objectPool.Count > 0 ? this._objectPool.Dequeue() : Object.Instantiate(this._objectPrefab);

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public void ReturnToPool(T instance)
        {
            instance.gameObject.SetActive(false);
            _objectPool.Enqueue(instance);
        }
    }
}