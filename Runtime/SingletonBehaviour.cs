using UnityEngine;

namespace Guanomancer
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        private static T _currentInstance;

        [System.Obsolete("Use Current instead.")]
        public static T Instance => Current;

        public static bool HasCurrent
        {
            get
            {
                if (_currentInstance != null) return true;
                if (_currentInstance = FindAnyObjectByType<T>(FindObjectsInactive.Include)) return true;
                return false;
            }
        }

        public static T Current
        {
            get
            {
                if (_currentInstance == null)
                {
                    _currentInstance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                    if (_currentInstance == null)
                    {
                        Log.Warn(null, $"Unable to find and instance of {typeof(T).Name}.");
                    }
                }
                return _currentInstance;
            }
        }
    }
}
