using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CloudNoiseGeneratorWindow : EditorWindow
{
    private enum NoiseMode
    {
        Shape,
        Detail
    }

    private ComputeShader computeShader;
    private NoiseMode noiseMode = NoiseMode.Shape;
    private int size = 64;
    private int seed = 1;
    private float shapeContrast = 1.0f;
    private float worleyErosion = 1.0f;
    private string outputPath = "Assets/Clouds/GeneratedNoise/ShapeWorley64.asset";

    [MenuItem("Tools/Volumetric Clouds/Noise Generator")]
    private static void Open()
    {
        GetWindow<CloudNoiseGeneratorWindow>("Cloud Noise");
    }

    private void OnGUI()
    {
        computeShader = (ComputeShader)EditorGUILayout.ObjectField(
            "Compute Shader",
            computeShader,
            typeof(ComputeShader),
            false
        );

        EditorGUI.BeginChangeCheck();

        size = EditorGUILayout.IntPopup(
            "Texture Size",
            size,
            new[] { "32", "64", "128" },
            new[] { 32, 64, 128 }
        );

        if (EditorGUI.EndChangeCheck())
        {
            outputPath = GetDefaultOutputPath();
        }

        EditorGUI.BeginChangeCheck();

        noiseMode = (NoiseMode)EditorGUILayout.EnumPopup("Noise Type", noiseMode);

        if (EditorGUI.EndChangeCheck())
        {
            outputPath = GetDefaultOutputPath();
        }

        seed = EditorGUILayout.IntField("Seed", seed);

        shapeContrast = EditorGUILayout.Slider(
            "Shape Contrast",
            shapeContrast,
            0.25f,
            3.0f
        );

        worleyErosion = EditorGUILayout.Slider(
            "Worley Erosion",
            worleyErosion,
            0.0f,
            2.0f
        );

        if (GUILayout.Button("Use Default Output Path"))
        {
            outputPath = GetDefaultOutputPath();
        }

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button(noiseMode == NoiseMode.Shape ? "Generate Shape Noise" : "Generate Detail Noise"))
            Generate();
    }

    private void Generate()
    {
        if (computeShader == null)
        {
            Debug.LogError("Please assign CloudNoiseGenerator.compute first.");
            return;
        }

        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Compute shaders are not supported on this device");
            return;
        }

        outputPath = outputPath.Replace("\\", "/");
        string folder = Path.GetDirectoryName(outputPath)?.Replace("\\", "/");
        if (!outputPath.StartsWith("Assets/") || !outputPath.EndsWith(".asset") ||
            outputPath.Contains("../") || string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError("Choose an existing Assets folder and an .asset output path");
            return;
        }

        Object existingAsset = AssetDatabase.LoadMainAssetAtPath(outputPath);
        if (existingAsset != null && !(existingAsset is Texture3D))
        {
            Debug.LogError("Output path already contains a different asset type");
            return;
        }

        int voxelCount = size * size * size;
        Vector4[] noiseData = new Vector4[voxelCount];
        using (ComputeBuffer buffer = new ComputeBuffer(voxelCount, sizeof(float) * 4))
        {
            int kernel = computeShader.FindKernel("CSMain");
            computeShader.SetInt("_Size", size);
            computeShader.SetInt("_Seed", seed);
            computeShader.SetFloat("_ShapeContrast", shapeContrast);
            computeShader.SetFloat("_WorleyErosion", worleyErosion);
            computeShader.SetInt("_NoiseMode", (int)noiseMode);
            computeShader.SetBuffer(kernel, "_Result", buffer);

            int groups = Mathf.CeilToInt(size / 8.0f);
            computeShader.Dispatch(kernel, groups, groups, groups);
            buffer.GetData(noiseData);
        }

        Color32[] colors = new Color32[voxelCount];

        for (int i = 0; i < voxelCount; i++)
        {
            Vector4 v = noiseData[i];

            colors[i] = new Color32(
                FloatToByte(v.x),
                FloatToByte(v.y),
                FloatToByte(v.z),
                FloatToByte(v.w)
            );
        }

        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        try
        {
            texture.name = Path.GetFileNameWithoutExtension(outputPath);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixels32(colors);
            texture.Apply(false, false);

            if (existingAsset != null)
            {
                // Preserve renderer references when regenerating a texture
                EditorUtility.CopySerialized(texture, existingAsset);
                EditorUtility.SetDirty(existingAsset);
            }
            else
            {
                AssetDatabase.CreateAsset(texture, outputPath);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture3D>(outputPath);
            Debug.Log($"Generated 3D noise texture: {outputPath}, voxels: {voxelCount}");
        }
        finally
        {
            if (!AssetDatabase.Contains(texture))
                DestroyImmediate(texture);
        }
    }

    private static byte FloatToByte(float value)
    {
        return (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255.0f), 0, 255);
    }

    private string GetDefaultOutputPath()
    {
        string fileName = noiseMode == NoiseMode.Shape
            ? $"ShapeWorley{size}.asset"
            : $"DetailWorley{size}.asset";

        return $"Assets/Clouds/GeneratedNoise/{fileName}";
    }
}
