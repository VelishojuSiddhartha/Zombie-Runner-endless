using UnityEngine;
using ZombieRunner.Audio;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.Utilities;

namespace ZombieRunner.World
{
    public enum ObstacleType
    {
        Roadblock,
        AbandonedCar,
        ExplodingBarrel
    }

    /// <summary>Collision damage and break behavior for roadside obstacles.</summary>
    [DisallowMultipleComponent]
    public sealed class ObstacleController : MonoBehaviour, IDamageable
    {
        [SerializeField] private ObstacleType obstacleType;
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float collisionDamage = 25f;
        [SerializeField, Min(1f)] private float maximumHealth = 1f;
        [SerializeField, Min(0f)] private float currentHealth = 1f;
        [SerializeField, Min(0f)] private float explosionRadius = 4.5f;
        [SerializeField, Min(0f)] private float explosionDamage = 35f;

        private float nextCollisionDamageTime;
        private bool destroyed;

        public ObstacleType Type => obstacleType;
        public bool IsDestroyed => destroyed;

        private void Update()
        {
            if (player != null && player.position.z - transform.position.z > 9f)
                Destroy(gameObject);
        }

        public void Configure(ObstacleType type, Transform target)
        {
            obstacleType = type;
            player = target;
        }

        public void OnPlayerCollision(PlayerHealth health)
        {
            if (destroyed || health == null || Time.time < nextCollisionDamageTime)
                return;

            nextCollisionDamageTime = Time.time + 0.8f;
            health.TakeDamage(collisionDamage);
            if (obstacleType == ObstacleType.ExplodingBarrel)
                Explode();
        }

        public void TakeDamage(float amount)
        {
            if (destroyed || amount <= 0f || obstacleType != ObstacleType.ExplodingBarrel)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
                Explode();
        }

        private void Explode()
        {
            if (destroyed)
                return;

            destroyed = true;
            AudioManager.Instance?.Play(GameAudioCue.Explosion);
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider item in colliders)
            {
                PlayerHealth playerHealth = item.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(explosionDamage);

                ZombieController zombie = item.GetComponentInParent<ZombieController>();
                if (zombie != null)
                    zombie.TakeDamage(explosionDamage);
            }

            Collider ownCollider = GetComponent<Collider>();
            if (ownCollider != null)
                ownCollider.enabled = false;
            Destroy(gameObject, 0.15f);
        }

        private void OnValidate()
        {
            collisionDamage = Mathf.Max(0f, collisionDamage);
            maximumHealth = Mathf.Max(1f, maximumHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maximumHealth);
            explosionRadius = Mathf.Max(0f, explosionRadius);
            explosionDamage = Mathf.Max(0f, explosionDamage);
        }
    }
}
