using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieRunner.Collectibles;
using ZombieRunner.Enemies;
using ZombieRunner.Player;
using ZombieRunner.PowerUps;
using ZombieRunner.Weapons;
using ZombieRunner.World;

/// <summary>
/// Safe, repeatable project setup and placeholder scene tools for Zombie Runner.
/// Generated scenes live beside the original template scene and can be regenerated
/// only when they still contain this tool's recognizable root object.
/// </summary>
public static class ZombieRunnerProjectSetup
{
    private const string Root = "Assets/ZombieRunner";
    private const string GameplayScenePath = Root + "/Scenes/ZombieRunnerGameplay.unity";
    private const string MainMenuScenePath = Root + "/Scenes/ZombieRunnerMainMenu.unity";
    private const string RoadSegmentPrefabPath = Root + "/Prefabs/Environment/RoadSegment_30m.prefab";
    private const string NormalZombiePrefabPath = Root + "/Prefabs/Zombies/NormalZombie.prefab";
    private const string FastZombiePrefabPath = Root + "/Prefabs/Zombies/FastZombie.prefab";
    private const string BigZombiePrefabPath = Root + "/Prefabs/Zombies/BigZombie.prefab";
    private const string RoadblockPrefabPath = Root + "/Prefabs/Obstacles/Roadblock.prefab";
    private const string AbandonedCarPrefabPath = Root + "/Prefabs/Obstacles/AbandonedCar.prefab";
    private const string ExplosiveBarrelPrefabPath = Root + "/Prefabs/Obstacles/ExplosiveBarrel.prefab";
    private const string CoinPrefabPath = Root + "/Prefabs/Collectibles/GoldCoin.prefab";
    private const string AmmoPrefabPath = Root + "/Prefabs/Collectibles/AmmoBox.prefab";
    private const string HealthKitPrefabPath = Root + "/Prefabs/Collectibles/HealthKit.prefab";
    private const string PowerUpPrefabPath = Root + "/Prefabs/Collectibles/PowerUpPickup.prefab";
    private const string CityEnvironmentPrefabPath = Root + "/Prefabs/Environment/EnvironmentSegment_CityRuins.prefab";
    private const string SubwayEnvironmentPrefabPath = Root + "/Prefabs/Environment/EnvironmentSegment_SubwayTunnel.prefab";
    private const string MilitaryEnvironmentPrefabPath = Root + "/Prefabs/Environment/EnvironmentSegment_MilitaryBase.prefab";
    private const string FactoryEnvironmentPrefabPath = Root + "/Prefabs/Environment/EnvironmentSegment_Factory.prefab";
    private const string PlayerAnimatorControllerPath = Root + "/Animations/Player_Animator.controller";
    private const string ZombieAnimatorControllerPath = Root + "/Animations/Zombie_Animator.controller";
    private const string GameplayRootName = "Zombie Runner Generated - Gameplay";
    private const string MainMenuRootName = "Zombie Runner Generated - Main Menu";
    private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string NewCharacterModelPath = Root + "/Characters/Player/NewCharacter/Ch05_nonPBR.fbx";
    private const string NewCharacterRunAnimPath = Root + "/Characters/Player/NewCharacter/Animations/rifle run.fbx";
    private const string NewCharacterIdleAnimPath = Root + "/Characters/Player/NewCharacter/Animations/rifle aiming idle.fbx";
    private const string NewCharacterJumpAnimPath = Root + "/Characters/Player/NewCharacter/Animations/rifle jump.fbx";
    private const string NewCharacterSlideAnimPath = Root + "/Characters/Player/NewCharacter/Animations/Running Slide.fbx";
    private const string BackupCharacterSlideAnimPath = Root + "/Characters/Player/NewCharacter/Animations/rifle slide.fbx";
    private const string NewCharacterShootAnimPath = Root + "/Characters/Player/NewCharacter/Animations/Shoot Rifle.fbx";
    private const string BasicShooterPackZipPath = Root + "/Characters/Player/Basic Shooter Pack (1).zip";
    private const string MixamoCharacterPath = Root + "/Characters/Player/X Bot.fbx";
    private const string MixamoFallbackPath = Root + "/Models/Player/X Bot.fbx";
    private const string PlayerPrefabPath = Root + "/Prefabs/Player/PlayerPlaceholder.prefab";

    private static readonly string[] RequiredFolders =
    {
        "Scenes", "Editor",
        "Scripts/Core", "Scripts/Player", "Scripts/Enemies", "Scripts/World",
        "Scripts/Weapons", "Scripts/Collectibles", "Scripts/PowerUps", "Scripts/UI",
        "Scripts/Audio", "Scripts/Utilities",
        "Prefabs/Player", "Prefabs/Zombies", "Prefabs/Environment", "Prefabs/Obstacles",
        "Prefabs/Collectibles", "Prefabs/UI",
        "Models/Player", "Models/Zombies", "Models/Weapons", "Models/Environment",
        "Characters/Player", "Characters/Player/NewCharacter", "Characters/Player/NewCharacter/Animations",
        "Materials", "Animations", "Audio", "Settings"
    };

    [MenuItem("Tools/Zombie Runner/Setup Project")]
    public static void SetupProject()
    {
        EnsureFolders();
        CreatePlaceholderMaterials();
        CreateAnimationControllers();
        CreatePlayerPrefab();
        CreateRoadSegmentPrefab();
        CreateZombiePrefabs();
        CreateObstaclePrefabs();
        CreateCollectiblePrefabs();
        CreatePowerUpPrefab();
        CreateEnvironmentThemePrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Zombie Runner project folders, starter materials, and player placeholder are ready.");
    }

    [MenuItem("Tools/Zombie Runner/Integrate Player Model")]
    [MenuItem("Tools/Zombie Runner/Integrate Mixamo Player Model")]
    public static void IntegratePlayerModel()
    {
        bool useNewCharacter = System.IO.File.Exists(NewCharacterModelPath);
        string modelPath = useNewCharacter
            ? NewCharacterModelPath
            : (System.IO.File.Exists(MixamoCharacterPath) ? MixamoCharacterPath : MixamoFallbackPath);

        if (!System.IO.File.Exists(modelPath))
        {
            Debug.LogError("Could not find character model at " + modelPath);
            return;
        }

        // Materials setup
        Material matHead = null;
        Material matUpper = null;
        Material matLower = null;
        Material surfaceMat = null;
        Material jointsMat = null;

        if (useNewCharacter)
        {
            string newMatDir = Root + "/Characters/Player/NewCharacter/Materials";
            matHead = AssetDatabase.LoadAssetAtPath<Material>(newMatDir + "/Ch05_body.mat");
            matUpper = AssetDatabase.LoadAssetAtPath<Material>(newMatDir + "/Ch05_body1.mat");
            matLower = AssetDatabase.LoadAssetAtPath<Material>(newMatDir + "/Ch05_body2.mat");
        }
        else
        {
            string materialsDir = Root + "/Characters/Player/Materials";
            if (!AssetDatabase.IsValidFolder(materialsDir))
                AssetDatabase.CreateFolder(Root + "/Characters/Player", "Materials");

            surfaceMat = AssetDatabase.LoadAssetAtPath<Material>(materialsDir + "/Beta_HighLimbsGeoSG3.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_PlayerXBot_Surface.mat")
                ?? CreatePlayerModelMaterial("WR_PlayerXBot_Surface", new Color(0.86f, 0.88f, 0.91f), 0.2f, 0.6f);
            jointsMat = AssetDatabase.LoadAssetAtPath<Material>(materialsDir + "/Beta_Joints_MAT1.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_PlayerXBot_Joints.mat")
                ?? CreatePlayerModelMaterial("WR_PlayerXBot_Joints", new Color(0.12f, 0.13f, 0.15f), 0.7f, 0.65f);
        }

        // Configure ModelImporter for Humanoid Rig and URP Lit Material Remapping
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                modified = true;
            }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
                modified = true;
            }

            if (useNewCharacter)
            {
                if (matHead != null) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Ch05_body"), matHead);
                if (matUpper != null) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Ch05_body1"), matUpper);
                if (matLower != null) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Ch05_body2"), matLower);
            }
            else
            {
                var surfaceId = new AssetImporter.SourceAssetIdentifier(typeof(Material), "Beta_HighLimbsGeoSG3");
                var jointsId = new AssetImporter.SourceAssetIdentifier(typeof(Material), "Beta_Joints_MAT1");
                importer.AddRemap(surfaceId, surfaceMat);
                importer.AddRemap(jointsId, jointsMat);
            }
            modified = true;

            if (modified)
            {
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
            }
        }

        // Load Player Prefab
        GameObject player = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (player == null)
        {
            Debug.LogError("Could not load Player prefab at " + PlayerPrefabPath);
            return;
        }

        try
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = player.AddComponent<CharacterController>();
                ConfigureCharacterController(cc);
            }

            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller == null)
                controller = player.AddComponent<PlayerController>();

            if (player.GetComponent<PlayerHealth>() == null)
                player.AddComponent<PlayerHealth>();

            ShootingController shooting = player.GetComponent<ShootingController>();
            if (shooting == null)
                shooting = player.AddComponent<ShootingController>();
            EnsurePlayerWeapon(player);

            // Hide placeholder visual capsule
            Transform oldVisual = player.transform.Find("Visual - Placeholder Capsule");
            if (oldVisual != null)
                oldVisual.gameObject.SetActive(false);

            // Handle visual children
            string activeVisualName = useNewCharacter ? "Visual - New Character" : "Visual - Mixamo X Bot";
            Transform xbotVisual = player.transform.Find("Visual - Mixamo X Bot");
            if (useNewCharacter && xbotVisual != null)
            {
                // Keep X Bot GameObject in hierarchy as backup, but disable it
                xbotVisual.gameObject.SetActive(false);
            }

            Transform existingTargetVisual = player.transform.Find(activeVisualName);
            if (existingTargetVisual != null)
                UnityEngine.Object.DestroyImmediate(existingTargetVisual.gameObject);

            // Instantiate FBX model as child
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelPrefab == null)
            {
                Debug.LogError("Could not load model asset at " + modelPath);
                return;
            }

            GameObject visualObj = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            visualObj.name = activeVisualName;
            visualObj.transform.SetParent(player.transform, false);

            // Position feet at -1.0f (bottom of 2.0m CharacterController centered at 0), facing +Z
            visualObj.transform.localPosition = new Vector3(0f, -1.0f, 0f);
            visualObj.transform.localRotation = Quaternion.identity;
            visualObj.transform.localScale = Vector3.one;

            // Assign URP Lit materials to SkinnedMeshRenderers and record prefab modifications
            SkinnedMeshRenderer[] renderers = visualObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                if (useNewCharacter)
                {
                    if (renderers.Length == 1)
                    {
                        Material[] mats = new Material[Math.Max(3, smr.sharedMaterials.Length)];
                        mats[0] = matHead;
                        mats[1] = matUpper;
                        mats[2] = matLower;
                        smr.sharedMaterials = mats;
                    }
                    else
                    {
                        if (smr.name.Contains("body1") && matUpper != null)
                            smr.sharedMaterial = matUpper;
                        else if (smr.name.Contains("body2") && matLower != null)
                            smr.sharedMaterial = matLower;
                        else if (matHead != null)
                            smr.sharedMaterial = matHead;
                    }
                }
                else
                {
                    if (smr.name.Contains("Surface") || smr.name.Contains("HighLimbs"))
                        smr.sharedMaterial = surfaceMat;
                    else if (smr.name.Contains("Joints"))
                        smr.sharedMaterial = jointsMat;
                    else
                        smr.sharedMaterial = surfaceMat;
                }
                EditorUtility.SetDirty(smr);
                PrefabUtility.RecordPrefabInstancePropertyModifications(smr);
            }

            // Configure Animator
            Animator animator = visualObj.GetComponent<Animator>();
            if (animator == null)
                animator = visualObj.AddComponent<Animator>();

            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            animator.applyRootMotion = false;

            // Assign humanoid avatar
            Avatar avatar = null;
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            foreach (UnityEngine.Object subAsset in subAssets)
            {
                if (subAsset is Avatar av)
                {
                    avatar = av;
                    break;
                }
            }
            if (avatar != null)
                animator.avatar = avatar;

            // Wire references
            ConfigurePlayerReferences(controller, cc, visualObj.transform);

            // Wire weapon / shooting
            Transform weapon = player.transform.Find("Weapon Placeholder");
            Transform muzzle = weapon != null ? weapon.Find("Muzzle") : null;
            Transform flash = muzzle != null ? muzzle.Find("Muzzle Flash Placeholder") : null;
            SerializedObject shootingSerialized = new SerializedObject(shooting);
            if (shootingSerialized.FindProperty("muzzle") != null)
                shootingSerialized.FindProperty("muzzle").objectReferenceValue = muzzle;
            if (shootingSerialized.FindProperty("muzzleFlashVisual") != null)
                shootingSerialized.FindProperty("muzzleFlashVisual").objectReferenceValue = flash != null ? flash.gameObject : null;
            shootingSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Debug.Log("Successfully integrated character (" + activeVisualName + ") into PlayerPlaceholder prefab!");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(player);
        }

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.IsValid() && currentScene.path == GameplayScenePath)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetupPlayerRunAnimation();
    }

    [MenuItem("Tools/Zombie Runner/Setup Player Animations")]
    [MenuItem("Tools/Zombie Runner/Setup Player Jump Animation")]
    [MenuItem("Tools/Zombie Runner/Setup Player Slide Animation")]
    [MenuItem("Tools/Zombie Runner/Setup Player Shoot Animation")]
    [MenuItem("Tools/Zombie Runner/Setup Player Run Animation")]
    public static void SetupPlayerRunAnimation()
    {
        string animDir = Root + "/Characters/Player/NewCharacter/Animations";
        if (!AssetDatabase.IsValidFolder(animDir))
        {
            if (!AssetDatabase.IsValidFolder(Root + "/Characters/Player/NewCharacter"))
                AssetDatabase.CreateFolder(Root + "/Characters/Player", "NewCharacter");
            AssetDatabase.CreateFolder(Root + "/Characters/Player/NewCharacter", "Animations");
        }

        ExtractAnimationIfMissing(NewCharacterRunAnimPath, "rifle run.fbx");
        ExtractAnimationIfMissing(NewCharacterIdleAnimPath, "rifle aiming idle.fbx");
        ExtractAnimationIfMissing(NewCharacterJumpAnimPath, "rifle jump.fbx");
        string activeSlideAnimPath = System.IO.File.Exists(NewCharacterSlideAnimPath)
            ? NewCharacterSlideAnimPath
            : BackupCharacterSlideAnimPath;
        if (!System.IO.File.Exists(activeSlideAnimPath))
            ExtractAnimationIfMissing(BackupCharacterSlideAnimPath, "toss grenade.fbx");
        activeSlideAnimPath = System.IO.File.Exists(NewCharacterSlideAnimPath)
            ? NewCharacterSlideAnimPath
            : BackupCharacterSlideAnimPath;
        AssetDatabase.Refresh();

        Avatar characterAvatar = null;
        if (System.IO.File.Exists(NewCharacterModelPath))
        {
            foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(NewCharacterModelPath))
            {
                if (obj is Avatar av)
                {
                    characterAvatar = av;
                    break;
                }
            }
        }

        ConfigureAnimationModelImporter(NewCharacterRunAnimPath, "Run", characterAvatar, true);
        ConfigureAnimationModelImporter(NewCharacterIdleAnimPath, "Idle", characterAvatar, true);
        ConfigureAnimationModelImporter(NewCharacterJumpAnimPath, "Jump", characterAvatar, false);
        string slideClipPreferredName = activeSlideAnimPath == NewCharacterSlideAnimPath ? "Running Slide" : "Slide";
        ConfigureAnimationModelImporter(activeSlideAnimPath, slideClipPreferredName, characterAvatar, false);
        if (System.IO.File.Exists(NewCharacterShootAnimPath))
        {
            ConfigureAnimationModelImporter(NewCharacterShootAnimPath, "Shoot Rifle", characterAvatar, false);
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimatorControllerPath);
        if (controller != null && controller.layers.Length > 0)
        {
            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            AnimationClip runClip = LoadAnimationClip(NewCharacterRunAnimPath, "Run");
            AnimationClip idleClip = LoadAnimationClip(NewCharacterIdleAnimPath, "Idle");
            AnimationClip jumpClip = LoadAnimationClip(NewCharacterJumpAnimPath, "Jump");
            AnimationClip slideClip = LoadAnimationClip(activeSlideAnimPath, slideClipPreferredName)
                ?? LoadAnimationClip(activeSlideAnimPath, "Running Slide")
                ?? LoadAnimationClip(activeSlideAnimPath, "Slide");
            AnimationClip shootClip = System.IO.File.Exists(NewCharacterShootAnimPath)
                ? (LoadAnimationClip(NewCharacterShootAnimPath, "Shoot Rifle") ?? LoadAnimationClip(NewCharacterShootAnimPath, "Shoot"))
                : null;

            AnimatorState runState = null;
            AnimatorState idleState = null;
            AnimatorState deadState = null;
            AnimatorState jumpState = null;
            AnimatorState slideState = null;
            AnimatorState shootState = null;

            foreach (ChildAnimatorState child in sm.states)
            {
                if (child.state.name == "Run") runState = child.state;
                else if (child.state.name == "Idle") idleState = child.state;
                else if (child.state.name == "Dead") deadState = child.state;
                else if (child.state.name == "Jump") jumpState = child.state;
                else if (child.state.name == "Slide") slideState = child.state;
                else if (child.state.name == "Shoot") shootState = child.state;
            }

            if (runState == null)
            {
                runState = sm.AddState("Run");
                runState.name = "Run";
            }
            if (idleState == null)
            {
                idleState = sm.AddState("Idle");
                idleState.name = "Idle";
            }
            if (jumpState == null)
            {
                jumpState = sm.AddState("Jump");
                jumpState.name = "Jump";
            }
            if (slideState == null)
            {
                slideState = sm.AddState("Slide");
                slideState.name = "Slide";
            }
            if (shootState == null)
            {
                shootState = sm.AddState("Shoot");
                shootState.name = "Shoot";
            }

            if (runClip != null)
                runState.motion = runClip;
            if (idleClip != null)
                idleState.motion = idleClip;
            if (jumpClip != null)
                jumpState.motion = jumpClip;
            if (slideClip != null)
            {
                slideState.motion = slideClip;
                slideState.speed = 1.95f;
            }
            if (shootClip != null)
            {
                shootState.motion = shootClip;
                shootState.speed = 1.5f;
            }

            EnsureAnimatorParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureAnimatorParameter(controller, "Grounded", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "Sliding", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "Dead", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "Jump", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorParameter(controller, "Slide", AnimatorControllerParameterType.Trigger);
            EnsureAnimatorParameter(controller, "Fire", AnimatorControllerParameterType.Trigger);

            bool hasIdleToRun = false;
            foreach (AnimatorStateTransition trans in idleState.transitions)
            {
                if (trans.destinationState == runState)
                {
                    hasIdleToRun = true;
                    break;
                }
            }
            if (!hasIdleToRun)
            {
                AnimatorStateTransition t = idleState.AddTransition(runState);
                t.hasExitTime = false;
                t.duration = 0.1f;
                t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            }

            bool hasRunToIdle = false;
            foreach (AnimatorStateTransition trans in runState.transitions)
            {
                if (trans.destinationState == idleState)
                {
                    hasRunToIdle = true;
                    break;
                }
            }
            if (!hasRunToIdle)
            {
                AnimatorStateTransition t = runState.AddTransition(idleState);
                t.hasExitTime = false;
                t.duration = 0.15f;
                t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            }

            bool hasAnyToJump = false;
            foreach (AnimatorStateTransition trans in sm.anyStateTransitions)
            {
                if (trans.destinationState == jumpState)
                {
                    hasAnyToJump = true;
                    break;
                }
            }
            if (!hasAnyToJump)
            {
                AnimatorStateTransition t = sm.AddAnyStateTransition(jumpState);
                t.hasExitTime = false;
                t.duration = 0.12f;
                t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            }

            bool hasRunToJump = false;
            foreach (AnimatorStateTransition trans in runState.transitions)
            {
                if (trans.destinationState == jumpState)
                {
                    hasRunToJump = true;
                    break;
                }
            }
            if (!hasRunToJump)
            {
                AnimatorStateTransition t = runState.AddTransition(jumpState);
                t.hasExitTime = false;
                t.duration = 0.1f;
                t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            }

            bool hasJumpToRun = false;
            foreach (AnimatorStateTransition trans in jumpState.transitions)
            {
                if (trans.destinationState == runState)
                {
                    hasJumpToRun = true;
                    trans.hasExitTime = false;
                    trans.duration = 0.12f;
                    trans.canTransitionToSelf = false;
                    break;
                }
            }
            if (!hasJumpToRun)
            {
                AnimatorStateTransition t = jumpState.AddTransition(runState);
                t.hasExitTime = false;
                t.duration = 0.12f;
                t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            }

            if (slideState != null)
            {
                bool hasAnyToSlide = false;
                foreach (AnimatorStateTransition trans in sm.anyStateTransitions)
                {
                    if (trans.destinationState == slideState)
                    {
                        hasAnyToSlide = true;
                        bool hasGrounded = trans.conditions.Any(c => c.parameter == "Grounded");
                        if (!hasGrounded)
                            trans.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
                        break;
                    }
                }
                if (!hasAnyToSlide)
                {
                    AnimatorStateTransition t = sm.AddAnyStateTransition(slideState);
                    t.hasExitTime = false;
                    t.duration = 0.12f;
                    t.canTransitionToSelf = false;
                    t.AddCondition(AnimatorConditionMode.If, 0f, "Sliding");
                    t.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
                }

                bool hasSlideToRun = false;
                foreach (AnimatorStateTransition trans in slideState.transitions)
                {
                    if (trans.destinationState == runState)
                    {
                        hasSlideToRun = true;
                        trans.hasExitTime = false;
                        trans.duration = 0.12f;
                        trans.canTransitionToSelf = false;
                        break;
                    }
                }
                if (!hasSlideToRun)
                {
                    AnimatorStateTransition t = slideState.AddTransition(runState);
                    t.hasExitTime = false;
                    t.duration = 0.12f;
                    t.canTransitionToSelf = false;
                    t.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
                }
            }

            if (shootState != null)
            {
                bool hasAnyToShoot = false;
                foreach (AnimatorStateTransition trans in sm.anyStateTransitions)
                {
                    if (trans.destinationState == shootState)
                    {
                        hasAnyToShoot = true;
                        trans.duration = 0.08f;
                        trans.canTransitionToSelf = true;
                        break;
                    }
                }
                if (!hasAnyToShoot)
                {
                    AnimatorStateTransition t = sm.AddAnyStateTransition(shootState);
                    t.hasExitTime = false;
                    t.duration = 0.08f;
                    t.canTransitionToSelf = true;
                    t.AddCondition(AnimatorConditionMode.If, 0f, "Fire");
                }

                bool hasShootToRun = false;
                foreach (AnimatorStateTransition trans in shootState.transitions)
                {
                    if (trans.destinationState == runState)
                    {
                        hasShootToRun = true;
                        trans.hasExitTime = true;
                        trans.exitTime = 0.85f;
                        trans.duration = 0.12f;
                        break;
                    }
                }
                if (!hasShootToRun)
                {
                    AnimatorStateTransition t = shootState.AddTransition(runState);
                    t.hasExitTime = true;
                    t.exitTime = 0.85f;
                    t.duration = 0.12f;
                    t.canTransitionToSelf = true;
                }
            }

            if (deadState != null)
            {
                bool hasDeadToIdle = false;
                foreach (AnimatorStateTransition trans in deadState.transitions)
                {
                    if (trans.destinationState == idleState || trans.destinationState == runState)
                    {
                        hasDeadToIdle = true;
                        break;
                    }
                }
                if (!hasDeadToIdle)
                {
                    AnimatorStateTransition t = deadState.AddTransition(idleState);
                    t.hasExitTime = false;
                    t.duration = 0.15f;
                    t.canTransitionToSelf = false;
                    t.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
                }
            }

            sm.defaultState = runState;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("Zombie Runner: Successfully configured Player Animator Controller with looping Run, Idle, and Jump states.");
        }

        GameObject player = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (player != null)
        {
            try
            {
                Transform capsule = player.transform.Find("Visual - Placeholder Capsule");
                if (capsule != null)
                {
                    Animator capsuleAnim = capsule.GetComponent<Animator>();
                    if (capsuleAnim != null)
                        UnityEngine.Object.DestroyImmediate(capsuleAnim, true);
                }

                Transform visualNewChar = player.transform.Find("Visual - New Character");
                if (visualNewChar != null)
                {
                    Animator anim = visualNewChar.GetComponent<Animator>();
                    if (anim == null)
                        anim = visualNewChar.gameObject.AddComponent<Animator>();

                    anim.applyRootMotion = false;
                    if (characterAvatar != null)
                        anim.avatar = characterAvatar;
                    anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
                    EditorUtility.SetDirty(anim);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(anim);

                    PlayerController pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        SerializedObject so = new SerializedObject(pc);
                        SerializedProperty animProp = so.FindProperty("animator");
                        if (animProp != null)
                            animProp.objectReferenceValue = anim;
                        SerializedProperty visProp = so.FindProperty("visualRoot");
                        if (visProp != null)
                            visProp.objectReferenceValue = visualNewChar;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
                Debug.Log("Zombie Runner: Verified Player prefab Animator with applyRootMotion = false and Humanoid Avatar.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }
        }
    }

    private static void ExtractAnimationIfMissing(string targetPath, string entryName)
    {
        if (System.IO.File.Exists(targetPath))
            return;
        if (!System.IO.File.Exists(BasicShooterPackZipPath))
            return;

        using (System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(BasicShooterPackZipPath))
        {
            System.IO.Compression.ZipArchiveEntry entry = archive.GetEntry(entryName);
            if (entry != null)
            {
                string dir = System.IO.Path.GetDirectoryName(targetPath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                using (System.IO.Stream inStream = entry.Open())
                using (System.IO.FileStream outStream = System.IO.File.Create(targetPath))
                {
                    inStream.CopyTo(outStream);
                }
            }
        }
    }

    private static void ConfigureAnimationModelImporter(string animPath, string clipName, Avatar avatar, bool loop = true)
    {
        if (!System.IO.File.Exists(animPath))
            return;

        ModelImporter importer = AssetImporter.GetAtPath(animPath) as ModelImporter;
        if (importer == null)
            return;

        bool changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            if (avatar != null)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = avatar;
            }
            else
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }
            changed = true;
        }

        if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            changed = true;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        if (clips != null && clips.Length > 0)
        {
            bool clipNeedsUpdate = false;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                if (clip.name != clipName) { clip.name = clipName; clipNeedsUpdate = true; }
                if (clip.loopTime != loop) { clip.loopTime = loop; clipNeedsUpdate = true; }
                if (clip.loopPose != loop) { clip.loopPose = loop; clipNeedsUpdate = true; }
                if (!clip.lockRootRotation) { clip.lockRootRotation = true; clipNeedsUpdate = true; }
                if (!clip.lockRootHeightY) { clip.lockRootHeightY = true; clipNeedsUpdate = true; }
                if (!clip.lockRootPositionXZ) { clip.lockRootPositionXZ = true; clipNeedsUpdate = true; }
                if (!clip.keepOriginalOrientation) { clip.keepOriginalOrientation = true; clipNeedsUpdate = true; }
                if (!clip.keepOriginalPositionY) { clip.keepOriginalPositionY = true; clipNeedsUpdate = true; }
                if (clipName == "Running Slide")
                {
                    if (clip.firstFrame != 0) { clip.firstFrame = 0; clipNeedsUpdate = true; }
                    if (clip.lastFrame != 46) { clip.lastFrame = 46; clipNeedsUpdate = true; }
                }
                else if (clipName == "Shoot Rifle" || clipName == "Shoot")
                {
                    if (clip.firstFrame != 0) { clip.firstFrame = 0; clipNeedsUpdate = true; }
                    if (clip.lastFrame != 28) { clip.lastFrame = 28; clipNeedsUpdate = true; }
                }
                else if (clipName == "Slide")
                {
                    if (clip.firstFrame != 26) { clip.firstFrame = 26; clipNeedsUpdate = true; }
                    if (clip.lastFrame != 56) { clip.lastFrame = 56; clipNeedsUpdate = true; }
                }
                else if (clip.lastFrame <= clip.firstFrame)
                {
                    if (importer.defaultClipAnimations != null && importer.defaultClipAnimations.Length > 0 && importer.defaultClipAnimations[0].lastFrame > 0)
                        clip.lastFrame = importer.defaultClipAnimations[0].lastFrame;
                    else
                        clip.lastFrame = clipName == "Run" ? 22 : (clipName == "Jump" ? 18 : 93);
                    clipNeedsUpdate = true;
                }
            }
            if (clipNeedsUpdate)
            {
                importer.clipAnimations = clips;
                changed = true;
            }
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip LoadAnimationClip(string path, string preferredName)
    {
        if (!System.IO.File.Exists(path))
            return null;

        AnimationClip fallback = null;
        foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                if (clip.name.Equals(preferredName, StringComparison.OrdinalIgnoreCase))
                    return clip;
                if (fallback == null)
                    fallback = clip;
            }
        }
        return fallback;
    }

    private static void EnsureAnimatorParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (controller.parameters.Any(p => p.name == name))
            return;
        controller.AddParameter(name, type);
    }

    [InitializeOnLoadMethod]
    private static void OnProjectLoaded()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            string targetModelPath = System.IO.File.Exists(NewCharacterModelPath)
                ? NewCharacterModelPath
                : (System.IO.File.Exists(MixamoCharacterPath) ? MixamoCharacterPath : MixamoFallbackPath);

            if (!System.IO.File.Exists(targetModelPath))
                return;

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab != null)
            {
                bool hasNewChar = System.IO.File.Exists(NewCharacterModelPath);
                string expectedVisualName = hasNewChar ? "Visual - New Character" : "Visual - Mixamo X Bot";
                Transform activeVisual = playerPrefab.transform.Find(expectedVisualName);
                SkinnedMeshRenderer[] smrs = playerPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool needsFix = activeVisual == null
                    || !activeVisual.gameObject.activeSelf
                    || smrs.Length == 0
                    || smrs.Any(s => s.sharedMaterial == null || s.sharedMaterial.shader == null || !s.sharedMaterial.shader.name.Contains("Universal"));
                if (needsFix)
                {
                    Debug.Log("Zombie Runner: Auto-configuring player character (" + expectedVisualName + ")...");
                    IntegratePlayerModel();
                }
                else if (System.IO.File.Exists(NewCharacterRunAnimPath))
                {
                    AnimatorController pController = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimatorControllerPath);
                    bool animNeedsSetup = false;
                    if (pController != null && pController.layers.Length > 0)
                    {
                        AnimatorStateMachine sm = pController.layers[0].stateMachine;
                        AnimatorState rState = sm.states.FirstOrDefault(s => s.state.name == "Run").state;
                        AnimatorState jState = sm.states.FirstOrDefault(s => s.state.name == "Jump").state;
                        AnimatorState sState = sm.states.FirstOrDefault(s => s.state.name == "Slide").state;
                        AnimatorState shState = sm.states.FirstOrDefault(s => s.state.name == "Shoot").state;
                        if (rState == null || rState.motion == null
                            || (System.IO.File.Exists(NewCharacterJumpAnimPath) && (jState == null || jState.motion == null))
                            || (System.IO.File.Exists(NewCharacterSlideAnimPath) && (sState == null || sState.motion == null))
                            || (System.IO.File.Exists(NewCharacterShootAnimPath) && (shState == null || shState.motion == null)))
                            animNeedsSetup = true;
                    }
                    if (animNeedsSetup)
                    {
                        Debug.Log("Zombie Runner: Auto-configuring player animations...");
                        SetupPlayerRunAnimation();
                    }
                }
            }
        };
    }

    [MenuItem("Tools/Zombie Runner/Create Gameplay Scene")]
    public static void CreateGameplayScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Exit Play Mode before running Zombie Runner scene setup.");
            return;
        }

        SetupProject();
        Scene scene;
        bool sceneExists = System.IO.File.Exists(GameplayScenePath);
        if (sceneExists)
        {
            scene = SceneManager.GetActiveScene();
            if (scene.path != GameplayScenePath)
                scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            if (!scene.GetRootGameObjects().Any(item => item.name == GameplayRootName))
            {
                Debug.LogError("Refusing to modify an existing scene that is not marked as a Zombie Runner generated scene: " + GameplayScenePath);
                return;
            }
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject(GameplayRootName);
            CreateGameplayEnvironment(root.transform);
            CreatePlayer(root.transform);
            CreateCamera(root.transform);
            CreateLighting(root.transform);
        }

        EnsureGameplayComponents(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, GameplayScenePath))
        {
            Debug.LogError("Zombie Runner could not save the gameplay scene at " + GameplayScenePath);
            return;
        }

        AddSceneToBuildSettings(GameplayScenePath);
        ReorderBuildScenes();
        AssetDatabase.Refresh();
        Debug.Log((sceneExists ? "Updated" : "Created") + " the Zombie Runner gameplay foundation at " + GameplayScenePath);
    }

    [MenuItem("Tools/Zombie Runner/Create Main Menu")]
    public static void CreateMainMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Exit Play Mode before running Zombie Runner scene setup.");
            return;
        }

        SetupProject();
        if (!CanRegenerateScene(MainMenuScenePath, MainMenuRootName))
            return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject(MainMenuRootName);
        CreateMenuCanvas(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, MainMenuScenePath))
        {
            Debug.LogError("Zombie Runner could not save the main menu scene at " + MainMenuScenePath);
            return;
        }

        AddSceneToBuildSettings(MainMenuScenePath);
        ReorderBuildScenes();
        AssetDatabase.Refresh();
        Debug.Log("Created the Zombie Runner main menu foundation at " + MainMenuScenePath);
    }

    [MenuItem("Tools/Zombie Runner/Validate Project")]
    public static void ValidateProject()
    {
        var issues = new List<string>();
        foreach (string folder in RequiredFolders)
        {
            string path = Root + "/" + folder;
            if (!AssetDatabase.IsValidFolder(path))
                issues.Add("Missing folder: " + path);
        }

        string graphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";
        string graphicsSettings = System.IO.File.Exists(graphicsSettingsPath)
            ? System.IO.File.ReadAllText(graphicsSettingsPath)
            : string.Empty;
        string urpAssetPath = "Assets/Settings/PC_RPAsset.asset";
        string urpAssetGuid = AssetDatabase.AssetPathToGUID(urpAssetPath);
        if (string.IsNullOrEmpty(urpAssetGuid) || !graphicsSettings.Contains("guid: " + urpAssetGuid))
            issues.Add("The URP asset is missing or is not assigned in Graphics settings.");

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) == null)
            issues.Add("Gameplay scene has not been created: " + GameplayScenePath);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) == null)
            issues.Add("Main menu scene has not been created: " + MainMenuScenePath);
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath) == null)
            issues.Add("TMP essentials are missing. Import the TMP Essential Resources from the installed uGUI package.");
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimatorControllerPath) == null ||
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ZombieAnimatorControllerPath) == null)
            issues.Add("One or more starter Animator Controllers are missing.");
        if (AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Asphalt.mat") == null)
            issues.Add("Starter asphalt material is missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Player/PlayerPlaceholder.prefab") == null)
            issues.Add("Player placeholder prefab is missing.");
        string characterCheckPath = System.IO.File.Exists(NewCharacterModelPath)
            ? NewCharacterModelPath
            : (System.IO.File.Exists(MixamoCharacterPath) ? MixamoCharacterPath : MixamoFallbackPath);
        if (!System.IO.File.Exists(characterCheckPath))
            issues.Add("Player character model FBX is missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RoadSegmentPrefabPath) == null)
            issues.Add("Road segment prefab is missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(NormalZombiePrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(FastZombiePrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(BigZombiePrefabPath) == null)
            issues.Add("One or more zombie archetype prefabs are missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RoadblockPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(AbandonedCarPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(ExplosiveBarrelPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(CoinPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(AmmoPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(HealthKitPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(PowerUpPrefabPath) == null)
            issues.Add("One or more obstacle or collectible prefabs are missing.");
        if (AssetDatabase.LoadAssetAtPath<GameObject>(CityEnvironmentPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(SubwayEnvironmentPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(MilitaryEnvironmentPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(FactoryEnvironmentPrefabPath) == null)
            issues.Add("One or more environment theme segment prefabs are missing.");

        if (issues.Count == 0)
        {
            Debug.Log("Zombie Runner validation passed: project structure, URP, generated scenes, TMP resources, Animator Controllers, materials, road/zombie/obstacle/collectible prefabs, and all four environment segments are present.");
        }
        else
        {
            foreach (string issue in issues)
                Debug.LogError("Zombie Runner validation: " + issue);
            Debug.LogError("Zombie Runner validation found " + issues.Count + " issue(s).");
        }
    }

    [MenuItem("Tools/Zombie Runner/Prepare Android Build")]
    public static void PrepareAndroidBuild()
    {
        PlayerSettings.companyName = "Zombie Runner";
        PlayerSettings.productName = "Zombie Runner";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.example.zombierunner");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.renderOutsideSafeArea = true;
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        PlayerSettings.Android.requestedVisibleInsets = AndroidWindowInsetsType.None;
        PlayerSettings.Android.resizeableActivity = false;
        AssetDatabase.SaveAssets();

        bool androidSupport = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);
        Debug.Log("Zombie Runner Android settings configured. Android build support is " + (androidSupport ? "installed." : "not installed; install the Android Build Support module for Unity 6000.5.9f1."));
    }

    // Used for deterministic batch-mode setup and verification.
    public static void BuildStage1()
    {
        SetupProject();
        CreateGameplayScene();
    }

    private static void EnsureFolders()
    {
        EnsureAssetFolder(Root);
        foreach (string folder in RequiredFolders)
            EnsureAssetFolder(Root + "/" + folder);
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureAssetFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void CreatePlaceholderMaterials()
    {
        CreateMaterial("WR_Asphalt", new Color(0.075f, 0.085f, 0.095f));
        CreateMaterial("WR_LanePaint", new Color(0.78f, 0.68f, 0.39f), true);
        CreateMaterial("WR_Concrete", new Color(0.25f, 0.27f, 0.27f));
        CreateMaterial("WR_Ruins", new Color(0.18f, 0.19f, 0.19f));
        CreateMaterial("WR_Metal", new Color(0.12f, 0.15f, 0.16f));
        CreateMaterial("WR_HazardOrange", new Color(0.95f, 0.24f, 0.055f), true);
        CreateMaterial("WR_PlayerPlaceholder", new Color(0.19f, 0.45f, 0.46f));
        CreateMaterial("WR_ZombieNormal", new Color(0.32f, 0.38f, 0.25f));
        CreateMaterial("WR_ZombieFast", new Color(0.42f, 0.34f, 0.2f));
        CreateMaterial("WR_ZombieBig", new Color(0.28f, 0.19f, 0.17f));
        CreateMaterial("WR_WeaponMetal", new Color(0.13f, 0.16f, 0.17f));
        CreateMaterial("WR_MuzzleFlash", new Color(1f, 0.38f, 0.04f), true);
        CreateMaterial("WR_CoinGold", new Color(1f, 0.62f, 0.08f), true);
        CreateMaterial("WR_AmmoBox", new Color(0.24f, 0.29f, 0.27f));
        CreateMaterial("WR_HealthKit", new Color(0.82f, 0.84f, 0.78f));
        CreateMaterial("WR_HealthAccent", new Color(0.82f, 0.1f, 0.08f));
        CreateMaterial("WR_BarrelOrange", new Color(0.72f, 0.2f, 0.07f));
        CreateMaterial("WR_CarPaint", new Color(0.16f, 0.2f, 0.2f));
        CreateMaterial("WR_Tire", new Color(0.035f, 0.04f, 0.04f));
        CreateMaterial("WR_PowerUp", new Color(0.18f, 0.72f, 0.94f), true);
        CreateMaterial("WR_SubwayTile", new Color(0.2f, 0.27f, 0.29f));
        CreateMaterial("WR_SubwayConcrete", new Color(0.12f, 0.15f, 0.16f));
        CreateMaterial("WR_TunnelLamp", new Color(0.94f, 0.63f, 0.3f), true);
        CreateMaterial("WR_MilitaryGreen", new Color(0.17f, 0.22f, 0.16f));
        CreateMaterial("WR_MilitaryCanvas", new Color(0.3f, 0.33f, 0.24f));
        CreateMaterial("WR_ContainerBlue", new Color(0.12f, 0.2f, 0.23f));
        CreateMaterial("WR_FactoryMetal", new Color(0.18f, 0.2f, 0.21f));
        CreateMaterial("WR_FactoryWarning", new Color(0.85f, 0.32f, 0.06f), true);
        CreateMaterial("WR_Pipe", new Color(0.31f, 0.34f, 0.32f));
    }

    private static Material CreateMaterial(string name, Color color, bool emissive = false)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
        {
            Debug.LogError("Could not find a supported shader to create " + name);
            return null;
        }

        material = new Material(shader) { name = name };
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        if (emissive && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.35f);
        }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Material CreatePlayerModelMaterial(string name, Color baseColor, float metallic, float smoothness)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit shader to create " + name);
            return null;
        }

        material = new Material(shader) { name = name };
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", baseColor);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", baseColor);

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", smoothness);

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreateAnimationControllers()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimatorControllerPath) == null)
        {
            AnimatorController playerController = AnimatorController.CreateAnimatorControllerAtPath(PlayerAnimatorControllerPath);
            playerController.AddParameter("Speed", AnimatorControllerParameterType.Float);
            playerController.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            playerController.AddParameter("Sliding", AnimatorControllerParameterType.Bool);
            playerController.AddParameter("Dead", AnimatorControllerParameterType.Bool);
            playerController.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            playerController.AddParameter("Slide", AnimatorControllerParameterType.Trigger);
            playerController.AddParameter("Fire", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = playerController.layers[0].stateMachine;
            AnimatorState run = machine.states.Length > 0 ? machine.states[0].state : machine.AddState("Run");
            run.name = "Run";
            run.motion = null;
            machine.defaultState = run;
            AnimatorState jump = machine.AddState("Jump");
            AnimatorState slide = machine.AddState("Slide");
            AnimatorState shoot = machine.AddState("Shoot");
            AnimatorState dead = machine.AddState("Dead");
            AddAnyStateTransition(machine, jump, "Jump", AnimatorConditionMode.If);
            AddAnyStateTransition(machine, slide, "Sliding", AnimatorConditionMode.If);
            AddAnyStateTransition(machine, shoot, "Fire", AnimatorConditionMode.If);
            AddAnyStateTransition(machine, dead, "Dead", AnimatorConditionMode.If);
            AnimatorStateTransition jumpReturn = jump.AddTransition(run);
            jumpReturn.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpReturn.duration = 0.12f;
            AnimatorStateTransition slideReturn = slide.AddTransition(run);
            slideReturn.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            slideReturn.duration = 0.12f;
            AddExitTransition(shoot, run);
            EditorUtility.SetDirty(playerController);
        }

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ZombieAnimatorControllerPath) == null)
        {
            AnimatorController zombieController = AnimatorController.CreateAnimatorControllerAtPath(ZombieAnimatorControllerPath);
            zombieController.AddParameter("Speed", AnimatorControllerParameterType.Float);
            zombieController.AddParameter("Attacking", AnimatorControllerParameterType.Bool);
            zombieController.AddParameter("Dead", AnimatorControllerParameterType.Bool);
            zombieController.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = zombieController.layers[0].stateMachine;
            AnimatorState locomotion = machine.states.Length > 0 ? machine.states[0].state : machine.AddState("Locomotion");
            locomotion.name = "Locomotion";
            locomotion.motion = null;
            machine.defaultState = locomotion;
            AnimatorState attack = machine.AddState("Attack");
            AnimatorState dead = machine.AddState("Dead");
            AddAnyStateTransition(machine, attack, "Attack", AnimatorConditionMode.If);
            AddAnyStateTransition(machine, dead, "Dead", AnimatorConditionMode.If);
            AddExitTransition(attack, locomotion);
            EditorUtility.SetDirty(zombieController);
        }

        AssetDatabase.SaveAssets();
    }

    private static void AddAnyStateTransition(AnimatorStateMachine machine, AnimatorState destination,
        string parameter, AnimatorConditionMode condition)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(destination);
        transition.hasExitTime = false;
        transition.duration = 0.12f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(condition, 0f, parameter);
    }

    private static void AddExitTransition(AnimatorState source, AnimatorState destination)
    {
        AnimatorStateTransition transition = source.AddTransition(destination);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.12f;
    }

    private static void CreatePlayerPrefab()
    {
        const string path = Root + "/Prefabs/Player/PlayerPlaceholder.prefab";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_PlayerPlaceholder.mat");
        bool prefabAlreadyExists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        GameObject player = prefabAlreadyExists
            ? PrefabUtility.LoadPrefabContents(path)
            : new GameObject("PlayerPlaceholder");

        player.name = "PlayerPlaceholder";
        MeshFilter oldMesh = player.GetComponent<MeshFilter>();
        MeshRenderer oldRenderer = player.GetComponent<MeshRenderer>();
        Transform visual = player.transform.Find("Visual - New Character");
        if (visual == null)
            visual = player.transform.Find("Visual - Mixamo X Bot");
        if (visual == null)
            visual = player.transform.Find("Visual - Placeholder Capsule");
        if (visual == null)
        {
            Mesh mesh = oldMesh != null ? oldMesh.sharedMesh : null;
            Material[] materials = oldRenderer != null ? oldRenderer.sharedMaterials : null;
            GameObject sourceCapsule = null;
            if (mesh == null)
            {
                sourceCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mesh = sourceCapsule.GetComponent<MeshFilter>().sharedMesh;
                if ((materials == null || materials.Length == 0) && material != null)
                    materials = new[] { material };
            }
            GameObject visualObject = new GameObject("Visual - Placeholder Capsule", typeof(MeshFilter), typeof(MeshRenderer));
            visualObject.transform.SetParent(player.transform, false);
            visual = visualObject.transform;
            visualObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer visualRenderer = visualObject.GetComponent<MeshRenderer>();
            if (materials != null && materials.Length > 0)
                visualRenderer.sharedMaterials = materials;
            else if (material != null)
                visualRenderer.sharedMaterial = material;

            if (sourceCapsule != null)
                UnityEngine.Object.DestroyImmediate(sourceCapsule);
            if (oldMesh != null) UnityEngine.Object.DestroyImmediate(oldMesh);
            if (oldRenderer != null) UnityEngine.Object.DestroyImmediate(oldRenderer);
        }

        CapsuleCollider oldCollider = player.GetComponent<CapsuleCollider>();
        if (oldCollider != null)
            UnityEngine.Object.DestroyImmediate(oldCollider);
        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
            UnityEngine.Object.DestroyImmediate(visualCollider);

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
            characterController = player.AddComponent<CharacterController>();
        ConfigureCharacterController(characterController);

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null)
            controller = player.AddComponent<PlayerController>();
        Animator animator = visual.GetComponent<Animator>();
        if (animator == null)
            animator = visual.gameObject.AddComponent<Animator>();
        if (animator.runtimeAnimatorController == null)
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
        animator.applyRootMotion = false;
        ConfigurePlayerReferences(controller, characterController, visual);

        if (player.GetComponent<PlayerHealth>() == null)
            player.AddComponent<PlayerHealth>();
        EnsurePlayerWeapon(player);

        PrefabUtility.SaveAsPrefabAsset(player, path);

        if (prefabAlreadyExists)
            PrefabUtility.UnloadPrefabContents(player);
        else
            UnityEngine.Object.DestroyImmediate(player);
    }

    private static void CreateRoadSegmentPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RoadSegmentPrefabPath) != null)
            return;

        Material asphalt = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Asphalt.mat");
        Material paint = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_LanePaint.mat");
        Material concrete = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Concrete.mat");
        GameObject segment = new GameObject("RoadSegment_30m");

        CreateBlock("Road Surface", segment.transform, new Vector3(0f, -0.16f, 0f), new Vector3(9f, 0.32f, 30f), asphalt);
        CreateBlock("Left Road Curb", segment.transform, new Vector3(-4.62f, 0.12f, 0f), new Vector3(0.24f, 0.48f, 30f), concrete);
        CreateBlock("Right Road Curb", segment.transform, new Vector3(4.62f, 0.12f, 0f), new Vector3(0.24f, 0.48f, 30f), concrete);
        CreateBlock("Left Shoulder", segment.transform, new Vector3(-6.3f, -0.2f, 0f), new Vector3(3.1f, 0.25f, 30f), concrete);
        CreateBlock("Right Shoulder", segment.transform, new Vector3(6.3f, -0.2f, 0f), new Vector3(3.1f, 0.25f, 30f), concrete);

        for (int lane = -1; lane <= 1; lane += 2)
        {
            for (int dash = 0; dash < 6; dash++)
                CreateBlock("Lane Marking", segment.transform,
                    new Vector3(lane * 1.5f, 0.012f, -12.5f + dash * 5f), new Vector3(0.09f, 0.025f, 2.5f), paint, false);
        }

        PrefabUtility.SaveAsPrefabAsset(segment, RoadSegmentPrefabPath);
        UnityEngine.Object.DestroyImmediate(segment);
    }

    private static void CreateZombiePrefabs()
    {
        CreateZombiePrefab(NormalZombiePrefabPath, "Zombie_Normal", ZombieArchetype.Normal,
            new Vector3(1f, 1f, 1f), "WR_ZombieNormal");
        CreateZombiePrefab(FastZombiePrefabPath, "Zombie_Fast", ZombieArchetype.Fast,
            new Vector3(0.82f, 0.9f, 0.82f), "WR_ZombieFast");
        CreateZombiePrefab(BigZombiePrefabPath, "Zombie_Big", ZombieArchetype.Big,
            new Vector3(1.3f, 1.25f, 1.3f), "WR_ZombieBig");
    }

    private static void CreateObstaclePrefabs()
    {
        CreateObstaclePrefab(RoadblockPrefabPath, "Obstacle_Roadblock", ObstacleType.Roadblock);
        CreateObstaclePrefab(AbandonedCarPrefabPath, "Obstacle_AbandonedCar", ObstacleType.AbandonedCar);
        CreateObstaclePrefab(ExplosiveBarrelPrefabPath, "Obstacle_ExplosiveBarrel", ObstacleType.ExplodingBarrel);
    }

    private static void CreateObstaclePrefab(string path, string name, ObstacleType type)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            return;

        GameObject obstacle = new GameObject(name);
        ObstacleController controller = obstacle.AddComponent<ObstacleController>();
        controller.Configure(type, null);
        if (type == ObstacleType.Roadblock)
        {
            BoxCollider collider = obstacle.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.58f, 0f);
            collider.size = new Vector3(2.5f, 1.16f, 0.8f);
            CreateBlock("Roadblock Body", obstacle.transform, new Vector3(0f, 0.58f, 0f), new Vector3(2.5f, 1.16f, 0.8f),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HazardOrange.mat"), false);
            CreateBlock("Roadblock Stripe", obstacle.transform, new Vector3(0f, 0.61f, -0.41f), new Vector3(2.1f, 0.2f, 0.04f),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HealthKit.mat"), false);
        }
        else if (type == ObstacleType.AbandonedCar)
        {
            BoxCollider collider = obstacle.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.62f, 0f);
            collider.size = new Vector3(2.25f, 1.24f, 3.8f);
            CreateBlock("Car Body", obstacle.transform, new Vector3(0f, 0.62f, 0f), new Vector3(2.25f, 1.24f, 3.8f),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_CarPaint.mat"), false);
            CreateBlock("Car Window", obstacle.transform, new Vector3(0f, 1.33f, 0.15f), new Vector3(1.5f, 0.35f, 1.45f),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Metal.mat"), false);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wheel.name = "Car Tire";
                    wheel.transform.SetParent(obstacle.transform, false);
                    wheel.transform.localPosition = new Vector3(side * 1.05f, 0.31f, axle * 1.2f);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    wheel.transform.localScale = new Vector3(0.62f, 0.22f, 0.62f);
                    Collider wheelCollider = wheel.GetComponent<Collider>();
                    if (wheelCollider != null) UnityEngine.Object.DestroyImmediate(wheelCollider);
                    Renderer wheelRenderer = wheel.GetComponent<Renderer>();
                    Material tire = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Tire.mat");
                    if (wheelRenderer != null && tire != null) wheelRenderer.sharedMaterial = tire;
                }
            }
        }
        else
        {
            CapsuleCollider collider = obstacle.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.82f, 0f);
            collider.radius = 0.4f;
            collider.height = 1.65f;
            GameObject barrelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrelVisual.name = "Explosive Barrel";
            barrelVisual.transform.SetParent(obstacle.transform, false);
            barrelVisual.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            barrelVisual.transform.localScale = new Vector3(0.8f, 0.82f, 0.8f);
            Collider barrelVisualCollider = barrelVisual.GetComponent<Collider>();
            if (barrelVisualCollider != null) UnityEngine.Object.DestroyImmediate(barrelVisualCollider);
            barrelVisual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_BarrelOrange.mat");
            CreateBlock("Barrel Hazard Band", obstacle.transform, new Vector3(0f, 0.92f, -0.405f), new Vector3(0.62f, 0.16f, 0.03f),
                AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HealthKit.mat"), false);
        }

        PrefabUtility.SaveAsPrefabAsset(obstacle, path);
        UnityEngine.Object.DestroyImmediate(obstacle);
    }

    private static void CreateCollectiblePrefabs()
    {
        CreateCollectiblePrefab(CoinPrefabPath, "Collectible_GoldCoin", CollectibleType.Coin, 1);
        CreateCollectiblePrefab(AmmoPrefabPath, "Collectible_AmmoBox", CollectibleType.Ammo, 15);
        CreateCollectiblePrefab(HealthKitPrefabPath, "Collectible_HealthKit", CollectibleType.Health, 25);
    }

    private static void CreatePowerUpPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PowerUpPrefabPath) != null)
            return;

        GameObject pickup = new GameObject("PowerUpPickup");
        SphereCollider trigger = pickup.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.7f;
        PowerUpPickup controller = pickup.AddComponent<PowerUpPickup>();
        controller.Configure(PowerUpType.CoinMagnet, 0f, null);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Power-Up Visual";
        visual.transform.SetParent(pickup.transform, false);
        visual.transform.localScale = Vector3.one * 0.85f;
        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null) UnityEngine.Object.DestroyImmediate(visualCollider);
        visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_PowerUp.mat");

        PrefabUtility.SaveAsPrefabAsset(pickup, PowerUpPrefabPath);
        UnityEngine.Object.DestroyImmediate(pickup);
    }

    private static void CreateEnvironmentThemePrefabs()
    {
        SaveThemePrefabIfMissing(CityEnvironmentPrefabPath, "EnvironmentSegment_CityRuins", BuildCityRuinsSegment);
        SaveThemePrefabIfMissing(SubwayEnvironmentPrefabPath, "EnvironmentSegment_SubwayTunnel", BuildSubwaySegment);
        SaveThemePrefabIfMissing(MilitaryEnvironmentPrefabPath, "EnvironmentSegment_MilitaryBase", BuildMilitarySegment);
        SaveThemePrefabIfMissing(FactoryEnvironmentPrefabPath, "EnvironmentSegment_Factory", BuildFactorySegment);
        RepairEnvironmentParticleVelocityModes();
    }

    [MenuItem("Tools/Zombie Runner/Fix Particle Velocity Curves")]
    public static void FixParticleVelocityCurves()
    {
        int repaired = RepairEnvironmentParticleVelocityModes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Zombie Runner particle velocity curves repaired on " + repaired + " environment particle system(s).");
    }

    private static int RepairEnvironmentParticleVelocityModes()
    {
        string[] prefabPaths =
        {
            CityEnvironmentPrefabPath,
            SubwayEnvironmentPrefabPath,
            MilitaryEnvironmentPrefabPath,
            FactoryEnvironmentPrefabPath
        };
        int repairedCount = 0;

        foreach (string prefabPath in prefabPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                continue;

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            bool changed = false;
            try
            {
                foreach (ParticleSystem particles in prefabContents.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
                    if (!velocity.enabled)
                        continue;

                    ParticleSystem.MinMaxCurve x = velocity.x;
                    ParticleSystem.MinMaxCurve y = velocity.y;
                    ParticleSystem.MinMaxCurve z = velocity.z;
                    if (x.mode == y.mode && y.mode == z.mode)
                        continue;

                    if (!IsConstantVelocityCurve(x) || !IsConstantVelocityCurve(y) || !IsConstantVelocityCurve(z))
                    {
                        Debug.LogWarning("Skipped non-constant velocity curves on " + particles.name + " in " + prefabPath + ".");
                        continue;
                    }

                    velocity.x = AsTwoConstantVelocityCurve(x);
                    velocity.y = AsTwoConstantVelocityCurve(y);
                    velocity.z = AsTwoConstantVelocityCurve(z);
                    changed = true;
                    repairedCount++;
                }

                if (changed)
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        return repairedCount;
    }

    private static bool IsConstantVelocityCurve(ParticleSystem.MinMaxCurve curve)
    {
        return curve.mode == ParticleSystemCurveMode.Constant || curve.mode == ParticleSystemCurveMode.TwoConstants;
    }

    private static ParticleSystem.MinMaxCurve AsTwoConstantVelocityCurve(ParticleSystem.MinMaxCurve curve)
    {
        if (curve.mode == ParticleSystemCurveMode.TwoConstants)
            return curve;
        return new ParticleSystem.MinMaxCurve(curve.constant, curve.constant);
    }

    private static void SaveThemePrefabIfMissing(string path, string name, Action<Transform> build)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            return;

        GameObject segment = new GameObject(name);
        build(segment.transform);
        PrefabUtility.SaveAsPrefabAsset(segment, path);
        UnityEngine.Object.DestroyImmediate(segment);
    }

    private static void BuildCityRuinsSegment(Transform root)
    {
        Material ruins = LoadThemeMaterial("WR_Ruins");
        Material concrete = LoadThemeMaterial("WR_Concrete");
        Material metal = LoadThemeMaterial("WR_Metal");
        Material window = LoadThemeMaterial("WR_SubwayTile");
        for (int side = -1; side <= 1; side += 2)
        {
            for (int building = 0; building < 2; building++)
            {
                float z = building == 0 ? -8f : 8f;
                float height = building == 0 ? 8.5f : 6.5f;
                float x = side * (9.4f + building * 1.5f);
                CreateThemeBlock(root, "Ruined Building", new Vector3(x, height * 0.5f - 0.2f, z), new Vector3(4.7f, height, 12f), ruins);
                CreateThemeBlock(root, "Broken Concrete Facade", new Vector3(x - side * 2.35f, height * 0.38f, z - 2f), new Vector3(0.22f, height * 0.32f, 7f), concrete);
                for (int floor = 0; floor < 3; floor++)
                    CreateThemeBlock(root, "Dark Broken Window", new Vector3(x - side * 2.49f, 1.6f + floor * 2.15f, z + 2f), new Vector3(0.04f, 0.9f, 1.35f), window);
            }
            CreateThemeBlock(root, "Street Debris", new Vector3(side * 5.15f, 0.28f, -8f), new Vector3(1.15f, 0.48f, 1.8f), metal);
            CreateThemeBlock(root, "Collapsed Slab", new Vector3(side * 5.8f, 0.18f, 8f), new Vector3(1.35f, 0.32f, 1.2f), concrete);
        }

        CreateThemePointLight(root, "Sodium Street Light", new Vector3(-5.7f, 3.6f, 0f), new Color(1f, 0.42f, 0.18f), 0.8f, 9f);
        CreateAmbientParticles(root, "Ash Drift", new Vector3(0f, 2f, 0f), new Color(0.49f, 0.53f, 0.5f, 0.58f), 2.5f, 0.045f, new Vector3(13f, 5f, 28f));
    }

    private static void BuildSubwaySegment(Transform root)
    {
        Material concrete = LoadThemeMaterial("WR_SubwayConcrete");
        Material tile = LoadThemeMaterial("WR_SubwayTile");
        Material metal = LoadThemeMaterial("WR_Metal");
        Material lamp = LoadThemeMaterial("WR_TunnelLamp");

        CreateThemeBlock(root, "Tunnel Left Wall", new Vector3(-7.35f, 2.5f, 0f), new Vector3(1.35f, 5f, 30f), concrete);
        CreateThemeBlock(root, "Tunnel Right Wall", new Vector3(7.35f, 2.5f, 0f), new Vector3(1.35f, 5f, 30f), concrete);
        for (int arch = -1; arch <= 1; arch++)
        {
            float z = arch * 10f;
            CreateThemeBlock(root, "Tunnel Arch Left", new Vector3(-6.45f, 2.4f, z), new Vector3(0.42f, 4.8f, 0.55f), tile);
            CreateThemeBlock(root, "Tunnel Arch Right", new Vector3(6.45f, 2.4f, z), new Vector3(0.42f, 4.8f, 0.55f), tile);
            CreateThemeBlock(root, "Tunnel Crown", new Vector3(0f, 4.82f, z), new Vector3(13.2f, 0.5f, 0.62f), tile);
        }
        CreateThemeBlock(root, "Left Track Rail", new Vector3(-5.2f, 0.08f, 0f), new Vector3(0.13f, 0.15f, 30f), metal);
        CreateThemeBlock(root, "Right Track Rail", new Vector3(5.2f, 0.08f, 0f), new Vector3(0.13f, 0.15f, 30f), metal);
        for (int tie = 0; tie < 7; tie++)
            CreateThemeBlock(root, "Railway Sleeper", new Vector3(0f, 0.035f, -13f + tie * 4.3f), new Vector3(10.6f, 0.11f, 0.24f), metal);
        CreateThemeBlock(root, "Tunnel Lamp Panel", new Vector3(0f, 4.48f, -2f), new Vector3(1.25f, 0.12f, 2.2f), lamp);
        CreateThemePointLight(root, "Cool Tunnel Light", new Vector3(0f, 4.1f, 0f), new Color(0.54f, 0.73f, 1f), 1.35f, 17f);
        CreateAmbientParticles(root, "Tunnel Dust", new Vector3(0f, 2.6f, 0f), new Color(0.58f, 0.63f, 0.64f, 0.3f), 1.1f, 0.06f, new Vector3(10f, 3f, 28f));
    }

    private static void BuildMilitarySegment(Transform root)
    {
        Material green = LoadThemeMaterial("WR_MilitaryGreen");
        Material canvas = LoadThemeMaterial("WR_MilitaryCanvas");
        Material container = LoadThemeMaterial("WR_ContainerBlue");
        Material metal = LoadThemeMaterial("WR_Metal");
        Material hazard = LoadThemeMaterial("WR_HazardOrange");

        for (int side = -1; side <= 1; side += 2)
        {
            CreateThemeBlock(root, "Military Container", new Vector3(side * 10f, 1.45f, 2f), new Vector3(4.2f, 2.9f, 13f), container);
            CreateThemeBlock(root, "Bunker Block", new Vector3(side * 13.4f, 2.3f, -9f), new Vector3(3.5f, 4.6f, 8f), green);
            CreateThemeBlock(root, "Canvas Cover", new Vector3(side * 8.9f, 3.15f, -10f), new Vector3(4.4f, 0.32f, 6.5f), canvas);
            for (int post = 0; post < 6; post++)
                CreateThemeBlock(root, "Perimeter Fence Post", new Vector3(side * 5.65f, 1.1f, -13f + post * 5.2f), new Vector3(0.16f, 2.2f, 0.16f), metal);
            CreateThemeBlock(root, "Perimeter Fence Upper Rail", new Vector3(side * 5.65f, 1.75f, 0f), new Vector3(0.14f, 0.14f, 30f), metal);
            CreateThemeBlock(root, "Perimeter Fence Lower Rail", new Vector3(side * 5.65f, 0.82f, 0f), new Vector3(0.12f, 0.12f, 30f), metal);
            CreateThemeBlock(root, "Hazard Marker", new Vector3(side * 5.15f, 0.5f, 5f), new Vector3(0.28f, 0.9f, 0.28f), hazard);
        }
        CreateThemePointLight(root, "Base Flood Light", new Vector3(0f, 4.4f, 0f), new Color(0.82f, 0.9f, 0.68f), 1.15f, 14f);
        CreateAmbientParticles(root, "Base Dust", new Vector3(0f, 0.7f, 0f), new Color(0.58f, 0.55f, 0.44f, 0.28f), 0.8f, 0.05f, new Vector3(12f, 1.3f, 28f));
    }

    private static void BuildFactorySegment(Transform root)
    {
        Material metal = LoadThemeMaterial("WR_FactoryMetal");
        Material pipe = LoadThemeMaterial("WR_Pipe");
        Material warning = LoadThemeMaterial("WR_FactoryWarning");

        for (int side = -1; side <= 1; side += 2)
        {
            for (int column = 0; column < 3; column++)
                CreateThemeBlock(root, "Factory Support", new Vector3(side * 7.6f, 3.1f, -10f + column * 10f), new Vector3(0.55f, 6.2f, 0.55f), metal);
            CreateThemeBlock(root, "Overhead Factory Girder", new Vector3(0f, 5.75f, 0f), new Vector3(15.7f, 0.42f, 0.65f), metal);
            CreateThemeCylinder(root, "Steam Pipe", new Vector3(side * 6.25f, 3.8f, 0f), new Vector3(0.42f, 0.42f, 30f), Quaternion.Euler(90f, 0f, 0f), pipe);
            CreateThemeCylinder(root, "Exhaust Stack", new Vector3(side * 12.2f, 2.5f, 1.5f), new Vector3(1.3f, 2.7f, 1.3f), Quaternion.identity, metal);
            CreateThemeBlock(root, "Factory Hazard Stripe", new Vector3(side * 7.2f, 0.75f, -4f), new Vector3(0.95f, 0.2f, 1.1f), warning);
        }
        CreateThemePointLight(root, "Factory Warning Light", new Vector3(0f, 4.95f, -1f), new Color(1f, 0.37f, 0.12f), 1.25f, 16f);
        CreateAmbientParticles(root, "Factory Steam", new Vector3(0f, 2.4f, 0f), new Color(0.65f, 0.7f, 0.7f, 0.32f), 4.5f, 0.09f, new Vector3(11f, 3.5f, 26f));
    }

    private static Material LoadThemeMaterial(string materialName)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + materialName + ".mat");
    }

    private static GameObject CreateThemeBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject block = CreateBlock(name, parent, position, scale, material, false);
        return block;
    }

    private static GameObject CreateThemeCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = position;
        cylinder.transform.localRotation = rotation;
        cylinder.transform.localScale = scale;
        Collider collider = cylinder.GetComponent<Collider>();
        if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        Renderer renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null && material != null) renderer.sharedMaterial = material;
        return cylinder;
    }

    private static void CreateThemePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name, typeof(Light));
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    private static void CreateAmbientParticles(Transform parent, string name, Vector3 position, Color color,
        float emissionRate, float particleSize, Vector3 volume)
    {
        GameObject particlesObject = new GameObject(name, typeof(ParticleSystem));
        particlesObject.transform.SetParent(parent, false);
        particlesObject.transform.localPosition = position;
        ParticleSystem particles = particlesObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.maxParticles = 96;
        main.startLifetime = 10f;
        main.startSpeed = 0.12f;
        main.startSize = particleSize;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = emissionRate;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = volume;
        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        particles.Play();
    }

    private static void CreateCollectiblePrefab(string path, string name, CollectibleType type, int amount)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            return;

        GameObject pickup = new GameObject(name);
        CollectiblePickup controller = pickup.AddComponent<CollectiblePickup>();
        controller.Configure(type, amount, null);
        if (type == CollectibleType.Coin)
        {
            SphereCollider collider = pickup.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.65f;
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Gold Coin Mesh";
            coin.transform.SetParent(pickup.transform, false);
            coin.transform.localScale = new Vector3(0.42f, 0.08f, 0.42f);
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Collider coinCollider = coin.GetComponent<Collider>();
            if (coinCollider != null) UnityEngine.Object.DestroyImmediate(coinCollider);
            coin.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_CoinGold.mat");
        }
        else
        {
            BoxCollider collider = pickup.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = type == CollectibleType.Health ? new Vector3(0.85f, 0.7f, 0.55f) : new Vector3(0.9f, 0.75f, 0.7f);
            Material bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + (type == CollectibleType.Health ? "WR_HealthKit.mat" : "WR_AmmoBox.mat"));
            CreateBlock(type == CollectibleType.Health ? "Health Kit" : "Ammo Box", pickup.transform,
                new Vector3(0f, 0.3f, 0f), new Vector3(0.8f, 0.6f, 0.55f), bodyMaterial, false);
            if (type == CollectibleType.Health)
            {
                Material red = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HealthAccent.mat");
                CreateBlock("Medical Cross Vertical", pickup.transform, new Vector3(0f, 0.3f, -0.285f), new Vector3(0.16f, 0.42f, 0.03f), red, false);
                CreateBlock("Medical Cross Horizontal", pickup.transform, new Vector3(0f, 0.3f, -0.29f), new Vector3(0.42f, 0.16f, 0.03f), red, false);
            }
            else
            {
                CreateBlock("Ammo Box Band", pickup.transform, new Vector3(0f, 0.3f, -0.285f), new Vector3(0.2f, 0.6f, 0.03f),
                    AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HazardOrange.mat"), false);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(pickup, path);
        UnityEngine.Object.DestroyImmediate(pickup);
    }

    private static void CreateZombiePrefab(string path, string name, ZombieArchetype archetype, Vector3 scale, string materialName)
    {
        bool prefabAlreadyExists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
        GameObject zombie = prefabAlreadyExists
            ? PrefabUtility.LoadPrefabContents(path)
            : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        zombie.name = name;
        if (!prefabAlreadyExists)
            zombie.transform.localScale = scale;
        Renderer renderer = zombie.GetComponent<Renderer>();
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + materialName + ".mat");
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
        ZombieController controller = zombie.GetComponent<ZombieController>();
        if (controller == null)
            controller = zombie.AddComponent<ZombieController>();
        controller.Configure(archetype, null);
        Animator animator = zombie.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = zombie.AddComponent<Animator>();
        if (animator.runtimeAnimatorController == null)
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ZombieAnimatorControllerPath);
        animator.applyRootMotion = false;
        SetSerializedReference(controller, "animator", animator);
        SerializedObject controllerSerialized = new SerializedObject(controller);
        SerializedProperty destroyDelay = controllerSerialized.FindProperty("destroyDelay");
        if (destroyDelay != null)
            destroyDelay.floatValue = Mathf.Max(destroyDelay.floatValue, 0.9f);
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(zombie, path);
        if (prefabAlreadyExists)
            PrefabUtility.UnloadPrefabContents(zombie);
        else
            UnityEngine.Object.DestroyImmediate(zombie);
    }

    private static void EnsurePlayerWeapon(GameObject player)
    {
        Transform weapon = player.transform.Find("Weapon Placeholder");
        if (weapon == null)
        {
            GameObject weaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObject.name = "Weapon Placeholder";
            weaponObject.transform.SetParent(player.transform, false);
            weaponObject.transform.localPosition = new Vector3(0.38f, 0.25f, 0.2f);
            weaponObject.transform.localScale = new Vector3(0.16f, 0.18f, 0.62f);
            Collider weaponCollider = weaponObject.GetComponent<Collider>();
            if (weaponCollider != null)
                UnityEngine.Object.DestroyImmediate(weaponCollider);
            Renderer renderer = weaponObject.GetComponent<Renderer>();
            Material weaponMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_WeaponMetal.mat");
            if (renderer != null && weaponMaterial != null)
                renderer.sharedMaterial = weaponMaterial;
            weapon = weaponObject.transform;
        }

        Transform muzzle = weapon.Find("Muzzle");
        if (muzzle == null)
        {
            muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(weapon, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.42f);
            muzzle.localRotation = Quaternion.identity;
        }

        Transform flash = muzzle.Find("Muzzle Flash Placeholder");
        if (flash == null)
        {
            GameObject flashObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flashObject.name = "Muzzle Flash Placeholder";
            flashObject.transform.SetParent(muzzle, false);
            flashObject.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            flashObject.transform.localScale = new Vector3(0.13f, 0.13f, 0.2f);
            Collider flashCollider = flashObject.GetComponent<Collider>();
            if (flashCollider != null)
                UnityEngine.Object.DestroyImmediate(flashCollider);
            Renderer flashRenderer = flashObject.GetComponent<Renderer>();
            Material flashMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_MuzzleFlash.mat");
            if (flashRenderer != null && flashMaterial != null)
                flashRenderer.sharedMaterial = flashMaterial;
            flash = flashObject.transform;
        }
        flash.gameObject.SetActive(false);

        ShootingController shooting = player.GetComponent<ShootingController>();
        if (shooting == null)
            shooting = player.AddComponent<ShootingController>();
        SerializedObject serialized = new SerializedObject(shooting);
        serialized.FindProperty("muzzle").objectReferenceValue = muzzle;
        serialized.FindProperty("muzzleFlashVisual").objectReferenceValue = flash.gameObject;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureGameplayComponents(Scene scene)
    {
        GameObject sceneRoot = scene.GetRootGameObjects().FirstOrDefault(item => item.name == GameplayRootName);
        if (sceneRoot == null)
        {
            Debug.LogError("Could not find the Zombie Runner generated root in the gameplay scene.");
            return;
        }

        GameObject playerObject = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(item => item.gameObject)
            .FirstOrDefault(item => item.name == "Player - Capsule Placeholder");
        if (playerObject == null)
        {
            Debug.LogError("Could not find the generated player object in the gameplay scene.");
            return;
        }

        playerObject.tag = "Player";
        CharacterController characterController = playerObject.GetComponent<CharacterController>();
        if (characterController == null)
            characterController = playerObject.AddComponent<CharacterController>();
        Collider extraCollider = playerObject.GetComponent<CapsuleCollider>();
        if (extraCollider != null)
            UnityEngine.Object.DestroyImmediate(extraCollider);
        ConfigureCharacterController(characterController);

        Transform visual = playerObject.transform.Find("Visual - New Character");
        if (visual == null)
            visual = playerObject.transform.Find("Visual - Mixamo X Bot");
        if (visual == null)
            visual = playerObject.transform.Find("Visual - Placeholder Capsule");
        if (visual == null)
            visual = playerObject.transform;
        PlayerController controller = playerObject.GetComponent<PlayerController>();
        if (controller == null)
            controller = playerObject.AddComponent<PlayerController>();
        Animator playerAnimator = visual.GetComponentInChildren<Animator>();
        if (playerAnimator == null)
            playerAnimator = visual.gameObject.AddComponent<Animator>();
        if (playerAnimator.runtimeAnimatorController == null)
            playerAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
        playerAnimator.applyRootMotion = false;
        ConfigurePlayerReferences(controller, characterController, visual);

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
        if (health == null)
            health = playerObject.AddComponent<PlayerHealth>();
        ShootingController shooting = playerObject.GetComponent<ShootingController>();
        if (shooting == null)
            shooting = playerObject.AddComponent<ShootingController>();
        Transform weapon = playerObject.transform.Find("Weapon Placeholder");
        Transform muzzle = weapon != null ? weapon.Find("Muzzle") : null;
        Transform flash = muzzle != null ? muzzle.Find("Muzzle Flash Placeholder") : null;
        SerializedObject shootingSerialized = new SerializedObject(shooting);
        if (shootingSerialized.FindProperty("muzzle") != null)
            shootingSerialized.FindProperty("muzzle").objectReferenceValue = muzzle;
        if (shootingSerialized.FindProperty("muzzleFlashVisual") != null)
            shootingSerialized.FindProperty("muzzleFlashVisual").objectReferenceValue = flash != null ? flash.gameObject : null;
        shootingSerialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject cameraObject = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
            .Select(camera => camera.gameObject)
            .FirstOrDefault(camera => camera.CompareTag("MainCamera"));
        if (cameraObject == null)
        {
            Debug.LogError("Could not find the Main Camera in the gameplay scene.");
            return;
        }

        RunnerCameraFollow follow = cameraObject.GetComponent<RunnerCameraFollow>();
        if (follow == null)
            follow = cameraObject.AddComponent<RunnerCameraFollow>();
        follow.SetTarget(playerObject.transform);

        Transform staticRoad = sceneRoot.transform.Find("Environment - City Ruins Placeholder/Road - Three Lanes");
        if (staticRoad != null)
            staticRoad.gameObject.SetActive(false);
        Transform environment = sceneRoot.transform.Find("Environment - City Ruins Placeholder");
        if (environment != null)
        {
            Transform leftShoulder = environment.Find("Left Shoulder");
            Transform rightShoulder = environment.Find("Right Shoulder");
            if (leftShoulder != null) leftShoulder.gameObject.SetActive(false);
            if (rightShoulder != null) rightShoulder.gameObject.SetActive(false);
            environment.gameObject.SetActive(false);
        }

        Transform managerTransform = sceneRoot.transform.Find("Road Streaming Manager");
        GameObject managerObject = managerTransform != null ? managerTransform.gameObject : new GameObject("Road Streaming Manager");
        if (managerTransform == null)
            managerObject.transform.SetParent(sceneRoot.transform, false);
        EndlessRoadManager roadManager = managerObject.GetComponent<EndlessRoadManager>();
        if (roadManager == null)
            roadManager = managerObject.AddComponent<EndlessRoadManager>();
        SerializedObject managerSerialized = new SerializedObject(roadManager);
        SerializedProperty prefabProperty = managerSerialized.FindProperty("segmentPrefab");
        SerializedProperty playerProperty = managerSerialized.FindProperty("player");
        if (prefabProperty != null)
            prefabProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(RoadSegmentPrefabPath);
        if (playerProperty != null)
            playerProperty.objectReferenceValue = playerObject.transform;
        managerSerialized.ApplyModifiedPropertiesWithoutUndo();

        Transform spawnerTransform = sceneRoot.transform.Find("Zombie Spawner");
        GameObject spawnerObject = spawnerTransform != null ? spawnerTransform.gameObject : new GameObject("Zombie Spawner");
        if (spawnerTransform == null)
            spawnerObject.transform.SetParent(sceneRoot.transform, false);
        ZombieSpawner zombieSpawner = spawnerObject.GetComponent<ZombieSpawner>();
        if (zombieSpawner == null)
            zombieSpawner = spawnerObject.AddComponent<ZombieSpawner>();
        SerializedObject spawnerSerialized = new SerializedObject(zombieSpawner);
        spawnerSerialized.FindProperty("player").objectReferenceValue = playerObject.transform;
        spawnerSerialized.FindProperty("normalZombiePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(NormalZombiePrefabPath);
        spawnerSerialized.FindProperty("fastZombiePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(FastZombiePrefabPath);
        spawnerSerialized.FindProperty("bigZombiePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(BigZombiePrefabPath);
        spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();

        Transform obstacleSpawnerTransform = sceneRoot.transform.Find("Obstacle Spawner");
        GameObject obstacleSpawnerObject = obstacleSpawnerTransform != null ? obstacleSpawnerTransform.gameObject : new GameObject("Obstacle Spawner");
        if (obstacleSpawnerTransform == null)
            obstacleSpawnerObject.transform.SetParent(sceneRoot.transform, false);
        ObstacleSpawner obstacleSpawner = obstacleSpawnerObject.GetComponent<ObstacleSpawner>();
        if (obstacleSpawner == null)
            obstacleSpawner = obstacleSpawnerObject.AddComponent<ObstacleSpawner>();
        SerializedObject obstacleSerialized = new SerializedObject(obstacleSpawner);
        obstacleSerialized.FindProperty("player").objectReferenceValue = playerObject.transform;
        obstacleSerialized.FindProperty("roadblockPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(RoadblockPrefabPath);
        obstacleSerialized.FindProperty("abandonedCarPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AbandonedCarPrefabPath);
        obstacleSerialized.FindProperty("explosiveBarrelPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosiveBarrelPrefabPath);
        obstacleSerialized.ApplyModifiedPropertiesWithoutUndo();

        Transform collectibleManagerTransform = sceneRoot.transform.Find("Collectible Manager");
        GameObject collectibleManagerObject = collectibleManagerTransform != null ? collectibleManagerTransform.gameObject : new GameObject("Collectible Manager");
        if (collectibleManagerTransform == null)
            collectibleManagerObject.transform.SetParent(sceneRoot.transform, false);
        CollectibleManager collectibleManager = collectibleManagerObject.GetComponent<CollectibleManager>();
        if (collectibleManager == null)
            collectibleManager = collectibleManagerObject.AddComponent<CollectibleManager>();
        SerializedObject collectibleSerialized = new SerializedObject(collectibleManager);
        collectibleSerialized.FindProperty("player").objectReferenceValue = playerObject.transform;
        collectibleSerialized.FindProperty("coinPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(CoinPrefabPath);
        collectibleSerialized.FindProperty("ammoPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AmmoPrefabPath);
        collectibleSerialized.FindProperty("healthPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(HealthKitPrefabPath);
        collectibleSerialized.ApplyModifiedPropertiesWithoutUndo();

        Transform powerUpManagerTransform = sceneRoot.transform.Find("Power-Up Manager");
        GameObject powerUpManagerObject = powerUpManagerTransform != null ? powerUpManagerTransform.gameObject : new GameObject("Power-Up Manager");
        if (powerUpManagerTransform == null)
            powerUpManagerObject.transform.SetParent(sceneRoot.transform, false);
        PowerUpManager powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();
        if (powerUpManager == null)
            powerUpManager = powerUpManagerObject.AddComponent<PowerUpManager>();
        SerializedObject powerUpSerialized = new SerializedObject(powerUpManager);
        powerUpSerialized.FindProperty("player").objectReferenceValue = playerObject.transform;
        powerUpSerialized.FindProperty("collectibleManager").objectReferenceValue = collectibleManager;
        powerUpSerialized.FindProperty("pickupPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PowerUpPrefabPath);
        powerUpSerialized.ApplyModifiedPropertiesWithoutUndo();

        EnsureGameplayManagers(scene, sceneRoot, playerObject, collectibleManager, powerUpManager, zombieSpawner);
    }

    private static void EnsureGameplayManagers(Scene scene, GameObject sceneRoot, GameObject playerObject,
        CollectibleManager collectibleManager, PowerUpManager powerUpManager, ZombieSpawner zombieSpawner)
    {
        GameObject persistentSave = scene.GetRootGameObjects().FirstOrDefault(item => item.name == "Persistent Save Manager");
        if (persistentSave == null)
            persistentSave = new GameObject("Persistent Save Manager");
        if (persistentSave.GetComponent<ZombieRunner.Core.SaveManager>() == null)
            persistentSave.AddComponent<ZombieRunner.Core.SaveManager>();
        if (persistentSave.GetComponent<ZombieRunner.Audio.AudioManager>() == null)
            persistentSave.AddComponent<ZombieRunner.Audio.AudioManager>();

        Transform systemsTransform = GetOrCreateGameplaySystems(sceneRoot);
        GameObject systems = systemsTransform.gameObject;

        ZombieRunner.Core.ScoreManager scoreManager = systems.GetComponent<ZombieRunner.Core.ScoreManager>();
        if (scoreManager == null) scoreManager = systems.AddComponent<ZombieRunner.Core.ScoreManager>();
        SetSerializedReference(scoreManager, "player", playerObject.transform);
        SetSerializedReference(scoreManager, "collectibleManager", collectibleManager);
        SetSerializedReference(scoreManager, "zombieSpawner", zombieSpawner);

        ZombieRunner.Core.GameManager gameManager = systems.GetComponent<ZombieRunner.Core.GameManager>();
        if (gameManager == null) gameManager = systems.AddComponent<ZombieRunner.Core.GameManager>();
        SetSerializedReference(gameManager, "playerHealth", playerObject.GetComponent<PlayerHealth>());
        SetSerializedReference(gameManager, "scoreManager", scoreManager);
        SetSerializedReference(gameManager, "collectibleManager", collectibleManager);
        SetSerializedReference(gameManager, "powerUpManager", powerUpManager);

        Transform themeManagerTransform = sceneRoot.transform.Find("Environment Theme Manager");
        GameObject themeManagerObject = themeManagerTransform != null ? themeManagerTransform.gameObject : new GameObject("Environment Theme Manager");
        if (themeManagerTransform == null)
            themeManagerObject.transform.SetParent(sceneRoot.transform, false);
        ZombieRunner.World.EnvironmentThemeManager themeManager = themeManagerObject.GetComponent<ZombieRunner.World.EnvironmentThemeManager>();
        if (themeManager == null) themeManager = themeManagerObject.AddComponent<ZombieRunner.World.EnvironmentThemeManager>();
        SerializedObject themeSerialized = new SerializedObject(themeManager);
        themeSerialized.FindProperty("player").objectReferenceValue = playerObject.transform;
        GameObject directionalLight = scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<Light>(true))
            .FirstOrDefault(item => item.type == LightType.Directional)?.gameObject;
        themeSerialized.FindProperty("keyLight").objectReferenceValue = directionalLight != null ? directionalLight.GetComponent<Light>() : null;
        SerializedProperty themePrefabs = themeSerialized.FindProperty("themeSegmentPrefabs");
        themePrefabs.arraySize = 4;
        themePrefabs.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(CityEnvironmentPrefabPath);
        themePrefabs.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SubwayEnvironmentPrefabPath);
        themePrefabs.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(MilitaryEnvironmentPrefabPath);
        themePrefabs.GetArrayElementAtIndex(3).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(FactoryEnvironmentPrefabPath);
        themeSerialized.ApplyModifiedPropertiesWithoutUndo();

        EnsureGameplayCanvas(sceneRoot, systems, playerObject, scoreManager, gameManager, collectibleManager, powerUpManager);
    }

    private static Transform GetOrCreateGameplaySystems(GameObject sceneRoot)
    {
        Transform systemsTransform = sceneRoot.transform.Find("Game Systems");
        if (systemsTransform == null)
            systemsTransform = sceneRoot.transform.Find("Game Systems - Stage 1 placeholders");

        if (systemsTransform == null)
        {
            GameObject systemsObject = new GameObject("Game Systems");
            systemsObject.transform.SetParent(sceneRoot.transform, false);
            systemsTransform = systemsObject.transform;
        }

        systemsTransform.name = "Game Systems";
        for (int i = sceneRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = sceneRoot.transform.GetChild(i);
            if (child != systemsTransform && child.name == "Game Systems")
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        return systemsTransform;
    }

    [MenuItem("Tools/Zombie Runner/Repair Duplicate Gameplay Systems")]
    public static void RepairDuplicateGameplaySystems()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open the Zombie Runner gameplay scene before repairing its Game Systems objects.");
            return;
        }

        GameObject sceneRoot = scene.GetRootGameObjects()
            .FirstOrDefault(item => item.name == "Zombie Runner Generated - Gameplay");
        if (sceneRoot == null)
        {
            Debug.LogError("The active scene is not the generated Zombie Runner gameplay scene. No objects were changed.");
            return;
        }

        int before = sceneRoot.transform.Cast<Transform>().Count(child => child.name == "Game Systems");
        Undo.RegisterFullObjectHierarchyUndo(sceneRoot, "Repair Zombie Runner Game Systems");
        GetOrCreateGameplaySystems(sceneRoot);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Zombie Runner gameplay systems repaired: " + before + " Game Systems object(s) reduced to one.");
    }

    [MenuItem("Tools/Zombie Runner/Repair Gameplay HUD Text Glyphs")]
    public static void RepairGameplayHudTextGlyphs()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        GameObject sceneRoot = scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().FirstOrDefault(item => item.name == "Zombie Runner Generated - Gameplay")
            : null;
        if (sceneRoot == null)
        {
            Debug.LogError("Open the Zombie Runner gameplay scene before repairing its HUD text glyphs. No objects were changed.");
            return;
        }

        int repaired = 0;
        foreach (TMP_Text label in sceneRoot.GetComponentsInChildren<TMP_Text>(true))
        {
            string corrected = label.text.Replace("\u25C6", "$").Replace("\u25C0", "<").Replace("\u25B6", ">").Replace("\u2161", "||").Replace("\u2022", "|").Replace("\u2500", "-");
            if (corrected == label.text)
                continue;

            Undo.RecordObject(label, "Repair Zombie Runner HUD Glyph");
            label.text = corrected;
            repaired++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Zombie Runner HUD text glyphs repaired: " + repaired + " label(s) updated.");
    }

    private static void SetSerializedReference(Component component, string propertyName, UnityEngine.Object value)
    {
        if (component == null) return;
        SerializedObject serialized = new SerializedObject(component);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void EnsureGameplayCanvas(GameObject sceneRoot, GameObject systems, GameObject playerObject,
        ZombieRunner.Core.ScoreManager scoreManager, ZombieRunner.Core.GameManager gameManager,
        CollectibleManager collectibleManager, PowerUpManager powerUpManager)
    {
        Transform existing = sceneRoot.transform.Find("Gameplay Canvas");
        if (existing != null && existing.Find("Safe Area") != null)
        {
            Transform safeArea = existing.Find("Safe Area");
            ZombieRunner.UI.UIManager existingUi = safeArea.GetComponent<ZombieRunner.UI.UIManager>();
            if (existingUi != null)
                EnsurePauseSettingsPanel(safeArea, existingUi);
            return;
        }

        GameObject canvasObject = CreateCanvas("Gameplay Canvas", sceneRoot.transform);
        CreateEventSystem(sceneRoot.transform);
        CreatePanel(canvasObject.transform, "HUD Background", new Color(0.025f, 0.045f, 0.05f, 0.28f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);

        GameObject safeAreaObject = CreateUIObject("Safe Area", canvasObject.transform);
        RectTransform safeAreaRect = safeAreaObject.GetComponent<RectTransform>();
        safeAreaRect.anchorMin = Vector2.zero;
        safeAreaRect.anchorMax = Vector2.one;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
        safeAreaObject.AddComponent<ZombieRunner.UI.SafeAreaFitter>();

        TMP_Text scoreText = CreateText(safeAreaObject.transform, "Score Value", "0", 74,
            Color.white, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -42f), new Vector2(390f, 98f), TextAlignmentOptions.Left);
        CreateText(safeAreaObject.transform, "Score Label", "SCORE", 25, new Color(0.72f, 0.79f, 0.8f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -24f), new Vector2(210f, 42f), TextAlignmentOptions.Left);
        TMP_Text distanceText = CreateText(safeAreaObject.transform, "Distance Value", "0 m", 39,
            Color.white, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -132f), new Vector2(330f, 62f), TextAlignmentOptions.Left);
        TMP_Text coinText = CreateText(safeAreaObject.transform, "Coin Count", "0", 48,
            new Color(1f, 0.75f, 0.22f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-176f, -52f), new Vector2(250f, 75f), TextAlignmentOptions.Right);
        CreateText(safeAreaObject.transform, "Coin Icon Label", "COINS  $", 24, new Color(1f, 0.83f, 0.43f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -24f), new Vector2(220f, 42f), TextAlignmentOptions.Right);
        TMP_Text ammoText = CreateText(safeAreaObject.transform, "Ammo Count", "AMMO  30 / 60", 30,
            Color.white, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -211f), new Vector2(400f, 58f), TextAlignmentOptions.Left);

        CreatePanel(safeAreaObject.transform, "Health Bar Back", new Color(0.035f, 0.05f, 0.06f, 0.92f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -202f), new Vector2(360f, 42f), false);
        Image healthFill = CreatePanel(safeAreaObject.transform, "Health Bar Fill", new Color(0.86f, 0.15f, 0.11f, 0.98f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -204f), new Vector2(356f, 34f), false);
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFill.fillAmount = 1f;
        TMP_Text healthText = CreateText(safeAreaObject.transform, "Health Value", "100 / 100", 23, Color.white,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-205f, -202f), new Vector2(220f, 38f), TextAlignmentOptions.Center);
        TMP_Text powerUpText = CreateText(safeAreaObject.transform, "Active Power Ups", "", 23, new Color(0.46f, 0.91f, 0.97f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -282f), new Vector2(920f, 54f), TextAlignmentOptions.Center);

        Button pauseButton = CreateButton(safeAreaObject.transform, "Pause Button", "||", new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(-54f, -128f), new Vector2(112f, 96f), new Color(0.58f, 0.12f, 0.075f, 0.96f), 39);

        Button leftButton = CreateButton(safeAreaObject.transform, "Move Left Button", "<", new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(175f, 182f), new Vector2(138f, 142f), new Color(0.09f, 0.15f, 0.17f, 0.8f), 50);
        Button rightButton = CreateButton(safeAreaObject.transform, "Move Right Button", ">", new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(335f, 182f), new Vector2(138f, 142f), new Color(0.09f, 0.15f, 0.17f, 0.8f), 50);
        Button jumpButton = CreateButton(safeAreaObject.transform, "Jump Button", "JUMP", new Vector2(1f, 0f),
            new Vector2(1f, 0f), new Vector2(-175f, 182f), new Vector2(142f, 142f), new Color(0.09f, 0.15f, 0.17f, 0.8f), 25);
        Button slideButton = CreateButton(safeAreaObject.transform, "Slide Button", "SLIDE", new Vector2(1f, 0f),
            new Vector2(1f, 0f), new Vector2(-335f, 182f), new Vector2(142f, 142f), new Color(0.09f, 0.15f, 0.17f, 0.8f), 24);
        Button shootButton = CreateButton(safeAreaObject.transform, "Shoot Button", "FIRE", new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(210f, 92f), new Color(0.68f, 0.17f, 0.07f, 0.93f), 28);

        GameObject pausePanel = CreatePanelObject(safeAreaObject.transform, "Pause Panel", new Color(0.015f, 0.025f, 0.03f, 0.88f));
        CreatePanel(pausePanel.transform, "Pause Card", new Color(0.08f, 0.12f, 0.13f, 0.99f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 880f), false);
        CreateText(pausePanel.transform, "Pause Title", "PAUSED", 74, new Color(1f, 0.43f, 0.21f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 290f), new Vector2(720f, 120f), TextAlignmentOptions.Center);
        Button resumeButton = CreateButton(pausePanel.transform, "Resume Button", "RESUME", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(620f, 120f), new Color(0.17f, 0.44f, 0.39f, 1f), 32);
        Button restartButton = CreateButton(pausePanel.transform, "Pause Restart Button", "RESTART RUN", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(620f, 120f), new Color(0.58f, 0.19f, 0.1f, 1f), 30);
        Button settingsButton = CreateButton(pausePanel.transform, "Pause Settings Button", "SETTINGS", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(620f, 112f), new Color(0.15f, 0.28f, 0.29f, 1f), 29);
        Button pauseHomeButton = CreateButton(pausePanel.transform, "Pause Home Button", "MAIN MENU", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -330f), new Vector2(620f, 112f), new Color(0.18f, 0.24f, 0.25f, 1f), 29);
        pausePanel.SetActive(false);

        GameObject pauseSettingsPanel = CreatePanelObject(safeAreaObject.transform, "Pause Settings Panel", new Color(0.015f, 0.025f, 0.03f, 0.9f));
        CreatePanel(pauseSettingsPanel.transform, "Pause Settings Card", new Color(0.08f, 0.12f, 0.13f, 0.99f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 760f), false);
        CreateText(pauseSettingsPanel.transform, "Pause Settings Title", "SETTINGS", 62, new Color(1f, 0.43f, 0.21f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(720f, 100f), TextAlignmentOptions.Center);
        CreateText(pauseSettingsPanel.transform, "Pause Volume Label", "MASTER VOLUME", 30, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 95f), new Vector2(660f, 72f), TextAlignmentOptions.Center);
        Slider pauseVolumeSlider = CreateVolumeSlider(pauseSettingsPanel.transform);
        Button settingsBackButton = CreateButton(pauseSettingsPanel.transform, "Settings Back Button", "BACK TO GAME", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(620f, 112f), new Color(0.18f, 0.3f, 0.3f, 1f), 29);
        pauseSettingsPanel.SetActive(false);

        GameObject gameOverPanel = CreatePanelObject(safeAreaObject.transform, "Game Over Panel", new Color(0.015f, 0.025f, 0.03f, 0.92f));
        CreatePanel(gameOverPanel.transform, "Game Over Card", new Color(0.08f, 0.12f, 0.13f, 0.99f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(890f, 1170f), false);
        CreateText(gameOverPanel.transform, "Game Over Title", "RUN OVER", 68, new Color(1f, 0.32f, 0.16f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 430f), new Vector2(780f, 105f), TextAlignmentOptions.Center);
        TMP_Text summary = CreateText(gameOverPanel.transform, "Game Over Summary", "SCORE  0\nDISTANCE  0 m\nZOMBIES  0\nCOINS  0", 36, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(760f, 360f), TextAlignmentOptions.Center);
        Button reviveButton = CreateButton(gameOverPanel.transform, "Revive Button", "REVIVE", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(650f, 112f), new Color(0.2f, 0.48f, 0.4f, 1f), 31);
        Button retryButton = CreateButton(gameOverPanel.transform, "Retry Button", "TRY AGAIN", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -225f), new Vector2(650f, 112f), new Color(0.64f, 0.18f, 0.09f, 1f), 31);
        Button gameOverHomeButton = CreateButton(gameOverPanel.transform, "Game Over Home Button", "MAIN MENU", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -370f), new Vector2(650f, 112f), new Color(0.18f, 0.24f, 0.25f, 1f), 29);
        gameOverPanel.SetActive(false);

        ZombieRunner.UI.UIManager ui = safeAreaObject.AddComponent<ZombieRunner.UI.UIManager>();
        SetSerializedReference(ui, "scoreText", scoreText);
        SetSerializedReference(ui, "distanceText", distanceText);
        SetSerializedReference(ui, "coinText", coinText);
        SetSerializedReference(ui, "ammoText", ammoText);
        SetSerializedReference(ui, "healthText", healthText);
        SetSerializedReference(ui, "powerUpText", powerUpText);
        SetSerializedReference(ui, "healthFill", healthFill);
        SetSerializedReference(ui, "pausePanel", pausePanel);
        SetSerializedReference(ui, "pauseSettingsPanel", pauseSettingsPanel);
        SetSerializedReference(ui, "gameOverPanel", gameOverPanel);
        SetSerializedReference(ui, "gameOverSummary", summary);
        SetSerializedReference(ui, "reviveButton", reviveButton);
        SetSerializedReference(ui, "pauseButton", pauseButton);
        SetSerializedReference(ui, "resumeButton", resumeButton);
        SetSerializedReference(ui, "settingsButton", settingsButton);
        SetSerializedReference(ui, "settingsBackButton", settingsBackButton);
        SetSerializedReference(ui, "settingsVolumeSlider", pauseVolumeSlider);
        SetSerializedReference(ui, "restartButton", restartButton);
        SetSerializedReference(ui, "pauseHomeButton", pauseHomeButton);
        SetSerializedReference(ui, "gameOverRetryButton", retryButton);
        SetSerializedReference(ui, "gameOverHomeButton", gameOverHomeButton);
        SetSerializedReference(ui, "leftButton", leftButton);
        SetSerializedReference(ui, "rightButton", rightButton);
        SetSerializedReference(ui, "jumpButton", jumpButton);
        SetSerializedReference(ui, "slideButton", slideButton);
        SetSerializedReference(ui, "shootButton", shootButton);

        SetSerializedReference(scoreManager, "player", playerObject.transform);
        SetSerializedReference(gameManager, "playerHealth", playerObject.GetComponent<PlayerHealth>());
        SetSerializedReference(gameManager, "scoreManager", scoreManager);
        SetSerializedReference(gameManager, "collectibleManager", collectibleManager);
        SetSerializedReference(gameManager, "powerUpManager", powerUpManager);
    }

    private static void EnsurePauseSettingsPanel(Transform safeArea, ZombieRunner.UI.UIManager ui)
    {
        if (safeArea.Find("Pause Settings Panel") != null)
            return;

        Transform pausePanel = safeArea.Find("Pause Panel");
        if (pausePanel == null)
            return;

        Button homeButton = pausePanel.Find("Pause Home Button") != null
            ? pausePanel.Find("Pause Home Button").GetComponent<Button>()
            : null;
        if (homeButton != null)
        {
            RectTransform homeRect = homeButton.GetComponent<RectTransform>();
            homeRect.anchoredPosition = new Vector2(0f, -330f);
            homeRect.sizeDelta = new Vector2(620f, 112f);
        }

        Button settingsButton = CreateButton(pausePanel, "Pause Settings Button", "SETTINGS", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -190f), new Vector2(620f, 112f), new Color(0.15f, 0.28f, 0.29f, 1f), 29);
        GameObject settingsPanel = CreatePanelObject(safeArea, "Pause Settings Panel", new Color(0.015f, 0.025f, 0.03f, 0.9f));
        CreatePanel(settingsPanel.transform, "Pause Settings Card", new Color(0.08f, 0.12f, 0.13f, 0.99f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 760f), false);
        CreateText(settingsPanel.transform, "Pause Settings Title", "SETTINGS", 62, new Color(1f, 0.43f, 0.21f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(720f, 100f), TextAlignmentOptions.Center);
        CreateText(settingsPanel.transform, "Pause Volume Label", "MASTER VOLUME", 30, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 95f), new Vector2(660f, 72f), TextAlignmentOptions.Center);
        Slider volumeSlider = CreateVolumeSlider(settingsPanel.transform);
        Button backButton = CreateButton(settingsPanel.transform, "Settings Back Button", "BACK TO GAME", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(620f, 112f), new Color(0.18f, 0.3f, 0.3f, 1f), 29);
        settingsPanel.SetActive(false);
        SetSerializedReference(ui, "settingsButton", settingsButton);
        SetSerializedReference(ui, "settingsBackButton", backButton);
        SetSerializedReference(ui, "pauseSettingsPanel", settingsPanel);
        SetSerializedReference(ui, "settingsVolumeSlider", volumeSlider);
    }

    private static GameObject CreateCanvas(string name, Transform parent)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvasObject;
    }

    private static void CreateEventSystem(Transform parent)
    {
        if (parent.GetComponentsInChildren<EventSystem>(true).Length > 0)
            return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(parent, false);
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject instance = new GameObject(name, typeof(RectTransform));
        instance.transform.SetParent(parent, false);
        return instance;
    }

    private static RectTransform ConfigureRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateUIObject(name, parent).GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 position, Vector2 size, bool raycastTarget)
    {
        RectTransform rect = ConfigureRect(parent, name, anchorMin, anchorMax,
            new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f), position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static GameObject CreatePanelObject(Transform parent, string name, Color color)
    {
        Image overlay = CreatePanel(parent, name, color, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
        return overlay.gameObject;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color,
        Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        RectTransform rect = ConfigureRect(parent, name, anchor, anchor, pivot, position, dimensions);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size, Color color, float fontSize)
    {
        RectTransform rect = ConfigureRect(parent, name, anchor, anchor, pivot, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        button.colors = colors;
        TMP_Text text = CreateText(rect, "Label", label, fontSize, Color.white, Vector2.zero, new Vector2(0.5f, 0.5f),
            Vector2.zero, size, TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private static void ConfigureCharacterController(CharacterController controller)
    {
        controller.height = 2f;
        controller.radius = 0.42f;
        controller.center = Vector3.zero;
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;
        controller.skinWidth = 0.08f;
        controller.minMoveDistance = 0f;
    }

    private static void ConfigurePlayerReferences(PlayerController player, CharacterController characterController, Transform visual)
    {
        SerializedObject serialized = new SerializedObject(player);
        SerializedProperty controllerProperty = serialized.FindProperty("characterController");
        SerializedProperty visualProperty = serialized.FindProperty("visualRoot");
        SerializedProperty animatorProperty = serialized.FindProperty("animator");
        if (controllerProperty != null) controllerProperty.objectReferenceValue = characterController;
        if (visualProperty != null) visualProperty.objectReferenceValue = visual;
        if (animatorProperty != null) animatorProperty.objectReferenceValue = visual != null ? visual.GetComponentInChildren<Animator>() : null;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool CanRegenerateScene(string path, string knownRootName)
    {
        if (!System.IO.File.Exists(path))
            return true;

        Scene existing = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        bool isOurs = existing.GetRootGameObjects().Any(item => item.name == knownRootName);
        if (!isOurs)
        {
            Debug.LogError("Refusing to replace an existing scene that is not marked as generated by Zombie Runner: " + path);
            return false;
        }

        return true;
    }

    private static void CreateGameplayEnvironment(Transform parent)
    {
        Transform environment = new GameObject("Environment - City Ruins Placeholder").transform;
        environment.SetParent(parent, false);
        Transform road = new GameObject("Road - Three Lanes").transform;
        road.SetParent(environment, false);

        Material asphalt = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Asphalt.mat");
        Material paint = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_LanePaint.mat");
        Material concrete = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Concrete.mat");
        Material ruins = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Ruins.mat");
        Material metal = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_Metal.mat");
        Material orange = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/WR_HazardOrange.mat");

        CreateBlock("Road Surface", road, new Vector3(0f, -0.16f, 17f), new Vector3(9f, 0.32f, 66f), asphalt);
        CreateBlock("Left Road Curb", road, new Vector3(-4.62f, 0.12f, 17f), new Vector3(0.24f, 0.48f, 66f), concrete);
        CreateBlock("Right Road Curb", road, new Vector3(4.62f, 0.12f, 17f), new Vector3(0.24f, 0.48f, 66f), concrete);
        CreateBlock("Left Shoulder", environment, new Vector3(-6.3f, -0.2f, 17f), new Vector3(3.1f, 0.25f, 66f), concrete);
        CreateBlock("Right Shoulder", environment, new Vector3(6.3f, -0.2f, 17f), new Vector3(3.1f, 0.25f, 66f), concrete);

        for (int lane = -1; lane <= 1; lane += 2)
        {
            for (int dash = 0; dash < 11; dash++)
                CreateBlock("Lane Marking", road, new Vector3(lane * 1.5f, 0.012f, -11f + dash * 5.5f), new Vector3(0.09f, 0.025f, 2.5f), paint, false);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            for (int section = 0; section < 6; section++)
            {
                float z = 4f + section * 11f;
                float x = side * (8.2f + (section % 2) * 1.4f);
                float width = 3.5f + (section % 3) * 1.1f;
                float height = 6f + (section % 2) * 3f;
                CreateBlock("Ruined Building Block", environment,
                    new Vector3(x, height * 0.5f - 0.15f, z), new Vector3(width, height, 8f), ruins);
                CreateBlock("Broken Facade", environment,
                    new Vector3(x - side * 0.2f, height + 0.2f, z + 1.5f), new Vector3(width * 0.72f, 0.35f, 2.5f), concrete, false);
                CreateBlock("Emergency Light", environment,
                    new Vector3(side * 4.48f, 1.5f, z), new Vector3(0.08f, 0.11f, 0.7f), orange, false);
            }
        }

        for (int prop = 0; prop < 4; prop++)
        {
            float z = 10f + prop * 13f;
            float side = prop % 2 == 0 ? -1f : 1f;
            CreateBlock("Collapsed Road Debris", environment,
                new Vector3(side * 5.1f, 0.32f, z), new Vector3(0.6f, 0.55f, 1.3f), metal);
        }

        GameObject systems = new GameObject("Game Systems - Stage 1 placeholders");
        systems.transform.SetParent(parent, false);
    }

    private static void CreatePlayer(Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Player/PlayerPlaceholder.prefab");
        GameObject player = prefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
            : GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player - Capsule Placeholder";
        player.transform.SetParent(parent, false);
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.tag = "Player";
    }

    private static void CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(parent, false);
        cameraObject.transform.position = new Vector3(0f, 5.2f, -8.5f);
        cameraObject.transform.LookAt(new Vector3(0f, 1.1f, 8f));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 140f;
    }

    private static void CreateLighting(Transform parent)
    {
        GameObject lightObject = new GameObject("Directional Light", typeof(Light));
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.73f, 0.53f);
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.17f, 0.19f, 0.22f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.12f, 0.14f, 0.15f);
        RenderSettings.fogStartDistance = 22f;
        RenderSettings.fogEndDistance = 105f;
    }

    private static GameObject CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider = true)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = position;
        block.transform.localScale = scale;
        Renderer renderer = block.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
        if (!collider)
        {
            Collider blockCollider = block.GetComponent<Collider>();
            if (blockCollider != null)
                UnityEngine.Object.DestroyImmediate(blockCollider);
        }
        return block;
    }

    private static void CreateMenuCanvas(Transform parent)
    {
        GameObject canvasObject = CreateCanvas("Main Menu Canvas", parent);
        CreateEventSystem(parent);
        CreatePanel(canvasObject.transform, "Menu Background", new Color(0.025f, 0.043f, 0.05f, 1f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
        CreatePanel(canvasObject.transform, "Warm Light Block", new Color(0.42f, 0.095f, 0.04f, 0.2f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 470f), new Vector2(760f, 620f), false);
        CreatePanel(canvasObject.transform, "Horizon Line", new Color(0.62f, 0.17f, 0.08f, 0.65f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 510f), new Vector2(1080f, 5f), false);

        GameObject safeArea = CreateUIObject("Safe Area", canvasObject.transform);
        RectTransform safeRect = safeArea.GetComponent<RectTransform>();
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = Vector2.zero;
        safeRect.offsetMax = Vector2.zero;
        safeArea.AddComponent<ZombieRunner.UI.SafeAreaFitter>();

        CreateText(safeArea.transform, "Title", "ZOMBIE\nRUNNER", 112, new Color(1f, 0.37f, 0.17f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 485f), new Vector2(900f, 330f), TextAlignmentOptions.Center);
        CreateText(safeArea.transform, "Subtitle", "SURVIVE THE NIGHT", 34, new Color(0.81f, 0.86f, 0.86f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 275f), new Vector2(720f, 70f), TextAlignmentOptions.Center);

        CreatePanel(safeArea.transform, "Run Stats Card", new Color(0.075f, 0.11f, 0.12f, 0.94f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(850f, 220f), false);
        TMP_Text bestScore = CreateText(safeArea.transform, "Best Score", "BEST  0", 37, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-205f, 42f), new Vector2(360f, 92f), TextAlignmentOptions.Center);
        TMP_Text totalCoins = CreateText(safeArea.transform, "Total Coins", "BANKED COINS  0", 31, new Color(1f, 0.78f, 0.29f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(215f, 42f), new Vector2(430f, 92f), TextAlignmentOptions.Center);
        CreateText(safeArea.transform, "Stats Divider", "----------------", 20, new Color(0.42f, 0.53f, 0.54f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740f, 44f), TextAlignmentOptions.Center);
        CreateText(safeArea.transform, "Stats Hint", "DISTANCE  |  KILLS  |  COINS", 22, new Color(0.63f, 0.71f, 0.72f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -38f), new Vector2(700f, 50f), TextAlignmentOptions.Center);

        Button playButton = CreateButton(safeArea.transform, "Play Button", "RUN", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(700f, 132f), new Color(0.72f, 0.17f, 0.07f, 1f), 40);
        Button settingsButton = CreateButton(safeArea.transform, "Settings Button", "SETTINGS", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -355f), new Vector2(700f, 112f), new Color(0.14f, 0.22f, 0.23f, 1f), 30);
        CreateText(safeArea.transform, "Control Hint", "SWIPE TO DODGE  |  TAP FIRE TO SHOOT", 22, new Color(0.62f, 0.7f, 0.71f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(900f, 56f), TextAlignmentOptions.Center);

        GameObject settingsPanel = CreatePanelObject(safeArea.transform, "Settings Panel", new Color(0.01f, 0.02f, 0.025f, 0.91f));
        CreatePanel(settingsPanel.transform, "Settings Card", new Color(0.075f, 0.11f, 0.12f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850f, 760f), false);
        CreateText(settingsPanel.transform, "Settings Title", "SETTINGS", 62, new Color(1f, 0.4f, 0.2f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(700f, 95f), TextAlignmentOptions.Center);
        CreateText(settingsPanel.transform, "Volume Label", "MASTER VOLUME", 31, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 95f), new Vector2(650f, 72f), TextAlignmentOptions.Center);
        Slider volumeSlider = CreateVolumeSlider(settingsPanel.transform);
        Button closeSettings = CreateButton(settingsPanel.transform, "Close Settings Button", "BACK", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -220f), new Vector2(620f, 112f), new Color(0.18f, 0.3f, 0.3f, 1f), 31);
        settingsPanel.SetActive(false);

        ZombieRunner.UI.MainMenuController menu = safeArea.AddComponent<ZombieRunner.UI.MainMenuController>();
        SetSerializedReference(menu, "bestScoreText", bestScore);
        SetSerializedReference(menu, "totalCoinsText", totalCoins);
        SetSerializedReference(menu, "settingsPanel", settingsPanel);
        SetSerializedReference(menu, "volumeSlider", volumeSlider);
        SetSerializedReference(menu, "playButton", playButton);
        SetSerializedReference(menu, "settingsButton", settingsButton);
        SetSerializedReference(menu, "closeSettingsButton", closeSettings);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
        if (current.Any(scene => scene.path == scenePath))
            return;

        var scenes = current.ToList();
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void ReorderBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        var ordered = new List<EditorBuildSettingsScene>();
        foreach (string preferredPath in new[] { MainMenuScenePath, GameplayScenePath })
        {
            EditorBuildSettingsScene match = scenes.FirstOrDefault(scene => scene.path == preferredPath);
            if (match != null)
                ordered.Add(match);
        }
        ordered.AddRange(scenes.Where(scene => scene.path != MainMenuScenePath && scene.path != GameplayScenePath));
        EditorBuildSettings.scenes = ordered.ToArray();
    }

    private static Slider CreateVolumeSlider(Transform parent)
    {
        RectTransform rect = ConfigureRect(parent, "Volume Slider", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(620f, 50f));
        Image background = rect.gameObject.AddComponent<Image>();
        background.color = new Color(0.035f, 0.055f, 0.06f, 1f);
        Slider slider = rect.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        slider.direction = Slider.Direction.LeftToRight;
        slider.targetGraphic = background;

        RectTransform fillArea = ConfigureRect(rect, "Fill Area", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-18f, -20f));
        Image fill = fillArea.gameObject.AddComponent<Image>();
        fill.color = new Color(0.92f, 0.31f, 0.12f, 1f);
        slider.fillRect = fillArea;

        RectTransform handle = ConfigureRect(rect, "Handle", new Vector2(0.8f, 0.5f), new Vector2(0.8f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(42f, 68f));
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        return slider;
    }
}
