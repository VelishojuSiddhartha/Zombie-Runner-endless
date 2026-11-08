using UnityEngine;
using UnityEngine.Audio;

namespace ZombieRunner.Audio
{
    public enum GameAudioCue
    {
        Shot,
        ZombieDeath,
        Pickup,
        PlayerHit,
        PowerUp,
        Explosion,
        Button
    }

    /// <summary>
    /// Master survival horror audio coordinator.
    /// Manages the AdaptiveMusicController (4-stem synchronized horror score),
    /// AmbientAudioController (atmospheric city soundscape and sparse stingers),
    /// SFX routing with dynamic pitch variation, AudioMixer routing, and saved preferences.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        [Header("Subsystem Controllers")]
        [SerializeField] private AdaptiveMusicController musicController;
        [SerializeField] private AmbientAudioController ambientController;

        [Header("Audio Mixer Routing")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup uiGroup;

        [Header("Direct Audio Sources (SFX & UI)")]
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource legacyMusicSource;
        [SerializeField] private AudioSource legacyAmbientSource;

        [Header("Clips")]
        [SerializeField] private AudioClip[] cueClips = new AudioClip[7];
        [SerializeField] private AudioClip[] gunshotClips;
        [SerializeField] private AudioClip legacyMusicClip;
        [SerializeField] private AudioClip legacyAmbientWindClip;

        [Header("Volume Balances")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.95f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.72f;
        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.80f;

        public static AudioManager Instance { get; private set; }

        public AdaptiveMusicController Music => musicController;
        public AmbientAudioController Ambience => ambientController;
        public AudioMixer Mixer => audioMixer;
        public AudioMixerGroup MasterGroup => masterGroup;
        public AudioMixerGroup MusicGroup => musicGroup;
        public AudioMixerGroup AmbientGroup => ambientGroup;
        public AudioMixerGroup SFXGroup => sfxGroup;
        public AudioMixerGroup UIGroup => uiGroup;
        public float MasterVolume => masterVolume;
        public float EffectsVolume => effectsVolume;
        public float MusicVolume => musicVolume;
        public float AmbientVolume => ambientVolume;

        private AudioClip[] proceduralClips;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureSubsystems();
            EnsureSources();
            LoadSavedSettings();
        }

        private void Start()
        {
            EnsureSubsystems();
            EnsureSources();
            LoadSavedSettings();
        }

        private void EnsureSubsystems()
        {
            if (musicController == null)
                musicController = GetComponentInChildren<AdaptiveMusicController>();
            if (musicController == null)
            {
                GameObject musicObj = new GameObject("AdaptiveMusicController");
                musicObj.transform.SetParent(transform, false);
                musicController = musicObj.AddComponent<AdaptiveMusicController>();
            }

            if (ambientController == null)
                ambientController = GetComponentInChildren<AmbientAudioController>();
            if (ambientController == null)
            {
                GameObject ambientObj = new GameObject("AmbientAudioController");
                ambientObj.transform.SetParent(transform, false);
                ambientController = ambientObj.AddComponent<AmbientAudioController>();
            }

            if (musicGroup != null && musicController != null)
                musicController.SetMixerGroup(musicGroup);

            if (ambientGroup != null && ambientController != null)
                ambientController.SetMixerGroup(ambientGroup);
        }

        private void EnsureSources()
        {
            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
            }
            effectsSource.spatialBlend = 0f;
            effectsSource.playOnAwake = false;
            effectsSource.priority = 40;
            if (sfxGroup != null) effectsSource.outputAudioMixerGroup = sfxGroup;

            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("AudioSource_UI");
                uiObj.transform.SetParent(transform, false);
                uiSource = uiObj.AddComponent<AudioSource>();
            }
            uiSource.spatialBlend = 0f;
            uiSource.playOnAwake = false;
            uiSource.priority = 50;
            if (uiGroup != null) uiSource.outputAudioMixerGroup = uiGroup;
        }

        private void LoadSavedSettings()
        {
            if (ZombieRunner.Core.SaveManager.Instance != null)
            {
                var save = ZombieRunner.Core.SaveManager.Instance;
                float mVol = save.MasterVolume > 0.05f ? save.MasterVolume : masterVolume;
                float musVol = save.MusicVolume > 0.05f ? save.MusicVolume : musicVolume;
                float ambVol = save.AmbienceVolume > 0.05f ? save.AmbienceVolume : ambientVolume;
                float sfxVol = save.SFXVolume > 0.05f ? save.SFXVolume : effectsVolume;
                SetMasterVolume(mVol);
                SetMusicVolume(musVol);
                SetAmbienceVolume(ambVol);
                SetSFXVolume(sfxVol);
            }
            else
            {
                SetMasterVolume(masterVolume);
                SetMusicVolume(musicVolume);
                SetAmbienceVolume(ambientVolume);
                SetSFXVolume(effectsVolume);
            }
        }

        public void Play(GameAudioCue cue, float volumeScale = 1f)
        {
            EnsureSources();

            // Notify combat dynamics on weapon fire, damage or explosions
            if (cue == GameAudioCue.Shot || cue == GameAudioCue.PlayerHit || cue == GameAudioCue.Explosion)
            {
                musicController?.NotifyCombatActivity(cue == GameAudioCue.Explosion ? 4.5f : 3.5f);
            }

            if (cue == GameAudioCue.Shot && gunshotClips != null && gunshotClips.Length > 0)
            {
                AudioClip shotClip = gunshotClips[UnityEngine.Random.Range(0, gunshotClips.Length)];
                effectsSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                effectsSource.PlayOneShot(shotClip, Mathf.Clamp01(effectsVolume * volumeScale));
                return;
            }

            AudioSource targetSource = (cue == GameAudioCue.Button) ? uiSource : effectsSource;
            targetSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);

            AudioClip clip = GetAudioClip(cue);
            if (clip != null)
            {
                targetSource.PlayOneShot(clip, Mathf.Clamp01(effectsVolume * volumeScale));
            }
        }

        public void AssignHorrorClips(AudioClip[] shots, AudioClip ambientWind)
        {
            gunshotClips = shots;
            legacyAmbientWindClip = ambientWind;

            if (ambientController != null && ambientWind != null)
            {
                ambientController.AssignClips(ambientWind, null);
            }
        }

        public void AssignAllHorrorAssets(
            AudioClip[] shots,
            AudioClip[] cues,
            AudioClip ambientWind,
            AudioClip[] ambientStingers,
            AudioClip layer1Exploration,
            AudioClip layer2Tension,
            AudioClip layer3Combat,
            AudioClip layer4Critical,
            AudioClip gameOverSting,
            AudioMixer mixer = null,
            AudioMixerGroup masterGrp = null,
            AudioMixerGroup musicGrp = null,
            AudioMixerGroup ambGrp = null,
            AudioMixerGroup sfxGrp = null,
            AudioMixerGroup uiGrp = null)
        {
            gunshotClips = shots;
            cueClips = cues;
            audioMixer = mixer;
            masterGroup = masterGrp;
            musicGroup = musicGrp;
            ambientGroup = ambGrp;
            sfxGroup = sfxGrp;
            uiGroup = uiGrp;

            EnsureSubsystems();
            EnsureSources();

            if (effectsSource != null && sfxGroup != null) effectsSource.outputAudioMixerGroup = sfxGroup;
            if (uiSource != null && uiGroup != null) uiSource.outputAudioMixerGroup = uiGroup;

            if (ambientController != null)
            {
                ambientController.AssignClips(ambientWind, ambientStingers);
                if (ambientGroup != null) ambientController.SetMixerGroup(ambientGroup);
            }

            if (musicController != null)
            {
                musicController.AssignClips(layer1Exploration, layer2Tension, layer3Combat, layer4Critical, gameOverSting);
                if (musicGroup != null) musicController.SetMixerGroup(musicGroup);
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicController != null) musicController.SetMasterMusicVolume(musicVolume);
        }

        public void SetAmbienceVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientController != null) ambientController.SetMasterAmbientVolume(ambientVolume);
        }

        public void SetSFXVolume(float volume)
        {
            effectsVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicEnabled(bool enabled)
        {
            if (musicController != null)
            {
                musicController.SetMasterMusicVolume(enabled ? musicVolume : 0f);
            }
        }

        private AudioClip GetAudioClip(GameAudioCue cue)
        {
            int index = (int)cue;
            if (cueClips != null && index >= 0 && index < cueClips.Length && cueClips[index] != null)
                return cueClips[index];

            if (proceduralClips == null)
                proceduralClips = new AudioClip[System.Enum.GetValues(typeof(GameAudioCue)).Length];

            if (index >= 0 && index < proceduralClips.Length)
            {
                if (proceduralClips[index] == null)
                    proceduralClips[index] = GenerateHorrorFallbackClip(cue);
                return proceduralClips[index];
            }

            return null;
        }

        private static AudioClip GenerateHorrorFallbackClip(GameAudioCue cue)
        {
            const int sampleRate = 44100;
            float duration = cue switch
            {
                GameAudioCue.Button => 0.06f,
                GameAudioCue.Shot => 0.35f,
                GameAudioCue.Pickup => 0.16f,
                GameAudioCue.PlayerHit => 0.24f,
                GameAudioCue.ZombieDeath => 0.35f,
                GameAudioCue.PowerUp => 0.40f,
                GameAudioCue.Explosion => 0.65f,
                _ => 0.12f
            };

            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * (4.5f / duration));
                float sample = 0f;

                switch (cue)
                {
                    case GameAudioCue.Button:
                        // Muffled tactile acoustic click (no harsh 1100Hz sine beep)
                        sample = Mathf.Sin(2f * Mathf.PI * 340f * t) * Mathf.Exp(-t * 85f) * 0.4f;
                        break;
                    case GameAudioCue.Pickup:
                        // Warm resonant chime (520Hz + 1040Hz soft overtone)
                        sample = (Mathf.Sin(2f * Mathf.PI * 520f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 1040f * t) * 0.25f) * env;
                        break;
                    case GameAudioCue.PlayerHit:
                        // Heavy visceral blunt impact
                        float grunt = Mathf.Sin(2f * Mathf.PI * (110f - 50f * (t / duration)) * t) * 0.7f;
                        float flesh = (UnityEngine.Random.value * 2f - 1f) * 0.25f;
                        sample = (grunt + flesh) * env;
                        break;
                    case GameAudioCue.ZombieDeath:
                        float gFreq = Mathf.Lerp(120f, 45f, t / duration);
                        sample = Mathf.Sin(2f * Mathf.PI * gFreq * t) * env * 0.6f;
                        break;
                    case GameAudioCue.PowerUp:
                        // Visceral dark sub surge
                        float f = Mathf.Lerp(55f, 210f, t / duration);
                        sample = Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.7f;
                        break;
                    case GameAudioCue.Explosion:
                        float sub = Mathf.Sin(2f * Mathf.PI * (85f - 40f * (t / duration)) * t) * 0.65f;
                        float rumble = (UnityEngine.Random.value * 2f - 1f) * 0.4f;
                        sample = (sub + rumble) * env;
                        break;
                    default:
                        sample = Mathf.Sin(2f * Mathf.PI * 220f * t) * env * 0.3f;
                        break;
                }

                samples[i] = Mathf.Clamp(sample * 0.7f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("HorrorFallback_" + cue, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
