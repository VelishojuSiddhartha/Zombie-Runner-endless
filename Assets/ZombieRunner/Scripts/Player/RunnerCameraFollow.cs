using UnityEngine;

namespace ZombieRunner.Player
{
    /// <summary>Soft third-person camera follow with a short forward look-ahead.</summary>
    [DisallowMultipleComponent]
    public sealed class RunnerCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 2.75f, -4.8f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;
        [SerializeField, Min(0f)] private float lookAheadDistance = 14f;
        [SerializeField] private float lookHeight = 1.35f;

        private Vector3 followVelocity;
        private float shakeTimer;
        private float shakeIntensity;

        public static RunnerCameraFollow Instance { get; private set; }
        public Transform Target => target;

        public void AddShake(float intensity = 0.05f, float duration = 0.08f)
        {
            shakeIntensity = intensity;
            shakeTimer = duration;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Awake()
        {
            Instance = this;
            if (target == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null)
                    target = runner.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 shakeOffset = Vector3.zero;
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                shakeOffset = UnityEngine.Random.insideUnitSphere * shakeIntensity;
            }

            Vector3 desiredPosition = target.position + followOffset + shakeOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, smoothTime);

            Vector3 lookTarget = target.position + target.forward * lookAheadDistance + Vector3.up * lookHeight;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), 1f - Mathf.Exp(-12f * Time.deltaTime));
        }
    }
}
