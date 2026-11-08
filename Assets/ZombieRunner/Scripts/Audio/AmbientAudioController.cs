using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace ZombieRunner.Audio
{
    /// <summary>
    /// Procedural and looped environmental audio controller for the abandoned city.
    /// Manages the continuous atmospheric wind loop and triggers sparse, unsettling
    /// environmental textures (distant metal creaks, subterranean rumbles, structural groans).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AmbientAudioController : MonoBehaviour
    {
        [Header("Audio Mixer Routing")]
        [SerializeField] private AudioMixerGroup ambientMixerGroup;

        [Header("Continuous Bed - City")]
        [SerializeField] private AudioClip windAmbienceClip;
        [SerializeField, Range(0f, 1f)] private float windBaseVolume = 0.80f;

        [Header("Continuous Bed - Tunnel")]
        [SerializeField] private AudioClip tunnelAmbienceClip;
        [SerializeField, Range(0f, 1f)] private float tunnelBaseVolume = 0.38f;

        [Header("Sparse Environmental Events")]
        [SerializeField] private AudioClip[] atmosphericStingerClips;
        [SerializeField, Range(0f, 1f)] private float stingerVolume = 0.22f;
        [SerializeField, Min(5f)] private float minStingerInterval = 18f;
        [SerializeField, Min(10f)] private float maxStingerInterval = 38f;

        private AudioSource windSource;
        private AudioSource tunnelSource;
        private AudioSource stingerSource;
        private Coroutine stingerRoutine;
        private float masterAmbientVolume = 0.85f;
        private bool isInitialized;

        public float MasterAmbientVolume => masterAmbientVolume;

        private void Awake()
        {
            EnsureClipsAssigned();
            EnsureSources();
        }

        private void Start()
        {
            StartAmbience();
        }

        private void EnsureClipsAssigned()
        {
#if UNITY_EDITOR
            if (windAmbienceClip == null)
            {
                windAmbienceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/ProvidedAudio/music/city sound.mp3");
                if (windAmbienceClip == null)
                    windAmbienceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/Ambience_City_Night_Wind.wav");
            }
#endif
        }

        private void Update()
        {
            UpdateEnvironmentTransition();
        }

        private void UpdateEnvironmentTransition()
        {
            if (windSource == null) return;

            // Maintain continuous city ambient loop without sample-reset glitching
            if (windAmbienceClip != null && !windSource.isPlaying)
            {
                if (windSource.clip != windAmbienceClip)
                {
                    windSource.clip = windAmbienceClip;
                }
                windSource.loop = true;
                windSource.Play();
            }

            float blend = 0f;
            var themeMgr = FindAnyObjectByType<ZombieRunner.World.EnvironmentThemeManager>();
            if (themeMgr != null)
            {
                blend = Mathf.Clamp01(themeMgr.BlendFactor);
            }

            float cityTarget = windBaseVolume * masterAmbientVolume * (1f - blend);
            windSource.volume = Mathf.MoveTowards(windSource.volume, cityTarget, Time.deltaTime * 0.8f);

            if (tunnelSource != null)
            {
                if (tunnelAmbienceClip != null && !tunnelSource.isPlaying && blend > 0.02f)
                {
                    if (tunnelSource.clip != tunnelAmbienceClip)
                    {
                        tunnelSource.clip = tunnelAmbienceClip;
                    }
                    tunnelSource.loop = true;
                    tunnelSource.Play();
                }

                if (tunnelSource.isPlaying)
                {
                    float tunnelTarget = tunnelBaseVolume * masterAmbientVolume * blend;
                    tunnelSource.volume = Mathf.MoveTowards(tunnelSource.volume, tunnelTarget, Time.deltaTime * 0.8f);
                }
            }
        }

        public void SetMixerGroup(AudioMixerGroup group)
        {
            ambientMixerGroup = group;
            if (windSource != null) windSource.outputAudioMixerGroup = group;
            if (tunnelSource != null) tunnelSource.outputAudioMixerGroup = group;
            if (stingerSource != null) stingerSource.outputAudioMixerGroup = group;
        }

        public void AssignClips(AudioClip windLoop, AudioClip[] stingers, AudioClip tunnelLoop = null)
        {
            windAmbienceClip = windLoop;
            atmosphericStingerClips = stingers;
            if (tunnelLoop != null) tunnelAmbienceClip = tunnelLoop;

            if (isInitialized)
            {
                StartAmbience();
            }
        }

        public void SetMasterAmbientVolume(float volume)
        {
            masterAmbientVolume = Mathf.Clamp01(volume);
            if (windSource != null)
            {
                windSource.volume = windBaseVolume * masterAmbientVolume;
            }
        }

        private void EnsureSources()
        {
            if (windSource == null)
            {
                GameObject windObj = new GameObject("Ambient_Wind_Loop");
                windObj.transform.SetParent(transform, false);

                windSource = windObj.AddComponent<AudioSource>();
                windSource.spatialBlend = 0f; // 2D Stereo bed
                windSource.loop = true;
                windSource.playOnAwake = false;
                windSource.priority = 48;
                if (ambientMixerGroup != null) windSource.outputAudioMixerGroup = ambientMixerGroup;
            }

            if (tunnelSource == null)
            {
                GameObject tunnelObj = new GameObject("Ambient_Tunnel_Loop");
                tunnelObj.transform.SetParent(transform, false);

                tunnelSource = tunnelObj.AddComponent<AudioSource>();
                tunnelSource.spatialBlend = 0f; // 2D Stereo bed
                tunnelSource.loop = true;
                tunnelSource.playOnAwake = false;
                tunnelSource.priority = 48;
                if (ambientMixerGroup != null) tunnelSource.outputAudioMixerGroup = ambientMixerGroup;
            }

            if (stingerSource == null)
            {
                GameObject stingerObj = new GameObject("Ambient_Sparse_Stinger");
                stingerObj.transform.SetParent(transform, false);

                stingerSource = stingerObj.AddComponent<AudioSource>();
                stingerSource.spatialBlend = 0.35f; // Slight spatial width for depth
                stingerSource.loop = false;
                stingerSource.playOnAwake = false;
                stingerSource.priority = 64;
                if (ambientMixerGroup != null) stingerSource.outputAudioMixerGroup = ambientMixerGroup;
            }

            isInitialized = true;
        }

        private void StartAmbience()
        {
            EnsureClipsAssigned();
            EnsureSources();

            if (windAmbienceClip != null && windSource != null)
            {
                windSource.clip = windAmbienceClip;
                windSource.loop = true;
                windSource.volume = windBaseVolume * masterAmbientVolume;
                if (!windSource.isPlaying)
                {
                    windSource.Play();
                }
                Debug.Log($"[AmbientAudioController] Started City Environment Sound: {windAmbienceClip.name} (volume: {windSource.volume:F2})");
            }

            if (tunnelAmbienceClip != null && tunnelSource != null)
            {
                tunnelSource.clip = tunnelAmbienceClip;
                tunnelSource.loop = true;
                tunnelSource.volume = 0f;
            }

            if (stingerRoutine != null)
                StopCoroutine(stingerRoutine);

            stingerRoutine = StartCoroutine(SparseStingerLoop());
        }

        private IEnumerator SparseStingerLoop()
        {
            // Initial delay before first atmospheric stinger
            yield return new WaitForSeconds(Random.Range(6f, 12f));

            while (true)
            {
                if (atmosphericStingerClips != null && atmosphericStingerClips.Length > 0 && stingerSource != null)
                {
                    AudioClip clip = atmosphericStingerClips[Random.Range(0, atmosphericStingerClips.Length)];
                    if (clip != null)
                    {
                        stingerSource.panStereo = Random.Range(-0.45f, 0.45f);
                        stingerSource.pitch = Random.Range(0.92f, 1.05f);
                        stingerSource.PlayOneShot(clip, stingerVolume * masterAmbientVolume);
                    }
                }

                float waitTime = Random.Range(minStingerInterval, maxStingerInterval);
                yield return new WaitForSeconds(waitTime);
            }
        }

        private void OnDisable()
        {
            if (stingerRoutine != null)
            {
                StopCoroutine(stingerRoutine);
                stingerRoutine = null;
            }
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                if (windSource != null && windAmbienceClip != null && !windSource.isPlaying)
                {
                    windSource.Play();
                }
                if (stingerRoutine == null)
                {
                    stingerRoutine = StartCoroutine(SparseStingerLoop());
                }
            }
        }
    }
}
