using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieRunner.UI;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Ensures MenuIdle_Animator is cleanly generated via Unity's native AnimatorController API
    /// and assigned to the Menu_Survivor character in the main menu scene.
    /// Eliminates invalid fileIDs, HumanTemplate dereferencing mismatches, and missing state machine errors.
    /// Safely skips scene modifications while Unity is in Play Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class MenuAnimatorFixer
    {
        private const string ControllerPath = "Assets/ZombieRunner/Animations/MenuIdle_Animator.controller";
        private const string ClipPath = "Assets/ZombieRunner/Characters/Player/NewCharacter/Animations/rifle aiming idle.fbx";
        private const string ControllerGuid = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6";

        static MenuAnimatorFixer()
        {
            EditorApplication.delayCall += EnsureValidAnimator;
        }

        [MenuItem("Zombie Runner/Fix Menu Animator & Character")]
        public static void EnsureValidAnimator()
        {
            // Do not perform editor scene modifications while in play mode or transitioning
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            {
                return;
            }

            bool needsRebuild = false;
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

            if (controller == null || controller.layers == null || controller.layers.Length == 0)
            {
                needsRebuild = true;
            }
            else
            {
                try
                {
                    var sm = controller.layers[0].stateMachine;
                    if (sm == null || sm.states == null || sm.states.Length == 0)
                    {
                        needsRebuild = true;
                    }
                }
                catch
                {
                    needsRebuild = true;
                }
            }

            if (needsRebuild)
            {
                RebuildController();
            }
            else
            {
                ApplyToSceneSurvivor();
            }
        }

        public static void RebuildController()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            {
                return;
            }

            // Find rifle aiming idle clip
            AnimationClip idleClip = null;
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(ClipPath);
            if (subAssets != null)
            {
                foreach (var asset in subAssets)
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        idleClip = clip;
                        break;
                    }
                }
            }

            // Create controller using official Unity API
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            if (ctrl != null && ctrl.layers.Length > 0)
            {
                var rootSm = ctrl.layers[0].stateMachine;
                if (rootSm != null)
                {
                    var idleState = rootSm.AddState("RifleAimIdle");
                    if (idleClip != null)
                    {
                        idleState.motion = idleClip;
                    }
                    rootSm.defaultState = idleState;
                }
                EditorUtility.SetDirty(ctrl);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Ensure GUID remains stable for existing scene references
            EnsureGuid();

            ApplyToSceneSurvivor();
            Debug.Log("[MenuAnimatorFixer] Successfully verified/rebuilt MenuIdle_Animator.controller cleanly.");
        }

        private static void EnsureGuid()
        {
            string metaPath = ControllerPath + ".meta";
            if (File.Exists(metaPath))
            {
                string text = File.ReadAllText(metaPath);
                if (!text.Contains("guid: " + ControllerGuid))
                {
                    // Update GUID to match the one referenced in scenes
                    text = System.Text.RegularExpressions.Regex.Replace(
                        text,
                        @"guid:\s*[0-9a-fA-F]+",
                        "guid: " + ControllerGuid);
                    File.WriteAllText(metaPath, text);
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void ApplyToSceneSurvivor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            {
                return;
            }

            GameObject survivor = GameObject.Find("Menu_Survivor");
            if (survivor != null)
            {
                Animator anim = survivor.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
                    if (ctrl != null)
                    {
                        anim.runtimeAnimatorController = ctrl;
                        anim.enabled = true;
                        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                        anim.Rebind();
                        anim.Update(0f);
                        EditorUtility.SetDirty(anim);
                    }
                }

                MenuWeaponSocket weaponSocket = survivor.GetComponent<MenuWeaponSocket>();
                if (weaponSocket == null)
                {
                    weaponSocket = survivor.AddComponent<MenuWeaponSocket>();
                }
                weaponSocket.AttachWeapon();
                EditorUtility.SetDirty(weaponSocket);

                if (survivor.scene.IsValid() && survivor.scene.isLoaded)
                {
                    EditorSceneManager.MarkSceneDirty(survivor.scene);
                }
            }
        }
    }
}
