using System.Collections.Generic;
using UnityEngine;
using ZombieRunner.Player;

namespace ZombieRunner.World
{
    /// <summary>Places dodgeable obstacles in one of the three runner lanes.</summary>
    [DisallowMultipleComponent]
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject roadblockPrefab;
        [SerializeField] private GameObject abandonedCarPrefab;
        [SerializeField] private GameObject explosiveBarrelPrefab;
        [SerializeField, Min(0.5f)] private float spawnInterval = 3.1f;
        [SerializeField, Min(10f)] private float spawnDistance = 36f;
        [SerializeField, Min(1)] private int maximumAlive = 8;
        [SerializeField, Min(0.1f)] private float laneWidth = 3f;

        private readonly List<ObstacleController> activeObstacles = new List<ObstacleController>();
        private float spawnTimer;

        public int ActiveObstacleCount => activeObstacles.Count;

        private void Start()
        {
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null)
                    player = runner.transform;
            }
            spawnTimer = spawnInterval * 0.65f;
        }

        private void Update()
        {
            if (player == null || roadblockPrefab == null)
                return;

            activeObstacles.RemoveAll(obstacle => obstacle == null);
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
                return;

            spawnTimer = spawnInterval;
            if (activeObstacles.Count < maximumAlive)
                SpawnOne();
        }

        public void SpawnOne()
        {
            if (player == null)
                return;

            ObstacleType type = (ObstacleType)Random.Range(0, 3);
            GameObject prefab = GetPrefab(type);
            if (prefab == null)
                prefab = roadblockPrefab;
            int lane = Random.Range(-1, 2);
            Vector3 position = player.position + Vector3.forward * spawnDistance + Vector3.right * (lane * laneWidth);
            position.y = 0f;
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
            ObstacleController obstacle = instance.GetComponent<ObstacleController>();
            if (obstacle == null)
                obstacle = instance.AddComponent<ObstacleController>();
            obstacle.Configure(type, player);
            activeObstacles.Add(obstacle);
        }

        private GameObject GetPrefab(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.AbandonedCar: return abandonedCarPrefab;
                case ObstacleType.ExplodingBarrel: return explosiveBarrelPrefab;
                default: return roadblockPrefab;
            }
        }

        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.5f, spawnInterval);
            spawnDistance = Mathf.Max(10f, spawnDistance);
            maximumAlive = Mathf.Max(1, maximumAlive);
            laneWidth = Mathf.Max(0.1f, laneWidth);
        }
    }
}
