using UnityEditor;
using UnityEngine;

public class CloudNoisePreviewWindow : EditorWindow
{
    private Texture3D noiseTexture;
    private Material previewMaterial;
    private float slice = 0.5f;
    private int channel = 0;

    private readonly string[] channelNames = { "R - Shape", "G - Worley 8", "B - Worley 16", "A - Worley 32" };

    [MenuItem("Tools/Volumetric Clouds/Noise Preview")]
    private static void Open()
    {
        GetWindow<CloudNoisePreviewWindow>("Noise Preview");
    }

    private void OnEnable()
    {
        Shader shader = Shader.Find("Hidden/VolumetricClouds/CloudNoisePreview");

        if (shader != null)
            previewMaterial = new Material(shader);
    }

    private void OnDisable()
    {
        if (previewMaterial != null)
            DestroyImmediate(previewMaterial);
    }

    private void OnGUI()
    {
        noiseTexture = (Texture3D)EditorGUILayout.ObjectField(
            "Noise Texture",
            noiseTexture,
            typeof(Texture3D),
            false
        );

        slice = EditorGUILayout.Slider("Slice", slice, 0.0f, 1.0f);

        channel = GUILayout.Toolbar(channel, channelNames);

        GUILayout.Space(8);

        Rect previewRect = GUILayoutUtility.GetAspectRect(1.0f);

        if (noiseTexture == null)
        {
            EditorGUI.HelpBox(previewRect, "Assign a Texture3D to preview.", MessageType.Info);
            return;
        }

        if (previewMaterial == null)
        {
            EditorGUI.HelpBox(previewRect, "Preview shader not found.", MessageType.Error);
            return;
        }

        previewMaterial.SetTexture("_NoiseTex", noiseTexture);
        previewMaterial.SetFloat("_Slice", slice);
        previewMaterial.SetFloat("_Channel", channel);

        EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, previewMaterial);
    }
}