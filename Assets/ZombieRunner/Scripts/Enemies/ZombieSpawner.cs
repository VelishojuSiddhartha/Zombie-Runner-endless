using System.Collections.Generic;
using UnityEngine;
using ZombieRunner.Player;

namespace ZombieRunner.Enemies
{
    /// <summary>Spawns configurable zombie archetypes ahead of the runner, across the three lanes.</summary>
    [DisallowMultipleComponent]
    public sealed class ZombieSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject normalZombiePrefab;
        [SerializeField] private GameObject fastZombiePrefab;
        [SerializeField] private GameObject bigZombiePrefab;
        [SerializeField, Min(0.25f)] private float spawnInterval = 2.8f;
        [SerializeField, Min(10f)] private float spawnDistance = 48f;
        [SerializeField, Min(1)] private int maximumAlive = 6;
        [SerializeField, Min(0.1f)] private float laneWidth = 3f;

        [Header("Testing & Predictable Spawning")]
        [SerializeField] private bool testModePredictableSpawning = false;
        [SerializeField] private ZombieArchetype forcedTestArchetype = ZombieArchetype.Normal;
        [SerializeField] private bool cycleTestArchetypes = false;

        private readonly List<ZombieController> activeZombies = new List<ZombieController>();
        private float spawnTimer;
        private int testCycleIndex;

        public event System.Action<ZombieController> ZombieKilled;
        public int ActiveZombieCount => activeZombies.Count;
        public IReadOnlyList<ZombieController> ActiveZombies => activeZombies;

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
            if (player == null || normalZombiePrefab == null)
                return;

            for (int i = activeZombies.Count - 1; i >= 0; i--)
            {
                if (activeZombies[i] == null)
                {
                    activeZombies.RemoveAt(i);
                    continue;
                }
                if (player.position.z - activeZombies[i].transform.position.z > 12f)
                {
                    ZombieController missedZombie = activeZombies[i];
                    activeZombies.RemoveAt(i);
                    missedZombie.Died -= HandleZombieDied;
                    Destroy(missedZombie.gameObject);
                }
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
                return;

            spawnTimer = spawnInterval;
            if (activeZombies.Count < maximumAlive)
                SpawnOne();
        }

        public void SpawnOne()
        {
            if (player == null)
                return;

            ZombieArchetype archetype = ChooseArchetype();
            GameObject prefab = GetPrefab(archetype);
            if (prefab == null)
                prefab = normalZombiePrefab;
            if (prefab == null)
                return;

            int lane = Random.Range(-1, 2);
            Vector3 position = new Vector3(lane * laneWidth, 0f, player.position.z + spawnDistance);
            GameObject instance = Instantiate(prefab, position, Quaternion.Euler(0f, 180f, 0f), transform);
            ZombieController zombie = instance.GetComponent<ZombieController>();
            if (zombie == null)
                zombie = instance.AddComponent<ZombieController>();
            zombie.Configure(archetype, player);
            zombie.Died += HandleZombieDied;
            activeZombies.Add(zombie);
            Debug.Log($"[ZombieRunner] Spawner -> Spawned {archetype} zombie ('{prefab.name}') at lane {lane} (Pos: {position})");
        }

        private ZombieArchetype ChooseArchetype()
        {
            if (testModePredictableSpawning)
            {
                if (cycleTestArchetypes)
                {
                    ZombieArchetype[] cycle = { ZombieArchetype.Normal, ZombieArchetype.Fast, ZombieArchetype.Big };
                    ZombieArchetype chosen = cycle[testCycleIndex % cycle.Length];
                    testCycleIndex++;
                    return chosen;
                }
                return forcedTestArchetype;
            }

            float choice = Random.value;
            if (choice < 0.25f && bigZombiePrefab != null)
                return ZombieArchetype.Big;
            if (choice < 0.55f && fastZombiePrefab != null)
                return ZombieArchetype.Fast;
            return ZombieArchetype.Normal;
        }

        private GameObject GetPrefab(ZombieArchetype archetype)
        {
            switch (archetype)
            {
                case ZombieArchetype.Fast: return fastZombiePrefab;
                case ZombieArchetype.Big: return bigZombiePrefab;
                default: return normalZombiePrefab;
            }
        }

        private void HandleZombieDied(ZombieController zombie)
        {
            zombie.Died -= HandleZombieDied;
            activeZombies.Remove(zombie);
            ZombieKilled?.Invoke(zombie);
        }

        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.25f, spawnInterval);
            spawnDistance = Mathf.Max(10f, spawnDistance);
            maximumAlive = Mathf.Max(1, maximumAlive);
            laneWidth = Mathf.Max(0.1f, laneWidth);
        }
    }
}
