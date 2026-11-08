using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZombieRunner.Editor
{
    public static class UISpriteGenerator
    {
        private const string UIDir = "Assets/ZombieRunner/Textures/UI";

        public static void GenerateAllSprites()
        {
            string fullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", UIDir));
            if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);

            GenerateRoundedRect(Path.Combine(fullDir, "UI_RoundedRect_64.png"), 128, 128, 28, new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.4f), 3);
            GenerateCircleButton(Path.Combine(fullDir, "UI_Circle_Button.png"), 256, new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.6f), 6);
            GenerateCoinIcon(Path.Combine(fullDir, "UI_Coin_Icon.png"), 128);
            GenerateCrosshairIcon(Path.Combine(fullDir, "UI_Crosshair_Icon.png"), 128);
            GenerateArrowIcon(Path.Combine(fullDir, "UI_Arrow_Left.png"), 128, true);
            GenerateArrowIcon(Path.Combine(fullDir, "UI_Arrow_Right.png"), 128, false);

            // Also generate upgraded architectural building textures
            string envDir = Path.GetFullPath(Path.Combine(Application.dataPath, "ZombieRunner/Textures/Environment"));
            if (!Directory.Exists(envDir)) Directory.CreateDirectory(envDir);
            GenerateBuildingTextures(Path.Combine(envDir, "T_Building_Ruins_BC.png"), Path.Combine(envDir, "T_Building_Ruins_EM.png"), 512);

            AssetDatabase.Refresh();

            ConfigureAsSprite(UIDir + "/UI_RoundedRect_64.png", new Vector4(30, 30, 30, 30));
            ConfigureAsSprite(UIDir + "/UI_Circle_Button.png", Vector4.zero);
            ConfigureAsSprite(UIDir + "/UI_Coin_Icon.png", Vector4.zero);
            ConfigureAsSprite(UIDir + "/UI_Crosshair_Icon.png", Vector4.zero);
            ConfigureAsSprite(UIDir + "/UI_Arrow_Left.png", Vector4.zero);
            ConfigureAsSprite(UIDir + "/UI_Arrow_Right.png", Vector4.zero);

            ConfigureAsTexture("Assets/ZombieRunner/Textures/Environment/T_Building_Ruins_BC.png", true);
            ConfigureAsTexture("Assets/ZombieRunner/Textures/Environment/T_Building_Ruins_EM.png", true);

            AssetDatabase.SaveAssets();
            Debug.Log("[UISpriteGenerator] Generated all professional UI sprites and Building textures!");
        }

        private static void ConfigureAsTexture(string assetPath, bool isSRGB)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = isSRGB;
                importer.alphaIsTransparency = false;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateBuildingTextures(string baseColorPath, string emissionPath, int size)
        {
            Texture2D bcTex = new Texture2D(size, size, TextureFormat.RGB24, false);
            Texture2D emTex = new Texture2D(size, size, TextureFormat.RGB24, false);

            Color wallBase = new Color(0.48f, 0.49f, 0.52f);
            Color wallMortar = new Color(0.36f, 0.37f, 0.40f);
            Color ledgeColor = new Color(0.32f, 0.33f, 0.36f);
            Color ledgeHighlight = new Color(0.60f, 0.62f, 0.66f);
            Color frameColor = new Color(0.20f, 0.22f, 0.25f);
            Color unlitGlass = new Color(0.16f, 0.22f, 0.30f);
            Color warmLitGlass = new Color(0.96f, 0.74f, 0.36f);
            Color warmBlindSlat = new Color(0.78f, 0.56f, 0.22f);
            Color amberEmission = new Color(0.96f, 0.72f, 0.32f);
            Color black = Color.black;

            int floors = 4;
            int cols = 4;
            int floorHeight = size / floors;
            int colWidth = size / cols;
            int windowMarginX = 22;
            int windowMarginY = 20;

            // Pattern of lit windows (row, col)
            bool[,] litGrid = new bool[4, 4]
            {
                { false, false, true,  false },
                { true,  false, true,  false },
                { false, true,  false, false },
                { false, false, false, true  }
            };

            for (int y = 0; y < size; y++)
            {
                int floor = y / floorHeight;
                int localY = y % floorHeight;
                bool isCorniceLedge = (localY < 12);
                bool isCorniceTop = (localY == 11);

                for (int x = 0; x < size; x++)
                {
                    int col = x / colWidth;
                    int localX = x % colWidth;

                    // Masonry block texture
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.12f;
                    bool isMortar = (localX == 0 || (localY % 18 == 0));
                    Color pxColor = isMortar ? wallMortar : (wallBase + new Color(noise, noise, noise));

                    if (isCorniceLedge)
                    {
                        pxColor = isCorniceTop ? ledgeHighlight : ledgeColor;
                        bcTex.SetPixel(x, y, pxColor);
                        emTex.SetPixel(x, y, black);
                        continue;
                    }

                    // Window boundaries
                    int winX0 = windowMarginX;
                    int winX1 = colWidth - windowMarginX;
                    int winY0 = windowMarginY;
                    int winY1 = floorHeight - windowMarginY;

                    if (localX >= winX0 && localX <= winX1 && localY >= winY0 && localY <= winY1)
                    {
                        // Frame border
                        bool isBorder = (localX <= winX0 + 3 || localX >= winX1 - 3 || localY <= winY0 + 3 || localY >= winY1 - 3 ||
                                         Mathf.Abs(localX - (winX0 + winX1) / 2) <= 1 ||
                                         Mathf.Abs(localY - (winY0 + winY1) / 2) <= 1);

                        if (isBorder)
                        {
                            bcTex.SetPixel(x, y, frameColor);
                            emTex.SetPixel(x, y, black);
                        }
                        else
                        {
                            bool isLit = litGrid[floor, col];
                            if (isLit)
                            {
                                bool isBlind = ((localY % 6) <= 1);
                                Color glassCol = isBlind ? warmBlindSlat : warmLitGlass;
                                bcTex.SetPixel(x, y, glassCol);
                                emTex.SetPixel(x, y, amberEmission * (isBlind ? 0.7f : 1.0f));
                            }
                            else
                            {
                                // Unlit window with diagonal sky reflection sheen
                                float sheen = Mathf.Clamp01(1.0f - Mathf.Abs((localX - winX0) - (localY - winY0)) * 0.15f) * 0.25f;
                                Color glassCol = unlitGlass + new Color(sheen * 0.4f, sheen * 0.6f, sheen * 0.8f);
                                bcTex.SetPixel(x, y, glassCol);
                                emTex.SetPixel(x, y, black);
                            }
                        }
                    }
                    else
                    {
                        bcTex.SetPixel(x, y, pxColor);
                        emTex.SetPixel(x, y, black);
                    }
                }
            }

            bcTex.Apply();
            emTex.Apply();

            File.WriteAllBytes(baseColorPath, bcTex.EncodeToPNG());
            File.WriteAllBytes(emissionPath, emTex.EncodeToPNG());

            Object.DestroyImmediate(bcTex);
            Object.DestroyImmediate(emTex);
        }

        private static void ConfigureAsSprite(string assetPath, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
        }

        private static void GenerateRoundedRect(string filePath, int width, int height, int radius, Color fill, Color borderCol, int borderWidth)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = GetRoundedRectDistance(x, y, width, height, radius);
                    if (dist > 0.5f)
                    {
                        tex.SetPixel(x, y, clear);
                    }
                    else if (dist > -0.5f)
                    {
                        float alpha = Mathf.Clamp01(0.5f - dist);
                        Color c = Color.Lerp(borderCol, fill, Mathf.Clamp01(-dist));
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist > -(borderWidth + 0.5f))
                    {
                        tex.SetPixel(x, y, borderCol);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fill);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static float GetRoundedRectDistance(int x, int y, int width, int height, int radius)
        {
            int cx = x < radius ? radius : (x >= width - radius ? width - radius - 1 : x);
            int cy = y < radius ? radius : (y >= height - radius ? height - radius - 1 : y);
            float dx = x - cx;
            float dy = y - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) - radius;
        }

        private static void GenerateCircleButton(string filePath, int size, Color fill, Color borderCol, int borderWidth)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float radius = center - 4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (d > radius + 1f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > radius - 1f)
                    {
                        float a = Mathf.Clamp01(radius + 1f - d);
                        Color c = borderCol;
                        c.a *= a;
                        tex.SetPixel(x, y, c);
                    }
                    else if (d > radius - borderWidth)
                    {
                        tex.SetPixel(x, y, borderCol);
                    }
                    else
                    {
                        float grad = 1.0f - (d / radius) * 0.25f;
                        Color c = fill * grad;
                        c.a = fill.a;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void GenerateCoinIcon(string filePath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float radius = center - 3f;

            Color goldOuter = new Color(0.96f, 0.72f, 0.12f, 1f);
            Color goldInner = new Color(1.0f, 0.88f, 0.32f, 1f);
            Color goldDark = new Color(0.82f, 0.55f, 0.08f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (d > radius + 1f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > radius - 4f)
                    {
                        float a = Mathf.Clamp01(radius + 1f - d);
                        Color c = goldOuter;
                        c.a = a;
                        tex.SetPixel(x, y, c);
                    }
                    else if (d > radius - 7f)
                    {
                        tex.SetPixel(x, y, goldDark);
                    }
                    else
                    {
                        // Embossed dollar/star interior
                        bool isDollar = (Mathf.Abs(dx) < 3f && Mathf.Abs(dy) < radius * 0.6f) ||
                                        (Mathf.Abs(dy) < 3f && Mathf.Abs(dx) < radius * 0.45f);
                        Color c = isDollar ? goldDark : goldInner;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void GenerateCrosshairIcon(string filePath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            float rOuter = center - 14f;
            float rInner = rOuter - 6f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    bool inRing = (d <= rOuter && d >= rInner);
                    bool inCross = (Mathf.Abs(dx) < 3f && d <= rOuter + 8f && d >= 8f) ||
                                  (Mathf.Abs(dy) < 3f && d <= rOuter + 8f && d >= 8f);
                    bool inDot = (d <= 5f);

                    if (inRing || inCross || inDot)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void GenerateArrowIcon(string filePath, int size, bool pointLeft)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - center) / (size * 0.45f);
                    float ny = (y - center) / (size * 0.45f);
                    if (!pointLeft) nx = -nx;

                    // Chevron shape
                    float chevronDist = Mathf.Abs(nx - (Mathf.Abs(ny) * 0.8f - 0.35f));
                    if (chevronDist < 0.14f && Mathf.Abs(ny) < 0.75f)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
