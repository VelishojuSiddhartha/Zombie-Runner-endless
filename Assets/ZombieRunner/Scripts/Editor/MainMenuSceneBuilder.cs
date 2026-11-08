using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieRunner.Core;
using ZombieRunner.UI;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Constructs the cinematic Windows PC Main Menu & Settings scene for Zombie Runner.
    /// Builds:
    /// - 3D Abandoned city street with asphalt, curbs, damaged facades, flickering streetlights, and linear fog
    /// - Foreground survivor character with rifle holding ready in idle breathing stance
    /// - Distant zombie silhouettes looming in the misty background
    /// - Cinematic 3-point lighting and subtle organic camera breathing sway
    /// - Widescreen 1920x1080 PC UI: Title, Play, Continue, Settings, Credits, Quit, Stats badge
    /// - Settings Screen with Graphics (Resolution, Fullscreen, Quality, VSync), Audio (Master, Music, SFX), and Controls
    /// - Credits Screen with project & audio attribution
    /// </summary>
    [InitializeOnLoad]
    public static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerMainMenu.unity";
        private const string AutoBuildKey = "ZombieRunner_MainMenuAutoBuilt_v4";

        static MainMenuSceneBuilder()
        {
            EditorApplication.delayCall += CheckAutoBuild;
        }

        private static void CheckAutoBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying) return;
            if (SessionState.GetBool(AutoBuildKey, false)) return;
            SessionState.SetBool(AutoBuildKey, true);

            BuildMainMenuScene();
        }

        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string RoundedRectPath = "Assets/ZombieRunner/Textures/UI/UI_RoundedRect_64.png";

        private const string RoadPrefabPath = "Assets/ZombieRunner/Prefabs/Environment/RoadSegment_30m.prefab";
        private const string CityRuinsPrefabPath = "Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab";
        private const string PlayerPrefabPath = "Assets/ZombieRunner/Prefabs/Player/PlayerPlaceholder.prefab";
        private const string PlayerAnimControllerPath = "Assets/ZombieRunner/Animations/Player_Animator.controller";
        private const string NormalZombiePrefabPath = "Assets/ZombieRunner/Prefabs/Zombies/NormalZombie.prefab";
        private const string BigZombiePrefabPath = "Assets/ZombieRunner/Prefabs/Zombies/BigZombie.prefab";

        private const string IntroMusicAudioPath = "Assets/ZombieRunner/Audio/ProvidedAudio/music/main screen intro music.mp3";
        private const string FallbackMusicAudioPath = "Assets/ZombieRunner/Audio/Clips/Music_Horror_Layer1_Exploration.wav";
        private const string CityAmbienceAudioPath = "Assets/ZombieRunner/Audio/ProvidedAudio/music/city sound.mp3";
        private const string FallbackWindAudioPath = "Assets/ZombieRunner/Audio/Clips/Ambience_Night_City_Wind_Loop.wav";
        private const string ButtonClickAudioPath = "Assets/ZombieRunner/Audio/Clips/SFX_UI_Button_Horror.wav";

        [MenuItem("Zombie Runner/UI/Build Cinematic Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying) return;
            Debug.Log("[MainMenuSceneBuilder] Starting Cinematic Main Menu Scene construction...");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Clean existing objects
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = rootObjects.Length - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(rootObjects[i]);
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedRectPath);

            SetupLightingAndFog();
            Setup3DEnvironment();
            SetupSurvivorCharacter();
            SetupDistantZombies();
            Camera mainCam = SetupCinematicCamera();
            SetupAtmosphericAudio();
            SetupUserInterface(font, roundedSprite, mainCam);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MainMenuSceneBuilder] Cinematic Main Menu Scene successfully built and saved!");
        }

        private static void SetupLightingAndFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.065f, 0.085f, 0.125f, 1f);
            RenderSettings.fogStartDistance = 14f;
            RenderSettings.fogEndDistance = 65f;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.36f, 0.48f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.24f, 0.28f, 0.35f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.20f, 0.25f, 1f);

            // Directional Key Light (Moonlight)
            GameObject moonObj = new GameObject("Directional Light (Moonlight)");
            Light moonLight = moonObj.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.72f, 0.82f, 0.98f);
            moonLight.intensity = 1.35f;
            moonLight.shadows = LightShadows.Soft;
            moonObj.transform.rotation = Quaternion.Euler(34f, -42f, 0f);
        }

        private static void Setup3DEnvironment()
        {
            GameObject envRoot = new GameObject("Environment Root");

            // Road Segments (continuous asphalt roadway)
            GameObject roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoadPrefabPath);
            if (roadPrefab != null)
            {
                GameObject road0 = PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject;
                if (road0 != null)
                {
                    road0.transform.SetParent(envRoot.transform);
                    road0.transform.position = Vector3.zero;
                    road0.name = "Road_Segment_0m";
                }

                GameObject road1 = PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject;
                if (road1 != null)
                {
                    road1.transform.SetParent(envRoot.transform);
                    road1.transform.position = new Vector3(0f, 0f, 30f);
                    road1.name = "Road_Segment_30m";
                }
            }

            // City Ruins Flanking Buildings
            GameObject cityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CityRuinsPrefabPath);
            if (cityPrefab != null)
            {
                GameObject city0 = PrefabUtility.InstantiatePrefab(cityPrefab) as GameObject;
                if (city0 != null)
                {
                    city0.transform.SetParent(envRoot.transform);
                    city0.transform.position = Vector3.zero;
                    city0.name = "City_Ruins_0m";
                }

                GameObject city1 = PrefabUtility.InstantiatePrefab(cityPrefab) as GameObject;
                if (city1 != null)
                {
                    city1.transform.SetParent(envRoot.transform);
                    city1.transform.position = new Vector3(0f, 0f, 30f);
                    city1.name = "City_Ruins_30m";
                }
            }

            // Warm Streetlight behind player to cast dramatic amber pool
            GameObject streetLamp = new GameObject("Menu_Streetlight");
            streetLamp.transform.SetParent(envRoot.transform);
            streetLamp.transform.position = new Vector3(4.8f, 0f, 4.2f);

            GameObject lampLightObj = new GameObject("Sodium_Light");
            lampLightObj.transform.SetParent(streetLamp.transform);
            lampLightObj.transform.localPosition = new Vector3(0f, 4.6f, 0f);
            lampLightObj.transform.rotation = Quaternion.Euler(72f, -25f, 0f);

            Light spotLight = lampLightObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.spotAngle = 65f;
            spotLight.innerSpotAngle = 45f;
            spotLight.range = 14f;
            spotLight.color = new Color(1.0f, 0.74f, 0.40f);
            spotLight.intensity = 4.2f;
            spotLight.shadows = LightShadows.Soft;

            StreetlightFlicker flicker = streetLamp.AddComponent<StreetlightFlicker>();
            SerializedObject flickerSO = new SerializedObject(flicker);
            flickerSO.FindProperty("targetLight").objectReferenceValue = spotLight;
            flickerSO.ApplyModifiedProperties();
        }

        private static void SetupSurvivorCharacter()
        {
            GameObject charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (charPrefab == null) return;

            GameObject charObj = PrefabUtility.InstantiatePrefab(charPrefab) as GameObject;
            if (charObj == null) return;

            charObj.name = "Menu_Survivor";
            // Placed on the right third of the frame, facing slightly toward the camera
            charObj.transform.position = new Vector3(1.18f, 0f, 2.40f);
            charObj.transform.rotation = Quaternion.Euler(0f, -162f, 0f);

            // Disable player control scripts in the menu
            var pc = charObj.GetComponent<ZombieRunner.Player.PlayerController>();
            if (pc != null) pc.enabled = false;

            var sc = charObj.GetComponent<ZombieRunner.Weapons.ShootingController>();
            if (sc != null) sc.enabled = false;

            var ph = charObj.GetComponent<ZombieRunner.Player.PlayerHealth>();
            if (ph != null) ph.enabled = false;

            var cc = charObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Hide placeholder geometries and combat reticles
            Transform[] allChildren = charObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                if (t != null && (t.name.Contains("Placeholder") || t.name.Contains("Reticle") || t.name.Contains("Muzzle Flash")))
                {
                    t.gameObject.SetActive(false);
                }
            }

            // Ensure Animator is playing Idle stance
            var anim = charObj.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                anim.SetFloat("Speed", 0f);
            }

            // Character Cinematic Lighting (Key + Rim)
            GameObject lightRoot = new GameObject("Survivor_Lighting");
            lightRoot.transform.SetParent(charObj.transform, false);

            // Cool Rim Light from behind
            GameObject rimObj = new GameObject("Rim_Light");
            rimObj.transform.SetParent(lightRoot.transform);
            rimObj.transform.localPosition = new Vector3(1.2f, 2.0f, 1.6f);
            rimObj.transform.localRotation = Quaternion.Euler(25f, -145f, 0f);
            Light rimLight = rimObj.AddComponent<Light>();
            rimLight.type = LightType.Spot;
            rimLight.spotAngle = 75f;
            rimLight.range = 6f;
            rimLight.color = new Color(0.60f, 0.85f, 1.0f);
            rimLight.intensity = 4.2f;

            // Soft Key Fill Light from front
            GameObject keyObj = new GameObject("Key_Fill_Light");
            keyObj.transform.SetParent(lightRoot.transform);
            keyObj.transform.localPosition = new Vector3(-0.9f, 1.6f, -1.3f);
            Light keyLight = keyObj.AddComponent<Light>();
            keyLight.type = LightType.Point;
            keyLight.range = 4.5f;
            keyLight.color = new Color(1.0f, 0.95f, 0.90f);
            keyLight.intensity = 3.8f;
        }

        private static void SetupDistantZombies()
        {
            GameObject zombieRoot = new GameObject("Distant_Zombies");

            // Normal Zombie in the mist in the middle-left lane
            GameObject normPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NormalZombiePrefabPath);
            if (normPrefab != null)
            {
                GameObject z1 = PrefabUtility.InstantiatePrefab(normPrefab) as GameObject;
                if (z1 != null)
                {
                    z1.name = "Distant_Zombie_Left";
                    z1.transform.SetParent(zombieRoot.transform);
                    z1.transform.position = new Vector3(-0.6f, 0f, 21.0f);
                    z1.transform.rotation = Quaternion.Euler(0f, 170f, 0f);

                    StripZombieGameplayComponents(z1);
                }
            }

            // Big Zombie silhouette in the deep background right lane
            GameObject bigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BigZombiePrefabPath);
            if (bigPrefab != null)
            {
                GameObject z2 = PrefabUtility.InstantiatePrefab(bigPrefab) as GameObject;
                if (z2 != null)
                {
                    z2.name = "Distant_Zombie_Right";
                    z2.transform.SetParent(zombieRoot.transform);
                    z2.transform.position = new Vector3(1.6f, 0f, 32.0f);
                    z2.transform.rotation = Quaternion.Euler(0f, 195f, 0f);

                    StripZombieGameplayComponents(z2);
                }
            }
        }

        private static void StripZombieGameplayComponents(GameObject z)
        {
            var cc = z.GetComponent<CharacterController>();
            if (cc != null) Object.DestroyImmediate(cc);

            // Destroy dependent scripts like ZombieSoundEmitter before ZombieController to prevent Unity dependency warnings
            var emitters = z.GetComponentsInChildren<ZombieRunner.Enemies.ZombieSoundEmitter>(true);
            foreach (var e in emitters) if (e != null) Object.DestroyImmediate(e);

            var ai = z.GetComponentsInChildren<ZombieRunner.Enemies.ZombieController>(true);
            foreach (var a in ai) if (a != null) Object.DestroyImmediate(a);

            var scripts = z.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var s in scripts)
            {
                if (s != null)
                {
                    try { Object.DestroyImmediate(s); } catch { }
                }
            }
        }

        private static Camera SetupCinematicCamera()
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = RenderSettings.fogColor;
            cam.fieldOfView = 52f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 120f;

            camObj.transform.position = new Vector3(-0.45f, 1.35f, 0.2f);
            camObj.transform.rotation = Quaternion.Euler(2.5f, 20.0f, 0f);

            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<MenuCameraPan>();

            UniversalAdditionalCameraData urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            return cam;
        }

        private static void SetupAtmosphericAudio()
        {
            GameObject audioObj = new GameObject("Menu_Atmosphere_Audio");

            MenuAudioController menuAudio = audioObj.AddComponent<MenuAudioController>();

            AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(IntroMusicAudioPath);
            if (musicClip == null)
                musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(FallbackMusicAudioPath);

            AudioClip ambClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CityAmbienceAudioPath);
            if (ambClip == null)
                ambClip = AssetDatabase.LoadAssetAtPath<AudioClip>(FallbackWindAudioPath);

            AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ButtonClickAudioPath);

            SerializedObject macSO = new SerializedObject(menuAudio);
            macSO.FindProperty("menuMusicClip").objectReferenceValue = musicClip;
            macSO.FindProperty("cityAmbienceClip").objectReferenceValue = ambClip;
            macSO.FindProperty("buttonClickClip").objectReferenceValue = clickClip;
            macSO.FindProperty("musicVolume").floatValue = 0.75f;
            macSO.FindProperty("ambienceVolume").floatValue = 0.55f;
            macSO.FindProperty("uiVolume").floatValue = 0.85f;
            macSO.FindProperty("musicFadeInSeconds").floatValue = 0.4f;
            macSO.FindProperty("ambienceFadeInSeconds").floatValue = 0.4f;
            macSO.ApplyModifiedProperties();

            Debug.Log($"[MainMenuSceneBuilder] Wired MenuAudioController with Intro Music: {musicClip?.name}, Ambience: {ambClip?.name}");
        }

        private static void SetupUserInterface(TMP_FontAsset font, Sprite roundedSprite, Camera cam)
        {
            // EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            // Canvas
            GameObject canvasObj = new GameObject("Main Menu Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();

            // 1. MAIN SCREEN PANEL
            GameObject mainPanel = CreateUIPanel(canvasObj.transform, "MainScreen_Panel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Left Sidebar Container (W: 520, aligned to left with 70px margin)
            GameObject leftCol = CreateUIPanel(mainPanel.transform, "Left_Column", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(70f, 0f), new Vector2(590f, 0f));
            leftCol.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

            // Title Block (at top: anchoredPosition y = -40, height = 160)
            GameObject titleGroup = CreateUIPanel(leftCol.transform, "Title_Group", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tgRect = titleGroup.GetComponent<RectTransform>();
            tgRect.pivot = new Vector2(0f, 1f);
            tgRect.anchoredPosition = new Vector2(0f, -40f);
            tgRect.sizeDelta = new Vector2(0f, 160f);

            TMP_Text titleText = CreateText(titleGroup.transform, "Title", "ZOMBIE RUNNER", font, 68, FontStyles.Bold, new Color(0.92f, 0.16f, 0.16f), TextAlignmentOptions.TopLeft);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.35f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            TMP_Text subTitleText = CreateText(titleGroup.transform, "Subtitle", "SURVIVAL HORROR • ENDLESS RUNNER", font, 16, FontStyles.Normal, new Color(0.68f, 0.74f, 0.84f), TextAlignmentOptions.BottomLeft);
            subTitleText.characterSpacing = 4f;
            subTitleText.rectTransform.anchorMin = new Vector2(0f, 0.08f);
            subTitleText.rectTransform.anchorMax = new Vector2(1f, 0.32f);
            subTitleText.rectTransform.offsetMin = Vector2.zero;
            subTitleText.rectTransform.offsetMax = Vector2.zero;

            // Crimson Accent Divider
            GameObject divider = CreateImage(titleGroup.transform, "Divider", roundedSprite, new Color(0.85f, 0.15f, 0.15f, 0.9f));
            RectTransform divRect = divider.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0f, 0f);
            divRect.anchorMax = new Vector2(1f, 0f);
            divRect.sizeDelta = new Vector2(0f, 4f);
            divRect.anchoredPosition = new Vector2(0f, 0f);

            // Buttons Vertical Stack (below Title: anchoredPosition y = -265, height = 400)
            GameObject btnStack = CreateUIPanel(leftCol.transform, "Button_Stack", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform bsRect = btnStack.GetComponent<RectTransform>();
            bsRect.pivot = new Vector2(0f, 1f);
            bsRect.anchoredPosition = new Vector2(0f, -265f);
            bsRect.sizeDelta = new Vector2(0f, 400f);

            VerticalLayoutGroup vlg = btnStack.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            Button playBtn = CreateMenuButton(btnStack.transform, "Button_Play", "PLAY SURVIVAL", font, roundedSprite, true);
            Button contBtn = CreateMenuButton(btnStack.transform, "Button_Continue", "CONTINUE RUN", font, roundedSprite, false);
            Button setBtn = CreateMenuButton(btnStack.transform, "Button_Settings", "SETTINGS", font, roundedSprite, false);
            Button credBtn = CreateMenuButton(btnStack.transform, "Button_Credits", "CREDITS", font, roundedSprite, false);
            Button quitBtn = CreateMenuButton(btnStack.transform, "Button_Quit", "QUIT GAME", font, roundedSprite, false);

            // Run Stats Badge
            GameObject statsBadge = CreateImage(leftCol.transform, "Stats_Badge", roundedSprite, new Color(0.08f, 0.10f, 0.14f, 0.88f));
            RectTransform statsRect = statsBadge.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0f, 0f);
            statsRect.anchorMax = new Vector2(1f, 0f);
            statsRect.pivot = new Vector2(0.5f, 0f);
            statsRect.anchoredPosition = new Vector2(0f, 95f);
            statsRect.sizeDelta = new Vector2(0f, 76f);

            TMP_Text bestDist = CreateText(statsBadge.transform, "BestScore", "BEST DISTANCE: 0m", font, 15, FontStyles.Bold, new Color(0.95f, 0.78f, 0.35f), TextAlignmentOptions.MidlineLeft);
            bestDist.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            bestDist.rectTransform.anchorMax = new Vector2(1f, 1f);
            bestDist.rectTransform.offsetMin = new Vector2(20f, 0f);
            bestDist.rectTransform.offsetMax = new Vector2(-20f, 0f);

            TMP_Text totalCoins = CreateText(statsBadge.transform, "TotalCoins", "BANKED COINS: 0", font, 15, FontStyles.Bold, new Color(0.75f, 0.85f, 0.95f), TextAlignmentOptions.MidlineLeft);
            totalCoins.rectTransform.anchorMin = new Vector2(0f, 0f);
            totalCoins.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            totalCoins.rectTransform.offsetMin = new Vector2(20f, 0f);
            totalCoins.rectTransform.offsetMax = new Vector2(-20f, 0f);

            // Bottom Footer
            TMP_Text verText = CreateText(mainPanel.transform, "Version_Text", "v1.2.0 • PC SURVIVAL EDITION", font, 14, FontStyles.Normal, new Color(0.45f, 0.50f, 0.60f), TextAlignmentOptions.BottomLeft);
            verText.rectTransform.anchorMin = new Vector2(0f, 0f);
            verText.rectTransform.anchorMax = new Vector2(0f, 0f);
            verText.rectTransform.pivot = new Vector2(0f, 0f);
            verText.rectTransform.anchoredPosition = new Vector2(80f, 35f);
            verText.rectTransform.sizeDelta = new Vector2(300f, 30f);

            TMP_Text copyText = CreateText(mainPanel.transform, "Copyright_Text", "UNIVERSAL RENDER PIPELINE • UNITY 6", font, 14, FontStyles.Normal, new Color(0.45f, 0.50f, 0.60f), TextAlignmentOptions.BottomRight);
            copyText.rectTransform.anchorMin = new Vector2(1f, 0f);
            copyText.rectTransform.anchorMax = new Vector2(1f, 0f);
            copyText.rectTransform.pivot = new Vector2(1f, 0f);
            copyText.rectTransform.anchoredPosition = new Vector2(-80f, 35f);
            copyText.rectTransform.sizeDelta = new Vector2(400f, 30f);

            // ============================================================
            // 2. SETTINGS SCREEN MODAL
            // ============================================================
            GameObject settingsModal = CreateUIPanel(canvasObj.transform, "Settings_Modal", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject setVignette = CreateImage(settingsModal.transform, "Vignette_Overlay", null, new Color(0.02f, 0.03f, 0.05f, 0.85f));
            setVignette.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            setVignette.GetComponent<RectTransform>().anchorMax = Vector2.one;
            setVignette.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            // Settings Card Window (1040 x 700)
            GameObject setCard = CreateImage(settingsModal.transform, "Settings_Card", roundedSprite, new Color(0.09f, 0.11f, 0.15f, 0.98f));
            RectTransform cardRect = setCard.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(1040f, 700f);

            // Settings Header
            TMP_Text setHeader = CreateText(setCard.transform, "Header", "SETTINGS", font, 32, FontStyles.Bold, new Color(0.92f, 0.18f, 0.18f), TextAlignmentOptions.TopLeft);
            setHeader.rectTransform.anchorMin = new Vector2(0f, 1f);
            setHeader.rectTransform.anchorMax = new Vector2(1f, 1f);
            setHeader.rectTransform.pivot = new Vector2(0f, 1f);
            setHeader.rectTransform.anchoredPosition = new Vector2(45f, -35f);
            setHeader.rectTransform.sizeDelta = new Vector2(400f, 40f);

            // Settings Tabs (Graphics | Audio | Controls)
            GameObject tabGroup = CreateUIPanel(setCard.transform, "Tab_Group", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -95f), new Vector2(-90f, 44f));
            tabGroup.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            HorizontalLayoutGroup hlg = tabGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            Button tabGraphics = CreateTabButton(tabGroup.transform, "Tab_Graphics", "GRAPHICS", font, roundedSprite);
            Button tabAudio = CreateTabButton(tabGroup.transform, "Tab_Audio", "AUDIO", font, roundedSprite);
            Button tabControls = CreateTabButton(tabGroup.transform, "Tab_Controls", "CONTROLS", font, roundedSprite);

            // Tab Content Host
            GameObject contentHost = CreateUIPanel(setCard.transform, "Content_Host", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(45f, 85f), new Vector2(-45f, -155f));

            // Tab 1: Graphics Content
            GameObject gfxContent = CreateUIPanel(contentHost.transform, "Graphics_Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TMP_Dropdown resDropdown = CreateDropdownRow(gfxContent.transform, "DISPLAY RESOLUTION", font, roundedSprite, 0);
            Toggle fsToggle = CreateToggleRow(gfxContent.transform, "FULLSCREEN MODE", font, roundedSprite, 1);
            TMP_Dropdown qualDropdown = CreateDropdownRow(gfxContent.transform, "QUALITY PRESET", font, roundedSprite, 2);
            Toggle vsToggle = CreateToggleRow(gfxContent.transform, "VERTICAL SYNC (V-SYNC)", font, roundedSprite, 3);

            // Graphics Apply & Default buttons
            GameObject gfxActionRow = CreateUIPanel(gfxContent.transform, "Action_Row", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(0f, 46f));
            HorizontalLayoutGroup gfxHlg = gfxActionRow.AddComponent<HorizontalLayoutGroup>();
            gfxHlg.spacing = 20f;
            gfxHlg.childControlWidth = true;
            gfxHlg.childControlHeight = true;
            gfxHlg.childForceExpandWidth = true;

            Button restoreDefaultsBtn = CreateSmallButton(gfxActionRow.transform, "Btn_RestoreDefaults", "RESTORE DEFAULTS", font, roundedSprite, new Color(0.18f, 0.22f, 0.28f));
            Button applyGfxBtn = CreateSmallButton(gfxActionRow.transform, "Btn_ApplyGraphics", "APPLY CHANGES", font, roundedSprite, new Color(0.75f, 0.16f, 0.16f));

            // Tab 2: Audio Content
            GameObject audContent = CreateUIPanel(contentHost.transform, "Audio_Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            (Slider masterSlider, TMP_Text masterVal) = CreateSliderRow(audContent.transform, "MASTER VOLUME", font, roundedSprite, 0);
            (Slider musicSlider, TMP_Text musicVal) = CreateSliderRow(audContent.transform, "MUSIC & SOUNDTRACK", font, roundedSprite, 1);
            (Slider sfxSlider, TMP_Text sfxVal) = CreateSliderRow(audContent.transform, "SOUND EFFECTS (SFX)", font, roundedSprite, 2);

            // Tab 3: Controls Content
            GameObject ctrlContent = CreateUIPanel(contentHost.transform, "Controls_Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateControlsView(ctrlContent.transform, font, roundedSprite);

            // Settings Bottom Back Button
            Button backSettingsBtn = CreateSmallButton(setCard.transform, "Button_BackSettings", "← BACK TO MAIN MENU", font, roundedSprite, new Color(0.18f, 0.22f, 0.28f));
            RectTransform backSetRect = backSettingsBtn.GetComponent<RectTransform>();
            backSetRect.anchorMin = new Vector2(0.5f, 0f);
            backSetRect.anchorMax = new Vector2(0.5f, 0f);
            backSetRect.pivot = new Vector2(0.5f, 0f);
            backSetRect.anchoredPosition = new Vector2(0f, 25f);
            backSetRect.sizeDelta = new Vector2(320f, 46f);

            // ============================================================
            // 3. CREDITS SCREEN MODAL
            // ============================================================
            GameObject creditsModal = CreateUIPanel(canvasObj.transform, "Credits_Modal", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject credVignette = CreateImage(creditsModal.transform, "Vignette_Overlay", null, new Color(0.02f, 0.03f, 0.05f, 0.85f));
            credVignette.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            credVignette.GetComponent<RectTransform>().anchorMax = Vector2.one;
            credVignette.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject credCard = CreateImage(creditsModal.transform, "Credits_Card", roundedSprite, new Color(0.09f, 0.11f, 0.15f, 0.98f));
            RectTransform credRect = credCard.GetComponent<RectTransform>();
            credRect.anchorMin = new Vector2(0.5f, 0.5f);
            credRect.anchorMax = new Vector2(0.5f, 0.5f);
            credRect.sizeDelta = new Vector2(940f, 620f);

            TMP_Text credHeader = CreateText(credCard.transform, "Header", "ZOMBIE RUNNER — CREDITS", font, 30, FontStyles.Bold, new Color(0.92f, 0.18f, 0.18f), TextAlignmentOptions.TopLeft);
            credHeader.rectTransform.anchorMin = new Vector2(0f, 1f);
            credHeader.rectTransform.anchorMax = new Vector2(1f, 1f);
            credHeader.rectTransform.pivot = new Vector2(0f, 1f);
            credHeader.rectTransform.anchoredPosition = new Vector2(45f, -35f);
            credHeader.rectTransform.sizeDelta = new Vector2(600f, 40f);

            string creditsTextContent =
                "<b>GAME DESIGN & TECHNICAL DIRECTION</b>\n" +
                "Lead Technical Artist & Unity Systems Engineering\n\n" +
                "<b>UNIVERSAL RENDER PIPELINE</b>\n" +
                "Unity 6 URP (17.5.0) • Cinematic Trilight & Volumetric Lighting\n\n" +
                "<b>CHARACTER RIGGING & ANIMATIONS</b>\n" +
                "Mixamo Survivor Character (Ch05) & Infected Entities (Ch10, Hulk, Yaku)\n\n" +
                "<b>AUDIO ARCHITECTURE</b>\n" +
                "Multi-Stem Procedural Adaptive Horror Audio & Ambient Soundscape\n\n" +
                "<b>INPUT & UI SYSTEM</b>\n" +
                "Unity New Input System & TextMesh Pro High-DPI PC Framework";

            TMP_Text credBody = CreateText(credCard.transform, "Credits_Body", creditsTextContent, font, 16, FontStyles.Normal, new Color(0.85f, 0.88f, 0.94f), TextAlignmentOptions.TopLeft);
            credBody.lineSpacing = 16f;
            credBody.rectTransform.anchorMin = new Vector2(0f, 0f);
            credBody.rectTransform.anchorMax = new Vector2(1f, 1f);
            credBody.rectTransform.offsetMin = new Vector2(45f, 95f);
            credBody.rectTransform.offsetMax = new Vector2(-45f, -95f);

            Button backCreditsBtn = CreateSmallButton(credCard.transform, "Button_BackCredits", "← BACK TO MAIN MENU", font, roundedSprite, new Color(0.18f, 0.22f, 0.28f));
            RectTransform backCredRect = backCreditsBtn.GetComponent<RectTransform>();
            backCredRect.anchorMin = new Vector2(0.5f, 0f);
            backCredRect.anchorMax = new Vector2(0.5f, 0f);
            backCredRect.pivot = new Vector2(0.5f, 0f);
            backCredRect.anchoredPosition = new Vector2(0f, 25f);
            backCredRect.sizeDelta = new Vector2(320f, 46f);

            // Wire all references into MainMenuController
            SerializedObject mcSO = new SerializedObject(menuController);
            mcSO.FindProperty("mainScreenPanel").objectReferenceValue = mainPanel;
            mcSO.FindProperty("settingsPanel").objectReferenceValue = settingsModal;
            mcSO.FindProperty("creditsPanel").objectReferenceValue = creditsModal;

            mcSO.FindProperty("playButton").objectReferenceValue = playBtn;
            mcSO.FindProperty("continueButton").objectReferenceValue = contBtn;
            mcSO.FindProperty("settingsButton").objectReferenceValue = setBtn;
            mcSO.FindProperty("creditsButton").objectReferenceValue = credBtn;
            mcSO.FindProperty("quitButton").objectReferenceValue = quitBtn;

            mcSO.FindProperty("bestScoreText").objectReferenceValue = bestDist;
            mcSO.FindProperty("totalCoinsText").objectReferenceValue = totalCoins;
            mcSO.FindProperty("versionText").objectReferenceValue = verText;

            mcSO.FindProperty("graphicsTabButton").objectReferenceValue = tabGraphics;
            mcSO.FindProperty("audioTabButton").objectReferenceValue = tabAudio;
            mcSO.FindProperty("controlsTabButton").objectReferenceValue = tabControls;
            mcSO.FindProperty("graphicsTabContent").objectReferenceValue = gfxContent;
            mcSO.FindProperty("audioTabContent").objectReferenceValue = audContent;
            mcSO.FindProperty("controlsTabContent").objectReferenceValue = ctrlContent;
            mcSO.FindProperty("closeSettingsButton").objectReferenceValue = backSettingsBtn;
            mcSO.FindProperty("applyGraphicsButton").objectReferenceValue = applyGfxBtn;
            mcSO.FindProperty("restoreDefaultsButton").objectReferenceValue = restoreDefaultsBtn;

            mcSO.FindProperty("resolutionDropdown").objectReferenceValue = resDropdown;
            mcSO.FindProperty("fullscreenToggle").objectReferenceValue = fsToggle;
            mcSO.FindProperty("qualityDropdown").objectReferenceValue = qualDropdown;
            mcSO.FindProperty("vsyncToggle").objectReferenceValue = vsToggle;

            mcSO.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider;
            mcSO.FindProperty("masterVolumeValueText").objectReferenceValue = masterVal;
            mcSO.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider;
            mcSO.FindProperty("musicVolumeValueText").objectReferenceValue = musicVal;
            mcSO.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
            mcSO.FindProperty("sfxVolumeValueText").objectReferenceValue = sfxVal;

            mcSO.FindProperty("closeCreditsButton").objectReferenceValue = backCreditsBtn;

            mcSO.ApplyModifiedProperties();

            // Set default active states
            settingsModal.SetActive(false);
            creditsModal.SetActive(false);
            mainPanel.SetActive(true);
        }

        // ============================================================
        // UI BUILDER HELPERS
        // ============================================================
        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return obj;
        }

        private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            return obj;
        }

        private static TMP_Text CreateText(Transform parent, string name, string content, TMP_FontAsset font, float size, FontStyles style, Color color, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = align;
            return text;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, TMP_FontAsset font, Sprite sprite, bool isPrimary)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(MenuButtonEffects));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, isPrimary ? 68f : 58f);

            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            Color baseColor = isPrimary ? new Color(0.55f, 0.12f, 0.14f, 0.95f) : new Color(0.12f, 0.14f, 0.18f, 0.85f);
            img.color = baseColor;

            Button btn = obj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = baseColor;
            cb.highlightedColor = isPrimary ? new Color(0.85f, 0.16f, 0.18f, 1f) : new Color(0.24f, 0.28f, 0.36f, 0.95f);
            cb.pressedColor = new Color(0.35f, 0.08f, 0.10f, 1f);
            cb.selectedColor = cb.highlightedColor;
            btn.colors = cb;

            MenuButtonEffects mbe = obj.GetComponent<MenuButtonEffects>();
            if (mbe != null && isPrimary)
            {
                mbe.SetPrimaryButton(true);
            }

            TMP_Text txt = CreateText(obj.transform, "Label", label, font, isPrimary ? 24 : 20, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;

            return btn;
        }

        private static Button CreateTabButton(Transform parent, string name, string label, TMP_FontAsset font, Sprite sprite)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.14f, 0.17f, 0.22f, 0.9f);

            Button btn = obj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.14f, 0.17f, 0.22f, 0.9f);
            cb.highlightedColor = new Color(0.65f, 0.15f, 0.16f, 1f);
            cb.pressedColor = new Color(0.45f, 0.10f, 0.12f, 1f);
            btn.colors = cb;

            TMP_Text txt = CreateText(obj.transform, "Label", label, font, 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;

            return btn;
        }

        private static Button CreateSmallButton(Transform parent, string name, string label, TMP_FontAsset font, Sprite sprite, Color bgColor)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            Button btn = obj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.35f;
            cb.pressedColor = bgColor * 0.75f;
            btn.colors = cb;

            TMP_Text txt = CreateText(obj.transform, "Label", label, font, 15, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.sizeDelta = Vector2.zero;

            return btn;
        }

        private static TMP_Dropdown CreateDropdownRow(Transform parent, string label, TMP_FontAsset font, Sprite sprite, int rowIdx)
        {
            float yPos = -20f - rowIdx * 68f;
            GameObject row = CreateUIPanel(parent, "Row_" + label, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos - 52f), new Vector2(0f, yPos));

            TMP_Text lbl = CreateText(row.transform, "Label", label, font, 16, FontStyles.Bold, new Color(0.85f, 0.88f, 0.92f), TextAlignmentOptions.MidlineLeft);
            lbl.rectTransform.anchorMin = new Vector2(0f, 0f);
            lbl.rectTransform.anchorMax = new Vector2(0.48f, 1f);
            lbl.rectTransform.offsetMin = Vector2.zero;
            lbl.rectTransform.offsetMax = Vector2.zero;

            GameObject ddObj = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            ddObj.transform.SetParent(row.transform, false);
            RectTransform ddRect = ddObj.GetComponent<RectTransform>();
            ddRect.anchorMin = new Vector2(0.5f, 0f);
            ddRect.anchorMax = new Vector2(1f, 1f);
            ddRect.offsetMin = Vector2.zero;
            ddRect.offsetMax = Vector2.zero;

            Image ddImg = ddObj.GetComponent<Image>();
            ddImg.sprite = sprite;
            ddImg.type = Image.Type.Sliced;
            ddImg.color = new Color(0.14f, 0.17f, 0.22f, 0.95f);

            TMP_Dropdown dd = ddObj.GetComponent<TMP_Dropdown>();

            // Caption Text
            TMP_Text caption = CreateText(ddObj.transform, "Label", "Select...", font, 15, FontStyles.Normal, Color.white, TextAlignmentOptions.MidlineLeft);
            caption.rectTransform.anchorMin = Vector2.zero;
            caption.rectTransform.anchorMax = Vector2.one;
            caption.rectTransform.offsetMin = new Vector2(16f, 0f);
            caption.rectTransform.offsetMax = new Vector2(-30f, 0f);
            dd.captionText = caption;

            // Template (Dropdown popup window)
            GameObject template = CreateImage(ddObj.transform, "Template", sprite, new Color(0.11f, 0.13f, 0.18f, 0.98f));
            RectTransform tempRect = template.GetComponent<RectTransform>();
            tempRect.anchorMin = new Vector2(0f, 0f);
            tempRect.anchorMax = new Vector2(1f, 0f);
            tempRect.pivot = new Vector2(0.5f, 1f);
            tempRect.anchoredPosition = new Vector2(0f, 2f);
            tempRect.sizeDelta = new Vector2(0f, 220f);

            ScrollRect sr = template.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            GameObject viewport = CreateUIPanel(template.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = Color.white;
            sr.viewport = viewport.GetComponent<RectTransform>();

            GameObject content = CreateUIPanel(viewport.transform, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            content.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 300f);
            sr.content = content.GetComponent<RectTransform>();

            GameObject item = CreateUIPanel(content.transform, "Item", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 38f));
            item.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            Toggle itemToggle = item.AddComponent<Toggle>();

            TMP_Text itemTxt = CreateText(item.transform, "Item Label", "Option", font, 15, FontStyles.Normal, Color.white, TextAlignmentOptions.MidlineLeft);
            itemTxt.rectTransform.anchorMin = Vector2.zero;
            itemTxt.rectTransform.anchorMax = Vector2.one;
            itemTxt.rectTransform.offsetMin = new Vector2(15f, 0f);
            itemTxt.rectTransform.offsetMax = new Vector2(-15f, 0f);

            dd.template = tempRect;
            dd.itemText = itemTxt;
            template.SetActive(false);

            return dd;
        }

        private static Toggle CreateToggleRow(Transform parent, string label, TMP_FontAsset font, Sprite sprite, int rowIdx)
        {
            float yPos = -20f - rowIdx * 68f;
            GameObject row = CreateUIPanel(parent, "Row_" + label, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos - 52f), new Vector2(0f, yPos));

            TMP_Text lbl = CreateText(row.transform, "Label", label, font, 16, FontStyles.Bold, new Color(0.85f, 0.88f, 0.92f), TextAlignmentOptions.MidlineLeft);
            lbl.rectTransform.anchorMin = new Vector2(0f, 0f);
            lbl.rectTransform.anchorMax = new Vector2(0.7f, 1f);
            lbl.rectTransform.offsetMin = Vector2.zero;
            lbl.rectTransform.offsetMax = Vector2.zero;

            GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(row.transform, false);
            RectTransform tglRect = toggleObj.GetComponent<RectTransform>();
            tglRect.anchorMin = new Vector2(1f, 0.5f);
            tglRect.anchorMax = new Vector2(1f, 0.5f);
            tglRect.pivot = new Vector2(1f, 0.5f);
            tglRect.anchoredPosition = Vector2.zero;
            tglRect.sizeDelta = new Vector2(36f, 36f);

            Toggle tgl = toggleObj.GetComponent<Toggle>();

            GameObject bg = CreateImage(toggleObj.transform, "Background", sprite, new Color(0.14f, 0.17f, 0.22f, 0.95f));
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject check = CreateImage(bg.transform, "Checkmark", sprite, new Color(0.85f, 0.18f, 0.18f, 1f));
            RectTransform checkRect = check.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.sizeDelta = Vector2.zero;

            tgl.targetGraphic = bg.GetComponent<Image>();
            tgl.graphic = check.GetComponent<Image>();

            return tgl;
        }

        private static (Slider slider, TMP_Text valueText) CreateSliderRow(Transform parent, string label, TMP_FontAsset font, Sprite sprite, int rowIdx)
        {
            float yPos = -25f - rowIdx * 82f;
            GameObject row = CreateUIPanel(parent, "Row_" + label, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos - 60f), new Vector2(0f, yPos));

            TMP_Text lbl = CreateText(row.transform, "Label", label, font, 16, FontStyles.Bold, new Color(0.85f, 0.88f, 0.92f), TextAlignmentOptions.MidlineLeft);
            lbl.rectTransform.anchorMin = new Vector2(0f, 0f);
            lbl.rectTransform.anchorMax = new Vector2(0.42f, 1f);
            lbl.rectTransform.offsetMin = Vector2.zero;
            lbl.rectTransform.offsetMax = Vector2.zero;

            // Slider Component
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(row.transform, false);
            RectTransform sldRect = sliderObj.GetComponent<RectTransform>();
            sldRect.anchorMin = new Vector2(0.44f, 0.5f);
            sldRect.anchorMax = new Vector2(0.86f, 0.5f);
            sldRect.sizeDelta = new Vector2(0f, 24f);

            Slider sld = sliderObj.GetComponent<Slider>();
            sld.minValue = 0f;
            sld.maxValue = 1f;
            sld.value = 0.8f;

            GameObject bg = CreateImage(sliderObj.transform, "Background", sprite, new Color(0.14f, 0.17f, 0.22f, 0.95f));
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject fillArea = CreateUIPanel(sliderObj.transform, "Fill Area", Vector2.zero, Vector2.one, new Vector2(5f, 0f), new Vector2(-5f, 0f));
            GameObject fill = CreateImage(fillArea.transform, "Fill", sprite, new Color(0.85f, 0.18f, 0.18f, 1f));
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            sld.fillRect = fill.GetComponent<RectTransform>();

            GameObject handleArea = CreateUIPanel(sliderObj.transform, "Handle Slide Area", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject handle = CreateImage(handleArea.transform, "Handle", sprite, new Color(0.95f, 0.95f, 0.95f, 1f));
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
            sld.handleRect = handle.GetComponent<RectTransform>();
            sld.targetGraphic = handle.GetComponent<Image>();

            // Value Label (e.g. "80%")
            TMP_Text valTxt = CreateText(row.transform, "Value", "80%", font, 16, FontStyles.Bold, new Color(0.95f, 0.78f, 0.35f), TextAlignmentOptions.MidlineRight);
            valTxt.rectTransform.anchorMin = new Vector2(0.88f, 0f);
            valTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
            valTxt.rectTransform.offsetMin = Vector2.zero;
            valTxt.rectTransform.offsetMax = Vector2.zero;

            return (sld, valTxt);
        }

        private static void CreateControlsView(Transform parent, TMP_FontAsset font, Sprite sprite)
        {
            string[,] controls = new string[,]
            {
                { "LANE SWITCH LEFT", "[ A ]  or  [ ← ]" },
                { "LANE SWITCH RIGHT", "[ D ]  or  [ → ]" },
                { "JUMP / VAULT", "[ W ]  or  [ ↑ ]  or  [ SPACE ]" },
                { "SLIDE / DUCK", "[ S ]  or  [ ↓ ]" },
                { "FIRE WEAPON", "[ LEFT MOUSE ]  or  [ F ]  or  [ CTRL ]" },
                { "TACTICAL AIM", "[ AUTOMATIC LOCK-ON WITH RETICLE ]" },
                { "PAUSE GAME", "[ ESC ]  or  [ P ]" }
            };

            for (int i = 0; i < controls.GetLength(0); i++)
            {
                float yPos = -10f - i * 52f;
                GameObject row = CreateUIPanel(parent, "Control_" + i, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, yPos - 44f), new Vector2(0f, yPos));

                TMP_Text action = CreateText(row.transform, "Action", controls[i, 0], font, 15, FontStyles.Bold, new Color(0.82f, 0.86f, 0.92f), TextAlignmentOptions.MidlineLeft);
                action.rectTransform.anchorMin = new Vector2(0f, 0f);
                action.rectTransform.anchorMax = new Vector2(0.42f, 1f);
                action.rectTransform.offsetMin = Vector2.zero;
                action.rectTransform.offsetMax = Vector2.zero;

                GameObject keyBadge = CreateImage(row.transform, "Key_Badge", sprite, new Color(0.14f, 0.17f, 0.22f, 0.95f));
                RectTransform badgeRect = keyBadge.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(0.44f, 0f);
                badgeRect.anchorMax = new Vector2(1f, 1f);
                badgeRect.offsetMin = Vector2.zero;
                badgeRect.offsetMax = Vector2.zero;

                TMP_Text keyText = CreateText(keyBadge.transform, "KeyText", controls[i, 1], font, 14, FontStyles.Bold, new Color(0.95f, 0.78f, 0.35f), TextAlignmentOptions.Center);
                keyText.rectTransform.anchorMin = Vector2.zero;
                keyText.rectTransform.anchorMax = Vector2.one;
                keyText.rectTransform.sizeDelta = Vector2.zero;
            }
        }
    }
}
