using UnityEngine;
using UnityEditor;
using System.IO;

public sealed class CreateCheckpointOrbSprite
{
    private const string MenuPath = "GameJam/Create Checkpoint Orb Sprite";
    private const string OutputPath = "Assets/Art/Generated/checkpoint_orb.png";
    private const int TextureSize = 128;

    [MenuItem(MenuPath)]
    public static void CreateSprite()
    {
        string directory = Path.GetDirectoryName(OutputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[TextureSize * TextureSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0);
        }

        float centerX = TextureSize / 2f;
        float centerY = TextureSize / 2f;
        float maxRadius = TextureSize / 2f;

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                Color pixelColor = Color.clear;

                if (distance < maxRadius * 0.25f)
                {
                    pixelColor = new Color(1f, 0.95f, 0.7f, 1f);
                }
                else if (distance < maxRadius * 0.5f)
                {
                    float t = (distance - maxRadius * 0.25f) / (maxRadius * 0.25f);
                    pixelColor = Color.Lerp(
                        new Color(1f, 0.95f, 0.7f, 1f),
                        new Color(1f, 0.843f, 0f, 1f),
                        t
                    );
                }
                else if (distance < maxRadius * 0.85f)
                {
                    float t = (distance - maxRadius * 0.5f) / (maxRadius * 0.35f);
                    pixelColor = Color.Lerp(
                        new Color(1f, 0.843f, 0f, 1f),
                        new Color(1f, 0.843f, 0f, 0.3f),
                        t
                    );
                }
                else if (distance < maxRadius)
                {
                    float t = (distance - maxRadius * 0.85f) / (maxRadius * 0.15f);
                    pixelColor = Color.Lerp(
                        new Color(1f, 0.843f, 0f, 0.3f),
                        Color.clear,
                        t
                    );
                }

                int pixelIndex = y * TextureSize + x;
                pixels[pixelIndex] = pixelColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), OutputPath);
        File.WriteAllBytes(fullPath, pngData);

        Object.DestroyImmediate(texture);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(OutputPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log($"Created checkpoint orb sprite at {OutputPath}");
    }
}
