using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ZombieRunner.Player;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    public static class ProjectPolisher
    {
        private const string LogPrefix = "[ProjectPolisher] ";
        private const string ScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity";

        [MenuItem("ZombieRunner/Visual Audit/Run Full Professional Polish")]
        public static void PolishAll()
        {
            Debug.Log(LogPrefix + "Starting full professional polish...");

            // 0. Generate Professional UI Sprites and Building Textures
            UISpriteGenerator.GenerateAllSprites();

            // 1. Fix Player Model, Humanoid Avatar, Animations and Third-Person Framing
            FixPlayerCharacterAndAnimations();

            // 2. Fix Streetlight Fixtures and Realistic Downward Illumination
            FixStreetlights();

            // 3. Polish Materials and Facade Readability
            PolishMaterials();

            // 4. Polish Global Lighting, Ambient Trilight & Fog
            PolishLightingAndEnvironment();

            // 5. Redesign Mobile Gameplay HUD
            RedesignGameplayHUD();

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
            VisualAuditHelper.RenderCameraView();
            Debug.Log(LogPrefix + "All polish steps completed successfully!");
        }

        public static void FixPlayerCharacterAndAnimations()
        {
            Debug.Log(LogPrefix + "Configuring Player Character and Animation Importers...");

            string modelPath = "Assets/ZombieRunner/Characters/Player/NewCharacter/Ch05_nonPBR.fbx";
            ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (modelImporter != null)
            {
                bool reimport = false;
                if (modelImporter.animationType != ModelImporterAnimationType.Human)
                {
                    modelImporter.animationType = ModelImporterAnimationType.Human;
                    reimport = true;
                }
                if (modelImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    reimport = true;
                }
                if (reimport)
                {
                    modelImporter.SaveAndReimport();
                }
            }

            // Find character avatar from model
            Avatar characterAvatar = null;
            foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (obj is Avatar av && av.isValid)
                {
                    characterAvatar = av;
                    Debug.Log(LogPrefix + $"Found valid Humanoid Avatar: '{av.name}' (isHuman: {av.isHuman})");
                    break;
                }
            }

            // Configure all animation FBXs to use CopyFromOther with this avatar
            string animDir = "Assets/ZombieRunner/Characters/Player/NewCharacter/Animations";
            if (Directory.Exists(animDir))
            {
                string[] animFiles = Directory.GetFiles(animDir, "*.fbx");
                foreach (string animPath in animFiles)
                {
                    string unityPath = animPath.Replace("\\", "/");
                    ModelImporter animImporter = AssetImporter.GetAtPath(unityPath) as ModelImporter;
                    if (animImporter == null) continue;

                    bool modified = false;
                    if (animImporter.animationType != ModelImporterAnimationType.Human)
                    {
                        animImporter.animationType = ModelImporterAnimationType.Human;
                        modified = true;
                    }

                    if (characterAvatar != null && (animImporter.avatarSetup != ModelImporterAvatarSetup.CopyFromOther || animImporter.sourceAvatar != characterAvatar))
                    {
                        animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        animImporter.sourceAvatar = characterAvatar;
                        modified = true;
                    }

                    // Configure looping for run and idle
                    ModelImporterClipAnimation[] clips = animImporter.clipAnimations;
                    if (clips == null || clips.Length == 0) clips = animImporter.defaultClipAnimations;
                    if (clips != null && clips.Length > 0)
                    {
                        foreach (var clip in clips)
                        {
                            string lower = unityPath.ToLowerInvariant();
                            if (lower.Contains("run") || lower.Contains("idle"))
                            {
                                clip.loopTime = true;
                                clip.loopPose = true;
                                clip.lockRootRotation = true;
                                clip.lockRootHeightY = true;
                                clip.lockRootPositionXZ = true;
                                modified = true;
                            }
                        }
                        animImporter.clipAnimations = clips;
                    }

                    if (modified)
                    {
                        animImporter.SaveAndReimport();
                        Debug.Log(LogPrefix + $"Reimported animation with Humanoid CopyFromOther Avatar: {Path.GetFileName(unityPath)}");
                    }
                }
            }

            // Update PlayerPlaceholder.prefab
            string playerPrefabPath = "Assets/ZombieRunner/Prefabs/Player/PlayerPlaceholder.prefab";
            GameObject playerPrefab = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            if (playerPrefab != null)
            {
                Transform visual = playerPrefab.transform.Find("Visual - New Character");
                if (visual != null)
                {
                    visual.localScale = new Vector3(1.35f, 1.35f, 1.35f);
                    visual.localPosition = new Vector3(0f, -1.0f, 0f);

                    Animator anim = visual.GetComponent<Animator>();
                    if (anim != null)
                    {
                        if (characterAvatar != null) anim.avatar = characterAvatar;
                        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ZombieRunner/Animations/Player_Animator.controller");
                        anim.applyRootMotion = false;
                        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                }

                CharacterController cc = playerPrefab.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.height = 2.0f;
                    cc.center = new Vector3(0f, 0f, 0f);
                    cc.radius = 0.42f;
                }

                // Add or update cool rim backlight for character readability
                Transform existingPlayerRim = playerPrefab.transform.Find("Player_Cool_Rim_Light");
                if (existingPlayerRim == null)
                {
                    GameObject rimGo = new GameObject("Player_Cool_Rim_Light");
                    rimGo.transform.SetParent(playerPrefab.transform, false);
                    rimGo.transform.localPosition = new Vector3(0f, 1.7f, -0.65f);

                    Light rim = rimGo.AddComponent<Light>();
                    rim.type = LightType.Point;
                    rim.range = 3.0f;
                    rim.intensity = 1.6f;
                    rim.color = new Color(0.60f, 0.78f, 1.0f, 1.0f);
                    rim.shadows = LightShadows.None;
                }
                else
                {
                    Light rim = existingPlayerRim.GetComponent<Light>();
                    if (rim != null)
                    {
                        rim.range = 3.0f;
                        rim.intensity = 1.6f;
                        rim.color = new Color(0.60f, 0.78f, 1.0f, 1.0f);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(playerPrefab, playerPrefabPath);
                PrefabUtility.UnloadPrefabContents(playerPrefab);
                Debug.Log(LogPrefix + "PlayerPlaceholder.prefab updated with scale 1.35 and Humanoid Avatar.");
            }

            // Update scene player if scene is open
            GameObject scenePlayer = GameObject.Find("Player - Capsule Placeholder");
            if (scenePlayer != null)
            {
                Transform visual = scenePlayer.transform.Find("Visual - New Character");
                if (visual != null)
                {
                    visual.localScale = new Vector3(1.35f, 1.35f, 1.35f);
                    visual.localPosition = new Vector3(0f, -1.0f, 0f);
                    Animator anim = visual.GetComponent<Animator>();
                    if (anim != null)
                    {
                        if (characterAvatar != null) anim.avatar = characterAvatar;
                        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/ZombieRunner/Animations/Player_Animator.controller");
                        anim.applyRootMotion = false;
                        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                    EditorUtility.SetDirty(scenePlayer);
                }
            }

            // Camera Framing for high-quality third person gameplay
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.fieldOfView = 58f;
                cam.farClipPlane = 220f;
                cam.backgroundColor = new Color(0.12f, 0.17f, 0.25f, 1.0f);

                RunnerCameraFollow follow = cam.GetComponent<RunnerCameraFollow>();
                if (follow != null)
                {
                    SerializedObject so = new SerializedObject(follow);
                    so.FindProperty("followOffset").vector3Value = new Vector3(0f, 2.25f, -4.0f);
                    so.FindProperty("smoothTime").floatValue = 0.10f;
                    so.FindProperty("lookAheadDistance").floatValue = 12f;
                    so.FindProperty("lookHeight").floatValue = 1.25f;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(follow);
                }
            }
        }

        public static void FixStreetlights()
        {
            Debug.Log(LogPrefix + "Fixing Streetlight fixture models, emissive materials, and light pools...");

            // 1. Material calibration: warm sodium amber with soft emission (not flat glowing cube)
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_TunnelLamp.mat");
            if (lampMat != null)
            {
                lampMat.SetColor("_BaseColor", new Color(1.0f, 0.85f, 0.60f, 1.0f));
                lampMat.SetColor("_EmissionColor", new Color(1.35f, 1.0f, 0.48f, 1.0f));
                lampMat.SetFloat("_Smoothness", 0.70f);
                lampMat.EnableKeyword("_EMISSION");
                EditorUtility.SetDirty(lampMat);
            }

            Material darkMetal = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Metal.mat");

            // 2. Prefab update for EnvironmentSegment_CityRuins
            string ruinsPrefabPath = "Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab";
            GameObject ruinsGo = PrefabUtility.LoadPrefabContents(ruinsPrefabPath);
            if (ruinsGo != null)
            {
                foreach (Transform t in ruinsGo.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Flickering_SodiumLamp")
                    {
                        // Reshape into downward slim lens
                        t.localScale = new Vector3(0.32f, 0.04f, 0.46f);
                        t.localPosition = new Vector3(-4.35f, 5.08f, -3.0f);
                        t.localEulerAngles = new Vector3(82.0f, 0f, 0f);

                        // Ensure dark metal housing hood above lens
                        Transform hood = t.parent.Find("Streetlight_HousingHood");
                        if (hood == null)
                        {
                            GameObject hoodGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            hoodGo.name = "Streetlight_HousingHood";
                            hoodGo.transform.SetParent(t.parent, false);
                            hood = hoodGo.transform;
                            Collider col = hoodGo.GetComponent<Collider>();
                            if (col != null) Object.DestroyImmediate(col);
                        }
                        hood.localPosition = new Vector3(-4.35f, 5.18f, -3.0f);
                        hood.localScale = new Vector3(0.44f, 0.16f, 0.62f);
                        hood.localRotation = Quaternion.identity;
                        Renderer hoodRend = hood.GetComponent<Renderer>();
                        if (hoodRend != null && darkMetal != null) hoodRend.sharedMaterial = darkMetal;

                        // Configure Spot Light on lens
                        Light lightComp = t.GetComponent<Light>();
                        if (lightComp == null) lightComp = t.gameObject.AddComponent<Light>();
                        lightComp.type = LightType.Spot;
                        lightComp.color = new Color(1.0f, 0.74f, 0.40f, 1.0f);
                        lightComp.intensity = 4.5f;
                        lightComp.range = 16.0f;
                        lightComp.spotAngle = 92.0f;
                        lightComp.innerSpotAngle = 58.0f;
                        lightComp.shadows = LightShadows.None;

                        // Ensure StreetlightFlicker is on the lens
                        StreetlightFlicker flicker = t.GetComponent<StreetlightFlicker>();
                        if (flicker == null) flicker = t.gameObject.AddComponent<StreetlightFlicker>();
                        SerializedObject flSo = new SerializedObject(flicker);
                        flSo.FindProperty("baseIntensity").floatValue = 4.5f;
                        flSo.FindProperty("lightColor").colorValue = new Color(1.0f, 0.74f, 0.40f, 1.0f);
                        flSo.FindProperty("baseEmissionColor").colorValue = new Color(1.35f, 1.0f, 0.48f, 1.0f);
                        flSo.FindProperty("targetLight").objectReferenceValue = lightComp;
                        flSo.FindProperty("fixtureRenderer").objectReferenceValue = t.GetComponent<Renderer>();
                        flSo.ApplyModifiedProperties();
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(ruinsGo, ruinsPrefabPath);
                PrefabUtility.UnloadPrefabContents(ruinsGo);
                Debug.Log(LogPrefix + "Streetlight fixtures upgraded with realistic hoods and downward Spot pools.");
            }
        }

        public static void PolishMaterials()
        {
            Debug.Log(LogPrefix + "Polishing road, facade, and character materials...");

            // Asphalt: Clean dark pavement without low-frequency wrinkling ripples
            Material asphalt = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Asphalt.mat");
            if (asphalt != null)
            {
                asphalt.SetColor("_BaseColor", new Color(0.20f, 0.22f, 0.25f, 1.0f));
                asphalt.SetFloat("_BumpScale", 0.28f);
                asphalt.SetFloat("_Smoothness", 0.30f);
                asphalt.SetColor("_SpecColor", new Color(0.16f, 0.17f, 0.19f, 1.0f));
                EditorUtility.SetDirty(asphalt);
            }

            // Lane Paint: Crisp bright yellow with slight retroreflection
            Material lanePaint = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_LanePaint.mat");
            if (lanePaint != null)
            {
                lanePaint.SetColor("_BaseColor", new Color(0.98f, 0.80f, 0.12f, 1.0f));
                lanePaint.SetFloat("_Smoothness", 0.50f);
                EditorUtility.SetDirty(lanePaint);
            }

            // Building Ruins: Detailed architectural facade with glowing warm windows
            Material ruins = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Ruins.mat");
            if (ruins != null)
            {
                Texture2D facadeBc = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ZombieRunner/Textures/Environment/T_Building_Ruins_BC.png");
                Texture2D facadeEm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ZombieRunner/Textures/Environment/T_Building_Ruins_EM.png");

                if (facadeBc != null)
                {
                    ruins.SetTexture("_BaseMap", facadeBc);
                    ruins.SetTexture("_MainTex", facadeBc);
                    ruins.SetTextureScale("_BaseMap", new Vector2(3f, 4f));
                    ruins.SetTextureScale("_MainTex", new Vector2(3f, 4f));
                }

                if (facadeEm != null)
                {
                    ruins.SetTexture("_EmissionMap", facadeEm);
                    ruins.SetTextureScale("_EmissionMap", new Vector2(3f, 4f));
                    ruins.EnableKeyword("_EMISSION");
                    ruins.SetColor("_EmissionColor", new Color(1.2f, 0.90f, 0.45f, 1.0f));
                }

                ruins.SetColor("_BaseColor", new Color(0.85f, 0.85f, 0.88f, 1.0f));
                ruins.SetFloat("_Smoothness", 0.26f);
                ruins.SetColor("_SpecColor", new Color(0.18f, 0.18f, 0.20f, 1.0f));
                EditorUtility.SetDirty(ruins);
            }

            // Concrete sidewalks: Clean readable stone
            Material concrete = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Concrete.mat");
            if (concrete != null)
            {
                concrete.SetColor("_BaseColor", new Color(0.42f, 0.44f, 0.47f, 1.0f));
                concrete.SetFloat("_Smoothness", 0.25f);
                EditorUtility.SetDirty(concrete);
            }

            // Metal props
            Material metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Metal.mat");
            if (metal != null)
            {
                metal.SetColor("_BaseColor", new Color(0.24f, 0.26f, 0.28f, 1.0f));
                metal.SetFloat("_Smoothness", 0.45f);
                metal.SetFloat("_Metallic", 0.85f);
                EditorUtility.SetDirty(metal);
            }

            // Zombie Normal: Distinct rotting olive tone readable in shadows
            Material zombieNormal = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_ZombieNormal.mat");
            if (zombieNormal != null)
            {
                zombieNormal.SetColor("_BaseColor", new Color(0.38f, 0.46f, 0.32f, 1.0f));
                zombieNormal.SetFloat("_Smoothness", 0.35f);
                EditorUtility.SetDirty(zombieNormal);
            }
        }

        public static void PolishLightingAndEnvironment()
        {
            Debug.Log(LogPrefix + "Calibrating Global Lighting, Trilight, Fog, and Skybox...");

            // Skybox: deep midnight indigo gradient with atmospheric depth
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_NightSkybox.mat");
            if (skybox != null)
            {
                skybox.SetFloat("_Exposure", 1.20f);
                skybox.SetFloat("_AtmosphereThickness", 0.85f);
                skybox.SetColor("_SkyTint", new Color(0.18f, 0.26f, 0.42f, 1.0f));
                skybox.SetColor("_GroundColor", new Color(0.10f, 0.14f, 0.20f, 1.0f));
                EditorUtility.SetDirty(skybox);
            }

            // Volume profile: ACES, balanced exposure, clean horror tone
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
            if (profile != null)
            {
                if (profile.TryGet<ColorAdjustments>(out var colorAdj))
                {
                    colorAdj.postExposure.value = 0.35f;
                    colorAdj.contrast.value = 15f;
                    colorAdj.saturation.value = 4f;
                    colorAdj.colorFilter.value = new Color(0.98f, 0.99f, 1.0f);
                }

                if (profile.TryGet<ShadowsMidtonesHighlights>(out var smh))
                {
                    smh.shadows.value = new Vector4(0.96f, 0.98f, 1.02f, 0f);
                    smh.midtones.value = new Vector4(1.02f, 1.01f, 1.00f, 0f);
                    smh.highlights.value = new Vector4(1.03f, 1.01f, 0.98f, 0f);
                }

                if (profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.threshold.value = 0.92f;
                    bloom.intensity.value = 0.40f;
                    bloom.scatter.value = 0.70f;
                }

                EditorUtility.SetDirty(profile);
            }

            // Update Directional Key Light in scene
            foreach (Light light in Object.FindObjectsByType<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    light.color = new Color(0.72f, 0.82f, 0.96f, 1.0f);
                    light.intensity = 1.65f;
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 0.75f;
                    light.transform.rotation = Quaternion.Euler(48f, 42f, 0f);
                    EditorUtility.SetDirty(light);
                    break;
                }
            }

            // Update EnvironmentThemeManager default CityRuins profile & pool depth
            EnvironmentThemeManager themeMgr = Object.FindAnyObjectByType<EnvironmentThemeManager>();
            if (themeMgr != null)
            {
                SerializedObject so = new SerializedObject(themeMgr);
                SerializedProperty pooledCountProp = so.FindProperty("pooledSegmentCount");
                if (pooledCountProp != null) pooledCountProp.intValue = 7; // 210m city depth

                SerializedProperty profilesProp = so.FindProperty("themeProfiles");
                if (profilesProp != null && profilesProp.arraySize > 0)
                {
                    SerializedProperty city = profilesProp.GetArrayElementAtIndex(0);
                    SerializedProperty prop;
                    if ((prop = city.FindPropertyRelative("fogColor")) != null) prop.colorValue = new Color(0.07f, 0.10f, 0.15f, 1.0f);
                    if ((prop = city.FindPropertyRelative("ambientSkyColor")) != null) prop.colorValue = new Color(0.30f, 0.37f, 0.48f, 1.0f);
                    if ((prop = city.FindPropertyRelative("ambientEquatorColor")) != null) prop.colorValue = new Color(0.24f, 0.29f, 0.38f, 1.0f);
                    if ((prop = city.FindPropertyRelative("ambientGroundColor")) != null) prop.colorValue = new Color(0.18f, 0.21f, 0.27f, 1.0f);
                    if ((prop = city.FindPropertyRelative("keyLightColor")) != null) prop.colorValue = new Color(0.72f, 0.82f, 0.96f, 1.0f);
                    if ((prop = city.FindPropertyRelative("keyLightIntensity")) != null) prop.floatValue = 1.65f;
                    if ((prop = city.FindPropertyRelative("fogStart")) != null) prop.floatValue = 42f;
                    if ((prop = city.FindPropertyRelative("fogEnd")) != null) prop.floatValue = 165f;
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(themeMgr);
            }

            // Update EndlessRoadManager pool depth
            EndlessRoadManager roadMgr = Object.FindAnyObjectByType<EndlessRoadManager>();
            if (roadMgr != null)
            {
                SerializedObject so = new SerializedObject(roadMgr);
                SerializedProperty countProp = so.FindProperty("pooledSegmentCount");
                if (countProp != null) countProp.intValue = 7; // 210m road depth
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(roadMgr);
            }

            // RenderSettings
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.12f, 0.17f, 0.25f, 1.0f);
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 140f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.32f, 0.38f, 0.50f);
            RenderSettings.ambientEquatorColor = new Color(0.24f, 0.28f, 0.36f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.19f, 0.25f);
            RenderSettings.ambientLight = RenderSettings.ambientEquatorColor;
        }

        public static void RedesignGameplayHUD()
        {
            Debug.Log(LogPrefix + "Applying professional mobile HUD sprites, layout, and colors...");

            Sprite roundedRect = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ZombieRunner/Textures/UI/UI_RoundedRect_64.png");
            Sprite circleBtn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ZombieRunner/Textures/UI/UI_Circle_Button.png");
            Sprite coinIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ZombieRunner/Textures/UI/UI_Coin_Icon.png");
            Sprite arrowLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ZombieRunner/Textures/UI/UI_Arrow_Left.png");
            Sprite arrowRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ZombieRunner/Textures/UI/UI_Arrow_Right.png");

            // 1. Shoot Button: Circular crimson combat button in right-thumb zone (NEVER in screen center!)
            GameObject shootBtnGo = GameObject.Find("Shoot Button");
            if (shootBtnGo != null)
            {
                RectTransform rt = shootBtnGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.sizeDelta = new Vector2(88f, 88f);
                    rt.anchoredPosition = new Vector2(-54f, 136f);
                }

                Image img = shootBtnGo.GetComponent<Image>();
                if (img != null)
                {
                    if (circleBtn != null) img.sprite = circleBtn;
                    img.color = new Color(0.85f, 0.16f, 0.14f, 0.92f);
                }

                Button btn = shootBtnGo.GetComponent<Button>();
                if (btn != null)
                {
                    ColorBlock cb = btn.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
                    cb.pressedColor = new Color(0.70f, 0.12f, 0.10f, 1f);
                    btn.colors = cb;
                }

                TMP_Text label = shootBtnGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = "FIRE";
                    label.fontSize = 18f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = Color.white;
                    label.alignment = TextAlignmentOptions.Center;
                }
                EditorUtility.SetDirty(shootBtnGo);
            }

            // 2. Movement Controls: Left & Right buttons (Translucent dark glass)
            GameObject leftBtnGo = GameObject.Find("Move Left Button");
            if (leftBtnGo != null)
            {
                RectTransform rt = leftBtnGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    rt.sizeDelta = new Vector2(72f, 72f);
                    rt.anchoredPosition = new Vector2(32f, 32f);
                }
                Image img = leftBtnGo.GetComponent<Image>();
                if (img != null)
                {
                    if (roundedRect != null) img.sprite = roundedRect;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.08f, 0.12f, 0.18f, 0.60f);
                }
                TMP_Text label = leftBtnGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = "<";
                    label.fontSize = 30f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = new Color(0.92f, 0.95f, 1.0f);
                }
                EditorUtility.SetDirty(leftBtnGo);
            }

            GameObject rightBtnGo = GameObject.Find("Move Right Button");
            if (rightBtnGo != null)
            {
                RectTransform rt = rightBtnGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    rt.sizeDelta = new Vector2(72f, 72f);
                    rt.anchoredPosition = new Vector2(116f, 32f);
                }
                Image img = rightBtnGo.GetComponent<Image>();
                if (img != null)
                {
                    if (roundedRect != null) img.sprite = roundedRect;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.08f, 0.12f, 0.18f, 0.60f);
                }
                TMP_Text label = rightBtnGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = ">";
                    label.fontSize = 30f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = new Color(0.92f, 0.95f, 1.0f);
                }
                EditorUtility.SetDirty(rightBtnGo);
            }

            // 3. Jump and Slide Buttons (Right thumb lower row)
            GameObject jumpBtnGo = GameObject.Find("Jump Button");
            if (jumpBtnGo != null)
            {
                RectTransform rt = jumpBtnGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.sizeDelta = new Vector2(72f, 72f);
                    rt.anchoredPosition = new Vector2(-32f, 32f);
                }
                Image img = jumpBtnGo.GetComponent<Image>();
                if (img != null)
                {
                    if (roundedRect != null) img.sprite = roundedRect;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.08f, 0.12f, 0.18f, 0.60f);
                }
                TMP_Text label = jumpBtnGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = "JUMP";
                    label.fontSize = 14f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = new Color(0.92f, 0.95f, 1.0f);
                }
                EditorUtility.SetDirty(jumpBtnGo);
            }

            GameObject slideBtnGo = GameObject.Find("Slide Button");
            if (slideBtnGo != null)
            {
                RectTransform rt = slideBtnGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.sizeDelta = new Vector2(72f, 72f);
                    rt.anchoredPosition = new Vector2(-116f, 32f);
                }
                Image img = slideBtnGo.GetComponent<Image>();
                if (img != null)
                {
                    if (roundedRect != null) img.sprite = roundedRect;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.08f, 0.12f, 0.18f, 0.60f);
                }
                TMP_Text label = slideBtnGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = "SLIDE";
                    label.fontSize = 14f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = new Color(0.92f, 0.95f, 1.0f);
                }
                EditorUtility.SetDirty(slideBtnGo);
            }

            // 4. Polished Compact Health Bar (Top-Right)
            GameObject healthBackGo = GameObject.Find("Health Bar Back");
            if (healthBackGo != null)
            {
                RectTransform rt = healthBackGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.sizeDelta = new Vector2(220f, 24f);
                    rt.anchoredPosition = new Vector2(-32f, -84f);
                }
                Image img = healthBackGo.GetComponent<Image>();
                if (img != null)
                {
                    if (roundedRect != null) img.sprite = roundedRect;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
                }
                EditorUtility.SetDirty(healthBackGo);
            }

            GameObject healthFillGo = GameObject.Find("Health Bar Fill");
            if (healthFillGo != null)
            {
                RectTransform rt = healthFillGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.sizeDelta = new Vector2(216f, 20f);
                    rt.anchoredPosition = new Vector2(-34f, -86f);
                }
                Image fill = healthFillGo.GetComponent<Image>();
                if (fill != null)
                {
                    if (roundedRect != null) fill.sprite = roundedRect;
                    fill.type = Image.Type.Filled;
                    fill.fillMethod = Image.FillMethod.Horizontal;
                    fill.color = new Color(0.88f, 0.18f, 0.14f, 0.95f);
                }
                EditorUtility.SetDirty(healthFillGo);
            }

            GameObject healthValGo = GameObject.Find("Health Value");
            if (healthValGo != null)
            {
                RectTransform rt = healthValGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.sizeDelta = new Vector2(180f, 24f);
                    rt.anchoredPosition = new Vector2(-142f, -84f);
                }
                TMP_Text txt = healthValGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = 13f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.color = Color.white;
                }
                EditorUtility.SetDirty(healthValGo);
            }

            // 5. Coins & Currency
            GameObject coinCountGo = GameObject.Find("Coin Count");
            if (coinCountGo != null)
            {
                RectTransform rt = coinCountGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.anchoredPosition = new Vector2(-32f, -28f);
                    rt.sizeDelta = new Vector2(100f, 40f);
                }
                TMP_Text txt = coinCountGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = 24f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(1.0f, 0.85f, 0.20f);
                }
                EditorUtility.SetDirty(coinCountGo);
            }

            GameObject coinLabelGo = GameObject.Find("Coin Icon Label");
            if (coinLabelGo != null)
            {
                RectTransform rt = coinLabelGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.anchoredPosition = new Vector2(-136f, -32f);
                    rt.sizeDelta = new Vector2(80f, 32f);
                }
                TMP_Text txt = coinLabelGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "COINS";
                    txt.fontSize = 14f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(1.0f, 0.85f, 0.20f);
                }
                EditorUtility.SetDirty(coinLabelGo);
            }

            // 6. Clean Score and Distance Typography (Top-Left)
            GameObject scoreLabelGo = GameObject.Find("Score Label");
            if (scoreLabelGo != null)
            {
                RectTransform rt = scoreLabelGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(32f, -24f);
                    rt.sizeDelta = new Vector2(160f, 22f);
                }
                TMP_Text txt = scoreLabelGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "SCORE";
                    txt.fontSize = 12f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(0.72f, 0.78f, 0.88f);
                }
                EditorUtility.SetDirty(scoreLabelGo);
            }

            GameObject scoreValGo = GameObject.Find("Score Value");
            if (scoreValGo != null)
            {
                RectTransform rt = scoreValGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(32f, -42f);
                    rt.sizeDelta = new Vector2(200f, 44f);
                }
                TMP_Text txt = scoreValGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = 32f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = Color.white;
                }
                EditorUtility.SetDirty(scoreValGo);
            }

            GameObject distValGo = GameObject.Find("Distance Value");
            if (distValGo != null)
            {
                RectTransform rt = distValGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(32f, -86f);
                    rt.sizeDelta = new Vector2(160f, 26f);
                }
                TMP_Text txt = distValGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = 16f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(0.78f, 0.84f, 0.94f);
                }
                EditorUtility.SetDirty(distValGo);
            }

            GameObject ammoCountGo = GameObject.Find("Ammo Count");
            if (ammoCountGo != null)
            {
                RectTransform rt = ammoCountGo.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(32f, -114f);
                    rt.sizeDelta = new Vector2(200f, 26f);
                }
                TMP_Text txt = ammoCountGo.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.fontSize = 16f;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(0.95f, 0.80f, 0.30f);
                }
                EditorUtility.SetDirty(ammoCountGo);
            }

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
