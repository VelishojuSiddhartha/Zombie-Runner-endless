using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ZombieRunner.Player;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    [InitializeOnLoad]
    public static class VisualAuditHelper
    {
        private const string ScreenshotDir = "Screenshots";
        private const string PrefKey_RunPlayModeAudit = "ZombieRunner_RunSequenceAudit_Requested";
        private const string PrefKey_RunMainMenuAudit = "ZombieRunner_RunMainMenuAudit_Requested";
        private const string AutoAuditKey = "ZombieRunner_TriggerMenuAudit_Once_v3";
        private const string MainMenuScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerMainMenu.unity";
        private const string BrainDir = @"C:\Users\metra\.gemini\antigravity\brain\fe7e4668-e2ec-41a7-86ac-d4f01ea7c586";

        static VisualAuditHelper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += CheckAutoMainMenuAudit;
        }

        private static void CheckAutoMainMenuAudit()
        {
            if (SessionState.GetBool(AutoAuditKey, false)) return;
            SessionState.SetBool(AutoAuditKey, true);

            EditorApplication.delayCall += () =>
            {
                StartMainMenuVisualAudit();
            };
        }

        [MenuItem("ZombieRunner/Visual Audit/Render Current Camera View (PNG)")]
        public static void RenderCameraView()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ScreenshotDir));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, "Audit_Polished_Camera_Render.png");

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[VisualAudit] Main Camera not found!");
                return;
            }

            int w = 1920;
            int h = 1080;
            RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4
            };
            RenderTexture prevRt = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevRt;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"[VisualAudit] Rendered Camera Screenshot saved to: {filePath}");

            try
            {
                if (Directory.Exists(BrainDir))
                {
                    File.Copy(filePath, Path.Combine(BrainDir, "Audit_Polished_Camera_Render.png"), true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VisualAudit] Failed copying to Brain dir: {ex.Message}");
            }
        }

        [MenuItem("ZombieRunner/Visual Audit/Run Main Menu Visual Audit")]
        public static void StartMainMenuVisualAudit()
        {
            if (EditorSceneManager.GetActiveScene().path != MainMenuScenePath)
            {
                EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            }

            SessionState.SetBool(PrefKey_RunMainMenuAudit, true);
            SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 0);
            SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("ZombieRunner/Visual Audit/Run Endless Sequence Visual Audit")]
        public static void StartSequenceVisualAudit()
        {
            SessionState.SetBool(PrefKey_RunPlayModeAudit, true);
            SessionState.SetInt("ZombieRunner_Audit_Stage", 0);
            SessionState.SetInt("ZombieRunner_Audit_StageTimer", 0);

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (SessionState.GetBool(PrefKey_RunMainMenuAudit, false))
                {
                    SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 0);
                    SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    Debug.Log("[VisualAudit] Started Main Menu & Settings Visual Audit in PlayMode.");
                }

                if (SessionState.GetBool(PrefKey_RunPlayModeAudit, false))
                {
                    SessionState.SetInt("ZombieRunner_Audit_Stage", 0);
                    SessionState.SetInt("ZombieRunner_Audit_StageTimer", 0);
                    Debug.Log("[VisualAudit] Started Endless City <-> Tunnel Sequence Visual Audit in PlayMode.");
                }
            }
        }

        private static void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying) return;

            // 1. MAIN MENU AUDIT
            if (SessionState.GetBool(PrefKey_RunMainMenuAudit, false))
            {
                var menu = Object.FindAnyObjectByType<ZombieRunner.UI.MainMenuController>();
                int mStage = SessionState.GetInt("ZombieRunner_MenuAudit_Stage", 0);
                int mTimer = SessionState.GetInt("ZombieRunner_MenuAudit_Timer", 0) + 1;
                SessionState.SetInt("ZombieRunner_MenuAudit_Timer", mTimer);

                // Stage 0: Overview of Main Menu (Survivor + 3D street + Left Buttons)
                if (mStage == 0)
                {
                    if (mTimer == 40)
                    {
                        CaptureAuditFrame("Audit_MainMenu_1_Overview");
                    }
                    else if (mTimer >= 46)
                    {
                        if (menu != null) menu.OpenSettings();
                        SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 1);
                        SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    }
                }
                // Stage 1: Settings - Graphics Tab
                else if (mStage == 1)
                {
                    if (mTimer == 30)
                    {
                        CaptureAuditFrame("Audit_MainMenu_2_Settings_Graphics");
                    }
                    else if (mTimer >= 36)
                    {
                        if (menu != null) menu.SwitchSettingsTab(1); // Audio Tab
                        SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 2);
                        SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    }
                }
                // Stage 2: Settings - Audio Tab
                else if (mStage == 2)
                {
                    if (mTimer == 30)
                    {
                        CaptureAuditFrame("Audit_MainMenu_3_Settings_Audio");
                    }
                    else if (mTimer >= 36)
                    {
                        if (menu != null) menu.SwitchSettingsTab(2); // Controls Tab
                        SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 3);
                        SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    }
                }
                // Stage 3: Settings - Controls Tab
                else if (mStage == 3)
                {
                    if (mTimer == 30)
                    {
                        CaptureAuditFrame("Audit_MainMenu_4_Settings_Controls");
                    }
                    else if (mTimer >= 36)
                    {
                        if (menu != null)
                        {
                            menu.CloseSettings();
                            menu.OpenCredits();
                        }
                        SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 4);
                        SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    }
                }
                // Stage 4: Credits Modal
                else if (mStage == 4)
                {
                    if (mTimer == 30)
                    {
                        CaptureAuditFrame("Audit_MainMenu_5_Credits");
                    }
                    else if (mTimer >= 36)
                    {
                        if (menu != null)
                        {
                            menu.CloseCredits();
                            menu.PlayGame(); // Test seamless scene transition into ZombieRunnerGameplay!
                        }
                        SessionState.SetInt("ZombieRunner_MenuAudit_Stage", 5);
                        SessionState.SetInt("ZombieRunner_MenuAudit_Timer", 0);
                    }
                }
                // Stage 5: Gameplay Transition
                else if (mStage == 5)
                {
                    if (mTimer == 50)
                    {
                        CaptureAuditFrame("Audit_MainMenu_6_GameplayTransition");
                    }
                    else if (mTimer >= 60)
                    {
                        SessionState.SetBool(PrefKey_RunMainMenuAudit, false);
                        Debug.Log("[VisualAudit] Main Menu & Settings Visual Audit Finished Successfully! Stopping PlayMode...");
                        EditorApplication.isPlaying = false;
                    }
                }
                return;
            }

            // 2. GAMEPLAY SEQUENCE AUDIT
            if (!SessionState.GetBool(PrefKey_RunPlayModeAudit, false)) return;

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null) return;

            int stage = SessionState.GetInt("ZombieRunner_Audit_Stage", 0);
            int timer = SessionState.GetInt("ZombieRunner_Audit_StageTimer", 0) + 1;
            SessionState.SetInt("ZombieRunner_Audit_StageTimer", timer);

            // Audit stages to capture:
            // Stage 0: Frame 60 -> Stage 1: City Run (Z ~ 20m)
            // Stage 1: Set player to Z = 80m, wait 30 frames -> Stage 2: Approaching Tunnel Entrance (Z ~ 90m)
            // Stage 2: Set player to Z = 135m, wait 30 frames -> Stage 3: Tunnel Interior (Z ~ 140m)
            // Stage 3: Set player to Z = 188m, wait 30 frames -> Stage 4: Emerging at Tunnel Exit (Z ~ 195m)
            // Stage 4: Set player to Z = 220m, wait 30 frames -> Stage 5: Returning to City (Z ~ 225m)

            if (stage == 0 && timer >= 60)
            {
                CaptureAuditFrame("Audit_Stage1_CityRuins");
                AdvanceStage(player, 1, 80f);
            }
            else if (stage == 1 && timer >= 30)
            {
                CaptureAuditFrame("Audit_Stage2_TunnelEntrance");
                AdvanceStage(player, 2, 135f);
            }
            else if (stage == 2 && timer >= 30)
            {
                CaptureAuditFrame("Audit_Stage3_TunnelInterior");
                AdvanceStage(player, 3, 188f);
            }
            else if (stage == 3 && timer >= 30)
            {
                CaptureAuditFrame("Audit_Stage4_TunnelExit");
                AdvanceStage(player, 4, 220f);
            }
            else if (stage == 4 && timer >= 30)
            {
                CaptureAuditFrame("Audit_Stage5_CityReturn");
                SessionState.SetBool(PrefKey_RunPlayModeAudit, false);
                Debug.Log("[VisualAudit] All 5 Sequence Audit Frames Captured Successfully! Stopping PlayMode...");
                EditorApplication.isPlaying = false;
            }
        }

        private static void AdvanceStage(PlayerController player, int nextStage, float targetZ)
        {
            SessionState.SetInt("ZombieRunner_Audit_Stage", nextStage);
            SessionState.SetInt("ZombieRunner_Audit_StageTimer", 0);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(0f, 0f, targetZ);
            if (cc != null) cc.enabled = true;

            // Update camera position immediately
            RunnerCameraFollow cam = Object.FindAnyObjectByType<RunnerCameraFollow>();
            if (cam != null)
            {
                cam.transform.position = player.transform.position + new Vector3(0f, 2.5f, -4.5f);
            }

            Debug.Log($"[VisualAudit] Advanced to Stage {nextStage} at Z = {targetZ:F1}m");
        }

        private static void CaptureAuditFrame(string baseName)
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ScreenshotDir));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, $"{baseName}.png");

            ScreenCapture.CaptureScreenshot(filePath);
            Debug.Log($"[VisualAudit] Saved capture: {filePath}");

            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (File.Exists(filePath) && Directory.Exists(BrainDir))
                    {
                        File.Copy(filePath, Path.Combine(BrainDir, $"{baseName}.png"), true);
                        Debug.Log($"[VisualAudit] Copied {baseName}.png to Brain artifacts.");
                    }
                }
                catch { }
            };
        }
    }
}
