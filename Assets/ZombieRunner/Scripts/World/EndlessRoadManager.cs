using System.Collections.Generic;
using UnityEngine;

namespace ZombieRunner.World
{
    /// <summary>
    /// Keeps a fixed pool of road segments ahead of the runner and reuses each
    /// segment after it passes behind the player.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EndlessRoadManager : MonoBehaviour
    {
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private Transform player;
        [SerializeField, Min(5f)] private float segmentLength = 30f;
        [SerializeField, Min(2)] private int pooledSegmentCount = 5;
        [SerializeField] private float initialCenterZ;
        [SerializeField, Min(0f)] private float recycleMargin = 5f;

        private readonly Queue<Transform> segments = new Queue<Transform>();
        private float nextSegmentCenterZ;
        private bool initialized;

        public int PooledSegmentCount => segments.Count;
        public float SegmentLength => segmentLength;

        private void Start()
        {
            InitializePool();
        }

        private void Update()
        {
            if (!initialized || player == null || segments.Count == 0)
                return;

            while (player.position.z > segments.Peek().position.z + segmentLength * 0.5f + recycleMargin)
            {
                Transform recycled = segments.Dequeue();
                recycled.position = new Vector3(0f, 0f, nextSegmentCenterZ);
                nextSegmentCenterZ += segmentLength;
                segments.Enqueue(recycled);
            }
        }

        /// <summary>Build the fixed-size segment pool. Safe to call more than once.</summary>
        public void InitializePool()
        {
            if (initialized)
                return;

            if (segmentPrefab == null)
            {
#if UNITY_EDITOR
                segmentPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/RoadSegment_30m.prefab");
#endif
            }

            if (segmentPrefab == null)
            {
                Debug.LogError("EndlessRoadManager needs a road segment prefab.", this);
                return;
            }

            if (player == null)
            {
                ZombieRunner.Player.PlayerController runner = FindAnyObjectByType<ZombieRunner.Player.PlayerController>();
                if (runner != null)
                    player = runner.transform;
            }

            if (player == null)
            {
                Debug.LogError("EndlessRoadManager could not find the runner.", this);
                return;
            }

            // Clean up any pre-existing scene preview segments
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject childGo = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(childGo);
                else DestroyImmediate(childGo);
            }

            segmentLength = Mathf.Max(5f, segmentLength);
            pooledSegmentCount = Mathf.Max(2, pooledSegmentCount);
            nextSegmentCenterZ = initialCenterZ;

            for (int i = 0; i < pooledSegmentCount; i++)
            {
                GameObject segment = Instantiate(segmentPrefab, new Vector3(0f, 0f, nextSegmentCenterZ), Quaternion.identity, transform);
                segment.name = "Road Segment " + (i + 1).ToString("00");
                segments.Enqueue(segment.transform);
                nextSegmentCenterZ += segmentLength;
            }

            initialized = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            segmentLength = Mathf.Max(5f, segmentLength);
            pooledSegmentCount = Mathf.Max(2, pooledSegmentCount);
            recycleMargin = Mathf.Max(0f, recycleMargin);
            if (segmentPrefab == null)
            {
                segmentPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/RoadSegment_30m.prefab");
                if (segmentPrefab != null)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
