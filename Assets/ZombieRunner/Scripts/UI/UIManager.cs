using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieRunner.Collectibles;
using ZombieRunner.Core;
using ZombieRunner.Player;
using ZombieRunner.PowerUps;
using ZombieRunner.Weapons;

namespace ZombieRunner.UI
{
    /// <summary>Updates the in-run HUD and routes on-screen controls to gameplay systems.</summary>
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text powerUpText;
        [SerializeField] private Image healthFill;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseSettingsPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameOverUIController gameOverUI;
        [SerializeField] private TMP_Text gameOverSummary;
        [SerializeField] private Button reviveButton;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Slider settingsVolumeSlider;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button pauseHomeButton;
        [SerializeField] private Button gameOverRetryButton;
        [SerializeField] private Button gameOverHomeButton;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button slideButton;
        [SerializeField] private Button shootButton;

        [Header("Combat UI")]
        [SerializeField] private AutoAimTargetIndicator autoAimIndicator;

        private GameManager gameManager;
        private ScoreManager scoreManager;
        private PlayerController player;
        private PlayerHealth playerHealth;
        private ShootingController shooting;
        private PowerUpManager powerUps;
        private SaveManager saveManager;
        private float powerUpRefreshTime;

        private void Awake()
        {
            ResolveReferences();
            BindButtons();
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (settingsVolumeSlider != null && saveManager != null)
            {
                settingsVolumeSlider.SetValueWithoutNotify(saveManager.AudioVolume);
                settingsVolumeSlider.onValueChanged.AddListener(saveManager.SetAudioVolume);
            }
        }

        private void Start()
        {
            ResolveReferences();
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                HandleStateChanged(gameManager.CurrentState);
            }
            if (scoreManager != null)
            {
                scoreManager.ScoreChanged += HandleScoreChanged;
                HandleScoreChanged(scoreManager.Score, scoreManager.DistanceMeters, scoreManager.RunCoins, scoreManager.ZombieKills);
            }
            if (playerHealth != null)
            {
                playerHealth.HealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaximumHealth);
            }
            if (shooting != null)
            {
                shooting.AmmoChanged += HandleAmmoChanged;
                HandleAmmoChanged(shooting.CurrentAmmo, shooting.MaximumAmmo);
            }
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame))
            {
                if (gameManager != null)
                {
                    if (gameManager.CurrentState == GameState.Playing)
                        Pause();
                    else if (gameManager.CurrentState == GameState.Paused)
                        Resume();
                }
            }

            if (Time.unscaledTime < powerUpRefreshTime)
                return;
            powerUpRefreshTime = Time.unscaledTime + 0.15f;
            RefreshPowerUpText();
        }

        public void Pause() { if (gameManager == null) ResolveReferences(); gameManager?.PauseGame(); }
        public void Resume() { if (gameManager == null) ResolveReferences(); gameManager?.ResumeGame(); }
        public void Restart() { if (gameManager == null) ResolveReferences(); gameManager?.RestartRun(); }
        public void ReturnToMenu() { if (gameManager == null) ResolveReferences(); gameManager?.ReturnToMainMenu(); }
        public void Revive() { if (gameManager == null) ResolveReferences(); gameManager?.Revive(); }
        public void OpenPauseSettings()
        {
            if (gameManager == null) ResolveReferences();
            if (gameManager == null || gameManager.CurrentState != GameState.Paused)
                return;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(true);
        }
        public void ClosePauseSettings()
        {
            if (pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);
            if (gameManager != null && gameManager.CurrentState == GameState.Paused && pausePanel != null)
                pausePanel.SetActive(true);
        }
        public void MoveLeft() { if (player == null) ResolveReferences(); player?.MoveLeft(); }
        public void MoveRight() { if (player == null) ResolveReferences(); player?.MoveRight(); }
        public void Jump() { if (player == null) ResolveReferences(); player?.Jump(); }
        public void Slide() { if (player == null) ResolveReferences(); player?.Slide(); }
        public void Shoot() { if (shooting == null) ResolveReferences(); shooting?.TryShoot(); }

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.Paused && pauseSettingsPanel != null) pauseSettingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused && (pauseSettingsPanel == null || !pauseSettingsPanel.activeSelf));

            if (state == GameState.GameOver)
            {
                SetHUDVisible(false);

                if (gameOverUI == null && gameOverPanel != null)
                {
                    gameOverUI = gameOverPanel.GetComponent<GameOverUIController>();
                    if (gameOverUI == null)
                        gameOverUI = gameOverPanel.AddComponent<GameOverUIController>();
                }

                int currentScore = scoreManager != null ? scoreManager.Score : 0;
                int bestScore = SaveManager.Instance != null ? SaveManager.Instance.BestScore : 0;
                int distance = scoreManager != null ? scoreManager.DistanceMeters : 0;
                int kills = scoreManager != null ? scoreManager.ZombieKills : 0;
                int coins = scoreManager != null ? scoreManager.RunCoins : 0;
                bool canRevive = gameManager != null && gameManager.CanRevive;

                if (gameOverUI != null)
                {
                    gameOverPanel?.transform.SetAsLastSibling();
                    gameOverUI.Show(currentScore, bestScore, distance, kills, coins, canRevive);
                }
                else if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    if (reviveButton != null) reviveButton.gameObject.SetActive(canRevive);
                }

                if (gameOverSummary != null && scoreManager != null && gameOverUI == null)
                {
                    bool isNewBest = currentScore > bestScore;
                    int displayBest = Mathf.Max(currentScore, bestScore);
                    string recordBadge = isNewBest ? "   <size=15><color=#F59E0B><b>★ NEW RECORD!</b></color></size>" : string.Empty;
                    string bestColorHex = isNewBest ? "#F59E0B" : "#E2E8F0";

                    gameOverSummary.text =
                        $"<size=14><color=#8B98A5>FINAL SCORE</color></size>{recordBadge}\n" +
                        $"<size=44><color=#FFFFFF><b>{currentScore:N0}</b></color></size>\n" +
                        $"<size=12><color=#3A4454>───────────────────────────────────</color></size>\n" +
                        $"<size=16><line-height=160%><color=#8B98A5>BEST SCORE:</color>  <color={bestColorHex}><b>{displayBest:N0}</b></color>\n" +
                        $"<color=#8B98A5>DISTANCE:</color>  <color=#E2E8F0><b>{distance:N0} m</b></color>\n" +
                        $"<color=#8B98A5>ZOMBIES ELIMINATED:</color>  <color=#E2E8F0><b>{kills:N0}</b></color>\n" +
                        $"<color=#8B98A5>COINS COLLECTED:</color>  <color=#E2E8F0><b>{coins:N0}</b></color></size>";
                }
            }
            else
            {
                if (gameOverUI != null)
                {
                    gameOverUI.Hide();
                }
                else if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(false);
                }

                SetHUDVisible(state == GameState.Playing);
            }
        }

        private void SetHUDVisible(bool visible)
        {
            if (scoreText != null) scoreText.gameObject.SetActive(visible);
            if (distanceText != null) distanceText.gameObject.SetActive(visible);
            if (coinText != null) coinText.gameObject.SetActive(visible);
            if (ammoText != null) ammoText.gameObject.SetActive(visible);
            if (healthText != null) healthText.gameObject.SetActive(visible);
            if (powerUpText != null) powerUpText.gameObject.SetActive(visible);
            if (healthFill != null && healthFill.transform.parent != null && healthFill.transform.parent != transform)
            {
                healthFill.transform.parent.gameObject.SetActive(visible);
            }
            if (pauseButton != null) pauseButton.gameObject.SetActive(visible);
            if (leftButton != null) leftButton.gameObject.SetActive(visible);
            if (rightButton != null) rightButton.gameObject.SetActive(visible);
            if (jumpButton != null) jumpButton.gameObject.SetActive(visible);
            if (slideButton != null) slideButton.gameObject.SetActive(visible);
            if (shootButton != null) shootButton.gameObject.SetActive(visible);
        }

        private void HandleScoreChanged(int score, int distance, int coins, int _)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (distanceText != null) distanceText.text = distance.ToString("N0") + " m";
            if (coinText != null) coinText.text = coins.ToString("N0");
        }

        private void HandleHealthChanged(float current, float maximum)
        {
            if (healthText != null) healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(maximum);
            if (healthFill != null) healthFill.fillAmount = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
        }

        private void HandleAmmoChanged(int current, int maximum)
        {
            if (ammoText != null) ammoText.text = "AMMO  " + current + " / " + maximum;
        }

        private void RefreshPowerUpText()
        {
            if (powerUpText == null || powerUps == null)
                return;

            string active = string.Empty;
            AddActivePowerUp(ref active, PowerUpType.CoinMagnet, "MAGNET");
            AddActivePowerUp(ref active, PowerUpType.Shield, "SHIELD");
            AddActivePowerUp(ref active, PowerUpType.SpeedBoost, "SPEED");
            AddActivePowerUp(ref active, PowerUpType.DoubleCoins, "2x COINS");
            powerUpText.text = active;
        }

        private void AddActivePowerUp(ref string value, PowerUpType type, string label)
        {
            float remaining = powerUps.GetRemainingDuration(type);
            if (remaining <= 0f)
                return;
            if (value.Length > 0)
                value += "   |   ";
            value += label + " " + Mathf.CeilToInt(remaining) + "s";
        }

        private void ResolveReferences()
        {
            if (gameManager == null) gameManager = GameManager.Instance != null ? GameManager.Instance : FindAnyObjectByType<GameManager>();
            if (saveManager == null) saveManager = SaveManager.Instance != null ? SaveManager.Instance : FindAnyObjectByType<SaveManager>();
            if (scoreManager == null) scoreManager = FindAnyObjectByType<ScoreManager>();
            if (player == null) player = FindAnyObjectByType<PlayerController>();
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (shooting == null) shooting = FindAnyObjectByType<ShootingController>();
            if (powerUps == null) powerUps = FindAnyObjectByType<PowerUpManager>();
            if (autoAimIndicator == null)
            {
                autoAimIndicator = FindAnyObjectByType<AutoAimTargetIndicator>();
                if (autoAimIndicator == null)
                {
                    GameObject indicatorGO = new GameObject("AutoAimTargetIndicator", typeof(AutoAimTargetIndicator));
                    indicatorGO.transform.SetParent(transform, false);
                    autoAimIndicator = indicatorGO.GetComponent<AutoAimTargetIndicator>();
                }
            }
        }

        private void BindButtons()
        {
            AddListener(pauseButton, Pause);
            AddListener(resumeButton, Resume);
            AddListener(settingsButton, OpenPauseSettings);
            AddListener(settingsBackButton, ClosePauseSettings);
            AddListener(restartButton, Restart);
            AddListener(pauseHomeButton, ReturnToMenu);
            AddListener(gameOverRetryButton, Restart);
            AddListener(gameOverHomeButton, ReturnToMenu);
            AddListener(reviveButton, Revive);
            AddListener(leftButton, MoveLeft);
            AddListener(rightButton, MoveRight);
            AddListener(jumpButton, Jump);
            AddListener(slideButton, Slide);
            AddListener(shootButton, Shoot);
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(() =>
                {
                    ZombieRunner.Audio.AudioManager.Instance?.Play(ZombieRunner.Audio.GameAudioCue.Button);
                    action();
                });
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.StateChanged -= HandleStateChanged;
            if (scoreManager != null) scoreManager.ScoreChanged -= HandleScoreChanged;
            if (playerHealth != null) playerHealth.HealthChanged -= HandleHealthChanged;
            if (shooting != null) shooting.AmmoChanged -= HandleAmmoChanged;
            if (settingsVolumeSlider != null && saveManager != null)
                settingsVolumeSlider.onValueChanged.RemoveListener(saveManager.SetAudioVolume);
        }
    }
}
