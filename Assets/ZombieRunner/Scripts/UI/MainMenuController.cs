using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieRunner.Core;
using ZombieRunner.Player;

namespace ZombieRunner.UI
{
    /// <summary>
    /// Master coordinator for the cinematic Windows PC Main Menu and Settings system.
    /// Manages:
    /// - Main menu button actions (Play, Continue, Settings, Credits, Quit)
    /// - Comprehensive PC Settings (Resolution, Fullscreen, Quality, VSync, Master/Music/SFX Audio)
    /// - Persistent settings synchronization via SaveManager
    /// - PC Controls display and Credits screen
    /// - Keyboard/Gamepad and mouse navigation with cursor unlock
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Main Menu Panels")]
        [SerializeField] private GameObject mainScreenPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Run Progress Displays")]
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text totalCoinsText;
        [SerializeField] private TMP_Text versionText;

        [Header("Settings Tabs")]
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private GameObject graphicsTabContent;
        [SerializeField] private GameObject audioTabContent;
        [SerializeField] private GameObject controlsTabContent;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button applyGraphicsButton;
        [SerializeField] private Button restoreDefaultsButton;

        [Header("Graphics Controls")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle vsyncToggle;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValueText;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TMP_Text musicVolumeValueText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text sfxVolumeValueText;

        [Header("Credits Controls")]
        [SerializeField] private Button closeCreditsButton;

        private SaveManager saveManager;
        private readonly List<Resolution> availableResolutions = new List<Resolution>();
        private int selectedResolutionIndex = 0;

        private void Awake()
        {
            // Ensure mouse cursor is visible and freely movable in menus
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            EnsureManagers();
            InitializeButtons();
            InitializeResolutions();
            InitializeQualityPresets();
            LoadAudioSettings();
            RefreshRunProgress();

            if (versionText != null)
            {
                versionText.text = $"v{Application.version} • PC SURVIVAL EDITION";
            }

            SetupMenuSurvivorPose();
            AdjustMenuLayoutAndSizes();
            ShowMainScreen();
        }

        private void Start()
        {
            SetupMenuSurvivorPose();
            AdjustMenuLayoutAndSizes();

            // Select Play button for keyboard / gamepad navigation
            if (EventSystem.current != null && playButton != null)
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            }
        }

        /// <summary>
        /// Enforces polished typography, larger button dimensions, and generous breathing room.
        /// </summary>
        private void AdjustMenuLayoutAndSizes()
        {
            Transform leftCol = transform.Find("Main_Panel/Left_Column");
            if (leftCol == null) leftCol = transform.Find("Left_Column");
            if (leftCol == null)
            {
                GameObject colObj = GameObject.Find("Left_Column");
                if (colObj != null) leftCol = colObj.transform;
            }

            if (leftCol != null)
            {
                RectTransform lcRect = leftCol.GetComponent<RectTransform>();
                if (lcRect != null)
                {
                    // Widen column from 460 to 520 for bolder button presence
                    lcRect.offsetMin = new Vector2(70f, lcRect.offsetMin.y);
                    lcRect.offsetMax = new Vector2(590f, lcRect.offsetMax.y);
                }

                // Title Group
                Transform tg = leftCol.Find("Title_Group");
                if (tg != null)
                {
                    RectTransform tgRect = tg.GetComponent<RectTransform>();
                    if (tgRect != null)
                    {
                        tgRect.anchoredPosition = new Vector2(0f, -40f);
                        tgRect.sizeDelta = new Vector2(0f, 160f);
                    }

                    TMP_Text title = tg.Find("Title")?.GetComponent<TMP_Text>();
                    if (title != null)
                    {
                        title.fontSize = 68f;
                        title.fontStyle = FontStyles.Bold;
                    }

                    TMP_Text subtitle = tg.Find("Subtitle")?.GetComponent<TMP_Text>();
                    if (subtitle != null)
                    {
                        subtitle.fontSize = 16f;
                        subtitle.characterSpacing = 4f;
                    }

                    Transform divider = tg.Find("Divider");
                    if (divider != null)
                    {
                        RectTransform divRect = divider.GetComponent<RectTransform>();
                        if (divRect != null)
                        {
                            divRect.sizeDelta = new Vector2(0f, 4f);
                        }
                    }
                }

                // Buttons Vertical Stack
                Transform bs = leftCol.Find("Button_Stack");
                if (bs != null)
                {
                    RectTransform bsRect = bs.GetComponent<RectTransform>();
                    if (bsRect != null)
                    {
                        // Generous 65px breathing room below title divider
                        bsRect.anchoredPosition = new Vector2(0f, -265f);
                        bsRect.sizeDelta = new Vector2(0f, 400f);
                    }

                    VerticalLayoutGroup vlg = bs.GetComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.spacing = 16f;
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                    }

                    // Enlarge buttons: Play = 68px (fontSize 24), others = 58px (fontSize 20)
                    AdjustButtonSize(bs, "Button_Play", 68f, 24f);
                    AdjustButtonSize(bs, "Button_Continue", 58f, 20f);
                    AdjustButtonSize(bs, "Button_Settings", 58f, 20f);
                    AdjustButtonSize(bs, "Button_Credits", 58f, 20f);
                    AdjustButtonSize(bs, "Button_Quit", 58f, 20f);
                }
            }
        }

        private void AdjustButtonSize(Transform parent, string buttonName, float height, float fontSize)
        {
            Transform btnTrans = parent.Find(buttonName);
            if (btnTrans != null)
            {
                RectTransform rt = btnTrans.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(0f, height);
                }

                LayoutElement le = btnTrans.GetComponent<LayoutElement>();
                if (le == null) le = btnTrans.gameObject.AddComponent<LayoutElement>();
                le.minHeight = height;
                le.preferredHeight = height;

                TMP_Text txt = btnTrans.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = fontSize;
                    txt.fontStyle = FontStyles.Bold;
                }

                if (buttonName.Contains("Play"))
                {
                    Image img = btnTrans.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.72f, 0.14f, 0.16f, 0.95f);
                    }
                    MenuButtonEffects mbe = btnTrans.GetComponent<MenuButtonEffects>();
                    if (mbe != null)
                    {
                        mbe.SetPrimaryButton(true);
                    }
                }
            }
        }

        private void SetupMenuSurvivorPose()
        {
            GameObject survivor = GameObject.Find("Menu_Survivor");
            if (survivor == null) return;

            // Ensure guaranteed 3D survivor model is rendered
            PlayerVisualModel pvm = survivor.GetComponent<PlayerVisualModel>();
            if (pvm == null)
                pvm = survivor.AddComponent<PlayerVisualModel>();
            pvm.EnsureVisualRepresentation();

            Animator animator = survivor.GetComponentInChildren<Animator>();
            if (animator == null) return;

            // Ensure valid animator controller is assigned
            if (animator.runtimeAnimatorController == null)
            {
                RuntimeAnimatorController animCtrl = Resources.Load<RuntimeAnimatorController>("MenuAnimators/MenuIdle_Animator");
                #if UNITY_EDITOR
                if (animCtrl == null)
                {
                    animCtrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ZombieRunner/Animations/MenuIdle_Animator.controller");
                }
                if (animCtrl == null)
                {
                    animCtrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ZombieRunner/Animations/Player_Animator.controller");
                }
                #endif
                if (animCtrl != null)
                {
                    animator.runtimeAnimatorController = animCtrl;
                }
            }

            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (animator.HasState(0, Animator.StringToHash("RifleAimIdle")))
            {
                animator.Play("RifleAimIdle", 0, 0f);
            }
            else if (animator.HasState(0, Animator.StringToHash("Idle")))
            {
                animator.Play("Idle", 0, 0f);
            }

            // Attach Tommy Gun weapon to menu survivor's right hand
            MenuWeaponSocket weaponSocket = survivor.GetComponent<MenuWeaponSocket>();
            if (weaponSocket == null)
            {
                weaponSocket = survivor.AddComponent<MenuWeaponSocket>();
            }
            else
            {
                weaponSocket.AttachWeapon();
            }
        }

        private void Update()
        {
            // Global Escape key navigation for sub-panels
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else if (creditsPanel != null && creditsPanel.activeSelf)
                {
                    CloseCredits();
                }
            }
        }

        private void EnsureManagers()
        {
            saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                GameObject saveObj = new GameObject("Persistent Save Manager");
                saveManager = saveObj.AddComponent<SaveManager>();
            }

            if (FindAnyObjectByType<ZombieRunner.World.AtmosphericLightningFlash>() == null)
            {
                GameObject lightningObj = new GameObject("Menu_AtmosphericLightning");
                lightningObj.AddComponent<ZombieRunner.World.AtmosphericLightningFlash>();
            }

            EnsureMenuAudioController();
        }

        private void EnsureMenuAudioController()
        {
            MenuAudioController mac = FindAnyObjectByType<MenuAudioController>();
            if (mac == null)
            {
                GameObject audioObj = GameObject.Find("Menu_Atmosphere_Audio");
                if (audioObj == null)
                {
                    audioObj = new GameObject("Menu_Atmosphere_Audio");
                }
                mac = audioObj.GetComponent<MenuAudioController>();
                if (mac == null)
                {
                    mac = audioObj.AddComponent<MenuAudioController>();
                }
            }

#if UNITY_EDITOR
            if (mac != null)
            {
                AudioClip musicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/ProvidedAudio/music/main screen intro music.mp3");
                if (musicClip == null)
                    musicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/Music_Horror_Layer1_Exploration.wav");

                AudioClip ambClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/ProvidedAudio/music/city sound.mp3");
                if (ambClip == null)
                    ambClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/Ambience_Night_City_Wind_Loop.wav");

                AudioClip clickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_UI_Button_Horror.wav");

                mac.Configure(musicClip, ambClip, clickClip);
            }
#endif
        }

        private void InitializeButtons()
        {
            if (playButton != null) playButton.onClick.AddListener(PlayGame);
            if (continueButton != null)
            {
                bool hasProgress = saveManager != null && (saveManager.BestScore > 0 || saveManager.TotalCoins > 0);
                continueButton.interactable = hasProgress;
                continueButton.onClick.AddListener(ContinueGame);
            }
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
            if (applyGraphicsButton != null) applyGraphicsButton.onClick.AddListener(ApplyGraphicsSettings);
            if (restoreDefaultsButton != null) restoreDefaultsButton.onClick.AddListener(RestoreDefaultSettings);

            if (graphicsTabButton != null) graphicsTabButton.onClick.AddListener(() => SwitchSettingsTab(0));
            if (audioTabButton != null) audioTabButton.onClick.AddListener(() => SwitchSettingsTab(1));
            if (controlsTabButton != null) controlsTabButton.onClick.AddListener(() => SwitchSettingsTab(2));

            if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(CloseCredits);

            // Audio sliders live feedback
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        public void PlayGame()
        {
            PlayButtonClickSound();
            FindAnyObjectByType<MenuAudioController>()?.FadeOut(0.4f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameManager.GameplaySceneName);
        }

        public void ContinueGame()
        {
            PlayButtonClickSound();
            FindAnyObjectByType<MenuAudioController>()?.FadeOut(0.4f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameManager.GameplaySceneName);
        }

        public void OpenSettings()
        {
            PlayButtonClickSound();
            if (mainScreenPanel != null) mainScreenPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);

            SwitchSettingsTab(0); // Default to Graphics tab
            LoadAudioSettings();

            if (EventSystem.current != null && closeSettingsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(closeSettingsButton.gameObject);
            }
        }

        public void CloseSettings()
        {
            PlayButtonClickSound();
            ShowMainScreen();
        }

        public void OpenCredits()
        {
            PlayButtonClickSound();
            if (mainScreenPanel != null) mainScreenPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);

            if (EventSystem.current != null && closeCreditsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(closeCreditsButton.gameObject);
            }
        }

        public void CloseCredits()
        {
            PlayButtonClickSound();
            ShowMainScreen();
        }

        public void QuitGame()
        {
            PlayButtonClickSound();
            Debug.Log("[MainMenuController] Exiting application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMainScreen()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (mainScreenPanel != null) mainScreenPanel.SetActive(true);

            RefreshRunProgress();

            if (EventSystem.current != null && playButton != null)
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            }
        }

        public void SwitchSettingsTab(int tabIndex)
        {
            PlayButtonClickSound();

            if (graphicsTabContent != null) graphicsTabContent.SetActive(tabIndex == 0);
            if (audioTabContent != null) audioTabContent.SetActive(tabIndex == 1);
            if (controlsTabContent != null) controlsTabContent.SetActive(tabIndex == 2);
        }

        private void InitializeResolutions()
        {
            if (resolutionDropdown == null) return;

            resolutionDropdown.ClearOptions();
            availableResolutions.Clear();

            Resolution[] all = Screen.resolutions;
            List<string> options = new List<string>();
            int currentMatchIndex = 0;

            int targetW = Screen.width;
            int targetH = Screen.height;
            if (saveManager != null && saveManager.SavedWidth > 0 && saveManager.SavedHeight > 0)
            {
                targetW = saveManager.SavedWidth;
                targetH = saveManager.SavedHeight;
            }

            for (int i = 0; i < all.Length; i++)
            {
                // Filter to unique (width x height) pairs, preferring highest refresh rate
                Resolution res = all[i];
                int existingIdx = availableResolutions.FindIndex(r => r.width == res.width && r.height == res.height);
                if (existingIdx >= 0)
                {
                    if (res.refreshRateRatio.value > availableResolutions[existingIdx].refreshRateRatio.value)
                        availableResolutions[existingIdx] = res;
                    continue;
                }

                availableResolutions.Add(res);
                string option = $"{res.width} x {res.height} ({Mathf.RoundToInt((float)res.refreshRateRatio.value)}Hz)";
                options.Add(option);

                if (res.width == targetW && res.height == targetH)
                {
                    currentMatchIndex = availableResolutions.Count - 1;
                }
            }

            if (options.Count == 0)
            {
                options.Add($"{Screen.currentResolution.width} x {Screen.currentResolution.height}");
                availableResolutions.Add(Screen.currentResolution);
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentMatchIndex;
            resolutionDropdown.RefreshShownValue();
            selectedResolutionIndex = currentMatchIndex;

            resolutionDropdown.onValueChanged.AddListener(idx => selectedResolutionIndex = idx);

            if (fullscreenToggle != null)
            {
                bool fs = saveManager != null ? saveManager.Fullscreen : Screen.fullScreen;
                fullscreenToggle.isOn = fs;
            }

            if (vsyncToggle != null)
            {
                bool vs = saveManager != null ? saveManager.VSync : QualitySettings.vSyncCount > 0;
                vsyncToggle.isOn = vs;
            }
        }

        private void InitializeQualityPresets()
        {
            if (qualityDropdown == null) return;

            qualityDropdown.ClearOptions();
            string[] names = QualitySettings.names;
            List<string> options = new List<string>();

            if (names != null && names.Length > 0)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    options.Add(names[i].ToUpper());
                }
            }
            else
            {
                options.Add("DEFAULT");
            }

            qualityDropdown.AddOptions(options);

            int currentQuality = saveManager != null ? saveManager.QualityLevel : QualitySettings.GetQualityLevel();
            qualityDropdown.value = Mathf.Clamp(currentQuality, 0, options.Count - 1);
            qualityDropdown.RefreshShownValue();
        }

        public void ApplyGraphicsSettings()
        {
            PlayButtonClickSound();

            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
            if (selectedResolutionIndex >= 0 && selectedResolutionIndex < availableResolutions.Count)
            {
                w = availableResolutions[selectedResolutionIndex].width;
                h = availableResolutions[selectedResolutionIndex].height;
            }

            bool fs = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;
            int quality = qualityDropdown != null ? qualityDropdown.value : QualitySettings.GetQualityLevel();
            bool vsync = vsyncToggle != null ? vsyncToggle.isOn : true;

            saveManager?.SetGraphicsSettings(w, h, fs, quality, vsync);
            Debug.Log($"[MainMenuController] Applied Graphics Settings: {w}x{h}, Fullscreen: {fs}, Quality: {quality}, VSync: {vsync}");
        }

        public void RestoreDefaultSettings()
        {
            PlayButtonClickSound();
            saveManager?.RestoreDefaultSettings();

            InitializeResolutions();
            InitializeQualityPresets();
            LoadAudioSettings();
            Debug.Log("[MainMenuController] Restored default settings.");
        }

        private void LoadAudioSettings()
        {
            if (saveManager == null) return;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(saveManager.MasterVolume);
                UpdateVolumeLabel(masterVolumeValueText, saveManager.MasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(saveManager.MusicVolume);
                UpdateVolumeLabel(musicVolumeValueText, saveManager.MusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(saveManager.SFXVolume);
                UpdateVolumeLabel(sfxVolumeValueText, saveManager.SFXVolume);
            }
        }

        private void OnMasterVolumeChanged(float val)
        {
            saveManager?.SetAudioVolume(val);
            UpdateVolumeLabel(masterVolumeValueText, val);
        }

        private void OnMusicVolumeChanged(float val)
        {
            saveManager?.SetMusicVolume(val);
            UpdateVolumeLabel(musicVolumeValueText, val);
            FindAnyObjectByType<MenuAudioController>()?.OnMusicVolumeChanged(val);
        }

        private void OnSFXVolumeChanged(float val)
        {
            saveManager?.SetSFXVolume(val);
            UpdateVolumeLabel(sfxVolumeValueText, val);
        }

        private void UpdateVolumeLabel(TMP_Text label, float val)
        {
            if (label != null)
            {
                label.text = Mathf.RoundToInt(val * 100f) + "%";
            }
        }

        private void RefreshRunProgress()
        {
            if (saveManager == null) return;

            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST DISTANCE: {saveManager.BestScore:N0}m";
            }

            if (totalCoinsText != null)
            {
                totalCoinsText.text = $"BANKED COINS: {saveManager.TotalCoins:N0}";
            }
        }

        private void PlayButtonClickSound()
        {
            FindAnyObjectByType<MenuAudioController>()?.PlayClick();
        }
    }
}
