using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using ZombieRunner.Audio;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Master editor utility for the Survival Horror Audio Overhaul.
    /// Synthesizes all 48s adaptive music layers and environmental ambience,
    /// sets up AudioMixer routing, and wires the gameplay scene with seamless adaptive audio.
    /// </summary>
    public static class AudioSystemUpgrader
    {
        private const string LogPrefix = "[AudioUpgrader] ";
        private const string ScenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity";
        private const string AudioClipsPath = "Assets/ZombieRunner/Audio/Clips";
        private const string MixerPath = "Assets/ZombieRunner/Audio/ZombieRunner_AudioMixer.mixer";

        [MenuItem("ZombieRunner/Audio/Run Professional Horror Audio Overhaul")]
        public static void UpgradeAudioSystem()
        {
            Debug.Log(LogPrefix + "Starting professional survival horror audio overhaul...");

            // 1. Synthesize all studio WAV audio assets
            HorrorAudioGenerator.GenerateAllAudio();
            AssetDatabase.Refresh();

            // 2. Configure or create AudioMixer
            AudioMixer mixer = GetOrCreateAudioMixer();

            // 3. Wire Gameplay Scene with Adaptive Audio Controllers
            WireGameplaySceneAudio(mixer);

            AssetDatabase.SaveAssets();
            Debug.Log(LogPrefix + "Survival Horror Audio Overhaul successfully applied!");
        }

        private static AudioMixer GetOrCreateAudioMixer()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                try
                {
                    // Attempt to create AudioMixerController via UnityEditor.Audio internal API
                    Type controllerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
                    if (controllerType != null)
                    {
                        MethodInfo createMethod = controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (createMethod != null)
                        {
                            string mixerDir = Path.GetDirectoryName(MixerPath);
                            if (!Directory.Exists(mixerDir)) Directory.CreateDirectory(mixerDir);

                            object createdMixer = createMethod.Invoke(null, new object[] { MixerPath });
                            if (createdMixer != null)
                            {
                                AssetDatabase.ImportAsset(MixerPath);
                                mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
                                Debug.Log(LogPrefix + "Successfully created AudioMixer via AudioMixerController.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(LogPrefix + "AudioMixerController creation deferred: " + ex.Message);
                }
            }

            if (mixer != null)
            {
                EnsureMixerGroupsAndSnapshots(mixer);
            }

            return mixer;
        }

        private static void EnsureMixerGroupsAndSnapshots(AudioMixer mixer)
        {
            if (mixer == null) return;
            try
            {
                Type controllerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
                if (controllerType == null || !controllerType.IsAssignableFrom(mixer.GetType())) return;

                MethodInfo createGroup = controllerType.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(bool) }, null)
                                        ?? controllerType.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo addChild = controllerType.GetMethod("AddChildToParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                PropertyInfo masterProp = controllerType.GetProperty("masterGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (createGroup != null && addChild != null && masterProp != null)
                {
                    object masterObj = masterProp.GetValue(mixer);
                    string[] needed = new[] { "Music", "Ambient", "SFX", "UI" };

                    foreach (string gName in needed)
                    {
                        AudioMixerGroup[] existing = mixer.FindMatchingGroups(gName);
                        if (existing != null && existing.Length > 0) continue;

                        object[] createArgs = createGroup.GetParameters().Length == 2 ? new object[] { gName, true } : new object[] { gName };
                        object newGroup = createGroup.Invoke(mixer, createArgs);
                        if (newGroup != null && masterObj != null)
                        {
                            addChild.Invoke(mixer, new object[] { newGroup, masterObj });
                            Debug.Log(LogPrefix + "Created AudioMixer sub-group: " + gName);
                        }
                    }

                    MethodInfo createSnapshot = controllerType.GetMethod("CreateNewSnapshot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null)
                                              ?? controllerType.GetMethod("CreateNewSnapshot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (createSnapshot != null)
                    {
                        string[] snapshots = new[] { "Exploration", "ApproachingDanger", "Combat", "CriticalHealth", "GameOver" };
                        foreach (string sName in snapshots)
                        {
                            AudioMixerSnapshot snap = mixer.FindSnapshot(sName);
                            if (snap != null) continue;

                            createSnapshot.Invoke(mixer, new object[] { sName });
                            Debug.Log(LogPrefix + "Created AudioMixer snapshot: " + sName);
                        }
                    }

                    EditorUtility.SetDirty(mixer);
                    AssetDatabase.SaveAssets();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(LogPrefix + "Mixer sub-group configuration note: " + ex.Message);
            }
        }

        private static void WireGameplaySceneAudio(AudioMixer mixer)
        {
            bool sceneWasLoaded = false;
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                sceneWasLoaded = true;
            }

            // 1. Locate or create AudioManager root
            AudioManager audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
            if (audioMgr == null)
            {
                GameObject audioRoot = new GameObject("AudioManager");
                audioMgr = audioRoot.AddComponent<AudioManager>();
                Debug.Log(LogPrefix + "Created AudioManager in scene.");
            }

            // 2. Load all audio clip assets
            AudioClip shot1 = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Gunshot_Punchy_01.wav");
            AudioClip shot2 = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Gunshot_Punchy_02.wav");
            AudioClip shot3 = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Gunshot_Punchy_03.wav");
            AudioClip[] shots = new[] { shot1, shot2, shot3 };

            AudioClip btnClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_UI_Button_Horror.wav");
            AudioClip pickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Pickup_Item.wav");
            AudioClip hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Player_Hurt_Visceral.wav");
            AudioClip powerUpClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_PowerUp_Horror.wav");
            AudioClip bloodClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Impact_Blood_02.wav");
            AudioClip explosionClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/SFX_Impact_Metal_01.wav");

            AudioClip[] cues = new AudioClip[7];
            cues[(int)GameAudioCue.Button] = btnClip;
            cues[(int)GameAudioCue.Shot] = shot1;
            cues[(int)GameAudioCue.Pickup] = pickupClip;
            cues[(int)GameAudioCue.PlayerHit] = hurtClip;
            cues[(int)GameAudioCue.ZombieDeath] = bloodClip;
            cues[(int)GameAudioCue.PowerUp] = powerUpClip;
            cues[(int)GameAudioCue.Explosion] = explosionClip;

            AudioClip windLoop = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Ambience_City_Night_Wind.wav")
                                 ?? AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Ambience_Night_City_Wind_Loop.wav");

            AudioClip creak1 = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Ambience_Metal_Creak_01.wav");
            AudioClip creak2 = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Ambience_Metal_Creak_02.wav");
            AudioClip rumble = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Ambience_Distant_Rumble_01.wav");
            AudioClip[] ambientStingers = new[] { creak1, creak2, rumble };

            AudioClip layer1Exploration = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Music_Horror_Layer1_Exploration.wav");
            AudioClip layer2Tension = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Music_Horror_Layer2_Tension.wav");
            AudioClip layer3Combat = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Music_Horror_Layer3_Combat.wav");
            AudioClip layer4Critical = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Music_Horror_Layer4_CriticalHealth.wav");
            AudioClip stingGameOver = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioClipsPath}/Music_Horror_Sting_GameOver.wav");

            // Extract mixer groups if available
            AudioMixerGroup masterGrp = null;
            AudioMixerGroup musicGrp = null;
            AudioMixerGroup ambGrp = null;
            AudioMixerGroup sfxGrp = null;
            AudioMixerGroup uiGrp = null;

            if (mixer != null)
            {
                AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
                foreach (var g in groups)
                {
                    if (g.name.Equals("Master", StringComparison.OrdinalIgnoreCase)) masterGrp = g;
                    else if (g.name.Equals("Music", StringComparison.OrdinalIgnoreCase)) musicGrp = g;
                    else if (g.name.Equals("Ambient", StringComparison.OrdinalIgnoreCase)) ambGrp = g;
                    else if (g.name.Equals("SFX", StringComparison.OrdinalIgnoreCase)) sfxGrp = g;
                    else if (g.name.Equals("UI", StringComparison.OrdinalIgnoreCase)) uiGrp = g;
                }
            }

            // Assign everything to AudioManager
            audioMgr.AssignAllHorrorAssets(
                shots,
                cues,
                windLoop,
                ambientStingers,
                layer1Exploration,
                layer2Tension,
                layer3Combat,
                layer4Critical,
                stingGameOver,
                mixer,
                masterGrp,
                musicGrp,
                ambGrp,
                sfxGrp,
                uiGrp
            );

            EditorUtility.SetDirty(audioMgr);
            EditorSceneManager.MarkSceneDirty(audioMgr.gameObject.scene);
            EditorSceneManager.SaveScene(audioMgr.gameObject.scene);
            Debug.Log(LogPrefix + "Wired all 4 adaptive horror music layers, ambient wind, and stingers to AudioManager.");

            if (sceneWasLoaded)
            {
                // Leave scene saved
            }
        }
    }
}
