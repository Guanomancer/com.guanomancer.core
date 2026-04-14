using System;
using UnityEngine;

namespace Guanomancer
{
    public class CircleGizmo : MonoBehaviour
    {
        [SerializeField] Color _color = Color.blue;
        [SerializeField] float _radius = 0.1f;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = _color; 
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
