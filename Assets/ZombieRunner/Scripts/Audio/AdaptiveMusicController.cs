using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.Weapons;

namespace ZombieRunner.Audio
{
    public enum MusicState
    {
        Exploration,
        ApproachingDanger,
        Combat,
        CriticalHealth,
        GameOver
    }

    /// <summary>
    /// Layered adaptive music system for survival horror.
    /// Runs 4 synchronized audio stems locked in phase:
    ///   Layer 1: Exploration (Subtle brooding atmospheric drone)
    ///   Layer 2: Approaching Danger (Low cinematic tension pulse)
    ///   Layer 3: Combat (Driving horror rhythm & distorted bass grit)
    ///   Layer 4: Critical Health (Visceral claustrophobic infrasound)
    /// Smoothly crossfades between layers based on real-time gameplay events without track restarts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdaptiveMusicController : MonoBehaviour
    {
        [Header("Audio Mixer Routing")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;

        [Header("Music Layer Audio Clips (48s Synchronized Stems)")]
        [SerializeField] private AudioClip layer1ExplorationClip;
        [SerializeField] private AudioClip layer2TensionClip;
        [SerializeField] private AudioClip layer3CombatClip;
        [SerializeField] private AudioClip layer4CriticalHealthClip;
        [SerializeField] private AudioClip gameOverStingClip;

        [Header("Layer Master Levels")]
        [SerializeField, Range(0f, 1f)] private float layer1MaxVolume = 0.38f;
        [SerializeField, Range(0f, 1f)] private float layer2MaxVolume = 0.48f;
        [SerializeField, Range(0f, 1f)] private float layer3MaxVolume = 0.58f;
        [SerializeField, Range(0f, 1f)] private float layer4MaxVolume = 0.62f;

        [Header("Crossfade Dynamics")]
        [SerializeField, Min(0.1f)] private float fadeInSpeed = 0.85f;
        [SerializeField, Min(0.1f)] private float fadeOutSpeed = 0.55f;
        [SerializeField, Min(0.1f)] private float dangerThresholdDistance = 34f;
        [SerializeField, Min(0.1f)] private float combatEngagementDistance = 15f;
        [SerializeField, Min(0.5f)] private float combatHangTime = 3.5f;
        [SerializeField, Range(0.1f, 0.5f)] private float criticalHealthThreshold = 0.35f;

        private AudioSource sourceLayer1;
        private AudioSource sourceLayer2;
        private AudioSource sourceLayer3;
        private AudioSource sourceLayer4;
        private AudioSource sourceSting;

        private float currentVol1;
        private float currentVol2;
        private float currentVol3;
        private float currentVol4;

        private float targetVol1;
        private float targetVol2;
        private float targetVol3;
        private float targetVol4;

        private MusicState currentState = MusicState.Exploration;
        private float combatTimer;
        private bool isInitialized;
        private bool isGameOverTriggered;
        private float masterMusicVolume = 0.75f;

        private PlayerHealth playerHealth;
        private Transform playerTransform;
        private ZombieSpawner zombieSpawner;

        public MusicState CurrentState => currentState;
        public float MasterMusicVolume => masterMusicVolume;

        private void Awake()
        {
            EnsureAudioSources();
        }

        private void Start()
        {
            InitializeGameplayReferences();
            StartSynchronizedPlayback();
        }

        private void Update()
        {
            UpdateCombatTimer();
            EvaluateGameplayState();
            CalculateTargetVolumes();
            ApplySmoothCrossfade();
        }

        public void SetMixerGroup(AudioMixerGroup group)
        {
            musicMixerGroup = group;
            if (sourceLayer1 != null) sourceLayer1.outputAudioMixerGroup = group;
            if (sourceLayer2 != null) sourceLayer2.outputAudioMixerGroup = group;
            if (sourceLayer3 != null) sourceLayer3.outputAudioMixerGroup = group;
            if (sourceLayer4 != null) sourceLayer4.outputAudioMixerGroup = group;
            if (sourceSting != null) sourceSting.outputAudioMixerGroup = group;
        }

        public void AssignClips(AudioClip layer1, AudioClip layer2, AudioClip layer3, AudioClip layer4, AudioClip sting)
        {
            layer1ExplorationClip = layer1;
            layer2TensionClip = layer2;
            layer3CombatClip = layer3;
            layer4CriticalHealthClip = layer4;
            gameOverStingClip = sting;

            if (isInitialized)
            {
                StartSynchronizedPlayback();
            }
        }

        public void SetMasterMusicVolume(float volume)
        {
            masterMusicVolume = Mathf.Clamp01(volume);
            ApplySourceVolumes();
        }

        public void NotifyCombatActivity(float duration = 4.0f)
        {
            combatTimer = Mathf.Max(combatTimer, duration);
        }

        private void EnsureAudioSources()
        {
            sourceLayer1 = CreateStemSource("Stem_Layer1_Exploration");
            sourceLayer2 = CreateStemSource("Stem_Layer2_Tension");
            sourceLayer3 = CreateStemSource("Stem_Layer3_Combat");
            sourceLayer4 = CreateStemSource("Stem_Layer4_CriticalHealth");
            sourceSting = CreateStingSource("Stem_GameOver_Sting");
            isInitialized = true;
        }

        private AudioSource CreateStemSource(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);

            AudioSource src = child.AddComponent<AudioSource>();
            src.spatialBlend = 0f; // 2D Stereo
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
            src.priority = 32; // High priority, never virtualize music
            if (musicMixerGroup != null) src.outputAudioMixerGroup = musicMixerGroup;
            return src;
        }

        private AudioSource CreateStingSource(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);

            AudioSource src = child.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            src.loop = false;
            src.playOnAwake = false;
            src.volume = 0f;
            src.priority = 24;
            if (musicMixerGroup != null) src.outputAudioMixerGroup = musicMixerGroup;
            return src;
        }

        private void InitializeGameplayReferences()
        {
            if (playerHealth == null)
                playerHealth = FindAnyObjectByType<PlayerHealth>();

            if (playerTransform == null && playerHealth != null)
                playerTransform = playerHealth.transform;

            if (playerTransform == null)
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            if (zombieSpawner == null)
                zombieSpawner = FindAnyObjectByType<ZombieSpawner>();

            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void StartSynchronizedPlayback()
        {
            if (sourceLayer1 == null) EnsureAudioSources();

            sourceLayer1.clip = layer1ExplorationClip;
            sourceLayer2.clip = layer2TensionClip;
            sourceLayer3.clip = layer3CombatClip;
            sourceLayer4.clip = layer4CriticalHealthClip;

            // Start all active stems at exact timeSamples = 0 for perfect phase-lock
            int sampleOffset = 0;
            if (sourceLayer1.clip != null) { sourceLayer1.timeSamples = sampleOffset; sourceLayer1.Play(); }
            if (sourceLayer2.clip != null) { sourceLayer2.timeSamples = sampleOffset; sourceLayer2.Play(); }
            if (sourceLayer3.clip != null) { sourceLayer3.timeSamples = sampleOffset; sourceLayer3.Play(); }
            if (sourceLayer4.clip != null) { sourceLayer4.timeSamples = sampleOffset; sourceLayer4.Play(); }

            currentVol1 = targetVol1 = layer1MaxVolume;
            currentVol2 = targetVol2 = 0f;
            currentVol3 = targetVol3 = 0f;
            currentVol4 = targetVol4 = 0f;

            ApplySourceVolumes();
        }

        private void UpdateCombatTimer()
        {
            if (combatTimer > 0f)
            {
                combatTimer -= Time.deltaTime;
                if (combatTimer < 0f) combatTimer = 0f;
            }
        }

        private void EvaluateGameplayState()
        {
            if (isGameOverTriggered)
            {
                currentState = MusicState.GameOver;
                return;
            }

            if (playerHealth != null && playerHealth.IsDead)
            {
                OnPlayerDied();
                return;
            }

            // Critical Health check (HP <= 35%)
            if (playerHealth != null && playerHealth.MaximumHealth > 0f)
            {
                float hpRatio = playerHealth.CurrentHealth / playerHealth.MaximumHealth;
                if (hpRatio <= criticalHealthThreshold)
                {
                    currentState = MusicState.CriticalHealth;
                    return;
                }
            }

            // Check if combat is currently active
            if (combatTimer > 0f)
            {
                currentState = MusicState.Combat;
                return;
            }

            // Check zombie proximity
            float closestZombieDist = GetClosestLivingZombieDistance();
            if (closestZombieDist <= combatEngagementDistance)
            {
                combatTimer = combatHangTime;
                currentState = MusicState.Combat;
            }
            else if (closestZombieDist <= dangerThresholdDistance)
            {
                currentState = MusicState.ApproachingDanger;
            }
            else
            {
                currentState = MusicState.Exploration;
            }
        }

        private float GetClosestLivingZombieDistance()
        {
            if (playerTransform == null) return float.MaxValue;

            float pZ = playerTransform.position.z;
            float minDistance = float.MaxValue;

            IReadOnlyList<ZombieController> list = zombieSpawner != null ? zombieSpawner.ActiveZombies : null;
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ZombieController z = list[i];
                    if (z == null || !z.isActiveAndEnabled || z.IsDead || z.CurrentHealth <= 0f) continue;

                    float deltaZ = z.transform.position.z - pZ;
                    // Only consider zombies in front of or directly at the player
                    if (deltaZ > -1f)
                    {
                        float dist = Vector3.Distance(playerTransform.position, z.transform.position);
                        if (dist < minDistance) minDistance = dist;
                    }
                }
            }
            else
            {
                ZombieController[] zombies = FindObjectsByType<ZombieController>(FindObjectsInactive.Exclude);
                for (int i = 0; i < zombies.Length; i++)
                {
                    ZombieController z = zombies[i];
                    if (z == null || z.IsDead || z.CurrentHealth <= 0f) continue;

                    float deltaZ = z.transform.position.z - pZ;
                    if (deltaZ > -1f)
                    {
                        float dist = Vector3.Distance(playerTransform.position, z.transform.position);
                        if (dist < minDistance) minDistance = dist;
                    }
                }
            }

            return minDistance;
        }

        private void CalculateTargetVolumes()
        {
            switch (currentState)
            {
                case MusicState.Exploration:
                    targetVol1 = layer1MaxVolume;
                    targetVol2 = 0f;
                    targetVol3 = 0f;
                    targetVol4 = 0f;
                    break;

                case MusicState.ApproachingDanger:
                    targetVol1 = layer1MaxVolume * 0.88f;
                    targetVol2 = layer2MaxVolume;
                    targetVol3 = 0f;
                    targetVol4 = 0f;
                    break;

                case MusicState.Combat:
                    targetVol1 = layer1MaxVolume * 0.65f;
                    targetVol2 = layer2MaxVolume * 0.85f;
                    targetVol3 = layer3MaxVolume;
                    targetVol4 = 0f;
                    break;

                case MusicState.CriticalHealth:
                    targetVol1 = layer1MaxVolume * 0.35f;
                    targetVol2 = layer2MaxVolume * 0.50f;
                    targetVol3 = layer3MaxVolume * 0.40f;
                    targetVol4 = layer4MaxVolume;
                    break;

                case MusicState.GameOver:
                    targetVol1 = 0f;
                    targetVol2 = 0f;
                    targetVol3 = 0f;
                    targetVol4 = 0f;
                    break;
            }
        }

        private void ApplySmoothCrossfade()
        {
            float dt = Time.deltaTime;

            currentVol1 = MoveVolume(currentVol1, targetVol1, dt);
            currentVol2 = MoveVolume(currentVol2, targetVol2, dt);
            currentVol3 = MoveVolume(currentVol3, targetVol3, dt);
            currentVol4 = MoveVolume(currentVol4, targetVol4, dt);

            ApplySourceVolumes();
        }

        private float MoveVolume(float current, float target, float dt)
        {
            float speed = target > current ? fadeInSpeed : fadeOutSpeed;
            return Mathf.MoveTowards(current, target, speed * dt);
        }

        private void ApplySourceVolumes()
        {
            float m = masterMusicVolume;
            if (sourceLayer1 != null) sourceLayer1.volume = currentVol1 * m;
            if (sourceLayer2 != null) sourceLayer2.volume = currentVol2 * m;
            if (sourceLayer3 != null) sourceLayer3.volume = currentVol3 * m;
            if (sourceLayer4 != null) sourceLayer4.volume = currentVol4 * m;
        }

        private void OnPlayerDied()
        {
            if (isGameOverTriggered) return;
            isGameOverTriggered = true;
            currentState = MusicState.GameOver;

            targetVol1 = 0f;
            targetVol2 = 0f;
            targetVol3 = 0f;
            targetVol4 = 0f;

            if (sourceSting != null && gameOverStingClip != null)
            {
                sourceSting.clip = gameOverStingClip;
                sourceSting.volume = masterMusicVolume * 0.85f;
                sourceSting.Play();
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }
        }
    }
}
