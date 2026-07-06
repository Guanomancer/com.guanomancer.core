using UnityEngine;

namespace Guanomancer
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        private static T _currentInstance;

        public static T Instance
        {
            get
            {
                if (_currentInstance == null)
                {
                    _currentInstance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                }
                return _currentInstance;
            }
        }
    }
}
