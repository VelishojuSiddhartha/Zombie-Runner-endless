using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRunner.Player;

namespace ZombieRunner.Collectibles
{
    /// <summary>Tracks run coins and spawns coin, ammo, and health pickups ahead of the runner.</summary>
    [DisallowMultipleComponent]
    public sealed class CollectibleManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject ammoPrefab;
        [SerializeField] private GameObject healthPrefab;
        [SerializeField, Min(0.5f)] private float spawnInterval = 2.2f;
        [SerializeField, Min(10f)] private float spawnDistance = 28f;
        [SerializeField, Min(1)] private int maximumAlive = 16;
        [SerializeField, Min(0.1f)] private float laneWidth = 3f;
        [SerializeField, Min(1f)] private float magnetRadius = 15f;
        [SerializeField, Min(0.1f)] private float magnetPullSpeed = 18f;

        private readonly List<CollectiblePickup> activePickups = new List<CollectiblePickup>();
        private float spawnTimer;
        private int runCoins;
        private bool magnetEnabled;
        private bool doubleCoinsEnabled;

        public event Action<int> CoinCountChanged;
        public int RunCoins => runCoins;
        public int ActivePickupCount => activePickups.Count;
        public bool MagnetEnabled => magnetEnabled;
        public bool DoubleCoinsEnabled => doubleCoinsEnabled;

        private void Start()
        {
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null)
                    player = runner.transform;
            }
            spawnTimer = spawnInterval * 0.5f;
        }

        private void Update()
        {
            if (player == null || coinPrefab == null)
                return;

            activePickups.RemoveAll(pickup => pickup == null);
            for (int i = activePickups.Count - 1; i >= 0; i--)
            {
                if (player.position.z - activePickups[i].transform.position.z > 8f)
                {
                    Destroy(activePickups[i].gameObject);
                    activePickups.RemoveAt(i);
                }
                else if (magnetEnabled && activePickups[i].Type == CollectibleType.Coin)
                {
                    Transform pickup = activePickups[i].transform;
                    float distance = Vector3.Distance(pickup.position, player.position);
                    if (distance <= magnetRadius)
                    {
                        pickup.position = Vector3.MoveTowards(pickup.position, player.position, magnetPullSpeed * Time.deltaTime);
                        if (distance <= 0.8f)
                            activePickups[i].Collect(player.GetComponent<PlayerController>());
                    }
                }
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
                return;

            spawnTimer = spawnInterval;
            if (activePickups.Count < maximumAlive)
                SpawnOne();
        }

        public void CollectCoins(int amount)
        {
            if (amount <= 0)
                return;
            runCoins += amount * (doubleCoinsEnabled ? 2 : 1);
            CoinCountChanged?.Invoke(runCoins);
        }

        public void SetMagnetEnabled(bool enabled)
        {
            magnetEnabled = enabled;
        }

        public void SetDoubleCoinsEnabled(bool enabled)
        {
            doubleCoinsEnabled = enabled;
        }

        public void NotifyCollected(CollectiblePickup pickup)
        {
            activePickups.Remove(pickup);
        }

        public void SpawnOne()
        {
            if (player == null)
                return;

            CollectibleType type = ChooseType();
            GameObject prefab = GetPrefab(type);
            if (prefab == null)
            {
                type = CollectibleType.Coin;
                prefab = coinPrefab;
            }
            if (prefab == null)
                return;

            int lane = UnityEngine.Random.Range(-1, 2);
            Vector3 position = player.position + Vector3.forward * spawnDistance + Vector3.right * (lane * laneWidth);
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
            CollectiblePickup pickup = instance.GetComponent<CollectiblePickup>();
            if (pickup == null)
                pickup = instance.AddComponent<CollectiblePickup>();
            pickup.Configure(type, DefaultAmount(type), this);
            activePickups.Add(pickup);
        }

        private CollectibleType ChooseType()
        {
            float roll = UnityEngine.Random.value;
            if (roll > 0.84f && healthPrefab != null)
                return CollectibleType.Health;
            if (roll > 0.62f && ammoPrefab != null)
                return CollectibleType.Ammo;
            return CollectibleType.Coin;
        }

        private GameObject GetPrefab(CollectibleType type)
        {
            switch (type)
            {
                case CollectibleType.Ammo: return ammoPrefab;
                case CollectibleType.Health: return healthPrefab;
                default: return coinPrefab;
            }
        }

        private static int DefaultAmount(CollectibleType type)
        {
            switch (type)
            {
                case CollectibleType.Ammo: return 15;
                case CollectibleType.Health: return 25;
                default: return 1;
            }
        }

        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.5f, spawnInterval);
            spawnDistance = Mathf.Max(10f, spawnDistance);
            maximumAlive = Mathf.Max(1, maximumAlive);
            laneWidth = Mathf.Max(0.1f, laneWidth);
            magnetRadius = Mathf.Max(1f, magnetRadius);
            magnetPullSpeed = Mathf.Max(0.1f, magnetPullSpeed);
        }
    }
}
