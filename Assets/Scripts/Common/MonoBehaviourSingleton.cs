using UnityEngine;

namespace Common
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    CreateInstance();
                
                return _instance;
            }
        }
        
        public static bool HasInstance => _instance != null;

        private static void CreateInstance()
        {
            if (_instance != null) 
                return;
            
            _instance = FindObjectOfType(typeof(T)) as T;
            
            if (_instance == null)
                _instance = new GameObject($"Singleton Instance {typeof(T)}", typeof(T)).GetComponent<T>();
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogError($"Another instance of {GetType().ToString()} is already exist! Destroying self");
                DestroyImmediate(this);
                return;
            }

            Initialize();
        }

        protected virtual void Initialize()
        {
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }
    }
}