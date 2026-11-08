using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieRunner.Core;
using ZombieRunner.Player;
using ZombieRunner.UI;
using ZombieRunner.Weapons;

[InitializeOnLoad]
public static class ZombieRunnerQualityProbe
{
    private const string TotalCoinsKey = "ZombieRunner.TotalCoins";
    private const string BestScoreKey = "ZombieRunner.BestScore";
    private const string AudioVolumeKey = "ZombieRunner.AudioVolume";
    private const string ArmedKey = "ZombieRunnerQualityProbe.Armed";

    private static bool armed = SessionState.GetBool(ArmedKey, false);
    private static IEnumerator routine;
    private static int failures;
    private static readonly PreferenceSnapshot totalCoins = new PreferenceSnapshot(TotalCoinsKey, true);
    private static readonly PreferenceSnapshot bestScore = new PreferenceSnapshot(BestScoreKey, true);
    private static readonly PreferenceSnapshot audioVolume = new PreferenceSnapshot(AudioVolumeKey, false);

    static ZombieRunnerQualityProbe()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    public static void Arm()
    {
        totalCoins.Capture();
        bestScore.Capture();
        audioVolume.Capture();
        failures = 0;
        armed = true;
        SessionState.SetBool(ArmedKey, true);
        Debug.Log("[ZQR] Play Mode quality probe armed; saved PlayerPrefs will be restored after the run.");
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ArmedKey, false))
            return;

        armed = true;
        routine = RunChecks();
        EditorApplication.update += AdvanceChecks;
    }

    private static void AdvanceChecks()
    {
        try
        {
            if (routine != null && routine.MoveNext())
                return;
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError("[ZQR] Probe aborted: " + exception);
        }

        EditorApplication.update -= AdvanceChecks;
        routine = null;
        armed = false;
        SessionState.SetBool(ArmedKey, false);
        Time.timeScale = 1f;
        totalCoins.Restore();
        bestScore.Restore();
        audioVolume.Restore();
        PlayerPrefs.Save();
        AudioListener.volume = audioVolume.HadValue ? audioVolume.FloatValue : 0.8f;
        Debug.Log("[ZQR SUMMARY] " + (failures == 0 ? "PASS" : "FAIL") + "; failures=" + failures + ". PlayerPrefs restored.");
    }

    private static IEnumerator RunChecks()
    {
        yield return null;
        yield return null;

        Check(Application.isPlaying, "Play Mode is active");
        Check(SceneManager.GetActiveScene().name == GameManager.GameplaySceneName, "Gameplay scene is active");

        PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        PlayerHealth health = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
        ShootingController shooter = UnityEngine.Object.FindAnyObjectByType<ShootingController>();
        RunnerCameraFollow cameraFollow = UnityEngine.Object.FindAnyObjectByType<RunnerCameraFollow>();
        UIManager ui = UnityEngine.Object.FindAnyObjectByType<UIManager>();
        GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        Check(player != null && health != null && shooter != null && cameraFollow != null && ui != null && manager != null,
            "Player, health, weapon, camera, HUD, and game manager resolve");
        if (player == null || health == null || shooter == null || cameraFollow == null || ui == null || manager == null)
            yield break;

        Check(UnityEngine.Object.FindObjectsByType<GameManager>().Length == 1 && GameManager.Instance == manager,
            "Exactly one GameManager owns the run");
        Check(cameraFollow.Target == player.transform, "Camera targets the runner");
        Check(ReadField<PlayerController>(ui, "player") == player, "HUD movement reference resolves to the runner");
        Check(ReadField<ShootingController>(ui, "shooting") == shooter, "HUD fire reference resolves to the weapon");
        Check(ReadField<PlayerHealth>(ui, "playerHealth") == health, "HUD health reference resolves to the runner");
        Check(ReadField<Transform>(shooter, "muzzle") != null, "Weapon muzzle is assigned");

        DisableSpawner<ZombieRunner.Enemies.ZombieSpawner>();
        DisableSpawner<ZombieRunner.World.ObstacleSpawner>();
        DisableSpawner<ZombieRunner.Collectibles.CollectibleManager>();
        DisableSpawner<ZombieRunner.PowerUps.PowerUpManager>();

        Button right = ReadField<Button>(ui, "rightButton");
        Button left = ReadField<Button>(ui, "leftButton");
        Button jump = ReadField<Button>(ui, "jumpButton");
        Button slide = ReadField<Button>(ui, "slideButton");
        Button shoot = ReadField<Button>(ui, "shootButton");
        Check(right != null && left != null && jump != null && slide != null && shoot != null,
            "All five mobile gameplay buttons are assigned");

        CharacterController character = player.GetComponent<CharacterController>();
        int startingLane = player.CurrentLane;
        Invoke(right);
        Check(player.CurrentLane == Mathf.Min(1, startingLane + 1), "Right button changes lane");
        for (int i = 0; i < 30; i++) yield return null;
        Check(cameraFollow.transform.position.x > 0.25f, "Camera follows the lane change");
        Invoke(left);
        Check(player.CurrentLane == startingLane, "Left button returns to the original lane");

        float originalHeight = character.height;
        if (!character.isGrounded)
        {
            for (int i = 0; i < 120 && !character.isGrounded; i++) yield return null;
        }
        bool startedGrounded = character.isGrounded;
        if (startedGrounded)
        {
            Invoke(jump);
            for (int i = 0; i < 3; i++) yield return null;
            Check(player.transform.position.y > 0.05f, "Jump button launches the runner");

            for (int i = 0; i < 150 && !character.isGrounded; i++) yield return null;
            bool landed = character.isGrounded;
            Check(landed, "Runner lands after jumping");
            if (landed)
            {
                Invoke(slide);
                Check(player.IsSliding && character.height < originalHeight, "Slide button lowers the collision body");
                for (int i = 0; i < 70 && player.IsSliding; i++) yield return null;
                Check(!player.IsSliding && Mathf.Approximately(character.height, originalHeight), "Slide restores the normal collision body");
            }
        }
        else
        {
            Check(false, "Runner is grounded for jump and slide controls");
        }

        int ammoBefore = shooter.CurrentAmmo;
        Invoke(shoot);
        Check(shooter.CurrentAmmo == ammoBefore - 1, "Fire button consumes one round");
        TMP_Text ammoText = ReadField<TMP_Text>(ui, "ammoText");
        Check(ammoText != null && ammoText.text.Contains(shooter.CurrentAmmo.ToString()), "Ammo HUD updates after firing");

        health.TakeDamage(10f);
        TMP_Text healthText = ReadField<TMP_Text>(ui, "healthText");
        Image healthFill = ReadField<Image>(ui, "healthFill");
        Check(Mathf.Approximately(health.CurrentHealth, health.MaximumHealth - 10f), "Damage reduces player health");
        Check(healthText != null && healthText.text.StartsWith("90"), "Health text updates after damage");
        Check(healthFill != null && Mathf.Abs(healthFill.fillAmount - 0.9f) < 0.02f, "Health bar updates after damage");

        Button pause = ReadField<Button>(ui, "pauseButton");
        Button resume = ReadField<Button>(ui, "resumeButton");
        Invoke(pause);
        GameObject pausePanel = ReadField<GameObject>(ui, "pausePanel");
        Check(manager.CurrentState == GameState.Paused && Time.timeScale == 0f, "Pause button pauses the run");
        Check(pausePanel != null && pausePanel.activeSelf, "Pause panel is visible");
        Invoke(resume);
        Check(manager.CurrentState == GameState.Playing && Time.timeScale == 1f, "Resume button resumes the run");

        health.TakeDamage(health.CurrentHealth + 1f);
        GameObject gameOverPanel = ReadField<GameObject>(ui, "gameOverPanel");
        Check(manager.CurrentState == GameState.GameOver && health.IsDead, "Death enters game-over state");
        Check(gameOverPanel != null && gameOverPanel.activeSelf, "Game-over panel is visible");

        PlayerController oldPlayer = player;
        Button retry = ReadField<Button>(ui, "gameOverRetryButton");
        Invoke(retry);
        bool restarted = false;
        for (int i = 0; i < 240; i++)
        {
            yield return null;
            PlayerController current = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            PlayerHealth currentHealth = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            ShootingController currentShooter = UnityEngine.Object.FindAnyObjectByType<ShootingController>();
            if (current != null && current != oldPlayer && currentHealth != null && currentShooter != null &&
                currentHealth.CurrentHealth == currentHealth.MaximumHealth && currentShooter.CurrentAmmo == 30)
            {
                restarted = true;
                break;
            }
        }
        Check(restarted, "Retry button reloads a fresh gameplay run");
        if (restarted)
        {
            GameManager currentManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            Check(currentManager != null && currentManager.CurrentState == GameState.Playing, "Restart returns to Playing state");
        }
    }

    private static void Check(bool condition, string label)
    {
        if (condition)
            Debug.Log("[ZQR PASS] " + label);
        else
        {
            failures++;
            Debug.LogError("[ZQR FAIL] " + label);
        }
    }

    private static void Invoke(Button button)
    {
        if (button != null)
            button.onClick.Invoke();
    }

    private static T ReadField<T>(object target, string fieldName) where T : class
    {
        if (target == null)
            return null;
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field != null ? field.GetValue(target) as T : null;
    }

    private static void DisableSpawner<T>() where T : MonoBehaviour
    {
        T spawner = UnityEngine.Object.FindAnyObjectByType<T>();
        if (spawner != null)
            spawner.enabled = false;
    }

    private sealed class PreferenceSnapshot
    {
        private readonly string key;
        private readonly bool integer;
        public bool HadValue { get; private set; }
        public int IntValue { get; private set; }
        public float FloatValue { get; private set; }

        public PreferenceSnapshot(string preferenceKey, bool isInteger)
        {
            key = preferenceKey;
            integer = isInteger;
        }

        public void Capture()
        {
            HadValue = PlayerPrefs.HasKey(key);
            IntValue = PlayerPrefs.GetInt(key, 0);
            FloatValue = PlayerPrefs.GetFloat(key, 0.8f);
            SessionState.SetBool(key + ".HadValue", HadValue);
            SessionState.SetInt(key + ".IntValue", IntValue);
            SessionState.SetFloat(key + ".FloatValue", FloatValue);
        }

        public void Restore()
        {
            bool hadValue = SessionState.GetBool(key + ".HadValue", false);
            int intValue = SessionState.GetInt(key + ".IntValue", 0);
            float floatValue = SessionState.GetFloat(key + ".FloatValue", 0.8f);
            HadValue = hadValue;
            IntValue = intValue;
            FloatValue = floatValue;
            if (!hadValue)
                PlayerPrefs.DeleteKey(key);
            else if (integer)
                PlayerPrefs.SetInt(key, intValue);
            else
                PlayerPrefs.SetFloat(key, floatValue);
        }
    }
}
