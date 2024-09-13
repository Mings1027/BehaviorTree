using System;
using UnityEngine;

namespace Tree
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        protected static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<T>();

                if (_instance != null) return _instance;
                
                var obj = new GameObject(typeof(T).Name).AddComponent<T>();
                _instance = obj;

                return _instance;

            }
        }

        public static bool Exists => _instance != null;

        [SerializeField] private bool enableDontDestroyOnLoad = true;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (enableDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if(_instance == this) _instance = null;
        }

    }
}