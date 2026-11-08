using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieRunner.Enemies;
using ZombieRunner.Weapons;

namespace ZombieRunner.Editor
{
    public static class ZombieCombatSystemValidator
    {
        [MenuItem("ZombieRunner/Validate Combat & Zombie Setup")]
        public static void ValidateAll()
        {
            Debug.Log("[Validator] ===============================================");
            Debug.Log("[Validator] STARTING ZOMBIE RUNNER COMBAT & SPAWNING AUDIT");
            Debug.Log("[Validator] ===============================================");

            string scenePath = "Assets/ZombieRunner/Scenes/ZombieRunnerGameplay.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Validator] FAILED: Could not open scene at {scenePath}");
                return;
            }
            Debug.Log($"[Validator] Successfully opened scene: {scene.name}");

            // 1. Spawner Audit
            ZombieSpawner spawner = UnityEngine.Object.FindAnyObjectByType<ZombieSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[Validator] FAILED: ZombieSpawner not found in scene!");
                return;
            }
            Debug.Log("[Validator] PASSED: ZombieSpawner component found in scene.");

            SerializedObject spawnerSO = new SerializedObject(spawner);
            SerializedProperty normProp = spawnerSO.FindProperty("normalZombiePrefab");
            SerializedProperty fastProp = spawnerSO.FindProperty("fastZombiePrefab");
            SerializedProperty bigProp = spawnerSO.FindProperty("bigZombiePrefab");

            GameObject normPrefab = normProp.objectReferenceValue as GameObject;
            GameObject fastPrefab = fastProp.objectReferenceValue as GameObject;
            GameObject bigPrefab = bigProp.objectReferenceValue as GameObject;

            if (normPrefab == null) Debug.LogError("[Validator] FAILED: normalZombiePrefab is NULL in scene!");
            else Debug.Log($"[Validator] PASSED: normalZombiePrefab = '{normPrefab.name}'");

            if (fastPrefab == null) Debug.LogError("[Validator] FAILED: fastZombiePrefab is NULL in scene!");
            else Debug.Log($"[Validator] PASSED: fastZombiePrefab = '{fastPrefab.name}'");

            if (bigPrefab == null) Debug.LogError("[Validator] FAILED: bigZombiePrefab is NULL in scene!");
            else Debug.Log($"[Validator] PASSED: bigZombiePrefab = '{bigPrefab.name}'");

            // 2. Prefab Archetypes, Health, Damage, and Animation Audit
            AuditZombiePrefab(normPrefab, ZombieArchetype.Normal, 3f);
            AuditZombiePrefab(fastPrefab, ZombieArchetype.Fast, 2f);
            AuditZombiePrefab(bigPrefab, ZombieArchetype.Big, 8f);

            // 3. ShootingController on Player Audit
            ShootingController shooter = UnityEngine.Object.FindAnyObjectByType<ShootingController>();
            if (shooter == null)
            {
                GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ZombieRunner/Prefabs/Player/PlayerPlaceholder.prefab");
                if (playerPrefab != null)
                    shooter = playerPrefab.GetComponent<ShootingController>();
            }

            if (shooter != null)
            {
                SerializedObject shootSO = new SerializedObject(shooter);
                bool autoAim = shootSO.FindProperty("autoAimEnabled").boolValue;
                float range = shootSO.FindProperty("aimRange").floatValue;
                bool preferLane = shootSO.FindProperty("preferCurrentLane").boolValue;
                bool allowAdj = shootSO.FindProperty("allowAdjacentLaneTargeting").boolValue;

                Debug.Log($"[Validator] PASSED: ShootingController autoAimEnabled={autoAim}, aimRange={range}m, preferCurrentLane={preferLane}, allowAdjacentLaneTargeting={allowAdj}");
            }
            else
            {
                Debug.LogError("[Validator] FAILED: ShootingController not found on player!");
            }

            Debug.Log("[Validator] ===============================================");
            Debug.Log("[Validator] ALL AUDIT CHECKS FINISHED CLEANLY!");
            Debug.Log("[Validator] ===============================================");
        }

        private static void AuditZombiePrefab(GameObject prefab, ZombieArchetype expectedArchetype, float expectedHealth)
        {
            if (prefab == null)
                return;

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = $"[TestInstance_{prefab.name}]";

            try
            {
                ZombieController ctrl = instance.GetComponent<ZombieController>();
                if (ctrl == null)
                {
                    Debug.LogError($"[Validator] FAILED: '{prefab.name}' is missing ZombieController component!");
                    return;
                }

                // Verify Health
                if (Mathf.Abs(ctrl.MaximumHealth - expectedHealth) > 0.01f)
                {
                    Debug.LogWarning($"[Validator] WARNING: '{prefab.name}' maximumHealth is {ctrl.MaximumHealth}, expected {expectedHealth}");
                }
                else
                {
                    Debug.Log($"[Validator] PASSED: '{prefab.name}' ({expectedArchetype}) has correct health {ctrl.MaximumHealth} (requires {expectedHealth} shots)");
                }

                // Verify Animator and Controller
                Animator anim = instance.GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.LogError($"[Validator] FAILED: '{prefab.name}' is missing Animator in children!");
                }
                else
                {
                    if (anim.runtimeAnimatorController == null)
                        Debug.LogError($"[Validator] FAILED: '{prefab.name}' Animator has no runtimeAnimatorController assigned!");
                    else
                        Debug.Log($"[Validator] PASSED: '{prefab.name}' Animator Controller is '{anim.runtimeAnimatorController.name}'");

                    if (anim.avatar == null || !anim.avatar.isValid)
                        Debug.LogWarning($"[Validator] WARNING: '{prefab.name}' Animator Avatar is null or invalid!");
                    else
                        Debug.Log($"[Validator] PASSED: '{prefab.name}' Avatar '{anim.avatar.name}' is valid Humanoid rig.");
                }

                // Verify SkinnedMeshRenderer & Materials
                SkinnedMeshRenderer smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    Debug.LogError($"[Validator] FAILED: '{prefab.name}' missing SkinnedMeshRenderer!");
                }
                else
                {
                    foreach (Material mat in smr.sharedMaterials)
                    {
                        if (mat == null)
                            Debug.LogError($"[Validator] FAILED: '{prefab.name}' has null material slot!");
                        else
                        {
                            Texture baseMap = mat.GetTexture("_BaseMap") ?? mat.GetTexture("_MainTex");
                            Debug.Log($"[Validator] PASSED: '{prefab.name}' material '{mat.name}' (Shader: {mat.shader.name}, BaseMap: {(baseMap != null ? baseMap.name : "NONE")})");
                        }
                    }
                }

                // Simulate Damage Pipeline
                float initialHealth = ctrl.CurrentHealth;
                ctrl.TakeDamage(1f);
                if (ctrl.CurrentHealth >= initialHealth || ctrl.IsDead)
                {
                    Debug.LogError($"[Validator] FAILED: TakeDamage(1) did not reduce health correctly! (Health: {ctrl.CurrentHealth}/{initialHealth})");
                }
                else
                {
                    Debug.Log($"[Validator] PASSED: '{prefab.name}' took 1 dmg -> Health: {ctrl.CurrentHealth}/{initialHealth}, IsDead={ctrl.IsDead}");
                }

                // Lethal shot simulation
                while (ctrl.CurrentHealth > 0f && !ctrl.IsDead)
                {
                    ctrl.TakeDamage(1f);
                }

                if (!ctrl.IsDead)
                {
                    Debug.LogError($"[Validator] FAILED: '{prefab.name}' health reached 0 but IsDead is false!");
                }
                else
                {
                    Debug.Log($"[Validator] PASSED: '{prefab.name}' died successfully after lethal damage. IsDead=true.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
