using System.Collections.Generic;
using UnityEngine;
using ZombieRunner.Audio;

namespace ZombieRunner.Weapons
{
    /// <summary>
    /// High-performance object pool and manager for visible bullet tracers and impact effects.
    /// Supports mobile optimization with zero runtime garbage collection.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitImpactManager : MonoBehaviour
    {
        public static HitImpactManager Instance { get; private set; }

        [Header("Materials & Shaders")]
        [SerializeField] private Material bulletTracerMaterial;
        [SerializeField] private Material bloodParticleMaterial;
        [SerializeField] private Material sparkParticleMaterial;
        [SerializeField] private Material concreteDustMaterial;

        [Header("Pool Capacities")]
        [SerializeField, Min(4)] private int initialTracerPoolSize = 16;
        [SerializeField, Min(4)] private int initialImpactPoolSize = 16;

        private readonly Queue<BulletTracer> tracerPool = new Queue<BulletTracer>();
        private readonly Queue<ParticleSystem> bloodPool = new Queue<ParticleSystem>();
        private readonly Queue<ParticleSystem> sparkPool = new Queue<ParticleSystem>();
        private readonly Queue<ParticleSystem> dustPool = new Queue<ParticleSystem>();

        private GameObject tracerPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureMaterials();
            BuildTracerPrefab();
            PrewarmPools();
        }

        private void EnsureMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");

            if (bulletTracerMaterial == null)
            {
                bulletTracerMaterial = new Material(unlitShader)
                {
                    name = "M_BulletTracer_Emissive",
                    color = new Color(1.0f, 0.95f, 0.65f, 1f)
                };
            }

            if (bloodParticleMaterial == null)
            {
                bloodParticleMaterial = new Material(unlitShader)
                {
                    name = "M_Blood_Splatter",
                    color = new Color(0.68f, 0.04f, 0.04f, 0.92f)
                };
            }

            if (sparkParticleMaterial == null)
            {
                sparkParticleMaterial = new Material(unlitShader)
                {
                    name = "M_Sparks_Glow",
                    color = new Color(1.0f, 0.85f, 0.35f, 1f)
                };
            }

            if (concreteDustMaterial == null)
            {
                concreteDustMaterial = new Material(unlitShader)
                {
                    name = "M_Concrete_Dust",
                    color = new Color(0.45f, 0.44f, 0.42f, 0.6f)
                };
            }
        }

        private void BuildTracerPrefab()
        {
            tracerPrefab = new GameObject("BulletTracer_Template");
            tracerPrefab.transform.SetParent(transform);

            // Glowing bullet core capsule/cylinder
            GameObject coreGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreGo.name = "Core";
            coreGo.transform.SetParent(tracerPrefab.transform, false);
            coreGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            coreGo.transform.localScale = new Vector3(0.045f, 0.28f, 0.045f);

            Collider col = coreGo.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            MeshRenderer mr = coreGo.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterial = bulletTracerMaterial;

            // Trail renderer for luminous ribbon tracer
            TrailRenderer trail = tracerPrefab.AddComponent<TrailRenderer>();
            trail.material = bulletTracerMaterial;
            trail.time = 0.09f;
            trail.startWidth = 0.09f;
            trail.endWidth = 0.015f;
            trail.minVertexDistance = 0.1f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 0.96f, 0.7f), 0.0f),
                    new GradientColorKey(new Color(1f, 0.55f, 0.15f), 0.65f),
                    new GradientColorKey(new Color(0.8f, 0.2f, 0.05f), 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.85f, 0.65f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trail.colorGradient = gradient;

            // Micro point light for bullet glow
            GameObject lightGo = new GameObject("TracerGlow");
            lightGo.transform.SetParent(tracerPrefab.transform, false);
            Light pLight = lightGo.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 3.5f;
            pLight.intensity = 2.4f;
            pLight.color = new Color(1.0f, 0.9f, 0.65f);
            pLight.shadows = LightShadows.None;

            BulletTracer tracerComp = tracerPrefab.AddComponent<BulletTracer>();
            // Use reflection or field setting
            var fields = typeof(BulletTracer).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (f.Name == "trail") f.SetValue(tracerComp, trail);
                else if (f.Name == "projectileCore") f.SetValue(tracerComp, mr);
                else if (f.Name == "projectileGlowLight") f.SetValue(tracerComp, pLight);
            }

            tracerPrefab.SetActive(false);
        }

        private void PrewarmPools()
        {
            for (int i = 0; i < initialTracerPoolSize; i++)
            {
                BulletTracer instance = Instantiate(tracerPrefab, transform).GetComponent<BulletTracer>();
                instance.ResetForPool();
                tracerPool.Enqueue(instance);
            }

            for (int i = 0; i < initialImpactPoolSize; i++)
            {
                bloodPool.Enqueue(CreateParticleSystemInstance("PS_BloodSplatter", bloodParticleMaterial, Color.red, 16));
                sparkPool.Enqueue(CreateParticleSystemInstance("PS_SparkRicochet", sparkParticleMaterial, new Color(1f, 0.85f, 0.2f), 20));
                dustPool.Enqueue(CreateParticleSystemInstance("PS_ConcreteDust", concreteDustMaterial, new Color(0.6f, 0.6f, 0.58f), 12));
            }
        }

        private ParticleSystem CreateParticleSystemInstance(string name, Material mat, Color color, int maxParticles)
        {
            GameObject go = new GameObject(name);
            // CRITICAL: Deactivate GameObject BEFORE adding ParticleSystem to ensure Unity does not auto-play on Awake
            go.SetActive(false);
            go.transform.SetParent(transform, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.35f;
            main.startLifetime = 0.32f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = color;
            main.maxParticles = maxParticles;
            main.gravityModifier = 2.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)(maxParticles / 2), (short)maxParticles) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.05f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }

        public BulletTracer SpawnTracer(
            Vector3 start,
            Vector3 target,
            bool hasHit,
            bool isZombie,
            bool isMetal,
            Vector3 normal)
        {
            BulletTracer tracer = null;
            while (tracerPool.Count > 0 && tracer == null)
            {
                tracer = tracerPool.Dequeue();
            }

            if (tracer == null)
            {
                tracer = Instantiate(tracerPrefab, transform).GetComponent<BulletTracer>();
            }

            tracer.Initialize(start, target, hasHit, isZombie, isMetal, normal, RecycleTracer);
            return tracer;
        }

        private void RecycleTracer(BulletTracer tracer)
        {
            if (tracer == null) return;
            tracer.ResetForPool();
            tracerPool.Enqueue(tracer);
        }

        public void SpawnImpact(Vector3 position, Vector3 normal, bool isZombie, bool isMetal)
        {
            if (isZombie)
            {
                // Play blood impact sound & spawn visceral crimson splatter
                AudioManager.Instance?.Play(GameAudioCue.ZombieDeath, 0.85f);
                PlayParticle(bloodPool, "PS_BloodSplatter", bloodParticleMaterial, Color.red, 16, position, normal);
            }
            else if (isMetal)
            {
                AudioManager.Instance?.Play(GameAudioCue.Explosion, 0.45f);
                PlayParticle(sparkPool, "PS_SparkRicochet", sparkParticleMaterial, new Color(1f, 0.85f, 0.2f), 20, position, normal);
            }
            else
            {
                PlayParticle(dustPool, "PS_ConcreteDust", concreteDustMaterial, new Color(0.6f, 0.6f, 0.58f), 12, position, normal);
            }
        }

        private void PlayParticle(Queue<ParticleSystem> pool, string name, Material mat, Color color, int maxParticles, Vector3 position, Vector3 normal)
        {
            ParticleSystem ps = null;
            while (pool != null && pool.Count > 0 && ps == null)
            {
                ps = pool.Dequeue();
            }

            if (ps == null)
            {
                ps = CreateParticleSystemInstance(name, mat, color, maxParticles);
            }

            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear();
                ps.transform.position = position;
                if (normal.sqrMagnitude > 0.001f)
                    ps.transform.rotation = Quaternion.LookRotation(normal);

                ps.gameObject.SetActive(true);
                ps.Play();

                float duration = ps.main.duration + ps.main.startLifetime.constantMax + 0.05f;
                StartCoroutine(RecycleParticleRoutine(ps, pool, duration));
            }
        }

        private System.Collections.IEnumerator RecycleParticleRoutine(ParticleSystem ps, Queue<ParticleSystem> pool, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ps != null && ps.gameObject != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear();
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }
        }
    }
}
