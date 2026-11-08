using UnityEngine;

namespace ZombieRunner.World
{
    /// <summary>
    /// Simulates a failing or flickering sodium-vapor streetlight in the abandoned city.
    /// Uses MaterialPropertyBlock to avoid runtime material instantiation and maintains
    /// smooth mobile performance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StreetlightFlicker : MonoBehaviour
    {
        [Header("Light Component")]
        [SerializeField] private Light targetLight;
        [SerializeField, Min(0f)] private float baseIntensity = 3.2f;
        [SerializeField] private Color lightColor = new Color(1.0f, 0.74f, 0.40f);

        [Header("Emissive Fixture")]
        [SerializeField] private Renderer fixtureRenderer;
        [SerializeField] private Color baseEmissionColor = new Color(1.8f, 1.25f, 0.55f);

        [Header("Flicker Behavior")]
        [SerializeField, Range(1f, 30f)] private float flickerFrequency = 14f;
        [SerializeField, Range(0f, 0.5f)] private float microFlickerAmount = 0.08f;
        [SerializeField] private bool allowBlackouts = true;
        [SerializeField, Range(1f, 10f)] private float minBlackoutInterval = 2.5f;
        [SerializeField, Range(2f, 15f)] private float maxBlackoutInterval = 7.0f;
        [SerializeField, Range(0.03f, 0.25f)] private float blackoutDuration = 0.06f;

        private float nextBlackoutTime;
        private float blackoutTimer;
        private bool isBlackout;
        private MaterialPropertyBlock propBlock;
        private static MaterialPropertyBlock sharedPropBlock;
        private static MaterialPropertyBlock SafePropBlock => sharedPropBlock ??= new MaterialPropertyBlock();
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (targetLight == null) targetLight = GetComponentInChildren<Light>();
            if (fixtureRenderer == null) fixtureRenderer = GetComponentInChildren<Renderer>();
            propBlock = new MaterialPropertyBlock();
            ScheduleNextBlackout();
        }

        private void OnEnable()
        {
            if (propBlock == null) propBlock = new MaterialPropertyBlock();
            ScheduleNextBlackout();
            blackoutTimer = 0f;
            isBlackout = false;
        }

        private void Update()
        {
            float intensityMultiplier = 1f;

            if (allowBlackouts)
            {
                if (isBlackout)
                {
                    blackoutTimer -= Time.deltaTime;
                    if (blackoutTimer <= 0f)
                    {
                        isBlackout = false;
                        ScheduleNextBlackout();
                        // Subtle surge on recovery
                        intensityMultiplier = 1.15f;
                    }
                    else
                    {
                        intensityMultiplier = 0.05f;
                    }
                }
                else
                {
                    if (Time.time >= nextBlackoutTime)
                    {
                        isBlackout = true;
                        blackoutTimer = Random.Range(blackoutDuration * 0.7f, blackoutDuration * 1.4f);
                        intensityMultiplier = 0.05f;
                    }
                }
            }

            if (!isBlackout)
            {
                // Subtle organic flicker using Perlin noise
                float noise = Mathf.PerlinNoise(Time.time * flickerFrequency, transform.position.z * 0.1f);
                float jitter = (noise - 0.5f) * 2f * microFlickerAmount;
                intensityMultiplier = Mathf.Clamp(intensityMultiplier + jitter, 0.1f, 1.25f);
            }

            // Apply to Light
            if (targetLight != null)
            {
                targetLight.intensity = baseIntensity * intensityMultiplier;
                targetLight.color = lightColor;
            }

            // Apply to Fixture Material
            if (fixtureRenderer != null)
            {
                MaterialPropertyBlock pb = propBlock ?? SafePropBlock;
                if (pb != null)
                {
                    fixtureRenderer.GetPropertyBlock(pb);
                    Color currentEmission = baseEmissionColor * intensityMultiplier;
                    pb.SetColor(EmissionColorId, currentEmission);
                    fixtureRenderer.SetPropertyBlock(pb);
                }
            }
        }

        private void ScheduleNextBlackout()
        {
            nextBlackoutTime = Time.time + Random.Range(minBlackoutInterval, maxBlackoutInterval);
        }
    }
}
