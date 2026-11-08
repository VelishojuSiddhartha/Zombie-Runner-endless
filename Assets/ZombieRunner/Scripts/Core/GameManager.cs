using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieRunner.Collectibles;
using ZombieRunner.Player;
using ZombieRunner.PowerUps;

namespace ZombieRunner.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    /// <summary>Owns the active run, pause flow, game-over flow, and one free revive.</summary>
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        public const string GameplaySceneName = "ZombieRunnerGameplay";
        public const string MainMenuSceneName = "ZombieRunnerMainMenu";

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private PowerUpManager powerUpManager;
        [SerializeField, Min(1f)] private float reviveHealth = 50f;
        [SerializeField, Min(0.1f)] private float reviveShieldDuration = 3f;

        private bool reviveUsed;
        private bool runFinalized;
        private GameState state = GameState.Playing;

        public static GameManager Instance { get; private set; }
        public event Action<GameState> StateChanged;
        public GameState CurrentState => state;
        public bool CanRevive => state == GameState.GameOver && !reviveUsed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Time.timeScale = 1f;
            if (Application.isMobilePlatform)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            ResolveReferences();
        }

        private void Start()
        {
            ResolveReferences();
            if (playerHealth != null)
                playerHealth.Died += HandlePlayerDied;
            SetState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (state != GameState.Playing)
                return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (state != GameState.Paused)
                return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void RestartRun()
        {
            FinalizeRun();
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameplaySceneName);
        }

        public void ReturnToMainMenu()
        {
            FinalizeRun();
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public void Revive()
        {
            if (!CanRevive)
                return;

            ResolveReferences();
            if (playerHealth == null)
                return;

            reviveUsed = true;
            playerHealth.Revive(Mathf.Min(reviveHealth, playerHealth.MaximumHealth));
            if (powerUpManager != null)
                powerUpManager.Activate(PowerUpType.Shield, reviveShieldDuration);
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        private void HandlePlayerDied()
        {
            if (state == GameState.GameOver)
                return;

            Time.timeScale = 0f;
            SetState(GameState.GameOver);
            if (!CanRevive)
                FinalizeRun();
        }

        private void FinalizeRun()
        {
            if (runFinalized)
                return;
            runFinalized = true;

            if (scoreManager != null)
                scoreManager.FinalizeRun();
            else if (SaveManager.Instance != null)
                SaveManager.Instance.RecordRun(0, collectibleManager != null ? collectibleManager.RunCoins : 0);
        }

        private void SetState(GameState nextState)
        {
            state = nextState;
            StateChanged?.Invoke(state);
        }

        private void ResolveReferences()
        {
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();
            if (collectibleManager == null) collectibleManager = FindAnyObjectByType<CollectibleManager>();
            if (powerUpManager == null) powerUpManager = FindAnyObjectByType<PowerUpManager>();
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.Died -= HandlePlayerDied;
            if (Instance == this)
                Instance = null;
        }
    }
}
