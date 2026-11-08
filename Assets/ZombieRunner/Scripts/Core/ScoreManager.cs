using System;
using UnityEngine;
using ZombieRunner.Collectibles;
using ZombieRunner.Enemies;
using ZombieRunner.Player;

namespace ZombieRunner.Core
{
    /// <summary>Tracks run distance, score, kills, and the collectible manager's coin count.</summary>
    [DisallowMultipleComponent]
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private ZombieSpawner zombieSpawner;
        [SerializeField, Min(0f)] private float pointsPerMeter = 1f;
        [SerializeField, Min(0)] private int pointsPerZombie = 25;

        private float startingZ;
        private int lastScore = -1;
        private int lastDistance = -1;
        private int lastCoins = -1;

        public event Action<int, int, int, int> ScoreChanged;
        public int Score { get; private set; }
        public int DistanceMeters { get; private set; }
        public int ZombieKills { get; private set; }
        public int RunCoins => collectibleManager != null ? collectibleManager.RunCoins : 0;

        private void Start()
        {
            ResolveReferences();
            startingZ = player != null ? player.position.z : 0f;
            if (collectibleManager != null)
                collectibleManager.CoinCountChanged += HandleCoinsChanged;
            if (zombieSpawner != null)
                zombieSpawner.ZombieKilled += HandleZombieKilled;
            RefreshScore(true);
        }

        private void Update()
        {
            ResolveReferences();
            RefreshScore(false);
        }

        public void FinalizeRun()
        {
            RefreshScore(true);
            if (SaveManager.Instance != null)
                SaveManager.Instance.RecordRun(Score, RunCoins);
        }

        private void RefreshScore(bool force)
        {
            DistanceMeters = player != null ? Mathf.Max(0, Mathf.FloorToInt(player.position.z - startingZ)) : 0;
            Score = Mathf.Max(0, Mathf.FloorToInt(DistanceMeters * pointsPerMeter) + ZombieKills * pointsPerZombie);
            int coins = RunCoins;
            if (!force && Score == lastScore && DistanceMeters == lastDistance && coins == lastCoins)
                return;

            lastScore = Score;
            lastDistance = DistanceMeters;
            lastCoins = coins;
            ScoreChanged?.Invoke(Score, DistanceMeters, coins, ZombieKills);
        }

        private void HandleCoinsChanged(int _) => RefreshScore(true);

        private void HandleZombieKilled(ZombieController _)
        {
            ZombieKills++;
            RefreshScore(true);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null) player = runner.transform;
            }
            if (collectibleManager == null) collectibleManager = FindAnyObjectByType<CollectibleManager>();
            if (zombieSpawner == null) zombieSpawner = FindAnyObjectByType<ZombieSpawner>();
        }

        private void OnDestroy()
        {
            if (collectibleManager != null)
                collectibleManager.CoinCountChanged -= HandleCoinsChanged;
            if (zombieSpawner != null)
                zombieSpawner.ZombieKilled -= HandleZombieKilled;
        }
    }
}
