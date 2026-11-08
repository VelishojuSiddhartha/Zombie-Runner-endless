using UnityEditor;
using UnityEngine;
using ZombieRunner.Enemies;

namespace ZombieRunner.Editor
{
    public static class ZombieMaterialAuditor
    {
        [MenuItem("ZombieRunner/Audit Zombie Materials & Textures")]
        public static void Audit()
        {
            Debug.Log("==========================================================");
            Debug.Log("DETAILED ZOMBIE MATERIAL & TEXTURE AUDIT");
            Debug.Log("==========================================================");

            string[] prefabPaths = new[]
            {
                "Assets/ZombieRunner/Prefabs/Zombies/CommonZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/FastZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/HeavyZombie.prefab"
            };

            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"Could not load prefab at {path}");
                    continue;
                }

                Debug.Log($"\n>>> PREFAB: {prefab.name} ({path})");
                SkinnedMeshRenderer[] smrs = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    Debug.Log($"  Renderer: '{smr.name}' (Root: '{smr.transform.root.name}')");
                    Mesh mesh = smr.sharedMesh;
                    Debug.Log($"    Mesh: '{(mesh != null ? mesh.name : "NULL")}', SubMeshCount: {(mesh != null ? mesh.subMeshCount : 0)}");

                    Material[] mats = smr.sharedMaterials;
                    Debug.Log($"    SharedMaterials Count: {mats.Length}");
                    for (int i = 0; i < mats.Length; i++)
                    {
                        Material mat = mats[i];
                        if (mat == null)
                        {
                            Debug.LogError($"      Slot [{i}]: NULL MATERIAL!");
                            continue;
                        }

                        Texture baseMap = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
                        Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                        Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                        Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.magenta;
                        Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.magenta;
                        int workflow = mat.HasProperty("_WorkflowMode") ? mat.GetInt("_WorkflowMode") : -1;
                        float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : -1f;
                        float smoothness = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : -1f;

                        Debug.Log($"      Slot [{i}]: '{mat.name}' (Shader: '{mat.shader.name}')");
                        Debug.Log($"        _BaseMap: '{(baseMap != null ? baseMap.name : "NULL")}' (Path: {(baseMap != null ? AssetDatabase.GetAssetPath(baseMap) : "N/A")})");
                        Debug.Log($"        _MainTex: '{(mainTex != null ? mainTex.name : "NULL")}'");
                        Debug.Log($"        _BumpMap: '{(bumpMap != null ? bumpMap.name : "NULL")}' (Path: {(bumpMap != null ? AssetDatabase.GetAssetPath(bumpMap) : "N/A")})");
                        Debug.Log($"        _BaseColor: {baseColor}, _Color: {color}");
                        Debug.Log($"        _WorkflowMode: {workflow}, _Metallic: {metallic:F2}, _Smoothness: {smoothness:F2}");
                    }
                }
            }
            Debug.Log("==========================================================");
        }
    }
}
