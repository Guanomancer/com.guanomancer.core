using UnityEngine;

namespace Guanomancer
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField] Transform _target;

        void Update()
        {
            if (_target != null)
            {
                transform.LookAt(_target);
            }
        }
    }
}
