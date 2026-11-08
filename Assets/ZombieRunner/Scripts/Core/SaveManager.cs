using UnityEngine;

namespace ZombieRunner.Core
{
    /// <summary>
    /// Stores persistent run totals, audio preferences, and PC graphics settings using PlayerPrefs.
    /// Acts as the single source of truth for all game settings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        private const string TotalCoinsKey = "ZombieRunner.TotalCoins";
        private const string BestScoreKey = "ZombieRunner.BestScore";
        private const string AudioVolumeKey = "ZombieRunner.AudioVolume";
        private const string MusicVolumeKey = "ZombieRunner.MusicVolume";
        private const string AmbienceVolumeKey = "ZombieRunner.AmbienceVolume";
        private const string SFXVolumeKey = "ZombieRunner.SFXVolume";

        private const string FullscreenKey = "ZombieRunner.Fullscreen";
        private const string QualityLevelKey = "ZombieRunner.QualityLevel";
        private const string VSyncKey = "ZombieRunner.VSync";
        private const string ResWidthKey = "ZombieRunner.ResWidth";
        private const string ResHeightKey = "ZombieRunner.ResHeight";

        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<SaveManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        instance = go.AddComponent<SaveManager>();
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        public int TotalCoins => PlayerPrefs.GetInt(TotalCoinsKey, 0);
        public int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);
        public float AudioVolume => Mathf.Clamp(PlayerPrefs.GetFloat(AudioVolumeKey, 0.85f), 0.05f, 1f);
        public float MasterVolume => AudioVolume;
        public float MusicVolume => Mathf.Clamp(PlayerPrefs.GetFloat(MusicVolumeKey, 0.72f), 0.05f, 1f);
        public float AmbienceVolume => Mathf.Clamp(PlayerPrefs.GetFloat(AmbienceVolumeKey, 0.80f), 0.05f, 1f);
        public float SFXVolume => Mathf.Clamp(PlayerPrefs.GetFloat(SFXVolumeKey, 0.95f), 0.05f, 1f);

        public bool Fullscreen => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        public int QualityLevel => PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.names != null && QualitySettings.names.Length > 1 ? 1 : 0);
        public bool VSync => PlayerPrefs.GetInt(VSyncKey, 1) == 1;
        public int SavedWidth => PlayerPrefs.GetInt(ResWidthKey, Screen.currentResolution.width);
        public int SavedHeight => PlayerPrefs.GetInt(ResHeightKey, Screen.currentResolution.height);

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyAllSavedSettings();
        }

        public void ApplyAllSavedSettings()
        {
            // Audio
            AudioListener.volume = AudioVolume;
            ZombieRunner.Audio.AudioManager.Instance?.SetMusicVolume(MusicVolume);
            ZombieRunner.Audio.AudioManager.Instance?.SetAmbienceVolume(AmbienceVolume);
            ZombieRunner.Audio.AudioManager.Instance?.SetSFXVolume(SFXVolume);

            // Graphics
            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
            {
                int q = Mathf.Clamp(QualityLevel, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(q, true);
            }

            QualitySettings.vSyncCount = VSync ? 1 : 0;

            if (SavedWidth > 0 && SavedHeight > 0)
            {
                Screen.SetResolution(SavedWidth, SavedHeight, Fullscreen);
            }
        }

        public void RecordRun(int score, int coinsCollected)
        {
            if (coinsCollected > 0)
                PlayerPrefs.SetInt(TotalCoinsKey, TotalCoins + coinsCollected);
            if (score > BestScore)
                PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }

        public void SetAudioVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(AudioVolumeKey, clamped);
            PlayerPrefs.Save();
            AudioListener.volume = clamped;
        }

        public void SetMusicVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, clamped);
            PlayerPrefs.Save();
            ZombieRunner.Audio.AudioManager.Instance?.SetMusicVolume(clamped);
        }

        public void SetAmbienceVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(AmbienceVolumeKey, clamped);
            PlayerPrefs.Save();
            ZombieRunner.Audio.AudioManager.Instance?.SetAmbienceVolume(clamped);
        }

        public void SetSFXVolume(float volume)
        {
            float clamped = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFXVolumeKey, clamped);
            PlayerPrefs.Save();
            ZombieRunner.Audio.AudioManager.Instance?.SetSFXVolume(clamped);
        }

        public void SetGraphicsSettings(int width, int height, bool fullscreen, int qualityLevel, bool vsync)
        {
            if (width > 0 && height > 0)
            {
                PlayerPrefs.SetInt(ResWidthKey, width);
                PlayerPrefs.SetInt(ResHeightKey, height);
            }

            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(QualityLevelKey, Mathf.Max(0, qualityLevel));
            PlayerPrefs.SetInt(VSyncKey, vsync ? 1 : 0);
            PlayerPrefs.Save();

            // Apply immediately
            if (width > 0 && height > 0)
            {
                Screen.SetResolution(width, height, fullscreen);
            }
            else
            {
                Screen.fullScreen = fullscreen;
            }

            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
            {
                int q = Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(q, true);
            }

            QualitySettings.vSyncCount = vsync ? 1 : 0;
        }

        public void RestoreDefaultSettings()
        {
            // Defaults: 100% audio, Highest PC quality, VSync ON, Fullscreen ON
            SetAudioVolume(0.85f);
            SetMusicVolume(0.72f);
            SetAmbienceVolume(0.80f);
            SetSFXVolume(0.95f);

            int bestQuality = QualitySettings.names != null ? Mathf.Max(0, QualitySettings.names.Length - 1) : 0;
            Resolution nativeRes = Screen.currentResolution;
            SetGraphicsSettings(nativeRes.width, nativeRes.height, true, bestQuality, true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
