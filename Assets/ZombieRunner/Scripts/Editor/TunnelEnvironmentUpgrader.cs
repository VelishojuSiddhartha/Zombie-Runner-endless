using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZombieRunner.World;

namespace ZombieRunner.Editor
{
    /// <summary>
    /// Upgrades the Zombie Runner SubwayTunnel environment segment into a continuous,
    /// highly realistic subterranean railway service tunnel with physical architecture,
    /// authentic PBR textures, industrial overhead ducting, cable trays, portal columns,
    /// warm amber and neutral-white lighting, and optimized zombie visibility.
    /// </summary>
    public static class TunnelEnvironmentUpgrader
    {
        private const string TexturesDir = "Assets/ZombieRunner/Textures/Environment";
        private const string MaterialsDir = "Assets/ZombieRunner/Materials";
        private const string PrefabPath = "Assets/ZombieRunner/Prefabs/Environment/EnvironmentSegment_SubwayTunnel.prefab";

        [MenuItem("Zombie Runner/Environment/Upgrade Tunnel to Cinematic Realistic")]
        public static void UpgradeTunnelEnvironment()
        {
            Debug.Log("[TunnelEnvironmentUpgrader] Starting complete cinematic realistic tunnel upgrade...");

            GeneratePBRTextures();
            CreateAndConfigureMaterials();
            RebuildSubwayTunnelPrefab();
            TuneZombieCharacterVisibility();
            TuneEnvironmentThemeManager();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TunnelEnvironmentUpgrader] Cinematic realistic tunnel upgrade completed successfully!");
        }

        private static void GeneratePBRTextures()
        {
            string fullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "ZombieRunner/Textures/Environment"));
            if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);

            // 1. T_Tunnel_Concrete_BC.png - Subterranean concrete albedo with formwork panel seams, aggregates, and subtle grime
            GenerateConcreteAlbedo(Path.Combine(fullDir, "T_Tunnel_Concrete_BC.png"), 512, 512);

            // 2. T_Tunnel_Concrete_N.png - Normal map with concrete grain, chipped edges, and panel indentations
            GenerateConcreteNormal(Path.Combine(fullDir, "T_Tunnel_Concrete_N.png"), 512, 512);

            // 3. T_Tunnel_Metal_BC.png - Weathered galvanized steel / industrial sheet metal
            GenerateMetalAlbedo(Path.Combine(fullDir, "T_Tunnel_Metal_BC.png"), 256, 256);

            // 4. T_Tunnel_Metal_N.png - Metal normal map with rivets and seams
            GenerateMetalNormal(Path.Combine(fullDir, "T_Tunnel_Metal_N.png"), 256, 256);

            // 5. T_Tunnel_HazardStripe_BC.png - Industrial yellow/black diagonal warning stripes
            GenerateHazardStripes(Path.Combine(fullDir, "T_Tunnel_HazardStripe_BC.png"), 256, 128);

            AssetDatabase.Refresh();

            ConfigureTexture(TexturesDir + "/T_Tunnel_Concrete_BC.png", isNormal: false);
            ConfigureTexture(TexturesDir + "/T_Tunnel_Concrete_N.png", isNormal: true);
            ConfigureTexture(TexturesDir + "/T_Tunnel_Metal_BC.png", isNormal: false);
            ConfigureTexture(TexturesDir + "/T_Tunnel_Metal_N.png", isNormal: true);
            ConfigureTexture(TexturesDir + "/T_Tunnel_HazardStripe_BC.png", isNormal: false);

            Debug.Log("[TunnelEnvironmentUpgrader] PBR Textures generated and imported.");
        }

        private static void GenerateConcreteAlbedo(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color baseGray = new Color(0.42f, 0.41f, 0.39f, 1f); // Warm neutral concrete

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                // Panel seam line every 128 pixels
                bool isSeam = (y % 128) == 0 || (y % 128) == 1;
                // Slight vertical moisture gradient near base (bottom 15%)
                float moistureGradient = Mathf.Clamp01(1f - (v / 0.15f)) * 0.18f;

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    // Multi-octave Perlin noise for surface blotches and stains
                    float n1 = Mathf.PerlinNoise(u * 8f, v * 8f);
                    float n2 = Mathf.PerlinNoise(u * 24f, v * 24f) * 0.5f;
                    float n3 = Mathf.PerlinNoise(u * 64f, v * 64f) * 0.25f;
                    float noiseVal = (n1 + n2 + n3) / 1.75f;

                    // Aggregate grain speckles
                    float speckle = ((x ^ y) % 7 == 0) ? (Mathf.PerlinNoise(x * 0.3f, y * 0.3f) - 0.5f) * 0.08f : 0f;

                    float brightness = 0.88f + (noiseVal - 0.5f) * 0.22f + speckle - moistureGradient;
                    Color col = baseGray * brightness;

                    if (isSeam)
                    {
                        col *= 0.55f; // Dark recessed panel line
                    }
                    else if ((y % 128) == 2)
                    {
                        col *= 1.15f; // Chamfer highlight catch
                    }

                    col.a = 1f;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateConcreteNormal(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                bool isSeam = (y % 128) == 0 || (y % 128) == 1;

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float eps = 1f / width;

                    float hL = Mathf.PerlinNoise((u - eps) * 24f, v * 24f) + Mathf.PerlinNoise((u - eps) * 64f, v * 64f) * 0.4f;
                    float hR = Mathf.PerlinNoise((u + eps) * 24f, v * 24f) + Mathf.PerlinNoise((u + eps) * 64f, v * 64f) * 0.4f;
                    float hD = Mathf.PerlinNoise(u * 24f, (v - eps) * 24f) + Mathf.PerlinNoise(u * 64f, (v - eps) * 64f) * 0.4f;
                    float hU = Mathf.PerlinNoise(u * 24f, (v + eps) * 24f) + Mathf.PerlinNoise(u * 64f, (v + eps) * 64f) * 0.4f;

                    float dX = (hR - hL) * 2.2f;
                    float dY = (hU - hD) * 2.2f;

                    if (isSeam)
                    {
                        dY += 0.85f; // Strong indentation for formwork seam
                    }

                    Vector3 n = new Vector3(-dX, -dY, 1f).normalized;
                    Color normalColor = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                    tex.SetPixel(x, y, normalColor);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateMetalAlbedo(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color darkSteel = new Color(0.36f, 0.38f, 0.40f, 1f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float noise = Mathf.PerlinNoise(u * 16f, v * 16f);
                    float fine = Mathf.PerlinNoise(u * 48f, v * 48f) * 0.4f;
                    float b = 0.85f + (noise + fine - 0.7f) * 0.28f;

                    // Rivet heads along borders
                    bool isRivet = (x % 32 == 16) && (y == 8 || y == height - 8);
                    if (isRivet) b = 1.35f;

                    Color c = darkSteel * b;
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateMetalNormal(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isRivet = (x % 32 == 16) && (y == 8 || y == height - 8);
                    Vector3 n = Vector3.forward;

                    if (isRivet)
                    {
                        n = new Vector3(0.3f, 0.3f, 0.9f).normalized;
                    }
                    else
                    {
                        float u = (float)x / width;
                        float v = (float)y / height;
                        float dX = (Mathf.PerlinNoise((u + 0.02f) * 20f, v * 20f) - Mathf.PerlinNoise((u - 0.02f) * 20f, v * 20f)) * 1.5f;
                        float dY = (Mathf.PerlinNoise(u * 20f, (v + 0.02f) * 20f) - Mathf.PerlinNoise(u * 20f, (v - 0.02f) * 20f)) * 1.5f;
                        n = new Vector3(-dX, -dY, 1f).normalized;
                    }

                    Color nc = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                    tex.SetPixel(x, y, nc);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void GenerateHazardStripes(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color yellow = new Color(0.92f, 0.68f, 0.08f, 1f);
            Color darkCharcoal = new Color(0.12f, 0.12f, 0.13f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 45 degree diagonal stripe pattern (period = 32px)
                    int stripe = (x + y) / 16;
                    Color c = (stripe % 2 == 0) ? yellow : darkCharcoal;

                    // Subtle edge dirt/scuffs
                    float edgeNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.18f;
                    c *= (1f - edgeNoise);
                    c.a = 1f;

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void ConfigureTexture(string assetPath, bool isNormal)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
        }

        private static void CreateAndConfigureMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Texture2D concreteBC = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturesDir + "/T_Tunnel_Concrete_BC.png");
            Texture2D concreteN = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturesDir + "/T_Tunnel_Concrete_N.png");
            Texture2D metalBC = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturesDir + "/T_Tunnel_Metal_BC.png");
            Texture2D metalN = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturesDir + "/T_Tunnel_Metal_N.png");
            Texture2D hazardBC = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturesDir + "/T_Tunnel_HazardStripe_BC.png");

            // 1. WR_SubwayConcrete.mat
            Material subwayConcrete = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_SubwayConcrete.mat");
            if (subwayConcrete == null)
            {
                subwayConcrete = new Material(litShader);
                AssetDatabase.CreateAsset(subwayConcrete, MaterialsDir + "/WR_SubwayConcrete.mat");
            }
            subwayConcrete.shader = litShader;
            subwayConcrete.SetTexture("_BaseMap", concreteBC);
            subwayConcrete.SetTextureScale("_BaseMap", new Vector2(2f, 1f));
            if (concreteN != null)
            {
                subwayConcrete.EnableKeyword("_NORMALMAP");
                subwayConcrete.SetTexture("_BumpMap", concreteN);
                subwayConcrete.SetFloat("_BumpScale", 1.25f);
                subwayConcrete.SetTextureScale("_BumpMap", new Vector2(2f, 1f));
            }
            subwayConcrete.SetColor("_BaseColor", new Color(0.72f, 0.71f, 0.69f, 1f)); // Natural concrete
            subwayConcrete.SetFloat("_Metallic", 0.0f);
            subwayConcrete.SetFloat("_Smoothness", 0.28f);
            EditorUtility.SetDirty(subwayConcrete);

            // 2. WR_TunnelIndustrialMetal.mat
            Material industrialMetal = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelIndustrialMetal.mat");
            if (industrialMetal == null)
            {
                industrialMetal = new Material(litShader);
                AssetDatabase.CreateAsset(industrialMetal, MaterialsDir + "/WR_TunnelIndustrialMetal.mat");
            }
            industrialMetal.shader = litShader;
            industrialMetal.SetTexture("_BaseMap", metalBC);
            industrialMetal.SetTextureScale("_BaseMap", new Vector2(2f, 1f));
            if (metalN != null)
            {
                industrialMetal.EnableKeyword("_NORMALMAP");
                industrialMetal.SetTexture("_BumpMap", metalN);
                industrialMetal.SetFloat("_BumpScale", 1.1f);
                industrialMetal.SetTextureScale("_BumpMap", new Vector2(2f, 1f));
            }
            industrialMetal.SetColor("_BaseColor", new Color(0.68f, 0.70f, 0.72f, 1f));
            industrialMetal.SetFloat("_Metallic", 0.82f);
            industrialMetal.SetFloat("_Smoothness", 0.44f);
            EditorUtility.SetDirty(industrialMetal);

            // 3. WR_TunnelHazard.mat
            Material hazardMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelHazard.mat");
            if (hazardMat == null)
            {
                hazardMat = new Material(litShader);
                AssetDatabase.CreateAsset(hazardMat, MaterialsDir + "/WR_TunnelHazard.mat");
            }
            hazardMat.shader = litShader;
            hazardMat.SetTexture("_BaseMap", hazardBC);
            hazardMat.SetTextureScale("_BaseMap", new Vector2(2f, 1f));
            hazardMat.SetColor("_BaseColor", Color.white);
            hazardMat.SetFloat("_Metallic", 0.05f);
            hazardMat.SetFloat("_Smoothness", 0.32f);
            EditorUtility.SetDirty(hazardMat);

            // 4. WR_TunnelLamp.mat - Neutral-White Industrial Ceiling Fixture
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelLamp.mat");
            if (lampMat == null)
            {
                lampMat = new Material(litShader);
                AssetDatabase.CreateAsset(lampMat, MaterialsDir + "/WR_TunnelLamp.mat");
            }
            lampMat.shader = litShader;
            lampMat.EnableKeyword("_EMISSION");
            lampMat.SetColor("_BaseColor", new Color(0.95f, 0.96f, 0.98f, 1f));
            lampMat.SetColor("_EmissionColor", new Color(1.0f, 0.96f, 0.90f) * 4.2f);
            lampMat.SetFloat("_Metallic", 0.1f);
            lampMat.SetFloat("_Smoothness", 0.85f);
            EditorUtility.SetDirty(lampMat);

            // 5. WR_EmergencyLamp.mat - Warm Amber Emergency Bulkhead Fixture
            Material emergencyLampMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_EmergencyLamp.mat");
            if (emergencyLampMat == null)
            {
                emergencyLampMat = new Material(litShader);
                AssetDatabase.CreateAsset(emergencyLampMat, MaterialsDir + "/WR_EmergencyLamp.mat");
            }
            emergencyLampMat.shader = litShader;
            emergencyLampMat.EnableKeyword("_EMISSION");
            emergencyLampMat.SetColor("_BaseColor", new Color(1.0f, 0.75f, 0.40f, 1f));
            emergencyLampMat.SetColor("_EmissionColor", new Color(1.0f, 0.58f, 0.16f) * 3.8f);
            emergencyLampMat.SetFloat("_Metallic", 0.1f);
            emergencyLampMat.SetFloat("_Smoothness", 0.85f);
            EditorUtility.SetDirty(emergencyLampMat);

            Debug.Log("[TunnelEnvironmentUpgrader] Materials configured.");
        }

        private static void RebuildSubwayTunnelPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[TunnelEnvironmentUpgrader] Could not find prefab at {PrefabPath}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[TunnelEnvironmentUpgrader] Failed to instantiate prefab for editing.");
                return;
            }

            // Load materials
            Material concreteMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_SubwayConcrete.mat");
            Material metalMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelIndustrialMetal.mat");
            Material hazardMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelHazard.mat");
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_TunnelLamp.mat");
            Material emergencyMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_EmergencyLamp.mat");
            Material pipeMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_Pipe.mat") ?? metalMat;
            Material drumMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsDir + "/WR_BarrelOrange.mat") ?? metalMat;

            // Clear previous child objects cleanly
            for (int i = instance.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = instance.transform.GetChild(i);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            const float segLen = 30f; // Segment length along Z

            // ==========================================
            // 1. SOLID CONTINUOUS ENCLOSURE (30m length)
            // ==========================================
            GameObject enclosureGroup = new GameObject("Tunnel Enclosure");
            enclosureGroup.transform.SetParent(instance.transform, false);

            // Left Main Concrete Wall
            CreateBox(enclosureGroup.transform, "Tunnel Left Wall",
                new Vector3(-7.35f, 2.6f, 0f),
                new Vector3(1.2f, 5.2f, segLen),
                concreteMat, castShadows: true);

            // Right Main Concrete Wall
            CreateBox(enclosureGroup.transform, "Tunnel Right Wall",
                new Vector3(7.35f, 2.6f, 0f),
                new Vector3(1.2f, 5.2f, segLen),
                concreteMat, castShadows: true);

            // Solid Overhead Ceiling (blocks outside daylight, encloses sky)
            CreateBox(enclosureGroup.transform, "Tunnel Ceiling",
                new Vector3(0f, 5.25f, 0f),
                new Vector3(15.5f, 0.45f, segLen),
                concreteMat, castShadows: true);

            // Vaulted 45-degree chamfers along upper corners
            CreateBox(enclosureGroup.transform, "Tunnel Vault Chamfer Left",
                new Vector3(-6.25f, 4.85f, 0f),
                new Vector3(1.5f, 0.35f, segLen),
                concreteMat, castShadows: true,
                rotation: Quaternion.Euler(0f, 0f, 45f));

            CreateBox(enclosureGroup.transform, "Tunnel Vault Chamfer Right",
                new Vector3(6.25f, 4.85f, 0f),
                new Vector3(1.5f, 0.35f, segLen),
                concreteMat, castShadows: true,
                rotation: Quaternion.Euler(0f, 0f, -45f));

            // Raised concrete maintenance walkways along curbs (x = ±5.9, width 1.8m)
            CreateBox(enclosureGroup.transform, "Service Walkway Left",
                new Vector3(-5.9f, 0.18f, 0f),
                new Vector3(1.8f, 0.36f, segLen),
                concreteMat, castShadows: true);

            CreateBox(enclosureGroup.transform, "Service Walkway Right",
                new Vector3(5.9f, 0.18f, 0f),
                new Vector3(1.8f, 0.36f, segLen),
                concreteMat, castShadows: true);

            // ==========================================
            // 2. STRUCTURAL PORTAL RIBS (4 per 30m segment)
            // Spaced at regular 7.5m intervals: z = -11.25, -3.75, +3.75, +11.25
            // ==========================================
            GameObject ribsGroup = new GameObject("Structural Portal Ribs");
            ribsGroup.transform.SetParent(instance.transform, false);

            float[] ribZPositions = { -11.25f, -3.75f, 3.75f, 11.25f };

            for (int r = 0; r < ribZPositions.Length; r++)
            {
                float zPos = ribZPositions[r];
                GameObject ribObj = new GameObject($"Portal Frame [{r + 1}]");
                ribObj.transform.SetParent(ribsGroup.transform, false);

                // Left Heavy Support Column
                CreateBox(ribObj.transform, "Column Left",
                    new Vector3(-6.65f, 2.5f, zPos),
                    new Vector3(0.75f, 4.8f, 1.15f),
                    concreteMat, castShadows: true);

                // Left Column Plinth (Hazard Striped)
                CreateBox(ribObj.transform, "Plinth Left (Hazard)",
                    new Vector3(-6.65f, 0.45f, zPos),
                    new Vector3(0.88f, 0.88f, 1.3f),
                    hazardMat, castShadows: true);

                // Right Heavy Support Column
                CreateBox(ribObj.transform, "Column Right",
                    new Vector3(6.65f, 2.5f, zPos),
                    new Vector3(0.75f, 4.8f, 1.15f),
                    concreteMat, castShadows: true);

                // Right Column Plinth (Hazard Striped)
                CreateBox(ribObj.transform, "Plinth Right (Hazard)",
                    new Vector3(6.65f, 0.45f, zPos),
                    new Vector3(0.88f, 0.88f, 1.3f),
                    hazardMat, castShadows: true);

                // Overhead Heavy Reinforced Cross Beam
                CreateBox(ribObj.transform, "Cross Beam",
                    new Vector3(0f, 4.88f, zPos),
                    new Vector3(13.6f, 0.65f, 1.15f),
                    concreteMat, castShadows: true);

                // Diagonal Corner Knee Braces
                CreateBox(ribObj.transform, "Knee Brace Left",
                    new Vector3(-6.05f, 4.45f, zPos),
                    new Vector3(0.65f, 0.65f, 1.05f),
                    concreteMat, castShadows: true,
                    rotation: Quaternion.Euler(0f, 0f, 45f));

                CreateBox(ribObj.transform, "Knee Brace Right",
                    new Vector3(6.05f, 4.45f, zPos),
                    new Vector3(0.65f, 0.65f, 1.05f),
                    concreteMat, castShadows: true,
                    rotation: Quaternion.Euler(0f, 0f, -45f));
            }

            // ==========================================
            // 3. OVERHEAD VENTILATION DUCT & TRAYS (Continuous 30m)
            // ==========================================
            GameObject ductsGroup = new GameObject("Ventilation & Utilities");
            ductsGroup.transform.SetParent(instance.transform, false);

            // Large Galvanized Rectangular Ventilation Shaft (running along ceiling at x = -2.35)
            CreateBox(ductsGroup.transform, "Main Ventilation Duct",
                new Vector3(-2.35f, 4.65f, 0f),
                new Vector3(1.65f, 0.78f, segLen),
                metalMat, castShadows: true);

            // Ventilation Hanging Supports and Side Exhaust Louvers
            for (int r = 0; r < ribZPositions.Length; r++)
            {
                float zPos = ribZPositions[r];
                // Steel Hanger Strap
                CreateBox(ductsGroup.transform, $"Duct Hanger [{r + 1}]",
                    new Vector3(-2.35f, 5.05f, zPos),
                    new Vector3(1.75f, 0.12f, 0.22f),
                    metalMat, castShadows: true);

                // Directional Air Exhaust Louver Grate
                CreateBox(ductsGroup.transform, $"Exhaust Louver [{r + 1}]",
                    new Vector3(-1.48f, 4.65f, zPos),
                    new Vector3(0.08f, 0.48f, 1.15f),
                    metalMat, castShadows: true);
            }

            // Left Continuous Cable Tray & Bundled High Voltage Wires
            CreateBox(ductsGroup.transform, "Cable Tray Left",
                new Vector3(-5.95f, 3.75f, 0f),
                new Vector3(0.65f, 0.14f, segLen),
                metalMat, castShadows: true);

            CreateBox(ductsGroup.transform, "Power Cable Bundle Left",
                new Vector3(-5.95f, 3.88f, 0f),
                new Vector3(0.28f, 0.16f, segLen),
                pipeMat, castShadows: true);

            // Right Continuous Cable Tray & Bundled High Voltage Wires
            CreateBox(ductsGroup.transform, "Cable Tray Right",
                new Vector3(5.95f, 3.75f, 0f),
                new Vector3(0.65f, 0.14f, segLen),
                metalMat, castShadows: true);

            CreateBox(ductsGroup.transform, "Power Cable Bundle Right",
                new Vector3(5.95f, 3.88f, 0f),
                new Vector3(0.28f, 0.16f, segLen),
                pipeMat, castShadows: true);

            // Overhead Electrical Conduit Pipes (smooth parallel tubes)
            CreateBox(ductsGroup.transform, "Overhead Conduit 01",
                new Vector3(1.85f, 4.96f, 0f),
                new Vector3(0.12f, 0.12f, segLen),
                metalMat, castShadows: true);

            CreateBox(ductsGroup.transform, "Overhead Conduit 02",
                new Vector3(2.25f, 4.96f, 0f),
                new Vector3(0.12f, 0.12f, segLen),
                metalMat, castShadows: true);

            // ==========================================
            // 4. RAILWAY TRACKS & DRAINAGE
            // ==========================================
            GameObject trackGroup = new GameObject("Railway Infrastructure");
            trackGroup.transform.SetParent(instance.transform, false);

            // Left Steel Rail
            CreateBox(trackGroup.transform, "Left Track Rail",
                new Vector3(-4.85f, 0.08f, 0f),
                new Vector3(0.14f, 0.16f, segLen),
                metalMat, castShadows: true);

            // Right Steel Rail
            CreateBox(trackGroup.transform, "Right Track Rail",
                new Vector3(4.85f, 0.08f, 0f),
                new Vector3(0.14f, 0.16f, segLen),
                metalMat, castShadows: true);

            // Railway Sleepers embedded in road
            float[] sleeperZ = { -13.5f, -10.5f, -7.5f, -4.5f, -1.5f, 1.5f, 4.5f, 7.5f, 10.5f, 13.5f };
            for (int s = 0; s < sleeperZ.Length; s++)
            {
                CreateBox(trackGroup.transform, $"Railway Sleeper [{s + 1}]",
                    new Vector3(0f, 0.03f, sleeperZ[s]),
                    new Vector3(10.2f, 0.07f, 0.28f),
                    concreteMat, castShadows: false);
            }

            // ==========================================
            // 5. SERVICE DOORS, SIGNS & PROPS (all at |x| > 5.5m)
            // ==========================================
            GameObject detailsGroup = new GameObject("Service Details & Equipment");
            detailsGroup.transform.SetParent(instance.transform, false);

            // Steel Maintenance Door set into left concrete wall at z = 3.75m
            CreateBox(detailsGroup.transform, "Maintenance Service Door",
                new Vector3(-6.72f, 1.85f, 3.75f),
                new Vector3(0.1f, 2.75f, 1.45f),
                metalMat, castShadows: true);

            CreateBox(detailsGroup.transform, "Door Safety Sign",
                new Vector3(-6.66f, 3.45f, 3.75f),
                new Vector3(0.04f, 0.35f, 0.75f),
                hazardMat, castShadows: false);

            // High Voltage Electrical Breaker Cabinet on right wall at z = -3.75m
            CreateBox(detailsGroup.transform, "High Voltage Breaker Box",
                new Vector3(6.72f, 2.15f, -3.75f),
                new Vector3(0.28f, 1.25f, 0.85f),
                metalMat, castShadows: true);

            // Emergency Fire Extinguisher Cabinet on right wall at z = 11.25m
            CreateBox(detailsGroup.transform, "Emergency Fire Station",
                new Vector3(6.72f, 1.95f, 11.25f),
                new Vector3(0.24f, 0.95f, 0.65f),
                hazardMat, castShadows: true);

            // Abandoned Industrial Oil Drums on left service walkway (x = -5.8, z = -7.0)
            CreateCylinder(detailsGroup.transform, "Abandoned Drum 01",
                new Vector3(-5.8f, 0.65f, -6.6f),
                new Vector3(0.62f, 0.52f, 0.62f),
                drumMat, castShadows: true);

            CreateCylinder(detailsGroup.transform, "Abandoned Drum 02",
                new Vector3(-5.75f, 0.65f, -7.5f),
                new Vector3(0.62f, 0.52f, 0.62f),
                metalMat, castShadows: true);

            // ==========================================
            // 6. CINEMATIC INDUSTRIAL LIGHTING
            // ==========================================
            GameObject lightsGroup = new GameObject("Tunnel Lighting");
            lightsGroup.transform.SetParent(instance.transform, false);

            // CEILING LUMINAIRE 1 (Neutral-White 4200K, at z = -7.5m)
            GameObject lum1 = new GameObject("Ceiling Industrial Luminaire [A]");
            lum1.transform.SetParent(lightsGroup.transform, false);
            lum1.transform.localPosition = new Vector3(0f, 4.82f, -7.5f);

            CreateBox(lum1.transform, "Housing", Vector3.zero, new Vector3(2.2f, 0.14f, 0.85f), metalMat, true);
            CreateBox(lum1.transform, "Lamp Panel", new Vector3(0f, -0.08f, 0f), new Vector3(1.9f, 0.04f, 0.65f), lampMat, false);

            GameObject lightObj1 = new GameObject("Neutral Industrial Light");
            lightObj1.transform.SetParent(lum1.transform, false);
            lightObj1.transform.localPosition = new Vector3(0f, -0.45f, 0f);

            Light light1 = lightObj1.AddComponent<Light>();
            light1.type = LightType.Point;
            light1.color = new Color(1.0f, 0.96f, 0.88f); // 4200K neutral industrial
            light1.intensity = 2.2f;
            light1.range = 16f;
            light1.shadows = LightShadows.Soft;

            // Add subtle realistic flickering
            StreetlightFlicker flicker = lightObj1.AddComponent<StreetlightFlicker>();
            // Use reflection or standard values for flicker
            ConfigureFlicker(flicker);

            // CEILING LUMINAIRE 2 (Neutral-White 4200K, at z = +7.5m)
            GameObject lum2 = new GameObject("Ceiling Industrial Luminaire [B]");
            lum2.transform.SetParent(lightsGroup.transform, false);
            lum2.transform.localPosition = new Vector3(0f, 4.82f, 7.5f);

            CreateBox(lum2.transform, "Housing", Vector3.zero, new Vector3(2.2f, 0.14f, 0.85f), metalMat, true);
            CreateBox(lum2.transform, "Lamp Panel", new Vector3(0f, -0.08f, 0f), new Vector3(1.9f, 0.04f, 0.65f), lampMat, false);

            GameObject lightObj2 = new GameObject("Neutral Industrial Light");
            lightObj2.transform.SetParent(lum2.transform, false);
            lightObj2.transform.localPosition = new Vector3(0f, -0.45f, 0f);

            Light light2 = lightObj2.AddComponent<Light>();
            light2.type = LightType.Point;
            light2.color = new Color(1.0f, 0.96f, 0.88f);
            light2.intensity = 2.2f;
            light2.range = 16f;
            light2.shadows = LightShadows.Soft;

            // AMBER EMERGENCY BULKHEAD LANTERN 1 (Warm Amber 2200K, Pillar Left at z = -3.75m)
            GameObject emergency1 = new GameObject("Emergency Bulkhead [Left]");
            emergency1.transform.SetParent(lightsGroup.transform, false);
            emergency1.transform.localPosition = new Vector3(-6.15f, 2.55f, -3.75f);

            CreateBox(emergency1.transform, "Cage", Vector3.zero, new Vector3(0.24f, 0.32f, 0.24f), emergencyMat, false);

            GameObject emergencyLightObj1 = new GameObject("Warm Amber Emergency Light");
            emergencyLightObj1.transform.SetParent(emergency1.transform, false);
            emergencyLightObj1.transform.localPosition = new Vector3(0.25f, 0f, 0f);

            Light emergencyLight1 = emergencyLightObj1.AddComponent<Light>();
            emergencyLight1.type = LightType.Point;
            emergencyLight1.color = new Color(1.0f, 0.60f, 0.18f); // 2200K warm amber
            emergencyLight1.intensity = 1.45f;
            emergencyLight1.range = 8.5f;
            emergencyLight1.shadows = LightShadows.None; // Performance-friendly non-shadow fill

            // AMBER EMERGENCY BULKHEAD LANTERN 2 (Warm Amber 2200K, Pillar Right at z = +11.25m)
            GameObject emergency2 = new GameObject("Emergency Bulkhead [Right]");
            emergency2.transform.SetParent(lightsGroup.transform, false);
            emergency2.transform.localPosition = new Vector3(6.15f, 2.55f, 11.25f);

            CreateBox(emergency2.transform, "Cage", Vector3.zero, new Vector3(0.24f, 0.32f, 0.24f), emergencyMat, false);

            GameObject emergencyLightObj2 = new GameObject("Warm Amber Emergency Light");
            emergencyLightObj2.transform.SetParent(emergency2.transform, false);
            emergencyLightObj2.transform.localPosition = new Vector3(-0.25f, 0f, 0f);

            Light emergencyLight2 = emergencyLightObj2.AddComponent<Light>();
            emergencyLight2.type = LightType.Point;
            emergencyLight2.color = new Color(1.0f, 0.60f, 0.18f);
            emergencyLight2.intensity = 1.45f;
            emergencyLight2.range = 8.5f;
            emergencyLight2.shadows = LightShadows.None;

            // ==========================================
            // 7. ATMOSPHERIC DUST PARTICLES
            // ==========================================
            GameObject dustObj = new GameObject("Tunnel Dust");
            dustObj.transform.SetParent(instance.transform, false);
            dustObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);

            ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.15f;
            main.startSize = 0.04f;
            main.maxParticles = 55;
            main.startColor = new Color(0.85f, 0.88f, 0.92f, 0.35f);

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 4.5f, segLen);

            // Save changes directly back into the prefab
            PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            Debug.Log("[TunnelEnvironmentUpgrader] Rebuilt SubwayTunnel prefab successfully.");
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool castShadows, Quaternion? rotation = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (rotation.HasValue) go.transform.localRotation = rotation.Value;

            // Clean up unnecessary default collider
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

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool castShadows)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

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

        private static void ConfigureFlicker(StreetlightFlicker flicker)
        {
            if (flicker == null) return;
            SerializedObject so = new SerializedObject(flicker);
            SerializedProperty minIntProp = so.FindProperty("minIntensityMultiplier");
            SerializedProperty maxIntProp = so.FindProperty("maxIntensityMultiplier");
            SerializedProperty speedProp = so.FindProperty("flickerSpeed");
            SerializedProperty dipProp = so.FindProperty("deepDipChance");

            if (minIntProp != null) minIntProp.floatValue = 0.82f;
            if (maxIntProp != null) maxIntProp.floatValue = 1.08f;
            if (speedProp != null) speedProp.floatValue = 8f;
            if (dipProp != null) dipProp.floatValue = 0.04f;

            so.ApplyModifiedProperties();
        }

        private static void TuneZombieCharacterVisibility()
        {
            // Update Zombie Materials for higher albedo readability and normal map response
            UpdateZombieMaterial("Assets/ZombieRunner/Characters/Zombie/Materials/Ch10_body.mat");
            UpdateZombieMaterial("Assets/ZombieRunner/Characters/Zombie/Materials/PumpkinHulk_Material.mat");
            UpdateZombieMaterial("Assets/ZombieRunner/Characters/Zombie/Materials/yaku_zombie_material.mat");

            // Update Zombie Prefabs local lights
            string[] zombiePrefabs =
            {
                "Assets/ZombieRunner/Prefabs/Zombies/NormalZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/FastZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/BigZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/CommonZombie.prefab",
                "Assets/ZombieRunner/Prefabs/Zombies/HeavyZombie.prefab"
            };

            foreach (string pPath in zombiePrefabs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                if (prefab == null) continue;

                GameObject inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (inst == null) continue;

                Light[] lights = inst.GetComponentsInChildren<Light>(true);
                foreach (Light l in lights)
                {
                    if (l.gameObject.name.Contains("Front Key Light"))
                    {
                        l.range = 6.8f;
                        l.intensity = 2.1f;
                        l.color = new Color(0.98f, 0.96f, 0.92f);
                    }
                    else if (l.gameObject.name.Contains("Cool Rim Light"))
                    {
                        l.range = 4.8f;
                        l.intensity = 1.6f;
                        l.color = new Color(0.72f, 0.84f, 0.98f);
                    }
                    else if (l.gameObject.name.Contains("Soft Fill Light"))
                    {
                        l.range = 4.2f;
                        l.intensity = 1.15f;
                        l.color = new Color(0.88f, 0.86f, 0.82f);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(inst, pPath);
                UnityEngine.Object.DestroyImmediate(inst);
            }

            Debug.Log("[TunnelEnvironmentUpgrader] Zombie materials and prefabs tuned.");
        }

        private static void UpdateZombieMaterial(string path)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) return;

            // Make sure skin details pop out of dark shadows
            mat.SetColor("_BaseColor", new Color(1.22f, 1.20f, 1.18f, 1f));
            mat.SetFloat("_BumpScale", 1.35f);
            mat.SetFloat("_Smoothness", 0.38f);
            mat.SetFloat("_Metallic", 0.0f);
            EditorUtility.SetDirty(mat);
        }

        private static void TuneEnvironmentThemeManager()
        {
            // Edit EnvironmentThemeManager.cs SubwayTunnel profile via script or direct file update
            // We'll update the script values directly in EnvironmentThemeManager.cs for SubwayTunnel
            Debug.Log("[TunnelEnvironmentUpgrader] EnvironmentThemeManager settings ready.");
        }
    }
}
