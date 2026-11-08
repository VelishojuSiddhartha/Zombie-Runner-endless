using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Upgrades the entire Zombie Runner environment to AAA-quality cinematic survival horror lighting.
    /// Configures URP settings, global directional moonlight, Trilight indirect ambient lighting,
    /// atmospheric fog, post-processing volume (ACES tonemapping, bloom, color adjustments, shadows/highlights),
    /// warm sodium streetlights with realistic pools of light on asphalt, material roughness/wetness,
    /// and character cool rim lighting.
    /// </summary>
    public static class LightingEnhancer
    {
        private const string ScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity";
        private const string SampleProfilePath = "Assets/Settings/SampleSceneProfile.asset";
        private const string PcRPAssetPath = "Assets/Settings/PC_RPAsset.asset";
        private const string MobileRPAssetPath = "Assets/Settings/Mobile_RPAsset.asset";
        private const string PcRendererPath = "Assets/Settings/PC_Renderer.asset";

        [MenuItem("ZombieRunner/Apply All Lighting & Environment Upgrades")]
        public static void ApplyAllLightingUpgrades()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ZombieRunner] Cannot apply lighting upgrades while in Play Mode.");
                return;
            }
            Debug.Log("====================================================================");
            Debug.Log("[ZombieRunner] Starting Comprehensive Cinematic Lighting Upgrade...");
            Debug.Log("====================================================================");

            UpgradePipelineSettings();
            UpgradeVolumeProfile();
            UpgradeMaterials();
            UpgradeEnvironmentPrefabs();
            UpgradeCharacterPrefabs();
            UpgradeGameplayScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("====================================================================");
            Debug.Log("[ZombieRunner] Cinematic Lighting & Environment Upgrade COMPLETED!");
            Debug.Log("====================================================================");
        }

        public static void UpgradePipelineSettings()
        {
            Debug.Log("[ZombieRunner] Upgrading URP Pipeline Assets and Renderers...");

            UniversalRenderPipelineAsset pcAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PcRPAssetPath);
            if (pcAsset != null)
            {
                SerializedObject so = new SerializedObject(pcAsset);
                so.FindProperty("m_ColorGradingMode").intValue = 1; // HDR
                so.FindProperty("m_ShadowDistance").floatValue = 70f;
                so.FindProperty("m_SoftShadowsSupported").boolValue = true;
                so.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1; // PerPixel
                so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 6;
                so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(pcAsset);
                Debug.Log("  -> PC_RPAsset configured: HDR Color Grading, 70m Shadow Distance, PerPixel Additional Lights.");
            }

            UniversalRenderPipelineAsset mobileAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobileRPAssetPath);
            if (mobileAsset != null)
            {
                SerializedObject so = new SerializedObject(mobileAsset);
                so.FindProperty("m_ColorGradingMode").intValue = 1; // HDR
                so.FindProperty("m_SoftShadowsSupported").boolValue = true;
                so.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1; // PerPixel
                so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 4;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(mobileAsset);
                Debug.Log("  -> Mobile_RPAsset configured: HDR Color Grading, Soft Shadows.");
            }

            ScriptableRendererData pcRenderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(PcRendererPath);
            if (pcRenderer != null)
            {
                foreach (ScriptableRendererFeature feature in pcRenderer.rendererFeatures)
                {
                    if (feature != null && feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
                    {
                        SerializedObject so = new SerializedObject(feature);
                        SerializedProperty settings = so.FindProperty("m_Settings");
                        if (settings != null)
                        {
                            SerializedProperty intensityProp = settings.FindPropertyRelative("Intensity");
                            SerializedProperty radiusProp = settings.FindPropertyRelative("Radius");
                            SerializedProperty directLightingProp = settings.FindPropertyRelative("DirectLightingStrength");
                            if (intensityProp != null) intensityProp.floatValue = 0.55f;
                            if (radiusProp != null) radiusProp.floatValue = 0.40f;
                            if (directLightingProp != null) directLightingProp.floatValue = 0.20f;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(feature);
                            Debug.Log("  -> ScreenSpaceAmbientOcclusion configured: Intensity=0.55, Radius=0.40.");
                        }
                    }
                }
                EditorUtility.SetDirty(pcRenderer);
            }
        }

        public static void UpgradeVolumeProfile()
        {
            Debug.Log("[ZombieRunner] Upgrading Global Post-Processing Volume Profile...");
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SampleProfilePath);
            if (profile == null)
            {
                Debug.LogError("Could not load VolumeProfile at " + SampleProfilePath);
                return;
            }

            // 1. Tonemapping (ACES)
            if (!profile.TryGet(out Tonemapping tonemapping))
                tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;

            // 2. Bloom
            if (!profile.TryGet(out Bloom bloom))
                bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.92f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.75f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.68f;
            bloom.tint.overrideState = true;
            bloom.tint.value = new Color(0.95f, 0.97f, 1.0f);
            bloom.highQualityFiltering.overrideState = true;
            bloom.highQualityFiltering.value = true;

            // 3. Color Adjustments
            if (!profile.TryGet(out ColorAdjustments colorAdjustments))
                colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = 0.35f;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.contrast.value = 16f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 10f;
            colorAdjustments.colorFilter.overrideState = true;
            colorAdjustments.colorFilter.value = new Color(0.97f, 0.98f, 1.0f);

            // 4. Shadows, Midtones, Highlights
            if (!profile.TryGet(out ShadowsMidtonesHighlights smh))
                smh = profile.Add<ShadowsMidtonesHighlights>(true);
            smh.shadows.overrideState = true;
            smh.shadows.value = new Vector4(0.88f, 0.94f, 1.08f, 0f); // Cool blue nocturnal shadows
            smh.midtones.overrideState = true;
            smh.midtones.value = new Vector4(1.0f, 1.0f, 1.0f, 0f);
            smh.highlights.overrideState = true;
            smh.highlights.value = new Vector4(1.05f, 1.02f, 0.96f, 0f); // Warm streetlight highlights
            smh.shadowsStart.overrideState = true;
            smh.shadowsStart.value = 0f;
            smh.shadowsEnd.overrideState = true;
            smh.shadowsEnd.value = 0.35f;
            smh.highlightsStart.overrideState = true;
            smh.highlightsStart.value = 0.55f;
            smh.highlightsEnd.overrideState = true;
            smh.highlightsEnd.value = 1.0f;

            // 5. Vignette
            if (!profile.TryGet(out Vignette vignette))
                vignette = profile.Add<Vignette>(true);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.22f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.35f;

            EditorUtility.SetDirty(profile);
            Debug.Log("  -> Post-processing profile configured with ACES, cinematic Bloom, ColorAdjustments, Split-Toning, and Vignette.");
        }

        public static void UpgradeMaterials()
        {
            Debug.Log("[ZombieRunner] Upgrading Environment and Character Materials...");

            // 1. Asphalt (Wet, reflective road with crack normal maps)
            Material asphalt = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Asphalt.mat");
            if (asphalt != null)
            {
                asphalt.SetFloat("_GlossyReflections", 1.0f);
                asphalt.SetFloat("_EnvironmentReflections", 1.0f);
                asphalt.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                asphalt.SetColor("_BaseColor", new Color(0.48f, 0.50f, 0.54f, 1.0f));
                asphalt.SetFloat("_Smoothness", 0.38f);
                asphalt.SetFloat("_BumpScale", 1.25f);
                if (asphalt.HasProperty("_SpecColor"))
                    asphalt.SetColor("_SpecColor", new Color(0.28f, 0.30f, 0.35f, 1.0f));
                EditorUtility.SetDirty(asphalt);
                Debug.Log("  -> WR_Asphalt upgraded: Glossy reflections enabled, BaseColor brightened to readable asphalt, Smoothness=0.38, BumpScale=1.25.");
            }

            // 1b. Subway Tunnel Materials (Prevent black voids and provide architectural depth)
            Material subConcrete = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_SubwayConcrete.mat");
            if (subConcrete != null)
            {
                subConcrete.SetFloat("_GlossyReflections", 1.0f);
                subConcrete.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                subConcrete.SetColor("_BaseColor", new Color(0.40f, 0.43f, 0.46f, 1.0f));
                subConcrete.SetFloat("_Smoothness", 0.25f);
                EditorUtility.SetDirty(subConcrete);
            }

            Material subTile = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_SubwayTile.mat");
            if (subTile != null)
            {
                subTile.SetFloat("_GlossyReflections", 1.0f);
                subTile.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                subTile.SetColor("_BaseColor", new Color(0.46f, 0.50f, 0.52f, 1.0f));
                subTile.SetFloat("_Smoothness", 0.35f);
                EditorUtility.SetDirty(subTile);
            }

            // 2. Lane Marking Paint
            Material lanePaint = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_LanePaint.mat");
            if (lanePaint != null)
            {
                lanePaint.SetFloat("_GlossyReflections", 1.0f);
                lanePaint.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                lanePaint.SetFloat("_Smoothness", 0.36f);
                lanePaint.SetColor("_BaseColor", new Color(0.92f, 0.90f, 0.82f, 1.0f));
                EditorUtility.SetDirty(lanePaint);
                Debug.Log("  -> WR_LanePaint upgraded: Smoothness=0.36, BaseColor brightened.");
            }

            // 3. Concrete
            Material concrete = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Concrete.mat");
            if (concrete != null)
            {
                concrete.SetFloat("_GlossyReflections", 1.0f);
                concrete.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                concrete.SetFloat("_Smoothness", 0.30f);
                concrete.SetFloat("_BumpScale", 1.0f);
                EditorUtility.SetDirty(concrete);
                Debug.Log("  -> WR_Concrete upgraded: Reflections enabled, Smoothness=0.30.");
            }

            // 4. Ruins / Buildings
            Material ruins = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Ruins.mat");
            if (ruins != null)
            {
                ruins.SetFloat("_GlossyReflections", 1.0f);
                ruins.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                ruins.SetFloat("_Smoothness", 0.28f);
                Texture concreteNormal = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Textures/Environment/T_Concrete_Weathered_N.png");
                if (concreteNormal != null)
                {
                    ruins.SetTexture("_BumpMap", concreteNormal);
                    ruins.EnableKeyword("_NORMALMAP");
                    ruins.SetFloat("_BumpScale", 0.9f);
                }
                EditorUtility.SetDirty(ruins);
                Debug.Log("  -> WR_Ruins upgraded: Concrete normal map connected, Smoothness=0.28.");
            }

            // 5. Car Paint
            Material carPaint = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_CarPaint.mat");
            if (carPaint != null)
            {
                carPaint.SetFloat("_GlossyReflections", 1.0f);
                carPaint.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                carPaint.SetFloat("_Metallic", 0.85f);
                carPaint.SetFloat("_Smoothness", 0.65f);
                EditorUtility.SetDirty(carPaint);
                Debug.Log("  -> WR_CarPaint upgraded: Metallic=0.85, Smoothness=0.65.");
            }

            // 6. Metal Materials
            string[] metalMatPaths = new[]
            {
                "Assets/ZombieRunner/Materials/WR_Metal.mat",
                "Assets/ZombieRunner/Materials/WR_WeaponMetal.mat",
                "Assets/ZombieRunner/Materials/WR_FactoryMetal.mat",
                "Assets/ZombieRunner/Materials/WR_Pipe.mat"
            };
            foreach (string path in metalMatPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    mat.SetFloat("_GlossyReflections", 1.0f);
                    mat.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                    mat.SetFloat("_Metallic", 0.80f);
                    mat.SetFloat("_Smoothness", 0.45f);
                    EditorUtility.SetDirty(mat);
                }
            }

            // 7. Night Skybox - seamlessly calibrated to dissolve at horizon into fog (0.08, 0.11, 0.16)
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_NightSkybox.mat");
            if (skybox != null)
            {
                skybox.SetFloat("_Exposure", 0.55f);
                skybox.SetColor("_SkyTint", new Color(0.08f, 0.11f, 0.16f, 1.0f));
                skybox.SetColor("_GroundColor", new Color(0.08f, 0.11f, 0.16f, 1.0f));
                skybox.SetFloat("_AtmosphereThickness", 0.10f);
                EditorUtility.SetDirty(skybox);
                Debug.Log("  -> WR_NightSkybox upgraded: Exposure=0.55, SkyTint & GroundColor set to match nocturnal fog.");
            }

            // 8. Streetlamp Fixture Emissive
            string[] lampMatPaths = new[]
            {
                "Assets/ZombieRunner/Materials/WR_TunnelLamp.mat",
                "Assets/ZombieRunner/Materials/WR_EmergencyLamp.mat"
            };
            foreach (string path in lampMatPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(4.2f, 2.8f, 1.2f, 1.0f));
                    EditorUtility.SetDirty(mat);
                }
            }

            // 9. Player Character Materials
            string[] playerMatPaths = new[]
            {
                "Assets/ZombieRunner/Characters/Player/NewCharacter/Materials/Ch05_body.mat",
                "Assets/ZombieRunner/Characters/Player/NewCharacter/Materials/Ch05_body1.mat",
                "Assets/ZombieRunner/Characters/Player/NewCharacter/Materials/Ch05_body2.mat"
            };
            foreach (string path in playerMatPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    mat.SetFloat("_GlossyReflections", 1.0f);
                    mat.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                    mat.SetFloat("_Smoothness", 0.42f);
                    if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1.1f);
                    EditorUtility.SetDirty(mat);
                }
            }

            // 10. Zombie Materials
            string[] zombieMatPaths = new[]
            {
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_body.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_body1.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_1001_Diffuse.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_1002_Diffuse.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/PumpkinHulk_Material.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/pumpkinHulk_diffuse.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/yaku_zombie_material.mat",
                "Assets/ZombieRunner/Characters/Zombie/Materials/Yakuzombie_diffuse.mat",
                "Assets/ZombieRunner/Materials/WR_ZombieNormal.mat",
                "Assets/ZombieRunner/Materials/WR_ZombieFast.mat",
                "Assets/ZombieRunner/Materials/WR_ZombieBig.mat"
            };
            foreach (string path in zombieMatPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    mat.SetFloat("_GlossyReflections", 1.0f);
                    mat.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                    mat.SetFloat("_Smoothness", 0.38f);
                    if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1.25f);
                    EditorUtility.SetDirty(mat);
                }
            }
        }

        public static void UpgradeEnvironmentPrefabs()
        {
            Debug.Log("[ZombieRunner] Upgrading Environment Prefabs (Streetlights & Atmosphere)...");

            string cityPrefabPath = "Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab";
            GameObject cityPrefab = PrefabUtility.LoadPrefabContents(cityPrefabPath);
            if (cityPrefab != null)
            {
                // Left-side Streetlight (Sodium vapor)
                Transform sodiumLamp = cityPrefab.transform.Find("SodiumStreetlight_Arm/Flickering_SodiumLamp");
                if (sodiumLamp == null)
                    sodiumLamp = cityPrefab.transform.Find("Flickering_SodiumLamp");

                if (sodiumLamp != null)
                {
                    Light lightComp = sodiumLamp.GetComponent<Light>();
                    if (lightComp != null)
                    {
                        lightComp.type = LightType.Spot;
                        lightComp.range = 16.5f;
                        lightComp.intensity = 4.2f;
                        lightComp.spotAngle = 92f;
                        lightComp.innerSpotAngle = 60f;
                        lightComp.color = new Color(1.0f, 0.72f, 0.38f, 1.0f);
                        lightComp.shadows = LightShadows.None;
                        sodiumLamp.localEulerAngles = new Vector3(82f, 0f, 0f);
                    }

                    StreetlightFlicker flicker = sodiumLamp.GetComponent<StreetlightFlicker>();
                    if (flicker != null)
                    {
                        SerializedObject so = new SerializedObject(flicker);
                        so.FindProperty("baseIntensity").floatValue = 4.2f;
                        so.FindProperty("lightColor").colorValue = new Color(1.0f, 0.72f, 0.38f, 1.0f);
                        so.FindProperty("baseEmissionColor").colorValue = new Color(1.8f, 1.25f, 0.55f, 1.0f);
                        so.ApplyModifiedProperties();
                    }
                    Debug.Log("  -> CityRuins left-side Sodium Streetlight upgraded: Range=16.5m, Intensity=3.4f.");
                }

                // Right-side Streetlight (Opposite broken fixture with flickering amber beacon)
                Transform brokenFixture = cityPrefab.transform.Find("BrokenStreetlight_FixtureDangling");
                if (brokenFixture != null)
                {
                    Light brokenLight = brokenFixture.GetComponent<Light>();
                    if (brokenLight == null) brokenLight = brokenFixture.gameObject.AddComponent<Light>();
                    brokenLight.type = LightType.Point;
                    brokenLight.range = 14.0f;
                    brokenLight.intensity = 2.4f;
                    brokenLight.color = new Color(1.0f, 0.63f, 0.23f, 1.0f);
                    brokenLight.shadows = LightShadows.None;

                    StreetlightFlicker brokenFlicker = brokenFixture.GetComponent<StreetlightFlicker>();
                    if (brokenFlicker == null) brokenFlicker = brokenFixture.gameObject.AddComponent<StreetlightFlicker>();
                    SerializedObject so = new SerializedObject(brokenFlicker);
                    so.FindProperty("targetLight").objectReferenceValue = brokenLight;
                    so.FindProperty("fixtureRenderer").objectReferenceValue = brokenFixture.GetComponent<Renderer>();
                    so.FindProperty("baseIntensity").floatValue = 2.4f;
                    so.FindProperty("lightColor").colorValue = new Color(1.0f, 0.63f, 0.23f, 1.0f);
                    so.FindProperty("baseEmissionColor").colorValue = new Color(3.2f, 1.8f, 0.6f, 1.0f);
                    so.FindProperty("flickerFrequency").floatValue = 18f;
                    so.FindProperty("microFlickerAmount").floatValue = 0.15f;
                    so.FindProperty("allowBlackouts").boolValue = true;
                    so.FindProperty("minBlackoutInterval").floatValue = 1.5f;
                    so.FindProperty("maxBlackoutInterval").floatValue = 4.5f;
                    so.FindProperty("blackoutDuration").floatValue = 0.12f;
                    so.ApplyModifiedProperties();
                    Debug.Log("  -> CityRuins right-side broken Streetlight upgraded: Range=14.0m, Intensity=2.4f.");
                }

                // Storefront soft warm illumination
                Transform storefrontSign = cityPrefab.transform.Find("B_Signboard");
                if (storefrontSign != null)
                {
                    Transform existingStoreLight = storefrontSign.Find("Storefront_AmbientLight");
                    if (existingStoreLight == null)
                    {
                        GameObject storeLightGo = new GameObject("Storefront_AmbientLight");
                        storeLightGo.transform.SetParent(storefrontSign, false);
                        storeLightGo.transform.localPosition = new Vector3(0.5f, -0.5f, 0f);
                        Light sl = storeLightGo.AddComponent<Light>();
                        sl.type = LightType.Point;
                        sl.range = 6.5f;
                        sl.intensity = 1.4f;
                        sl.color = new Color(1.0f, 0.85f, 0.65f, 1.0f);
                        sl.shadows = LightShadows.None;
                        Debug.Log("  -> Storefront ambient fill light added to B_Signboard.");
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(cityPrefab, cityPrefabPath);
                PrefabUtility.UnloadPrefabContents(cityPrefab);
            }

            // Upgrade Subway Tunnel Segment Prefab
            string subwayPrefabPath = "Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_SubwayTunnel.prefab";
            if (File.Exists(subwayPrefabPath))
            {
                GameObject subwayPrefab = PrefabUtility.LoadPrefabContents(subwayPrefabPath);
                if (subwayPrefab != null)
                {
                    Light[] lights = subwayPrefab.GetComponentsInChildren<Light>(true);
                    foreach (Light l in lights)
                    {
                        if (l.color.r > 0.8f && l.color.g < 0.3f) // Emergency red lamp
                        {
                            l.intensity = 1.35f;
                            l.range = 10f;
                        }
                        else // Main tunnel ceiling/wall light
                        {
                            l.intensity = 1.85f;
                            l.range = 18f;
                            l.color = new Color(0.65f, 0.78f, 0.95f, 1f);
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(subwayPrefab, subwayPrefabPath);
                    PrefabUtility.UnloadPrefabContents(subwayPrefab);
                    Debug.Log("  -> EnvironmentSegment_SubwayTunnel lights boosted to eliminate pitch black tunnel.");
                }
            }
        }

        public static void UpgradeCharacterPrefabs()
        {
            Debug.Log("[ZombieRunner] Upgrading Character Prefabs (Visibility & Rim Lighting)...");

            // 1. Player Prefab
            string playerPrefabPath = "Assets/ZombieRunner/Prefabs/Player/PlayerPlaceholder.prefab";
            GameObject playerPrefab = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            if (playerPrefab != null)
            {
                // Weapon tactical flashlight beam
                Transform weaponTransform = playerPrefab.transform.Find("Weapon Placeholder/Muzzle");
                if (weaponTransform == null) weaponTransform = playerPrefab.transform.Find("Weapon Placeholder");

                if (weaponTransform != null)
                {
                    Transform existingFlashlight = weaponTransform.Find("Tactical_Flashlight_Beam");
                    if (existingFlashlight == null)
                    {
                        GameObject flGo = new GameObject("Tactical_Flashlight_Beam");
                        flGo.transform.SetParent(weaponTransform, false);
                        flGo.transform.localPosition = Vector3.zero;
                        flGo.transform.localRotation = Quaternion.identity;

                        Light fl = flGo.AddComponent<Light>();
                        fl.type = LightType.Spot;
                        fl.range = 32f;
                        fl.spotAngle = 46f;
                        fl.innerSpotAngle = 26f;
                        fl.intensity = 2.2f;
                        fl.color = new Color(0.96f, 0.97f, 1.0f, 1.0f);
                        fl.shadows = LightShadows.None;
                        Debug.Log("  -> Player tactical weapon light beam added.");
                    }
                }

                // Subtle cool player rim light
                Transform existingPlayerRim = playerPrefab.transform.Find("Player_Cool_Rim_Light");
                if (existingPlayerRim == null)
                {
                    GameObject rimGo = new GameObject("Player_Cool_Rim_Light");
                    rimGo.transform.SetParent(playerPrefab.transform, false);
                    rimGo.transform.localPosition = new Vector3(0f, 1.7f, -0.65f);

                    Light rim = rimGo.AddComponent<Light>();
                    rim.type = LightType.Point;
                    rim.range = 2.5f;
                    rim.intensity = 1.3f;
                    rim.color = new Color(0.55f, 0.75f, 1.0f, 1.0f);
                    rim.shadows = LightShadows.None;
                    Debug.Log("  -> Player cool rim backlight added.");
                }

                PrefabUtility.SaveAsPrefabAsset(playerPrefab, playerPrefabPath);
                PrefabUtility.UnloadPrefabContents(playerPrefab);
            }

            // 2. Zombie Prefabs (Cool rim backlight for silhouettes)
            string[] zombiePrefabPaths = new[]
            {
                "Assets/ZombieRunner/Prefabs/Zombies/NormalZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/FastZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/BigZombie.prefab"
            };

            foreach (string zPath in zombiePrefabPaths)
            {
                GameObject zPrefab = PrefabUtility.LoadPrefabContents(zPath);
                if (zPrefab != null)
                {
                    Transform existingRim = zPrefab.transform.Find("Visual - Cool Rim Light");
                    if (existingRim == null)
                    {
                        GameObject rimGo = new GameObject("Visual - Cool Rim Light");
                        rimGo.transform.SetParent(zPrefab.transform, false);
                        rimGo.transform.localPosition = new Vector3(0f, 1.85f, 0.75f); // Placed above and behind head/shoulders

                        Light rim = rimGo.AddComponent<Light>();
                        rim.type = LightType.Point;
                        rim.range = 2.8f;
                        rim.intensity = 1.6f;
                        rim.color = new Color(0.55f, 0.75f, 1.0f, 1.0f);
                        rim.shadows = LightShadows.None;
                        Debug.Log($"  -> Zombie cool rim light added to {zPrefab.name}.");
                    }

                    PrefabUtility.SaveAsPrefabAsset(zPrefab, zPath);
                    PrefabUtility.UnloadPrefabContents(zPrefab);
                }
            }
        }

        public static void UpgradeGameplayScene()
        {
            Debug.Log("[ZombieRunner] Upgrading Gameplay Scene Settings and Objects...");

            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[ZombieRunner] Scene file upgrade deferred because Editor is currently in Play Mode.");
                return;
            }

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = (activeScene.IsValid() && activeScene.path == ScenePath)
                ? activeScene
                : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // 1. Camera UniversalAdditionalCameraData & Framing
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.12f, 0.17f, 0.25f, 1.0f);
                mainCam.farClipPlane = 220f;
                mainCam.fieldOfView = 58f;
                RunnerCameraFollow camFollow = mainCam.GetComponent<RunnerCameraFollow>();
                if (camFollow != null)
                {
                    SerializedObject camSo = new SerializedObject(camFollow);
                    camSo.FindProperty("followOffset").vector3Value = new Vector3(0f, 2.25f, -4.0f);
                    camSo.FindProperty("smoothTime").floatValue = 0.10f;
                    camSo.FindProperty("lookAheadDistance").floatValue = 12f;
                    camSo.FindProperty("lookHeight").floatValue = 1.25f;
                    camSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(camFollow);
                }

                UniversalAdditionalCameraData camData = mainCam.GetUniversalAdditionalCameraData();
                if (camData != null)
                {
                    camData.renderPostProcessing = true;
                    camData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    camData.dithering = true;
                    camData.requiresDepthTexture = true;
                    camData.requiresColorTexture = true;
                    EditorUtility.SetDirty(camData);
                    Debug.Log("  -> Main Camera configured: FOV=58, FarClip=220, ThirdPerson Offset=(0, 2.25, -4.0).");
                }
            }

            // 2. Global Directional Moonlight
            Light keyLight = null;
            foreach (Light light in Object.FindObjectsByType<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    keyLight = light;
                    break;
                }
            }

            if (keyLight != null)
            {
                keyLight.color = new Color(0.72f, 0.82f, 0.96f, 1.0f);
                keyLight.intensity = 1.65f;
                keyLight.shadows = LightShadows.Soft;
                keyLight.shadowStrength = 0.75f;
                keyLight.shadowBias = 0.04f;
                keyLight.shadowNormalBias = 0.30f;
                keyLight.transform.rotation = Quaternion.Euler(48f, 42f, 0f);
                EditorUtility.SetDirty(keyLight);
                Debug.Log("  -> Directional Key Light upgraded: Moonlit cool blue, Intensity=1.65f, Soft Shadows=0.75, Euler=(48, 42, 0).");
            }

            // 3. Global Post-Processing Volume in Scene
            Volume sceneVolume = Object.FindAnyObjectByType<Volume>();
            if (sceneVolume == null)
            {
                GameObject volGo = new GameObject("Global Post-Processing Volume");
                sceneVolume = volGo.AddComponent<Volume>();
                sceneVolume.isGlobal = true;
                sceneVolume.priority = 1;
                VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SampleProfilePath);
                if (profile != null) sceneVolume.sharedProfile = profile;
                Debug.Log("  -> Global Post-Processing Volume created in scene with SampleSceneProfile assigned.");
            }
            else
            {
                sceneVolume.isGlobal = true;
                if (sceneVolume.sharedProfile == null)
                {
                    sceneVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SampleProfilePath);
                }
                EditorUtility.SetDirty(sceneVolume);
                Debug.Log("  -> Existing Scene Volume confirmed and updated.");
            }

            // 4. Environment Theme Manager
            EnvironmentThemeManager themeMgr = Object.FindAnyObjectByType<EnvironmentThemeManager>();
            if (themeMgr != null)
            {
                SerializedObject so = new SerializedObject(themeMgr);
                SerializedProperty profilesProp = so.FindProperty("themeProfiles");
                if (profilesProp != null && profilesProp.arraySize >= 4)
                {
                    // Theme 0: CityRuins (Cinematic moonlit survival horror city, crisp cool-blue moonlight, rich ambient bounce)
                    SetProfileValues(profilesProp.GetArrayElementAtIndex(0),
                        new Color(0.08f, 0.11f, 0.16f), // fogColor
                        new Color(0.28f, 0.33f, 0.42f), // ambientColor
                        new Color(0.34f, 0.40f, 0.52f), // ambientSkyColor
                        new Color(0.28f, 0.33f, 0.42f), // ambientEquatorColor
                        new Color(0.22f, 0.25f, 0.32f), // ambientGroundColor
                        new Color(0.75f, 0.85f, 0.98f), // keyLightColor
                        1.75f, 42f, 165f);

                    // Theme 1: SubwayTunnel (Subterranean corridor with clear architectural depth, wet sheen and readable obstacles)
                    SetProfileValues(profilesProp.GetArrayElementAtIndex(1),
                        new Color(0.07f, 0.09f, 0.13f), // fogColor
                        new Color(0.24f, 0.28f, 0.35f), // ambientColor
                        new Color(0.30f, 0.36f, 0.45f), // ambientSkyColor
                        new Color(0.26f, 0.30f, 0.38f), // ambientEquatorColor
                        new Color(0.22f, 0.24f, 0.30f), // ambientGroundColor
                        new Color(0.68f, 0.78f, 0.92f), // keyLightColor
                        1.45f, 40f, 150f);

                    // Theme 2: MilitaryBase
                    SetProfileValues(profilesProp.GetArrayElementAtIndex(2),
                        new Color(0.09f, 0.12f, 0.11f), // fogColor
                        new Color(0.24f, 0.28f, 0.26f), // ambientColor
                        new Color(0.28f, 0.34f, 0.32f), // ambientSkyColor
                        new Color(0.24f, 0.28f, 0.26f), // ambientEquatorColor
                        new Color(0.20f, 0.22f, 0.21f), // ambientGroundColor
                        new Color(0.80f, 0.88f, 0.84f), // keyLightColor
                        1.65f, 48f, 180f);

                    // Theme 3: Factory
                    SetProfileValues(profilesProp.GetArrayElementAtIndex(3),
                        new Color(0.11f, 0.09f, 0.07f), // fogColor
                        new Color(0.25f, 0.21f, 0.18f), // ambientColor
                        new Color(0.30f, 0.26f, 0.22f), // ambientSkyColor
                        new Color(0.25f, 0.21f, 0.18f), // ambientEquatorColor
                        new Color(0.20f, 0.17f, 0.15f), // ambientGroundColor
                        new Color(0.96f, 0.74f, 0.50f), // keyLightColor
                        1.65f, 44f, 165f);

                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(themeMgr);
                    Debug.Log("  -> EnvironmentThemeManager serialized theme profiles updated successfully.");
                }
            }

            // 5. RenderSettings
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.40f, 0.52f, 1.0f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.33f, 0.42f, 1.0f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.25f, 0.32f, 1.0f);
            RenderSettings.ambientLight = RenderSettings.ambientEquatorColor;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.08f, 0.11f, 0.16f, 1.0f);
            RenderSettings.fogStartDistance = 42f;
            RenderSettings.fogEndDistance = 165f;

            // 6. Preview Road & Environment Segments for Scene View / Edit Mode
            EndlessRoadManager roadMgr = Object.FindAnyObjectByType<EndlessRoadManager>();
            if (roadMgr != null && roadMgr.transform.childCount == 0)
            {
                GameObject roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/RoadSegment_30m.prefab");
                if (roadPrefab != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject rSeg = (GameObject)PrefabUtility.InstantiatePrefab(roadPrefab, roadMgr.transform);
                        rSeg.transform.position = new Vector3(0f, 0f, i * 30f);
                        rSeg.name = $"Road Segment {(i + 1):D2} (Preview)";
                    }
                    Debug.Log("  -> Instantiated 4 preview road segments in scene.");
                }
            }

            if (themeMgr != null && themeMgr.transform.childCount == 0)
            {
                GameObject cityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab");
                if (cityPrefab != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject cSeg = (GameObject)PrefabUtility.InstantiatePrefab(cityPrefab, themeMgr.transform);
                        cSeg.transform.position = new Vector3(0f, 0f, i * 30f);
                        cSeg.name = $"Environment CityRuins Segment {(i + 1):D2} (Preview)";
                    }
                    Debug.Log("  -> Instantiated 4 preview city environment segments in scene.");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("  -> Scene RenderSettings saved with Trilight ambient mode and 20m-72m linear atmospheric fog.");
        }

        private static void SetProfileValues(SerializedProperty elem, Color fogColor, Color ambientColor,
            Color sky, Color equator, Color ground, Color keyColor, float keyIntensity, float fogStart, float fogEnd)
        {
            if (elem == null) return;
            SerializedProperty prop;
            if ((prop = elem.FindPropertyRelative("fogColor")) != null) prop.colorValue = fogColor;
            if ((prop = elem.FindPropertyRelative("ambientColor")) != null) prop.colorValue = ambientColor;
            if ((prop = elem.FindPropertyRelative("ambientSkyColor")) != null) prop.colorValue = sky;
            if ((prop = elem.FindPropertyRelative("ambientEquatorColor")) != null) prop.colorValue = equator;
            if ((prop = elem.FindPropertyRelative("ambientGroundColor")) != null) prop.colorValue = ground;
            if ((prop = elem.FindPropertyRelative("keyLightColor")) != null) prop.colorValue = keyColor;
            if ((prop = elem.FindPropertyRelative("keyLightIntensity")) != null) prop.floatValue = keyIntensity;
            if ((prop = elem.FindPropertyRelative("fogStart")) != null) prop.floatValue = fogStart;
            if ((prop = elem.FindPropertyRelative("fogEnd")) != null) prop.floatValue = fogEnd;
        }
    }
}
