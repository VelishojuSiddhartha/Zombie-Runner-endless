using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ZombieRunner.Audio;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.Weapons;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Automated studio-grade upgrader for Zombie Skin, Character 3-Point Lighting,
    /// Visible Bullet Tracers, Firearms VFX, and Atmospheric Horror Audio.
    /// </summary>
    public static class ZombieAndCombatUpgrader
    {
        private const string LogPrefix = "[ZombieCombatUpgrader] ";
        private const string ScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity";

        [MenuItem("ZombieRunner/Apply All Zombie, Combat & Audio Upgrades")]
        public static void UpgradeAll()
        {
            Debug.Log(LogPrefix + "Starting full upgrade pipeline...");

            // 1. Generate all studio-grade horror audio WAV assets
            HorrorAudioGenerator.GenerateAllAudio();
            AssetDatabase.Refresh();

            // 2. Upgrade all zombie skin materials with authentic textures and biological roughness
            UpgradeZombieSkinMaterials();

            // 3. Upgrade all zombie prefabs with 3-point character lighting & 3D sound emitters
            UpgradeZombiePrefabs();

            // 4. Upgrade Player Tactical Flashlight & Weapon VFX
            UpgradePlayerFlashlightAndWeapon();

            // 5. Update scene references, HitImpactManager, and audio routing
            UpgradeGameplayScene();

            AssetDatabase.SaveAssets();
            Debug.Log(LogPrefix + "All Zombie, Combat & Audio upgrades completed successfully!");
        }

        public static void UpgradeZombieSkinMaterials()
        {
            Debug.Log(LogPrefix + "Upgrading Zombie skin, clothing, and normal map materials...");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // Textures
            Texture ch10Diff1 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Ch10_1001_Diffuse.png")
                                ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Ch10_nonPBR.fbm/Ch10_1001_Diffuse.png");
            Texture ch10Norm1 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Ch10_1001_Normal.png")
                                ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Ch10_nonPBR.fbm/Ch10_1001_Normal.png");

            Texture ch10Diff2 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Ch10_1002_Diffuse.png")
                                ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Ch10_nonPBR.fbm/Ch10_1002_Diffuse.png");
            Texture ch10Norm2 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Ch10_1002_Normal.png")
                                ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Ch10_nonPBR.fbm/Ch10_1002_Normal.png");

            Texture hulkDiff = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/pumpkinHulk_diffuse.png")
                               ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Pumpkinhulk L Shaw.fbm/pumpkinHulk_diffuse.png");
            Texture hulkNorm = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/pumpkinHulk_normal.png")
                               ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Pumpkinhulk L Shaw.fbm/pumpkinHulk_normal.png");

            Texture yakuDiff = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Yakuzombie_diffuse.png")
                               ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Yakuzombie_.png")
                               ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Yaku J Ignite.fbm/Yakuzombie_diffuse.png");
            Texture yakuNorm = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Textures/Yakuzombie_normal.png")
                               ?? AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZombieRunner/Characters/Zombie/Yaku J Ignite.fbm/Yakuzombie_normal.png");

            // 1. Ch10_body.mat (Normal / Common Zombie Skin & Head)
            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_body.mat",
                litShader,
                ch10Diff1,
                ch10Norm1,
                baseColor: new Color(1.12f, 1.10f, 1.08f, 1f),
                smoothness: 0.28f,
                bumpScale: 1.20f
            );

            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_1001_Diffuse.mat",
                litShader,
                ch10Diff1,
                ch10Norm1,
                baseColor: new Color(1.12f, 1.10f, 1.08f, 1f),
                smoothness: 0.28f,
                bumpScale: 1.20f
            );

            // 2. Ch10_body1.mat (Normal / Common Zombie Clothing)
            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_body1.mat",
                litShader,
                ch10Diff2,
                ch10Norm2,
                baseColor: new Color(1.08f, 1.08f, 1.08f, 1f),
                smoothness: 0.25f,
                bumpScale: 1.15f
            );

            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_1002_Diffuse.mat",
                litShader,
                ch10Diff2,
                ch10Norm2,
                baseColor: new Color(1.08f, 1.08f, 1.08f, 1f),
                smoothness: 0.25f,
                bumpScale: 1.15f
            );

            // 3. PumpkinHulk_Material.mat (Heavy / Big Zombie Skin & Texture)
            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/PumpkinHulk_Material.mat",
                litShader,
                hulkDiff,
                hulkNorm,
                baseColor: new Color(1.15f, 1.12f, 1.10f, 1f),
                smoothness: 0.30f,
                bumpScale: 1.25f
            );

            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/pumpkinHulk_diffuse.mat",
                litShader,
                hulkDiff,
                hulkNorm,
                baseColor: new Color(1.15f, 1.12f, 1.10f, 1f),
                smoothness: 0.30f,
                bumpScale: 1.25f
            );

            // 4. yaku_zombie_material.mat (Fast Zombie Skin & Face)
            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/yaku_zombie_material.mat",
                litShader,
                yakuDiff,
                yakuNorm,
                baseColor: new Color(1.12f, 1.10f, 1.08f, 1f),
                smoothness: 0.28f,
                bumpScale: 1.20f
            );

            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Yakuzombie_.mat",
                litShader,
                yakuDiff,
                yakuNorm,
                baseColor: new Color(1.12f, 1.10f, 1.08f, 1f),
                smoothness: 0.28f,
                bumpScale: 1.20f
            );

            // 5. yifu.mat (Fast Zombie Clothes)
            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/yifu.mat",
                litShader,
                yakuDiff,
                yakuNorm,
                baseColor: new Color(1.08f, 1.08f, 1.08f, 1f),
                smoothness: 0.25f,
                bumpScale: 1.15f
            );

            ConfigureSkinMaterial(
                "Assets/ZombieRunner/Characters/Zombie/Materials/Yakuzombie_diffuse.mat",
                litShader,
                yakuDiff,
                yakuNorm,
                baseColor: new Color(1.08f, 1.08f, 1.08f, 1f),
                smoothness: 0.25f,
                bumpScale: 1.15f
            );

            // Also update WR_Zombie*.mat fallback assets
            ConfigureSkinMaterial("Assets/ZombieRunner/Materials/WR_ZombieNormal.mat", litShader, ch10Diff1, ch10Norm1, new Color(1.12f, 1.10f, 1.08f, 1f), 0.28f, 1.20f);
            ConfigureSkinMaterial("Assets/ZombieRunner/Materials/WR_ZombieBig.mat", litShader, hulkDiff, hulkNorm, new Color(1.15f, 1.12f, 1.10f, 1f), 0.30f, 1.25f);
            ConfigureSkinMaterial("Assets/ZombieRunner/Materials/WR_ZombieFast.mat", litShader, yakuDiff, yakuNorm, new Color(1.12f, 1.10f, 1.08f, 1f), 0.28f, 1.20f);

            // Environment materials upgrade
            Material subwayConc = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_SubwayConcrete.mat");
            if (subwayConc != null)
            {
                subwayConc.SetColor("_BaseColor", new Color(0.40f, 0.43f, 0.46f, 1f));
                subwayConc.SetFloat("_Smoothness", 0.25f);
                EditorUtility.SetDirty(subwayConc);
            }

            Material subwayTile = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_SubwayTile.mat");
            if (subwayTile != null)
            {
                subwayTile.SetColor("_BaseColor", new Color(0.46f, 0.50f, 0.52f, 1f));
                subwayTile.SetFloat("_Smoothness", 0.35f);
                EditorUtility.SetDirty(subwayTile);
            }

            Material asphaltMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ZombieRunner/Materials/WR_Asphalt.mat");
            if (asphaltMat != null)
            {
                asphaltMat.SetColor("_BaseColor", new Color(0.48f, 0.50f, 0.54f, 1f));
                asphaltMat.SetFloat("_Smoothness", 0.32f);
                EditorUtility.SetDirty(asphaltMat);
            }
        }

        private static void ConfigureSkinMaterial(string path, Shader shader, Texture diffuse, Texture normal, Color baseColor, float smoothness, float bumpScale)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            if (shader != null && mat.shader != shader)
                mat.shader = shader;

            mat.SetFloat("_WorkflowMode", 1.0f); // Metallic Lit
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Smoothness", smoothness);

            if (diffuse != null)
            {
                mat.SetTexture("_BaseMap", diffuse);
                mat.SetTexture("_MainTex", diffuse);
            }

            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", bumpScale);
                mat.EnableKeyword("_NORMALMAP");
            }
            else
            {
                mat.DisableKeyword("_NORMALMAP");
            }

            mat.DisableKeyword("_SPECULAR_SETUP");
            EditorUtility.SetDirty(mat);
            Debug.Log(LogPrefix + $"Configured material '{Path.GetFileName(path)}': Diffuse={(diffuse != null ? diffuse.name : "None")}, Normal={(normal != null ? normal.name : "None")}, Smoothness={smoothness:F2}");
        }

        public static void UpgradeZombiePrefabs()
        {
            Debug.Log(LogPrefix + "Upgrading Zombie Prefabs with 3-Point Lighting and Sound Emitters...");

            // Load audio clips
            AudioClip[] nGroans = new[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Normal_Groan_01.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Normal_Groan_02.wav")
            };
            AudioClip nAttack = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Normal_Attack_01.wav");
            AudioClip nHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Normal_Hurt_01.wav");
            AudioClip nDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Normal_Death_01.wav");

            AudioClip[] fHisses = new[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Fast_Hiss_01.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Fast_Hiss_02.wav")
            };
            AudioClip fAttack = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Fast_Attack_01.wav");
            AudioClip fHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Fast_Hurt_01.wav");
            AudioClip fDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Fast_Death_01.wav");

            AudioClip[] hRoars = new[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Heavy_Roar_01.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Heavy_Roar_02.wav")
            };
            AudioClip hAttack = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Heavy_Attack_01.wav");
            AudioClip hHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Heavy_Hurt_01.wav");
            AudioClip hDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Zombie_Heavy_Death_01.wav");

            string[] prefabPaths = new[]
            {
                "Assets/ZombieRunner/Prefabs/Zombies/NormalZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/CommonZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/FastZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/BigZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/HeavyZombie.prefab"
            };

            foreach (string pPath in prefabPaths)
            {
                if (!File.Exists(pPath)) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(pPath);
                if (root == null) continue;

                ZombieController zc = root.GetComponent<ZombieController>();
                if (zc == null) zc = root.AddComponent<ZombieController>();

                // Configure 3D Sound Emitter
                ZombieSoundEmitter emitter = root.GetComponent<ZombieSoundEmitter>();
                if (emitter == null) emitter = root.AddComponent<ZombieSoundEmitter>();
                emitter.AssignClips(nGroans, nAttack, nHurt, nDeath, fHisses, fAttack, fHurt, fDeath, hRoars, hAttack, hHurt, hDeath);

                bool isBig = pPath.Contains("Big") || pPath.Contains("Heavy");
                bool isFast = pPath.Contains("Fast");

                // Configure 3-Point Character Lighting (Humanoid local forward is +Z towards player, back is -Z)
                // 1. Front Key Light (Warm-white illumination on face, chest, skin, clothing)
                Vector3 keyPos = isBig ? new Vector3(0.35f, 1.85f, 1.85f) : (isFast ? new Vector3(0.18f, 1.30f, 1.30f) : new Vector3(0.20f, 1.45f, 1.35f));
                Color keyCol = isBig ? new Color(1.0f, 0.96f, 0.92f) : new Color(0.98f, 0.95f, 0.90f);
                float keyInt = isBig ? 1.85f : 1.45f;
                float keyRange = isBig ? 4.8f : (isFast ? 3.6f : 3.8f);
                SetupChildLight(root.transform, "Visual - Front Key Light", keyPos, keyCol, keyInt, keyRange);

                // 2. Soft Fill Light (Neutral fill softening shadows on dark side and clothes)
                Vector3 fillPos = isBig ? new Vector3(-0.65f, 1.45f, 1.50f) : (isFast ? new Vector3(-0.40f, 1.05f, 1.05f) : new Vector3(-0.45f, 1.15f, 1.10f));
                Color fillCol = new Color(0.88f, 0.90f, 0.95f);
                float fillInt = isBig ? 1.05f : 0.85f;
                float fillRange = isBig ? 4.0f : 3.2f;
                SetupChildLight(root.transform, "Visual - Soft Fill Light", fillPos, fillCol, fillInt, fillRange);

                // 3. Cool Rim Light (Atmospheric cool moonlight edge backlight on shoulders/head)
                Vector3 rimPos = isBig ? new Vector3(0f, 2.40f, -1.50f) : (isFast ? new Vector3(0f, 1.65f, -1.15f) : new Vector3(0f, 1.85f, -1.25f));
                Color rimCol = new Color(0.55f, 0.75f, 1.0f);
                float rimInt = isBig ? 2.20f : 1.85f;
                float rimRange = isBig ? 4.5f : 3.5f;
                SetupChildLight(root.transform, "Visual - Cool Rim Light", rimPos, rimCol, rimInt, rimRange);

                PrefabUtility.SaveAsPrefabAsset(root, pPath);
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log(LogPrefix + $"Upgraded prefab with 3-Point Character Lighting & Audio: {Path.GetFileName(pPath)}");
            }
        }

        private static void SetupChildLight(Transform parent, string childName, Vector3 localPosition, Color color, float intensity, float range)
        {
            Transform existing = parent.Find(childName);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(childName);
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
            }

            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;

            Light light = go.GetComponent<Light>();
            if (light == null) light = go.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
        }

        public static void UpgradePlayerFlashlightAndWeapon()
        {
            Debug.Log(LogPrefix + "Upgrading Player Tactical Weapon Flashlight & Muzzle VFX...");

            string playerPrefabPath = "Assets/ZombieRunner/Prefabs/Player/PlayerPlaceholder.prefab";
            GameObject playerRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            if (playerRoot == null) return;

            Transform muzzle = playerRoot.transform.Find("Weapon Placeholder/Muzzle");
            if (muzzle == null) muzzle = playerRoot.transform.Find("Muzzle");

            // Upgrade Tactical Flashlight Beam (0m to 48m reach)
            Transform flashTrans = muzzle != null ? muzzle.Find("Tactical_Flashlight_Beam") : playerRoot.transform.Find("Tactical_Flashlight_Beam");
            if (flashTrans != null)
            {
                Light fLight = flashTrans.GetComponent<Light>();
                if (fLight != null)
                {
                    fLight.type = LightType.Spot;
                    fLight.range = 48f;
                    fLight.spotAngle = 54f;
                    fLight.innerSpotAngle = 28f;
                    fLight.intensity = 3.0f;
                    fLight.color = new Color(0.98f, 0.98f, 1.0f);
                    fLight.shadows = LightShadows.Soft;
                }
            }

            // Add Tactical Flashlight Wide Ambient Fill (covers adjacent lanes)
            if (muzzle != null)
            {
                Transform wideTrans = muzzle.Find("Tactical_Flashlight_Wide_Fill");
                GameObject wideGo;
                if (wideTrans == null)
                {
                    wideGo = new GameObject("Tactical_Flashlight_Wide_Fill");
                    wideGo.transform.SetParent(muzzle, false);
                }
                else
                {
                    wideGo = wideTrans.gameObject;
                }

                wideGo.transform.localPosition = Vector3.zero;
                wideGo.transform.localRotation = Quaternion.identity;

                Light wLight = wideGo.GetComponent<Light>();
                if (wLight == null) wLight = wideGo.AddComponent<Light>();

                wLight.type = LightType.Spot;
                wLight.range = 40f;
                wLight.spotAngle = 78f;
                wLight.innerSpotAngle = 42f;
                wLight.intensity = 1.1f;
                wLight.color = new Color(0.90f, 0.92f, 0.98f);
                wLight.shadows = LightShadows.None;
            }

            // Ensure ShootingController references are pristine
            ShootingController shooter = playerRoot.GetComponent<ShootingController>();
            if (shooter != null)
            {
                SerializedObject shootSO = new SerializedObject(shooter);
                shootSO.FindProperty("damagePerShot").floatValue = 1f;
                shootSO.FindProperty("shotsPerSecond").floatValue = 4f;
                shootSO.FindProperty("maximumRange").floatValue = 90f;
                shootSO.FindProperty("aimRange").floatValue = 48f;
                shootSO.FindProperty("autoAimEnabled").boolValue = true;
                shootSO.FindProperty("kickbackDistance").floatValue = 0.07f;
                shootSO.FindProperty("kickbackAngle").floatValue = 3.5f;
                shootSO.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);
            Debug.Log(LogPrefix + "Player Tactical Flashlight & Weapon VFX successfully upgraded!");
        }

        public static void UpgradeGameplayScene()
        {
            Debug.Log(LogPrefix + "Upgrading ZombieRunnerGameplay scene...");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            // 1. Ensure HitImpactManager exists in scene
            HitImpactManager impactMgr = UnityEngine.Object.FindAnyObjectByType<HitImpactManager>();
            if (impactMgr == null)
            {
                GameObject mgrGo = new GameObject("HitImpactManager");
                impactMgr = mgrGo.AddComponent<HitImpactManager>();
                Debug.Log(LogPrefix + "Created HitImpactManager in scene.");
            }

            // 2. Wire AudioManager with gunshot clips and ambient wind loop
            AudioManager audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
            if (audioMgr != null)
            {
                AudioClip[] shots = new[]
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Gunshot_Punchy_01.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Gunshot_Punchy_02.wav"),
                    AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/SFX_Gunshot_Punchy_03.wav")
                };
                AudioClip windLoop = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ZombieRunner/Audio/Clips/Ambience_Night_City_Wind_Loop.wav");
                audioMgr.AssignHorrorClips(shots, windLoop);
                Debug.Log(LogPrefix + "Wired gunshot and ambient audio clips to AudioManager.");
            }

            // 3. Ensure EnvironmentThemeManager uses the upgraded profiles
            EnvironmentThemeManager themeMgr = UnityEngine.Object.FindAnyObjectByType<EnvironmentThemeManager>();
            if (themeMgr != null)
            {
                SerializedObject themeSO = new SerializedObject(themeMgr);
                SerializedProperty profilesProp = themeSO.FindProperty("themeProfiles");
                if (profilesProp != null && profilesProp.arraySize >= 4)
                {
                    // Theme 0: CityRuins
                    SetThemeProfileProp(profilesProp.GetArrayElementAtIndex(0),
                        new Color(0.08f, 0.11f, 0.16f),
                        new Color(0.34f, 0.40f, 0.52f),
                        new Color(0.28f, 0.33f, 0.42f),
                        new Color(0.22f, 0.25f, 0.32f),
                        new Color(0.75f, 0.85f, 0.98f),
                        1.75f, 42f, 165f);

                    // Theme 1: SubwayTunnel
                    SetThemeProfileProp(profilesProp.GetArrayElementAtIndex(1),
                        new Color(0.07f, 0.09f, 0.13f),
                        new Color(0.30f, 0.36f, 0.45f),
                        new Color(0.26f, 0.30f, 0.38f),
                        new Color(0.22f, 0.24f, 0.30f),
                        new Color(0.68f, 0.78f, 0.92f),
                        1.45f, 40f, 150f);

                    // Theme 2: MilitaryBase
                    SetThemeProfileProp(profilesProp.GetArrayElementAtIndex(2),
                        new Color(0.09f, 0.12f, 0.11f),
                        new Color(0.28f, 0.34f, 0.32f),
                        new Color(0.24f, 0.28f, 0.26f),
                        new Color(0.20f, 0.22f, 0.21f),
                        new Color(0.80f, 0.88f, 0.84f),
                        1.65f, 48f, 180f);

                    // Theme 3: Factory
                    SetThemeProfileProp(profilesProp.GetArrayElementAtIndex(3),
                        new Color(0.11f, 0.09f, 0.07f),
                        new Color(0.30f, 0.26f, 0.22f),
                        new Color(0.25f, 0.21f, 0.18f),
                        new Color(0.20f, 0.17f, 0.15f),
                        new Color(0.96f, 0.74f, 0.50f),
                        1.65f, 44f, 165f);

                    themeSO.ApplyModifiedProperties();
                }
            }

            // RenderSettings: Rich Trilight ambient and realistic atmospheric fog
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

            // 4. Update scene player instance if present
            GameObject scenePlayer = GameObject.Find("Player - Capsule Placeholder");
            if (scenePlayer != null)
            {
                Transform muzzle = scenePlayer.transform.Find("Weapon Placeholder/Muzzle") ?? scenePlayer.transform.Find("Muzzle");
                if (muzzle != null)
                {
                    Transform flash = muzzle.Find("Tactical_Flashlight_Beam");
                    if (flash != null)
                    {
                        Light l = flash.GetComponent<Light>();
                        if (l != null)
                        {
                            l.range = 48f;
                            l.spotAngle = 54f;
                            l.innerSpotAngle = 28f;
                            l.intensity = 3.0f;
                        }
                    }

                    Transform wide = muzzle.Find("Tactical_Flashlight_Wide_Fill");
                    if (wide == null)
                    {
                        GameObject wideGo = new GameObject("Tactical_Flashlight_Wide_Fill");
                        wideGo.transform.SetParent(muzzle, false);
                        Light wl = wideGo.AddComponent<Light>();
                        wl.type = LightType.Spot;
                        wl.range = 40f;
                        wl.spotAngle = 78f;
                        wl.innerSpotAngle = 42f;
                        wl.intensity = 1.1f;
                        wl.color = new Color(0.90f, 0.92f, 0.98f);
                    }
                }
            }

            // 5. Ensure Preview Zombies exist down the road for instant visual readability
            GameObject normPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Zombies/NormalZombie.prefab");
            GameObject fastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Zombies/FastZombie.prefab");
            GameObject bigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Zombies/BigZombie.prefab");

            if (GameObject.Find("Preview_NormalZombie") == null && normPrefab != null)
            {
                GameObject nz = (GameObject)PrefabUtility.InstantiatePrefab(normPrefab);
                nz.name = "Preview_NormalZombie";
                nz.transform.position = new Vector3(0f, 0f, 16f);
                nz.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (GameObject.Find("Preview_FastZombie") == null && fastPrefab != null)
            {
                GameObject fz = (GameObject)PrefabUtility.InstantiatePrefab(fastPrefab);
                fz.name = "Preview_FastZombie";
                fz.transform.position = new Vector3(-3f, 0f, 28f);
                fz.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (GameObject.Find("Preview_BigZombie") == null && bigPrefab != null)
            {
                GameObject bz = (GameObject)PrefabUtility.InstantiatePrefab(bigPrefab);
                bz.name = "Preview_BigZombie";
                bz.transform.position = new Vector3(3f, 0f, 40f);
                bz.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            // Update preview zombies currently in scene with latest 3-point lighting setup
            UpdatePreviewZombieInScene("Preview_NormalZombie", false, false);
            UpdatePreviewZombieInScene("Preview_FastZombie", false, true);
            UpdatePreviewZombieInScene("Preview_BigZombie", true, false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(LogPrefix + "ZombieRunnerGameplay scene saved with all combat, tracer, and lighting upgrades.");
        }

        private static void UpdatePreviewZombieInScene(string name, bool isBig, bool isFast)
        {
            GameObject zGo = GameObject.Find(name);
            if (zGo == null) return;

            Vector3 keyPos = isBig ? new Vector3(0.35f, 1.85f, 1.85f) : (isFast ? new Vector3(0.18f, 1.30f, 1.30f) : new Vector3(0.20f, 1.45f, 1.35f));
            Color keyCol = isBig ? new Color(1.0f, 0.96f, 0.92f) : new Color(0.98f, 0.95f, 0.90f);
            float keyInt = isBig ? 1.85f : 1.45f;
            float keyRange = isBig ? 4.8f : (isFast ? 3.6f : 3.8f);
            SetupChildLight(zGo.transform, "Visual - Front Key Light", keyPos, keyCol, keyInt, keyRange);

            Vector3 fillPos = isBig ? new Vector3(-0.65f, 1.45f, 1.50f) : (isFast ? new Vector3(-0.40f, 1.05f, 1.05f) : new Vector3(-0.45f, 1.15f, 1.10f));
            Color fillCol = new Color(0.88f, 0.90f, 0.95f);
            float fillInt = isBig ? 1.05f : 0.85f;
            float fillRange = isBig ? 4.0f : 3.2f;
            SetupChildLight(zGo.transform, "Visual - Soft Fill Light", fillPos, fillCol, fillInt, fillRange);

            Vector3 rimPos = isBig ? new Vector3(0f, 2.40f, -1.50f) : (isFast ? new Vector3(0f, 1.65f, -1.15f) : new Vector3(0f, 1.85f, -1.25f));
            Color rimCol = new Color(0.55f, 0.75f, 1.0f);
            float rimInt = isBig ? 2.20f : 1.85f;
            float rimRange = isBig ? 4.5f : 3.5f;
            SetupChildLight(zGo.transform, "Visual - Cool Rim Light", rimPos, rimCol, rimInt, rimRange);
        }

        private static void SetThemeProfileProp(SerializedProperty p, Color fogCol, Color skyCol, Color eqCol, Color gndCol, Color keyCol, float keyInt, float fogStart, float fogEnd)
        {
            if (p == null) return;
            SerializedProperty prop;
            if ((prop = p.FindPropertyRelative("fogColor")) != null) prop.colorValue = fogCol;
            if ((prop = p.FindPropertyRelative("ambientSkyColor")) != null) prop.colorValue = skyCol;
            if ((prop = p.FindPropertyRelative("ambientEquatorColor")) != null) prop.colorValue = eqCol;
            if ((prop = p.FindPropertyRelative("ambientGroundColor")) != null) prop.colorValue = gndCol;
            if ((prop = p.FindPropertyRelative("keyLightColor")) != null) prop.colorValue = keyCol;
            if ((prop = p.FindPropertyRelative("keyLightIntensity")) != null) prop.floatValue = keyInt;
            if ((prop = p.FindPropertyRelative("fogStart")) != null) prop.floatValue = fogStart;
            if ((prop = p.FindPropertyRelative("fogEnd")) != null) prop.floatValue = fogEnd;
        }
    }
}
