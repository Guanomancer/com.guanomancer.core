using UnityEngine;

namespace Guanomancer
{
    public class LineTrace : MonoBehaviour
    {
        [SerializeField] float _length = 10f;
        [SerializeField] Color _color = Color.blue;
        [SerializeField] Color _preHitColor = Color.green;
        [SerializeField] Color _postHitColor = Color.yellow;
        [SerializeField] Color _hitBoxColor = Color.red;

        void OnDrawGizmosSelected()
        {
            Vector3 start = transform.position;
            Vector3 end = start + transform.forward * _length;

            if (Physics.Raycast(start, transform.forward, out RaycastHit hit, _length))
            {
                Gizmos.color = _preHitColor;
                Gizmos.DrawLine(start, hit.point);

                Gizmos.color = _postHitColor;
                Gizmos.DrawLine(hit.point, end);

                Gizmos.color = _hitBoxColor;
                Gizmos.DrawWireCube(hit.point, Vector3.one * 0.1f);
            }
            else
            {
                Gizmos.color = _color;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
