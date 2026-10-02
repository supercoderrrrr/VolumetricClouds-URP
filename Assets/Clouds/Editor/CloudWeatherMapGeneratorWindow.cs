using System.IO;
using UnityEditor;
using UnityEngine;

public class CloudWeatherMapGeneratorWindow : EditorWindow
{
    private int resolution = 512;
    private int seed = 1;

    private float scale = 2.5f;
    private int octaves = 5;
    private float persistence = 0.55f;
    private float contrast = 0.75f;
    private float coverage = 0.8f;
    private float minimumCloudAmount = 0.15f;

    private string outputPath = "Assets/Clouds/Textures/WeatherMap.png";

    [MenuItem("Tools/Volumetric Clouds/Weather Map Generator")]
    private static void Open()
    {
        GetWindow<CloudWeatherMapGeneratorWindow>("Weather Map");
    }

    private void OnGUI()
    {
        resolution = EditorGUILayout.IntPopup(
            "Resolution",
            resolution,
            new[] { "256", "512", "1024" },
            new[] { 256, 512, 1024 }
        );

        seed = EditorGUILayout.IntField("Seed", seed);

        scale = EditorGUILayout.Slider("Scale", scale, 0.5f, 12.0f);
        octaves = EditorGUILayout.IntSlider("Octaves", octaves, 1, 8);
        persistence = EditorGUILayout.Slider("Persistence", persistence, 0.1f, 0.9f);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0.3f, 3.0f);
        minimumCloudAmount = EditorGUILayout.Slider(
            "Minimum Cloud Amount",
            minimumCloudAmount,
            0.0f,
            0.5f
        );

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        GUILayout.Space(8);

        if (GUILayout.Button("Generate Weather Map"))
            Generate();
    }

    private void Generate()
    {
        Texture2D texture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false,
            true
        );

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[resolution * resolution];

        float seedOffsetX = seed * 37.13f;
        float seedOffsetY = seed * 91.71f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = x / (float)resolution;
                float v = y / (float)resolution;

                float noise = FBM(
                    u * scale + seedOffsetX,
                    v * scale + seedOffsetY
                );

                float threshold = Mathf.Lerp(1.0f, 0.0f, coverage);
                float cloudAmount = Mathf.InverseLerp(threshold, 1.0f, noise);

                // Smooth coverage transitions
                cloudAmount = cloudAmount * cloudAmount * (3.0f - 2.0f * cloudAmount);

                // Keep a minimum cloud amount in sparse regions
                cloudAmount = Mathf.Lerp(minimumCloudAmount, 1.0f, cloudAmount);

                byte value = FloatToByte(cloudAmount);

                int index = x + y * resolution;
                pixels[index] = new Color32(value, value, value, 255);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);

        string folder = Path.GetDirectoryName(outputPath).Replace("\\", "/");

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError("Output folder does not exist: " + folder);
            return;
        }

        File.WriteAllBytes(outputPath, texture.EncodeToPNG());
        DestroyImmediate(texture);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        Debug.Log("Generated weather map: " + outputPath);
    }

    private float FBM(float x, float y)
    {
        float value = 0.0f;
        float amplitude = 0.5f;
        float frequency = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            frequency *= 2.0f;
            amplitude *= persistence;
        }

        return Mathf.Clamp01(value);
    }

    private static byte FloatToByte(float value)
    {
        return (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255.0f), 0, 255);
    }
}
