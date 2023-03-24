using UnityEngine;

namespace Script.GameFramework.Core
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                _instance = FindObjectOfType<T>();
                
                if (_instance != null) return _instance;
                
                var singletonObject = new GameObject
                {
                    name = typeof(T).Name
                };
                _instance = singletonObject.AddComponent<T>();
                DontDestroyOnLoad(singletonObject);
                
                return _instance;
            }
        }
    }
}