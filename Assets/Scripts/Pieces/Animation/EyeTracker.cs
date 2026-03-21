using UnityEngine;

namespace Pieces.Animation
{
    public class EyeTracker : MonoBehaviour
    {
        [SerializeField] private float maxRange = 0.1f;
        [SerializeField] private float distanceScale = 0.01f;

        private void Update()
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var parentWorld = transform.parent.position;

            var toMouse = (Vector2)(mouseWorld - parentWorld);
            var distance = toMouse.magnitude;

            if (distance < Mathf.Epsilon)
            {
                transform.localPosition = Vector3.zero;
                return;
            }

            var offset = Mathf.Min(distance * distanceScale, maxRange);
            var localDir = transform.parent.InverseTransformDirection(toMouse / distance);
            transform.localPosition = localDir * offset;
        }
    }
}