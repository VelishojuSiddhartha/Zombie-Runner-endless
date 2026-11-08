using System;
using UnityEngine;

namespace ZombieRunner.Weapons
{
    /// <summary>
    /// High-velocity visible bullet tracer projectile with glowing core,
    /// dynamic light, and aerodynamic trail renderer. Triggers visual impact effects at hit destination.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BulletTracer : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField, Min(50f)] private float speed = 195f;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private MeshRenderer projectileCore;
        [SerializeField] private Light projectileGlowLight;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 travelDirection;
        private float totalDistance;
        private float distanceTraveled;
        private bool isHitTarget;
        private bool isZombieHit;
        private bool isMetalHit;
        private Vector3 hitNormal;
        private Action<BulletTracer> onCompleteCallback;

        public void Initialize(
            Vector3 start,
            Vector3 target,
            bool hasHit,
            bool isZombie,
            bool isMetal,
            Vector3 normal,
            Action<BulletTracer> onComplete)
        {
            startPosition = start;
            targetPosition = target;
            travelDirection = (target - start).normalized;
            totalDistance = Vector3.Distance(start, target);
            distanceTraveled = 0f;
            isHitTarget = hasHit;
            isZombieHit = isZombie;
            isMetalHit = isMetal;
            hitNormal = normal.sqrMagnitude > 0.001f ? normal : -travelDirection;
            onCompleteCallback = onComplete;

            transform.position = start;
            transform.rotation = Quaternion.LookRotation(travelDirection);

            if (trail != null)
            {
                trail.Clear();
                trail.emitting = true;
            }

            if (projectileCore != null)
                projectileCore.enabled = true;

            if (projectileGlowLight != null)
                projectileGlowLight.enabled = true;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            distanceTraveled += step;

            if (distanceTraveled >= totalDistance)
            {
                transform.position = targetPosition;
                FinishProjectile();
                return;
            }

            transform.position += travelDirection * step;
        }

        private void FinishProjectile()
        {
            if (isHitTarget)
            {
                // Trigger visual and acoustic impact
                HitImpactManager.Instance?.SpawnImpact(targetPosition, hitNormal, isZombieHit, isMetalHit);
            }

            if (trail != null)
                trail.emitting = false;

            if (projectileGlowLight != null)
                projectileGlowLight.enabled = false;

            onCompleteCallback?.Invoke(this);
        }

        public void ResetForPool()
        {
            if (trail != null)
            {
                trail.Clear();
                trail.emitting = false;
            }
            if (projectileGlowLight != null)
                projectileGlowLight.enabled = false;
            gameObject.SetActive(false);
        }
    }
}
