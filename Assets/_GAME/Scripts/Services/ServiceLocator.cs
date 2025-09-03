namespace _GAME.Scripts.Services
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
                services[type] = service;
            }
            else
            {
                services.Add(type, service);
                Debug.Log($"Service of type {type.Name} registered.");
            }
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"Service of type {type.Name} not found.");
            return null;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                services.Remove(type);
                Debug.Log($"Service of type {type.Name} unregistered.");
            }
        }

        public static void Clear()
        {
            services.Clear();
            Debug.Log("All services cleared.");
        }
    }
}