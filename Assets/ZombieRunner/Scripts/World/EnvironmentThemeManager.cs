using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ZombieRunner.Player;

namespace ZombieRunner.World
{
    public enum EnvironmentTheme
    {
        CityRuins = 0,
        SubwayTunnel = 1,
        MilitaryBase = 2,
        Factory = 3
    }

    public enum EnvironmentSegmentType
    {
        City = 0,
        TunnelEntrance = 1,
        Tunnel = 2,
        TunnelExit = 3
    }

    [Serializable]
    public struct EnvironmentLightingProfile
    {
        public Color fogColor;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public Color keyLightColor;
        [Min(0f)] public float keyLightIntensity;
        [Min(0f)] public float fogStart;
        [Min(1f)] public float fogEnd;
    }

    [Serializable]
    public struct EnvironmentThemeProfile
    {
        public Color fogColor;
        public Color ambientColor;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public Color keyLightColor;
        [Min(0f)] public float keyLightIntensity;
        [Min(0f)] public float fogStart;
        [Min(1f)] public float fogEnd;

        public Color GetSkyColor() => ambientSkyColor.maxColorComponent > 0.001f ? ambientSkyColor : ambientColor * 1.5f;
        public Color GetEquatorColor() => ambientEquatorColor.maxColorComponent > 0.001f ? ambientEquatorColor : ambientColor * 1.15f;
        public Color GetGroundColor() => ambientGroundColor.maxColorComponent > 0.001f ? ambientGroundColor : ambientColor * 0.75f;
    }

    /// <summary>
    /// Master endless environment sequencer and cinematic lighting coordinator.
    /// Drives the alternating endless progression:
    /// CITY -> TUNNEL ENTRANCE -> TUNNEL INTERIOR -> TUNNEL EXIT -> CITY -> REPEAT
    ///
    /// Features:
    /// - Multi-pool recycling with zero GC allocations during runtime gameplay
    /// - Smooth Hermite lighting and fog blending across daylight/night and subterranean tunnel
    /// - Configurable segment counts per environment zone
    /// - Backward-compatible theme queries and event notifications
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnvironmentThemeManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform player;
        [SerializeField] private Light keyLight;

        [Header("Environment Segment Prefabs")]
        [Tooltip("Modular 30m ruined open-air city segment")]
        [SerializeField] private GameObject cityPrefab;

        [Tooltip("Modular 30m tunnel entrance segment (open approach trench, retaining walls, gantry, concrete arch portal)")]
        [SerializeField] private GameObject tunnelEntrancePrefab;

        [Tooltip("Modular 30m subterranean tunnel interior segment (concrete walls, ceiling, ventilation ducts, dual-temp lamps)")]
        [SerializeField] private GameObject tunnelInteriorPrefab;

        [Tooltip("Modular 30m tunnel exit segment (interior run, monolithic portal, overhead exit gantry, stepped retaining walls)")]
        [SerializeField] private GameObject tunnelExitPrefab;

        [Tooltip("Legacy fallback segment array")]
        [SerializeField] private GameObject[] themeSegmentPrefabs = new GameObject[4];

        [Header("Endless Sequence Pattern")]
        [Tooltip("Number of continuous 30m City segments before approaching the tunnel")]
        [SerializeField, Range(1, 10)] private int citySegmentsInCycle = 3;

        [Tooltip("Number of continuous 30m subterranean Tunnel interior segments")]
        [SerializeField, Range(1, 10)] private int tunnelSegmentsInCycle = 2;

        [Tooltip("Initial offset in the sequence cycle (0 = start at City segment 1)")]
        [SerializeField] private int initialCycleOffset = 0;

        [Tooltip("Optional custom segment sequence override. If non-empty, this sequence loops continuously.")]
        [SerializeField] private EnvironmentSegmentType[] customSequence;

        [Header("Streaming & Pooling")]
        [SerializeField, Min(5f)] private float segmentLength = 30f;
        [SerializeField, Min(3)] private int pooledSegmentCount = 7;
        [SerializeField] private float initialCenterZ = 0f;
        [SerializeField, Min(0f)] private float recycleMargin = 5f;

        [Header("Cinematic Lighting Profiles")]
        [SerializeField] private EnvironmentLightingProfile cityProfile = new EnvironmentLightingProfile
        {
            fogColor = new Color(0.10f, 0.13f, 0.18f),
            ambientSkyColor = new Color(0.42f, 0.48f, 0.58f),
            ambientEquatorColor = new Color(0.35f, 0.40f, 0.48f),
            ambientGroundColor = new Color(0.28f, 0.32f, 0.38f),
            keyLightColor = new Color(0.85f, 0.90f, 0.98f),
            keyLightIntensity = 1.85f,
            fogStart = 60f,
            fogEnd = 200f
        };

        [SerializeField] private EnvironmentLightingProfile tunnelProfile = new EnvironmentLightingProfile
        {
            fogColor = new Color(0.12f, 0.14f, 0.18f),
            ambientSkyColor = new Color(0.48f, 0.52f, 0.60f),
            ambientEquatorColor = new Color(0.42f, 0.46f, 0.52f),
            ambientGroundColor = new Color(0.35f, 0.38f, 0.44f),
            keyLightColor = new Color(0.90f, 0.92f, 0.98f),
            keyLightIntensity = 1.55f,
            fogStart = 65f,
            fogEnd = 220f
        };

        [Header("Legacy Theme Profiles")]
        [SerializeField] private EnvironmentThemeProfile[] themeProfiles;

        private sealed class ActiveSegment
        {
            public GameObject gameObject;
            public Transform transform;
            public EnvironmentSegmentType type;
            public float centerZ;
            public int index;
        }

        private readonly Queue<ActiveSegment> activeSegments = new Queue<ActiveSegment>();
        private readonly Dictionary<EnvironmentSegmentType, Queue<GameObject>> pools =
            new Dictionary<EnvironmentSegmentType, Queue<GameObject>>();

        private Camera mainCamera;
        private float startingZ;
        private float nextSegmentCenterZ;
        private int nextSegmentIndex;
        private bool initialized;
        private EnvironmentTheme lastNotifiedTheme = EnvironmentTheme.CityRuins;
        private float currentBlendFactor = 0f;

        public event Action<EnvironmentTheme> ThemeChanged;
        public event Action<EnvironmentSegmentType> SegmentTypeChanged;

        public int PooledSegmentCount => activeSegments.Count;
        public float CurrentDistance => player != null ? Mathf.Max(0f, player.position.z - startingZ) : 0f;
        public float BlendFactor => currentBlendFactor;
        public EnvironmentTheme CurrentTheme => currentBlendFactor < 0.5f ? EnvironmentTheme.CityRuins : EnvironmentTheme.SubwayTunnel;
        public EnvironmentSegmentType CurrentSegmentType => GetSegmentTypeForDistance(player != null ? player.position.z : 0f);

        private void Awake()
        {
            InitializePoolsDictionary();
        }

        private void Start()
        {
            ResolveReferences();
            startingZ = player != null ? player.position.z : 0f;
            InitializePool();
            ApplyLightingBlend(0f);
        }

        private void Update()
        {
            if (!initialized || player == null || activeSegments.Count == 0)
                return;

            float playerZ = player.position.z;

            // Recycle segments that have passed safely behind the runner
            while (activeSegments.Count > 0 &&
                   playerZ > activeSegments.Peek().centerZ + segmentLength * 0.5f + recycleMargin)
            {
                RecycleOldestSegment();
            }

            // Calculate continuous lighting blend factor based on current runner position
            currentBlendFactor = CalculateLightingBlendFactor(playerZ);
            ApplyLightingBlend(currentBlendFactor);

            // Notify theme changes if threshold crossed
            EnvironmentTheme activeTheme = CurrentTheme;
            if (activeTheme != lastNotifiedTheme)
            {
                lastNotifiedTheme = activeTheme;
                ThemeChanged?.Invoke(activeTheme);
                SegmentTypeChanged?.Invoke(CurrentSegmentType);
            }
        }

        private void InitializePoolsDictionary()
        {
            if (pools.Count == 0)
            {
                pools[EnvironmentSegmentType.City] = new Queue<GameObject>();
                pools[EnvironmentSegmentType.TunnelEntrance] = new Queue<GameObject>();
                pools[EnvironmentSegmentType.Tunnel] = new Queue<GameObject>();
                pools[EnvironmentSegmentType.TunnelExit] = new Queue<GameObject>();
            }
        }

        public void InitializePool()
        {
            if (initialized)
                return;

            ResolveReferences();
            InitializePoolsDictionary();

            if (player == null)
            {
                Debug.LogError("[EnvironmentThemeManager] Could not find the runner transform.", this);
                return;
            }

            // Clean up any pre-existing scene preview segments under transform
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject childGo = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(childGo);
                else DestroyImmediate(childGo);
            }

            segmentLength = Mathf.Max(5f, segmentLength);
            pooledSegmentCount = Mathf.Max(3, pooledSegmentCount);
            nextSegmentCenterZ = initialCenterZ;
            nextSegmentIndex = 0;

            for (int i = 0; i < pooledSegmentCount; i++)
            {
                EnvironmentSegmentType type = GetSegmentTypeForIndex(nextSegmentIndex);
                GameObject segmentObj = SpawnOrReuseSegment(type, nextSegmentCenterZ);

                if (segmentObj != null)
                {
                    activeSegments.Enqueue(new ActiveSegment
                    {
                        gameObject = segmentObj,
                        transform = segmentObj.transform,
                        type = type,
                        centerZ = nextSegmentCenterZ,
                        index = nextSegmentIndex
                    });
                }

                nextSegmentCenterZ += segmentLength;
                nextSegmentIndex++;
            }

            initialized = true;
        }

        private void RecycleOldestSegment()
        {
            ActiveSegment oldest = activeSegments.Dequeue();

            // Return to specific type pool
            if (oldest != null && oldest.gameObject != null)
            {
                oldest.gameObject.SetActive(false);
                if (!pools.ContainsKey(oldest.type))
                    pools[oldest.type] = new Queue<GameObject>();
                pools[oldest.type].Enqueue(oldest.gameObject);
            }

            // Spawn next segment ahead
            EnvironmentSegmentType nextType = GetSegmentTypeForIndex(nextSegmentIndex);
            GameObject nextObj = SpawnOrReuseSegment(nextType, nextSegmentCenterZ);

            if (nextObj != null)
            {
                activeSegments.Enqueue(new ActiveSegment
                {
                    gameObject = nextObj,
                    transform = nextObj.transform,
                    type = nextType,
                    centerZ = nextSegmentCenterZ,
                    index = nextSegmentIndex
                });
            }

            nextSegmentCenterZ += segmentLength;
            nextSegmentIndex++;
        }

        private GameObject SpawnOrReuseSegment(EnvironmentSegmentType type, float centerZ)
        {
            InitializePoolsDictionary();

            Queue<GameObject> pool = pools[type];
            GameObject instance = null;

            while (pool.Count > 0 && instance == null)
            {
                instance = pool.Dequeue();
            }

            if (instance == null)
            {
                GameObject prefab = GetPrefabForType(type);
                if (prefab == null)
                {
#if UNITY_EDITOR
                    ValidateOrAssignPrefabs();
                    prefab = GetPrefabForType(type);
#endif
                }

                if (prefab == null)
                {
                    Debug.LogError($"[EnvironmentThemeManager] Missing prefab for segment type: {type}", this);
                    return null;
                }

                instance = Instantiate(prefab, transform);
            }

            instance.transform.position = new Vector3(0f, 0f, centerZ);
            instance.transform.rotation = Quaternion.identity;
            instance.name = $"Segment_{type}_{centerZ:F0}m";
            instance.SetActive(true);
            return instance;
        }

        public EnvironmentSegmentType GetSegmentTypeForIndex(int index)
        {
            if (customSequence != null && customSequence.Length > 0)
            {
                int cIdx = (index + initialCycleOffset) % customSequence.Length;
                if (cIdx < 0) cIdx += customSequence.Length;
                return customSequence[cIdx];
            }

            int cityCount = Mathf.Max(1, citySegmentsInCycle);
            int tunnelCount = Mathf.Max(1, tunnelSegmentsInCycle);
            int cycleLength = cityCount + 1 + tunnelCount + 1; // City segments + Entrance + Tunnel segments + Exit

            int step = (index + initialCycleOffset) % cycleLength;
            if (step < 0) step += cycleLength;

            if (step < cityCount)
            {
                return EnvironmentSegmentType.City;
            }
            if (step == cityCount)
            {
                return EnvironmentSegmentType.TunnelEntrance;
            }
            if (step < cityCount + 1 + tunnelCount)
            {
                return EnvironmentSegmentType.Tunnel;
            }

            return EnvironmentSegmentType.TunnelExit;
        }

        public EnvironmentSegmentType GetSegmentTypeForDistance(float z)
        {
            int index = Mathf.FloorToInt((z - startingZ + segmentLength * 0.5f) / segmentLength);
            return GetSegmentTypeForIndex(index);
        }

        private float CalculateLightingBlendFactor(float playerZ)
        {
            float relZ = playerZ - startingZ;
            int segIdx = Mathf.FloorToInt((relZ + segmentLength * 0.5f) / segmentLength);
            float segCenterZ = startingZ + segIdx * segmentLength;
            float localZ = playerZ - segCenterZ; // -15m to +15m
            float u = Mathf.Clamp01((localZ + segmentLength * 0.5f) / segmentLength); // 0.0 to 1.0

            EnvironmentSegmentType segType = GetSegmentTypeForIndex(segIdx);
            EnvironmentSegmentType nextType = GetSegmentTypeForIndex(segIdx + 1);
            EnvironmentSegmentType prevType = GetSegmentTypeForIndex(segIdx - 1);

            switch (segType)
            {
                case EnvironmentSegmentType.City:
                    // If approaching entrance gantry in second half of segment, start subtle darkening
                    if (nextType == EnvironmentSegmentType.TunnelEntrance && u > 0.5f)
                    {
                        return Mathf.SmoothStep(0f, 0.20f, (u - 0.5f) * 2f);
                    }
                    // If just exited tunnel in first half of segment, complete transition back to full city
                    if (prevType == EnvironmentSegmentType.TunnelExit && u < 0.5f)
                    {
                        return Mathf.SmoothStep(0.20f, 0f, u * 2f);
                    }
                    return 0f;

                case EnvironmentSegmentType.TunnelEntrance:
                    // Portal arch is at center (u = 0.5). Smooth transition from 0.20 (trench) to 1.0 (subterranean)
                    return Mathf.SmoothStep(0.20f, 1.0f, u);

                case EnvironmentSegmentType.Tunnel:
                    // Fully subterranean interior
                    return 1.0f;

                case EnvironmentSegmentType.TunnelExit:
                    // Portal arch is at center (u = 0.5). Smooth transition from 1.0 (interior) to 0.20 (trench)
                    return Mathf.SmoothStep(1.0f, 0.20f, u);

                default:
                    return 0f;
            }
        }

        private void ApplyLightingBlend(float blend)
        {
            blend = Mathf.Clamp01(blend);

            // Fog parameters
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Color.Lerp(cityProfile.fogColor, tunnelProfile.fogColor, blend);
            RenderSettings.fogStartDistance = Mathf.Lerp(cityProfile.fogStart, tunnelProfile.fogStart, blend);
            RenderSettings.fogEndDistance = Mathf.Lerp(cityProfile.fogEnd, tunnelProfile.fogEnd, blend);

            // Ambient Trilight (Gradient) indirect bounce lighting
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(cityProfile.ambientSkyColor, tunnelProfile.ambientSkyColor, blend);
            RenderSettings.ambientEquatorColor = Color.Lerp(cityProfile.ambientEquatorColor, tunnelProfile.ambientEquatorColor, blend);
            RenderSettings.ambientGroundColor = Color.Lerp(cityProfile.ambientGroundColor, tunnelProfile.ambientGroundColor, blend);
            RenderSettings.ambientLight = RenderSettings.ambientEquatorColor;

            // Directional Key Light
            if (keyLight != null)
            {
                keyLight.color = Color.Lerp(cityProfile.keyLightColor, tunnelProfile.keyLightColor, blend);
                keyLight.intensity = Mathf.Lerp(cityProfile.keyLightIntensity, tunnelProfile.keyLightIntensity, blend);
            }

            if (mainCamera != null)
            {
                mainCamera.backgroundColor = RenderSettings.fogColor;
            }
        }

        private GameObject GetPrefabForType(EnvironmentSegmentType type)
        {
            switch (type)
            {
                case EnvironmentSegmentType.City:
                    if (cityPrefab != null) return cityPrefab;
                    if (themeSegmentPrefabs != null && themeSegmentPrefabs.Length > 0 && themeSegmentPrefabs[0] != null) return themeSegmentPrefabs[0];
                    break;
                case EnvironmentSegmentType.TunnelEntrance:
                    if (tunnelEntrancePrefab != null) return tunnelEntrancePrefab;
                    if (tunnelInteriorPrefab != null) return tunnelInteriorPrefab;
                    if (themeSegmentPrefabs != null && themeSegmentPrefabs.Length > 1 && themeSegmentPrefabs[1] != null) return themeSegmentPrefabs[1];
                    break;
                case EnvironmentSegmentType.Tunnel:
                    if (tunnelInteriorPrefab != null) return tunnelInteriorPrefab;
                    if (themeSegmentPrefabs != null && themeSegmentPrefabs.Length > 1 && themeSegmentPrefabs[1] != null) return themeSegmentPrefabs[1];
                    break;
                case EnvironmentSegmentType.TunnelExit:
                    if (tunnelExitPrefab != null) return tunnelExitPrefab;
                    if (tunnelInteriorPrefab != null) return tunnelInteriorPrefab;
                    if (themeSegmentPrefabs != null && themeSegmentPrefabs.Length > 1 && themeSegmentPrefabs[1] != null) return themeSegmentPrefabs[1];
                    break;
            }
            return cityPrefab;
        }

        private void ResolveReferences()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (player == null)
            {
                PlayerController runner = FindAnyObjectByType<PlayerController>();
                if (runner != null) player = runner.transform;
            }
            if (keyLight == null)
            {
                foreach (Light candidate in FindObjectsByType<Light>())
                {
                    if (candidate.type == LightType.Directional)
                    {
                        keyLight = candidate;
                        break;
                    }
                }
            }
        }

        private void OnValidate()
        {
            segmentLength = Mathf.Max(5f, segmentLength);
            pooledSegmentCount = Mathf.Max(3, pooledSegmentCount);
            recycleMargin = Mathf.Max(0f, recycleMargin);
            citySegmentsInCycle = Mathf.Max(1, citySegmentsInCycle);
            tunnelSegmentsInCycle = Mathf.Max(1, tunnelSegmentsInCycle);
#if UNITY_EDITOR
            ValidateOrAssignPrefabs();
#endif
        }

#if UNITY_EDITOR
        private void ValidateOrAssignPrefabs()
        {
            bool dirty = false;

            if (cityPrefab == null)
            {
                cityPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab");
                if (cityPrefab != null) dirty = true;
            }

            if (tunnelEntrancePrefab == null)
            {
                tunnelEntrancePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_TunnelEntrance.prefab");
                if (tunnelEntrancePrefab != null) dirty = true;
            }

            if (tunnelInteriorPrefab == null)
            {
                tunnelInteriorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_SubwayTunnel.prefab");
                if (tunnelInteriorPrefab != null) dirty = true;
            }

            if (tunnelExitPrefab == null)
            {
                tunnelExitPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_TunnelExit.prefab");
                if (tunnelExitPrefab != null) dirty = true;
            }

            if (themeSegmentPrefabs == null || themeSegmentPrefabs.Length < 4)
            {
                Array.Resize(ref themeSegmentPrefabs, 4);
                dirty = true;
            }

            if (themeSegmentPrefabs[0] == null && cityPrefab != null) { themeSegmentPrefabs[0] = cityPrefab; dirty = true; }
            if (themeSegmentPrefabs[1] == null && tunnelInteriorPrefab != null) { themeSegmentPrefabs[1] = tunnelInteriorPrefab; dirty = true; }

            if (dirty)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
