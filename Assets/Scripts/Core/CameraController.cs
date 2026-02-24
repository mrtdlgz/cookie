using UnityEngine;

namespace Cookie.Core
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Targeting")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 12, -10);
        [SerializeField] private float smoothSpeed = 5f;

        [Header("Shake Effect")]
        private float _shakeTimer;
        private float _shakeMagnitude;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Initial jump to target to avoid sweeping across the map on start
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Smooth Follow
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Shaking
            if (_shakeTimer > 0)
            {
                // Only shake X and Z for 2.5D visual stability, or fully spherical
                Vector2 randomCircle = Random.insideUnitCircle * _shakeMagnitude;
                smoothedPosition.x += randomCircle.x;
                smoothedPosition.z += randomCircle.y;
                _shakeTimer -= Time.deltaTime;
            }

            transform.position = smoothedPosition;
        }

        public void Shake(float duration = 0.15f, float magnitude = 0.3f)
        {
            _shakeTimer = duration;
            _shakeMagnitude = magnitude;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
