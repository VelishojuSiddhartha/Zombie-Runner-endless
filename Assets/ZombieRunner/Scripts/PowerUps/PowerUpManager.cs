using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRunner.Audio;
using ZombieRunner.Collectibles;
using ZombieRunner.Player;
using ZombieRunner.Weapons;

namespace ZombieRunner.PowerUps
{
    /// <summary>Applies instant refills and owns timed power-up activation and expiration.</summary>
    [DisallowMultipleComponent]
    public sealed class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private GameObject pickupPrefab;

        [Header("Instant effects")]
        [SerializeField, Min(1f)] private float healthRestoreAmount = 35f;

        [Header("Timed effects")]
        [SerializeField, Min(0.1f)] private float magnetDuration = 8f;
        [SerializeField, Min(0.1f)] private float shieldDuration = 7f;
        [SerializeField, Min(0.1f)] private float speedBoostDuration = 5f;
        [SerializeField, Min(0.1f)] private float doubleCoinsDuration = 10f;
        [SerializeField, Min(1f)] private float speedBoostMultiplier = 1.5f;

        [Header("World spawning")]
        [SerializeField, Min(1f)] private float spawnInterval = 9f;
        [SerializeField, Min(10f)] private float spawnDistance = 32f;
        [SerializeField, Min(1)] private int maximumAlive = 2;
        [SerializeField, Min(0.1f)] private float laneWidth = 3f;

        private readonly Dictionary<PowerUpType, float> activeTimers = new Dictionary<PowerUpType, float>();
        private readonly List<PowerUpType> expiredTypes = new List<PowerUpType>();
        private readonly List<PowerUpPickup> activePickups = new List<PowerUpPickup>();
        private float spawnTimer;

        public event Action<PowerUpType, float> PowerUpActivated;
        public event Action<PowerUpType> PowerUpExpired;

        public int ActivePickupCount => activePickups.Count;

        private void Start()
        {
            ResolveReferences();
            spawnTimer = spawnInterval * 0.5f;
        }

        private void Update()
        {
            ResolveReferences();
            UpdateTimers(Time.deltaTime);
            UpdatePickupSpawner(Time.deltaTime);
        }

        public bool IsActive(PowerUpType type)
        {
            return activeTimers.ContainsKey(type);
        }

        public float GetRemainingDuration(PowerUpType type)
        {
            return activeTimers.TryGetValue(type, out float remaining) ? remaining : 0f;
        }

        public void Activate(PowerUpType type, float durationOverride = 0f)
        {
            ResolveReferences();
            if (type == PowerUpType.HealthRestore)
            {
                PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
                if (health != null)
                    health.RestoreHealth(healthRestoreAmount);
                AudioManager.Instance?.Play(GameAudioCue.PowerUp);
                PowerUpActivated?.Invoke(type, 0f);
                return;
            }

            if (type == PowerUpType.AmmoRefill)
            {
                ShootingController shooter = player != null ? player.GetComponent<ShootingController>() : null;
                if (shooter != null)
                    shooter.RefillAmmo();
                AudioManager.Instance?.Play(GameAudioCue.PowerUp);
                PowerUpActivated?.Invoke(type, 0f);
                return;
            }

            float duration = durationOverride > 0f ? durationOverride : GetConfiguredDuration(type);
            bool wasActive = activeTimers.ContainsKey(type);
            activeTimers[type] = duration;
            if (!wasActive)
                ApplyTimedEffect(type, true);
            AudioManager.Instance?.Play(GameAudioCue.PowerUp);
            PowerUpActivated?.Invoke(type, duration);
        }

        public void NotifyCollected(PowerUpPickup pickup)
        {
            activePickups.Remove(pickup);
        }

        public void SpawnOne()
        {
            ResolveReferences();
            if (player == null || pickupPrefab == null)
                return;

            PowerUpType type = (PowerUpType)UnityEngine.Random.Range(0, 6);
            Vector3 position = player.position + Vector3.forward * spawnDistance + Vector3.right * (UnityEngine.Random.Range(-1, 2) * laneWidth);
            GameObject instance = Instantiate(pickupPrefab, position, Quaternion.identity, transform);
            PowerUpPickup pickup = instance.GetComponent<PowerUpPickup>();
            if (pickup == null)
                pickup = instance.AddComponent<PowerUpPickup>();
            pickup.Configure(type, 0f, this);
            activePickups.Add(pickup);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null)
                    player = runner.transform;
            }
            if (collectibleManager == null)
                collectibleManager = FindAnyObjectByType<CollectibleManager>();
        }

        private void UpdateTimers(float deltaTime)
        {
            if (activeTimers.Count == 0)
                return;

            expiredTypes.Clear();
            var keys = new List<PowerUpType>(activeTimers.Keys);
            foreach (PowerUpType type in keys)
            {
                float remaining = activeTimers[type] - deltaTime;
                if (remaining <= 0f)
                    expiredTypes.Add(type);
                else
                    activeTimers[type] = remaining;
            }

            foreach (PowerUpType type in expiredTypes)
            {
                activeTimers.Remove(type);
                ApplyTimedEffect(type, false);
                PowerUpExpired?.Invoke(type);
            }
        }

        private void UpdatePickupSpawner(float deltaTime)
        {
            activePickups.RemoveAll(pickup => pickup == null);
            if (player == null || pickupPrefab == null)
                return;

            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                if (player.position.z - activePickups[i].transform.position.z > 8f)
                {
                    Destroy(activePickups[i].gameObject);
                    activePickups.RemoveAt(i);
                }
            }

            spawnTimer -= deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                if (activePickups.Count < maximumAlive)
                    SpawnOne();
            }
        }

        private void ApplyTimedEffect(PowerUpType type, bool enabled)
        {
            ResolveReferences();
            switch (type)
            {
                case PowerUpType.CoinMagnet:
                    if (collectibleManager != null) collectibleManager.SetMagnetEnabled(enabled);
                    break;
                case PowerUpType.Shield:
                    PlayerHealth health = player != null ? player.GetComponent<PlayerHealth>() : null;
                    if (health != null) health.SetShielded(enabled);
                    break;
                case PowerUpType.SpeedBoost:
                    PlayerController movement = player != null ? player.GetComponent<PlayerController>() : null;
                    if (movement != null) movement.SetSpeedMultiplier(enabled ? speedBoostMultiplier : 1f);
                    break;
                case PowerUpType.DoubleCoins:
                    if (collectibleManager != null) collectibleManager.SetDoubleCoinsEnabled(enabled);
                    break;
            }
        }

        private float GetConfiguredDuration(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.CoinMagnet: return magnetDuration;
                case PowerUpType.Shield: return shieldDuration;
                case PowerUpType.SpeedBoost: return speedBoostDuration;
                case PowerUpType.DoubleCoins: return doubleCoinsDuration;
                default: return 0f;
            }
        }

        private void OnValidate()
        {
            healthRestoreAmount = Mathf.Max(1f, healthRestoreAmount);
            magnetDuration = Mathf.Max(0.1f, magnetDuration);
            shieldDuration = Mathf.Max(0.1f, shieldDuration);
            speedBoostDuration = Mathf.Max(0.1f, speedBoostDuration);
            doubleCoinsDuration = Mathf.Max(0.1f, doubleCoinsDuration);
            speedBoostMultiplier = Mathf.Max(1f, speedBoostMultiplier);
            spawnInterval = Mathf.Max(1f, spawnInterval);
            spawnDistance = Mathf.Max(10f, spawnDistance);
            maximumAlive = Mathf.Max(1, maximumAlive);
            laneWidth = Mathf.Max(0.1f, laneWidth);
        }
    }
}
