using System.IO;
using UnityEditor;
using UnityEngine;

public static class CloudDitherTextureGenerator
{
    [MenuItem("Tools/Volumetric Clouds/Generate Dither Texture")]
    private static void Generate()
    {
        const int size = 128;
        const string outputPath = "Assets/Clouds/Textures/CloudDither.png";

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;

        Color32[] pixels = new Color32[size * size];

        int index = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float value = InterleavedGradientNoise(x, y);
                byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255.0f), 0, 255);

                pixels[index] = new Color32(b, b, b, 255);
                index++;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        File.WriteAllBytes(outputPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        Debug.Log("Generated dither texture: " + outputPath);
    }

    private static float InterleavedGradientNoise(int x, int y)
    {
        return Mathf.Repeat(
            52.9829189f * Mathf.Repeat(x * 0.06711056f + y * 0.00583715f, 1.0f),
            1.0f
        );
    }
}