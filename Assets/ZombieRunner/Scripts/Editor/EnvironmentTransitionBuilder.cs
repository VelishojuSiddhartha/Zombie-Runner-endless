using System.IO;
using UnityEditor;
using UnityEngine;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Constructs dedicated transition segment prefabs for seamless City <-> Tunnel transitions:
    /// 1. EnvironmentSegment_TunnelEntrance.prefab (City street -> Sunken retaining trench -> Overhead highway gantry -> Massive concrete portal arch -> Subterranean tunnel interior)
    /// 2. EnvironmentSegment_TunnelExit.prefab (Subterranean tunnel interior -> Massive concrete exit portal -> Overhead highway exit gantry -> Stepped retaining walls -> Open city street)
    /// </summary>
    [InitializeOnLoad]
    public static class EnvironmentTransitionBuilder
    {
        private const string PrefabsDir = "Assets/ZombieRunner/Prefabs/Environment";
        private const string MaterialsDir = "Assets/ZombieRunner/Materials";
        private const string EntrancePrefabPath = PrefabsDir + "/EnvironmentSegment_TunnelEntrance.prefab";
        private const string ExitPrefabPath = PrefabsDir + "/EnvironmentSegment_TunnelExit.prefab";
        private const string SessionKey = "ZombieRunner_TransitionPrefabsBuilt_v1";

        static EnvironmentTransitionBuilder()
        {
            EditorApplication.update += RunOnce;
        }

        private static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            BuildAllTransitionPrefabs();
        }

        [MenuItem("Zombie Runner/Environment/Build Transition Prefabs (Entrance & Exit)")]
        public static void BuildAllTransitionPrefabs()
        {
            Debug.Log("[EnvironmentTransitionBuilder] Building dedicated Tunnel Entrance & Exit prefabs...");

            BuildTunnelEntrancePrefab();
            BuildTunnelExitPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EnvironmentTransitionBuilder] Both transition prefabs built successfully!");
        }

        private static void BuildTunnelEntrancePrefab()
        {
            Material concreteMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_SubwayConcrete.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelIndustrialMetal.mat");
            Material hazardMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelHazard.mat");
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelLamp.mat");
            Material emergencyMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_EmergencyLamp.mat");
            Material pipeMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_Pipe.mat") ?? metalMat;

            GameObject root = new GameObject("EnvironmentSegment_TunnelEntrance");

            // ============================================================
            // ZONE 1: OPEN-AIR SUNKEN APPROACH TRENCH (Z = -15m to 0m)
            // ============================================================
            GameObject trenchGroup = new GameObject("Approach Trench (Open Sky)");
            trenchGroup.transform.SetParent(root.transform, false);

            // Left Retaining Walls (Stepped upwards toward the portal: 2.2m -> 4.2m)
            CreateBox(trenchGroup.transform, "Retaining Wall Left Low",
                new Vector3(-7.35f, 1.1f, -11.25f), new Vector3(1.2f, 2.2f, 7.5f), concreteMat, true);
            CreateBox(trenchGroup.transform, "Retaining Wall Left High",
                new Vector3(-7.35f, 2.1f, -3.75f), new Vector3(1.2f, 4.2f, 7.5f), concreteMat, true);
            CreateBox(trenchGroup.transform, "Retaining Cap Left",
                new Vector3(-7.35f, 4.25f, -3.75f), new Vector3(1.4f, 0.25f, 7.5f), concreteMat, true);

            // Right Retaining Walls (Stepped upwards toward the portal)
            CreateBox(trenchGroup.transform, "Retaining Wall Right Low",
                new Vector3(7.35f, 1.1f, -11.25f), new Vector3(1.2f, 2.2f, 7.5f), concreteMat, true);
            CreateBox(trenchGroup.transform, "Retaining Wall Right High",
                new Vector3(7.35f, 2.1f, -3.75f), new Vector3(1.2f, 4.2f, 7.5f), concreteMat, true);
            CreateBox(trenchGroup.transform, "Retaining Cap Right",
                new Vector3(7.35f, 4.25f, -3.75f), new Vector3(1.4f, 0.25f, 7.5f), concreteMat, true);

            // Angled Earth / Concrete Embankment Wings
            CreateBox(trenchGroup.transform, "Embankment Wing Left",
                new Vector3(-8.8f, 2.5f, -7.5f), new Vector3(1.8f, 5.0f, 14.5f), concreteMat, true,
                Quaternion.Euler(0f, 6f, 0f));
            CreateBox(trenchGroup.transform, "Embankment Wing Right",
                new Vector3(8.8f, 2.5f, -7.5f), new Vector3(1.8f, 5.0f, 14.5f), concreteMat, true,
                Quaternion.Euler(0f, -6f, 0f));

            // Service Walkways along curbs in trench
            CreateBox(trenchGroup.transform, "Trench Walkway Left",
                new Vector3(-5.9f, 0.18f, -7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);
            CreateBox(trenchGroup.transform, "Trench Walkway Right",
                new Vector3(5.9f, 0.18f, -7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);

            // OVERHEAD HIGHWAY APPROACH GANTRY (at Z = -7.5m)
            GameObject gantryGroup = new GameObject("Overhead Highway Gantry");
            gantryGroup.transform.SetParent(trenchGroup.transform, false);

            CreateBox(gantryGroup.transform, "Gantry Column Left",
                new Vector3(-6.6f, 3.2f, -7.5f), new Vector3(0.55f, 6.4f, 0.55f), metalMat, true);
            CreateBox(gantryGroup.transform, "Gantry Column Right",
                new Vector3(6.6f, 3.2f, -7.5f), new Vector3(0.55f, 6.4f, 0.55f), metalMat, true);
            CreateBox(gantryGroup.transform, "Gantry Cross Truss",
                new Vector3(0f, 6.2f, -7.5f), new Vector3(14.0f, 0.7f, 0.7f), metalMat, true);

            // Main Highway Signboard (Green background with border)
            CreateBox(gantryGroup.transform, "Highway Signboard",
                new Vector3(0f, 5.35f, -7.45f), new Vector3(8.5f, 1.35f, 0.15f), metalMat, true);
            CreateBox(gantryGroup.transform, "Hazard Border Lower",
                new Vector3(0f, 4.6f, -7.45f), new Vector3(8.5f, 0.15f, 0.16f), hazardMat, false);

            // Amber warning flashing beacons on gantry
            GameObject beacon1 = new GameObject("Warning Beacon Left");
            beacon1.transform.SetParent(gantryGroup.transform, false);
            beacon1.transform.localPosition = new Vector3(-4.4f, 5.35f, -7.3f);
            CreateBox(beacon1.transform, "Lamp", Vector3.zero, new Vector3(0.35f, 0.35f, 0.25f), emergencyMat, false);
            Light bLight1 = beacon1.AddComponent<Light>();
            bLight1.type = LightType.Point;
            bLight1.color = new Color(1.0f, 0.65f, 0.15f);
            bLight1.intensity = 1.6f;
            bLight1.range = 7f;

            GameObject beacon2 = new GameObject("Warning Beacon Right");
            beacon2.transform.SetParent(gantryGroup.transform, false);
            beacon2.transform.localPosition = new Vector3(4.4f, 5.35f, -7.3f);
            CreateBox(beacon2.transform, "Lamp", Vector3.zero, new Vector3(0.35f, 0.35f, 0.25f), emergencyMat, false);
            Light bLight2 = beacon2.AddComponent<Light>();
            bLight2.type = LightType.Point;
            bLight2.color = new Color(1.0f, 0.65f, 0.15f);
            bLight2.intensity = 1.6f;
            bLight2.range = 7f;

            // ============================================================
            // ZONE 2: MASSIVE CONCRETE TUNNEL PORTAL ARCH (Z = 0m)
            // ============================================================
            GameObject portalGroup = new GameObject("Tunnel Entrance Portal Arch");
            portalGroup.transform.SetParent(root.transform, false);

            // Heavy Monolithic Portal Piers
            CreateBox(portalGroup.transform, "Portal Pier Left",
                new Vector3(-7.25f, 3.2f, 0f), new Vector3(1.6f, 6.4f, 2.8f), concreteMat, true);
            CreateBox(portalGroup.transform, "Portal Pier Right",
                new Vector3(7.25f, 3.2f, 0f), new Vector3(1.6f, 6.4f, 2.8f), concreteMat, true);

            // Pier Foundation Plinths (Hazard Striped)
            CreateBox(portalGroup.transform, "Pier Plinth Left (Hazard)",
                new Vector3(-7.25f, 0.5f, 0f), new Vector3(1.8f, 1.0f, 3.2f), hazardMat, true);
            CreateBox(portalGroup.transform, "Pier Plinth Right (Hazard)",
                new Vector3(7.25f, 0.5f, 0f), new Vector3(1.8f, 1.0f, 3.2f), hazardMat, true);

            // Heavy Portal Lintel Beam across the opening
            CreateBox(portalGroup.transform, "Portal Lintel Beam",
                new Vector3(0f, 5.7f, 0f), new Vector3(15.8f, 1.4f, 2.8f), concreteMat, true);

            // Upper Overpass Retaining Wall / Highway Berm
            CreateBox(portalGroup.transform, "Overpass Earth Berm",
                new Vector3(0f, 7.1f, 0f), new Vector3(22.0f, 1.4f, 3.2f), concreteMat, true);
            CreateBox(portalGroup.transform, "Overpass Guardrail",
                new Vector3(0f, 8.0f, -1.3f), new Vector3(21.0f, 0.8f, 0.2f), metalMat, true);

            // Diagonal Portal Corner Brackets
            CreateBox(portalGroup.transform, "Portal Arch Corner Left",
                new Vector3(-5.8f, 4.8f, 0f), new Vector3(1.1f, 1.1f, 2.6f), concreteMat, true,
                Quaternion.Euler(0f, 0f, 45f));
            CreateBox(portalGroup.transform, "Portal Arch Corner Right",
                new Vector3(5.8f, 4.8f, 0f), new Vector3(1.1f, 1.1f, 2.6f), concreteMat, true,
                Quaternion.Euler(0f, 0f, -45f));

            // Hazard warning clearance bar across the portal mouth
            CreateBox(portalGroup.transform, "Portal Clearance Warning Bar",
                new Vector3(0f, 4.95f, -1.45f), new Vector3(12.5f, 0.22f, 0.15f), hazardMat, false);

            // ============================================================
            // ZONE 3: SUBTERRANEAN TUNNEL INTERIOR (Z = 0m to +15m)
            // ============================================================
            GameObject interiorGroup = new GameObject("Subterranean Tunnel Section");
            interiorGroup.transform.SetParent(root.transform, false);

            // Solid Overhead Ceiling (Starts at Z = 0m, extends to +15m)
            CreateBox(interiorGroup.transform, "Tunnel Ceiling",
                new Vector3(0f, 5.25f, 7.5f), new Vector3(15.5f, 0.45f, 15f), concreteMat, true);

            // Left & Right Continuous Concrete Walls
            CreateBox(interiorGroup.transform, "Tunnel Left Wall",
                new Vector3(-7.35f, 2.6f, 7.5f), new Vector3(1.2f, 5.2f, 15f), concreteMat, true);
            CreateBox(interiorGroup.transform, "Tunnel Right Wall",
                new Vector3(7.35f, 2.6f, 7.5f), new Vector3(1.2f, 5.2f, 15f), concreteMat, true);

            // Vault Chamfers
            CreateBox(interiorGroup.transform, "Vault Chamfer Left",
                new Vector3(-6.25f, 4.85f, 7.5f), new Vector3(1.5f, 0.35f, 15f), concreteMat, true,
                Quaternion.Euler(0f, 0f, 45f));
            CreateBox(interiorGroup.transform, "Vault Chamfer Right",
                new Vector3(6.25f, 4.85f, 7.5f), new Vector3(1.5f, 0.35f, 15f), concreteMat, true,
                Quaternion.Euler(0f, 0f, -45f));

            // Service Walkways
            CreateBox(interiorGroup.transform, "Service Walkway Left",
                new Vector3(-5.9f, 0.18f, 7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);
            CreateBox(interiorGroup.transform, "Service Walkway Right",
                new Vector3(5.9f, 0.18f, 7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);

            // Ventilation Duct & Trays in interior section
            CreateBox(interiorGroup.transform, "Main Ventilation Duct",
                new Vector3(-2.35f, 4.65f, 7.5f), new Vector3(1.65f, 0.78f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Cable Tray Left",
                new Vector3(-5.95f, 3.75f, 7.5f), new Vector3(0.65f, 0.14f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Cable Tray Right",
                new Vector3(5.95f, 3.75f, 7.5f), new Vector3(0.65f, 0.14f, 15f), metalMat, true);

            // Steel Rail Tracks along curb edges
            CreateBox(interiorGroup.transform, "Left Track Rail",
                new Vector3(-4.85f, 0.08f, 7.5f), new Vector3(0.14f, 0.16f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Right Track Rail",
                new Vector3(4.85f, 0.08f, 7.5f), new Vector3(0.14f, 0.16f, 15f), metalMat, true);

            // Structural Portal Rib at Z = 7.5m
            GameObject rib = new GameObject("Portal Rib [Entrance Interior]");
            rib.transform.SetParent(interiorGroup.transform, false);
            CreateBox(rib.transform, "Column Left", new Vector3(-6.65f, 2.5f, 7.5f), new Vector3(0.75f, 4.8f, 1.15f), concreteMat, true);
            CreateBox(rib.transform, "Column Right", new Vector3(6.65f, 2.5f, 7.5f), new Vector3(0.75f, 4.8f, 1.15f), concreteMat, true);
            CreateBox(rib.transform, "Plinth Left (Hazard)", new Vector3(-6.65f, 0.45f, 7.5f), new Vector3(0.88f, 0.88f, 1.3f), hazardMat, true);
            CreateBox(rib.transform, "Plinth Right (Hazard)", new Vector3(6.65f, 0.45f, 7.5f), new Vector3(0.88f, 0.88f, 1.3f), hazardMat, true);
            CreateBox(rib.transform, "Cross Beam", new Vector3(0f, 4.88f, 7.5f), new Vector3(13.6f, 0.65f, 1.15f), concreteMat, true);

            // Industrial Ceiling Luminaire at Z = 7.5m (Neutral-White 4200K)
            GameObject lum = new GameObject("Ceiling Industrial Luminaire");
            lum.transform.SetParent(interiorGroup.transform, false);
            lum.transform.localPosition = new Vector3(0f, 4.82f, 7.5f);
            CreateBox(lum.transform, "Housing", Vector3.zero, new Vector3(2.2f, 0.14f, 0.85f), metalMat, true);
            CreateBox(lum.transform, "Lamp Panel", new Vector3(0f, -0.08f, 0f), new Vector3(1.9f, 0.04f, 0.65f), lampMat, false);

            GameObject lightObj = new GameObject("Neutral Industrial Light");
            lightObj.transform.SetParent(lum.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.96f, 0.88f);
            l.intensity = 2.2f;
            l.range = 16f;
            l.shadows = LightShadows.Soft;

            // Amber Bulkhead on Left Column at Z = 7.5m
            GameObject bulkhead = new GameObject("Emergency Bulkhead [Left]");
            bulkhead.transform.SetParent(interiorGroup.transform, false);
            bulkhead.transform.localPosition = new Vector3(-6.15f, 2.55f, 7.5f);
            CreateBox(bulkhead.transform, "Cage", Vector3.zero, new Vector3(0.24f, 0.32f, 0.24f), emergencyMat, false);
            GameObject bObj = new GameObject("Amber Light");
            bObj.transform.SetParent(bulkhead.transform, false);
            bObj.transform.localPosition = new Vector3(0.25f, 0f, 0f);
            Light bLight = bObj.AddComponent<Light>();
            bLight.type = LightType.Point;
            bLight.color = new Color(1.0f, 0.60f, 0.18f);
            bLight.intensity = 1.45f;
            bLight.range = 8.5f;

            // Save Prefab
            SaveOrReplacePrefab(root, EntrancePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("[EnvironmentTransitionBuilder] Saved " + EntrancePrefabPath);
        }

        private static void BuildTunnelExitPrefab()
        {
            Material concreteMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_SubwayConcrete.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelIndustrialMetal.mat");
            Material hazardMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelHazard.mat");
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelLamp.mat");
            Material emergencyMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_EmergencyLamp.mat");

            GameObject root = new GameObject("EnvironmentSegment_TunnelExit");

            // ============================================================
            // ZONE 1: SUBTERRANEAN TUNNEL INTERIOR RUN (Z = -15m to 0m)
            // ============================================================
            GameObject interiorGroup = new GameObject("Subterranean Tunnel Run");
            interiorGroup.transform.SetParent(root.transform, false);

            // Solid Overhead Ceiling (Starts at Z = -15m, ends at portal at Z = 0m)
            CreateBox(interiorGroup.transform, "Tunnel Ceiling",
                new Vector3(0f, 5.25f, -7.5f), new Vector3(15.5f, 0.45f, 15f), concreteMat, true);

            // Left & Right Continuous Concrete Walls
            CreateBox(interiorGroup.transform, "Tunnel Left Wall",
                new Vector3(-7.35f, 2.6f, -7.5f), new Vector3(1.2f, 5.2f, 15f), concreteMat, true);
            CreateBox(interiorGroup.transform, "Tunnel Right Wall",
                new Vector3(7.35f, 2.6f, -7.5f), new Vector3(1.2f, 5.2f, 15f), concreteMat, true);

            // Vault Chamfers
            CreateBox(interiorGroup.transform, "Vault Chamfer Left",
                new Vector3(-6.25f, 4.85f, -7.5f), new Vector3(1.5f, 0.35f, 15f), concreteMat, true,
                Quaternion.Euler(0f, 0f, 45f));
            CreateBox(interiorGroup.transform, "Vault Chamfer Right",
                new Vector3(6.25f, 4.85f, -7.5f), new Vector3(1.5f, 0.35f, 15f), concreteMat, true,
                Quaternion.Euler(0f, 0f, -45f));

            // Service Walkways
            CreateBox(interiorGroup.transform, "Service Walkway Left",
                new Vector3(-5.9f, 0.18f, -7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);
            CreateBox(interiorGroup.transform, "Service Walkway Right",
                new Vector3(5.9f, 0.18f, -7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);

            // Utilities
            CreateBox(interiorGroup.transform, "Main Ventilation Duct",
                new Vector3(-2.35f, 4.65f, -7.5f), new Vector3(1.65f, 0.78f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Cable Tray Left",
                new Vector3(-5.95f, 3.75f, -7.5f), new Vector3(0.65f, 0.14f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Cable Tray Right",
                new Vector3(5.95f, 3.75f, -7.5f), new Vector3(0.65f, 0.14f, 15f), metalMat, true);

            // Tracks
            CreateBox(interiorGroup.transform, "Left Track Rail",
                new Vector3(-4.85f, 0.08f, -7.5f), new Vector3(0.14f, 0.16f, 15f), metalMat, true);
            CreateBox(interiorGroup.transform, "Right Track Rail",
                new Vector3(4.85f, 0.08f, -7.5f), new Vector3(0.14f, 0.16f, 15f), metalMat, true);

            // Structural Portal Rib at Z = -7.5m
            GameObject rib = new GameObject("Portal Rib [Exit Interior]");
            rib.transform.SetParent(interiorGroup.transform, false);
            CreateBox(rib.transform, "Column Left", new Vector3(-6.65f, 2.5f, -7.5f), new Vector3(0.75f, 4.8f, 1.15f), concreteMat, true);
            CreateBox(rib.transform, "Column Right", new Vector3(6.65f, 2.5f, -7.5f), new Vector3(0.75f, 4.8f, 1.15f), concreteMat, true);
            CreateBox(rib.transform, "Plinth Left (Hazard)", new Vector3(-6.65f, 0.45f, -7.5f), new Vector3(0.88f, 0.88f, 1.3f), hazardMat, true);
            CreateBox(rib.transform, "Plinth Right (Hazard)", new Vector3(6.65f, 0.45f, -7.5f), new Vector3(0.88f, 0.88f, 1.3f), hazardMat, true);
            CreateBox(rib.transform, "Cross Beam", new Vector3(0f, 4.88f, -7.5f), new Vector3(13.6f, 0.65f, 1.15f), concreteMat, true);

            // Industrial Luminaire at Z = -7.5m
            GameObject lum = new GameObject("Ceiling Industrial Luminaire");
            lum.transform.SetParent(interiorGroup.transform, false);
            lum.transform.localPosition = new Vector3(0f, 4.82f, -7.5f);
            CreateBox(lum.transform, "Housing", Vector3.zero, new Vector3(2.2f, 0.14f, 0.85f), metalMat, true);
            CreateBox(lum.transform, "Lamp Panel", new Vector3(0f, -0.08f, 0f), new Vector3(1.9f, 0.04f, 0.65f), lampMat, false);

            GameObject lightObj = new GameObject("Neutral Industrial Light");
            lightObj.transform.SetParent(lum.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.96f, 0.88f);
            l.intensity = 2.2f;
            l.range = 16f;
            l.shadows = LightShadows.Soft;

            // ============================================================
            // ZONE 2: MASSIVE CONCRETE TUNNEL EXIT PORTAL ARCH (Z = 0m)
            // ============================================================
            GameObject portalGroup = new GameObject("Tunnel Exit Portal Arch");
            portalGroup.transform.SetParent(root.transform, false);

            // Heavy Monolithic Portal Piers
            CreateBox(portalGroup.transform, "Portal Pier Left",
                new Vector3(-7.25f, 3.2f, 0f), new Vector3(1.6f, 6.4f, 2.8f), concreteMat, true);
            CreateBox(portalGroup.transform, "Portal Pier Right",
                new Vector3(7.25f, 3.2f, 0f), new Vector3(1.6f, 6.4f, 2.8f), concreteMat, true);

            CreateBox(portalGroup.transform, "Pier Plinth Left (Hazard)",
                new Vector3(-7.25f, 0.5f, 0f), new Vector3(1.8f, 1.0f, 3.2f), hazardMat, true);
            CreateBox(portalGroup.transform, "Pier Plinth Right (Hazard)",
                new Vector3(7.25f, 0.5f, 0f), new Vector3(1.8f, 1.0f, 3.2f), hazardMat, true);

            CreateBox(portalGroup.transform, "Portal Lintel Beam",
                new Vector3(0f, 5.7f, 0f), new Vector3(15.8f, 1.4f, 2.8f), concreteMat, true);

            CreateBox(portalGroup.transform, "Overpass Earth Berm",
                new Vector3(0f, 7.1f, 0f), new Vector3(22.0f, 1.4f, 3.2f), concreteMat, true);
            CreateBox(portalGroup.transform, "Overpass Guardrail",
                new Vector3(0f, 8.0f, 1.3f), new Vector3(21.0f, 0.8f, 0.2f), metalMat, true);

            CreateBox(portalGroup.transform, "Portal Arch Corner Left",
                new Vector3(-5.8f, 4.8f, 0f), new Vector3(1.1f, 1.1f, 2.6f), concreteMat, true,
                Quaternion.Euler(0f, 0f, 45f));
            CreateBox(portalGroup.transform, "Portal Arch Corner Right",
                new Vector3(5.8f, 4.8f, 0f), new Vector3(1.1f, 1.1f, 2.6f), concreteMat, true,
                Quaternion.Euler(0f, 0f, -45f));

            // Overhead exit signage on the portal facade (Facing the exiting runner)
            CreateBox(portalGroup.transform, "Exit Signboard",
                new Vector3(0f, 5.7f, 1.45f), new Vector3(8.5f, 1.1f, 0.15f), metalMat, true);

            // ============================================================
            // ZONE 3: OPEN-AIR EMERGING TRENCH (Z = 0m to +15m)
            // ============================================================
            GameObject emergingGroup = new GameObject("Emerging City Street (Open Sky)");
            emergingGroup.transform.SetParent(root.transform, false);

            // Stepped Down Retaining Walls opening out into the city
            CreateBox(emergingGroup.transform, "Retaining Wall Left High",
                new Vector3(-7.35f, 2.1f, 3.75f), new Vector3(1.2f, 4.2f, 7.5f), concreteMat, true);
            CreateBox(emergingGroup.transform, "Retaining Wall Left Low",
                new Vector3(-7.35f, 1.1f, 11.25f), new Vector3(1.2f, 2.2f, 7.5f), concreteMat, true);

            CreateBox(emergingGroup.transform, "Retaining Wall Right High",
                new Vector3(7.35f, 2.1f, 3.75f), new Vector3(1.2f, 4.2f, 7.5f), concreteMat, true);
            CreateBox(emergingGroup.transform, "Retaining Wall Right Low",
                new Vector3(7.35f, 1.1f, 11.25f), new Vector3(1.2f, 2.2f, 7.5f), concreteMat, true);

            CreateBox(emergingGroup.transform, "Embankment Wing Left",
                new Vector3(-8.8f, 2.5f, 7.5f), new Vector3(1.8f, 5.0f, 14.5f), concreteMat, true,
                Quaternion.Euler(0f, -6f, 0f));
            CreateBox(emergingGroup.transform, "Embankment Wing Right",
                new Vector3(8.8f, 2.5f, 7.5f), new Vector3(1.8f, 5.0f, 14.5f), concreteMat, true,
                Quaternion.Euler(0f, 6f, 0f));

            CreateBox(emergingGroup.transform, "Emerging Walkway Left",
                new Vector3(-5.9f, 0.18f, 7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);
            CreateBox(emergingGroup.transform, "Emerging Walkway Right",
                new Vector3(5.9f, 0.18f, 7.5f), new Vector3(1.8f, 0.36f, 15f), concreteMat, true);

            // Save Prefab
            SaveOrReplacePrefab(root, ExitPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("[EnvironmentTransitionBuilder] Saved " + ExitPrefabPath);
        }

        private static void SaveOrReplacePrefab(GameObject go, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool castShadows, Quaternion? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (rotation.HasValue) go.transform.localRotation = rotation.Value;

            Collider col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = mat;
                mr.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }

            return go;
        }
    }
}
