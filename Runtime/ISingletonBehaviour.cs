using UnityEngine;

namespace Guanomancer
{
    public interface ISingletonBehaviour<T>
        where T : MonoBehaviour, ISingletonBehaviour<T>
    {
        private static T _currentInstance;

        public static T Instance
        {
            get
            {
                if (_currentInstance == null)
                {
                    _currentInstance = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
                    if(_currentInstance == null)
                    {
                        Log.Warn(null, $"Unable to find and instance of {typeof(T).Name}.");
                    }
                }
                return _currentInstance;
            }
        }
    }
}
