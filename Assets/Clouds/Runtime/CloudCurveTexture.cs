using UnityEngine;

public sealed class CloudCurveTexture
{
    private Texture2D texture;

    public Texture2D Texture => texture;

    public void Update(
        AnimationCurve densityCurve,
        AnimationCurve erosionCurve,
        AnimationCurve ambientOcclusionCurve,
        int resolution
    )
    {
        resolution = Mathf.Clamp(resolution, 16, 512);

        if (texture == null || texture.width != resolution)
        {
            Release();

            texture = new Texture2D(resolution, 1, TextureFormat.RGBA32, false, true);
            texture.name = "Cloud Density Curve Texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;
        }

        Color[] pixels = new Color[resolution];

        for (int x = 0; x < resolution; x++)
        {
            float t = x / (float)(resolution - 1);

            float density = Evaluate01(densityCurve, t, 1.0f);
            float erosion = Evaluate01(erosionCurve, t, 1.0f);
            float ambientOcclusion = Evaluate01(ambientOcclusionCurve, t, 1.0f);

            pixels[x] = new Color(density, erosion, ambientOcclusion, 1.0f);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
    }

    public void Release()
    {
        if (texture == null)
            return;

        if (Application.isPlaying)
            Object.Destroy(texture);
        else
            Object.DestroyImmediate(texture);

        texture = null;
    }

    private static float Evaluate01(AnimationCurve curve, float time, float fallback)
    {
        if (curve == null || curve.length == 0)
            return fallback;

        return Mathf.Clamp01(curve.Evaluate(time));
    }
}
