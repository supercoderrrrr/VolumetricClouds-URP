using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class CloudVolume : MonoBehaviour
{
    public enum CloudPreset
    {
        Sparse,
        Cloudy,
        Overcast,
        Stormy,
        Custom
    }

    public enum CloudDebugView
    {
        Final,
        CloudOnly,
        Alpha,
        Lighting,
        Weather,
        Height,
        RaySteps
    }

    [System.Serializable]
    private class CloudPresetState
    {
        public bool initialized;

        public bool showLayerGizmo;
        public float layerBottom;
        public float layerThickness;
        public float maxCloudDistance;
        public float distanceFade;

        public float shapeWorldSize;
        public float shapeHeightWorldSize;
        public float detailWorldSize;
        public float detailHeightWorldSize;
        public float weatherWorldSize;

        public float densityMultiplier;
        public float densityThreshold;
        public float coverage;
        public float shapeFactor;
        public Vector3 shapeOffset;

        public int curveTextureResolution;
        public AnimationCurve densityCurve;
        public AnimationCurve erosionCurve;
        public AnimationCurve ambientOcclusionCurve;

        public float bottomFade;
        public float topFade;
        public float topDensity;
        public float heightDensityPower;
        public float altitudeDistortion;

        public float erosionFactor;
        public bool microErosion;
        public float microErosionFactor;
        public float erosionOcclusion;
        public float detailStrength;

        public Texture2D weatherMap;
        public Vector2 weatherOffset;
        public float weatherInfluence;
        public float weatherMinimum;
        public float weatherContrast;
        public float weatherWindSpeedMultiplier;

        public float stepSize;
        public int maxSteps;
        public Texture2D ditherTexture;
        public float ditherStrength;

        public bool emptySpaceSkipping;
        public float emptyStepMultiplier;
        public int emptyStepTrigger;

        public bool temporalAccumulation;
        public float temporalBlend;
        public bool depthAwareUpsample;
        public float depthUpsampleTolerance;
        public float edgeSharpening;

        public Color horizonTint;
        public float aerialPerspective;
        public float horizonFade;

        public bool cloudShadows;
        public float cloudShadowStrength;
        public float cloudShadowSoftness;

        public CloudDebugView debugView;

        public Vector3 windDirection;
        public float windSpeed;
        public float shapeWindSpeedMultiplier;
        public float detailWindSpeedMultiplier;

        public Light sunLight;
        public Color cloudColor;
        public Color shadowColor;
        public float lightAbsorption;
        public int lightSteps;
        public float lightStepSize;
        public float ambientLight;
        public float sunLightDimmer;
        public Color scatteringTint;

        public float phaseForward;
        public float multiScattering;
        public float powderEffectIntensity;
        public float silverIntensity;
        public float silverSpread;
    }

    public static CloudVolume Active { get; private set; }

    [Header("Preset")]
    public CloudPreset cloudPreset = CloudPreset.Cloudy;

    [SerializeField, HideInInspector]
    private CloudPreset previousPreset = CloudPreset.Custom;

    [SerializeField, HideInInspector]
    private CloudPresetState sparsePreset = new CloudPresetState();

    [SerializeField, HideInInspector]
    private CloudPresetState cloudyPreset = new CloudPresetState();

    [SerializeField, HideInInspector]
    private CloudPresetState overcastPreset = new CloudPresetState();

    [SerializeField, HideInInspector]
    private CloudPresetState stormyPreset = new CloudPresetState();

    [Header("Cloud Layer")]
    public bool showLayerGizmo = false;

    [Min(0.0f)]
    public float layerBottom = 80.0f;

    [Min(1.0f)]
    public float layerThickness = 70.0f;

    [Min(10.0f)]
    public float maxCloudDistance = 1200.0f;

    [Min(1.0f)]
    public float distanceFade = 280.0f;

    [Header("World Noise Scale")]
    [Min(1.0f)]
    public float shapeWorldSize = 360.0f;

    [Min(1.0f)]
    public float shapeHeightWorldSize = 150.0f;

    [Min(1.0f)]
    public float detailWorldSize = 64.0f;

    [Min(1.0f)]
    public float detailHeightWorldSize = 48.0f;

    [Min(1.0f)]
    public float weatherWorldSize = 2200.0f;

    [Header("Shape")]
    [Range(0.0f, 24.0f)]
    public float densityMultiplier = 6.2f;

    [Range(0.0f, 1.0f)]
    public float densityThreshold = 0.24f;

    [Range(0.0f, 1.0f)]
    public float coverage = 0.78f;

    [Range(0.0f, 1.0f)]
    public float shapeFactor = 0.86f;

    public Vector3 shapeOffset = Vector3.zero;

    [Header("Height Curves")]
    [Range(16, 512)]
    public int curveTextureResolution = 128;

    public AnimationCurve densityCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f),
        new Keyframe(0.12f, 1.0f),
        new Keyframe(0.72f, 0.72f),
        new Keyframe(1.0f, 0.05f)
    );

    public AnimationCurve erosionCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.08f),
        new Keyframe(0.22f, 0.55f),
        new Keyframe(1.0f, 1.0f)
    );

    public AnimationCurve ambientOcclusionCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.45f),
        new Keyframe(0.45f, 0.75f),
        new Keyframe(1.0f, 1.0f)
    );

    [Header("Height Profile")]
    [Range(0.001f, 1.0f)]
    public float bottomFade = 0.08f;

    [Range(0.001f, 1.0f)]
    public float topFade = 0.32f;

    [Range(0.0f, 1.0f)]
    public float topDensity = 0.18f;

    [Range(0.2f, 4.0f)]
    public float heightDensityPower = 0.9f;

    [Range(-1.0f, 1.0f)]
    public float altitudeDistortion = 0.08f;

    [Header("Erosion")]
    [Range(0.0f, 1.0f)]
    public float erosionFactor = 0.38f;

    public bool microErosion = true;

    [Range(0.0f, 1.0f)]
    public float microErosionFactor = 0.14f;

    [Range(0.0f, 1.0f)]
    public float erosionOcclusion = 0.18f;

    [HideInInspector]
    public float detailStrength = 0.38f;

    [Header("Weather")]
    public Texture2D weatherMap;

    public Vector2 weatherOffset = Vector2.zero;

    [Range(0.0f, 1.0f)]
    public float weatherInfluence = 0.55f;

    [Range(0.0f, 1.0f)]
    public float weatherMinimum = 0.22f;

    [Range(0.1f, 3.0f)]
    public float weatherContrast = 1.15f;

    [Range(0.0f, 2.0f)]
    public float weatherWindSpeedMultiplier = 0.015f;

    [Header("Raymarch")]
    [Range(0.25f, 12.0f)]
    public float stepSize = 4.0f;

    [Range(8, 384)]
    public int maxSteps = 220;

    public Texture2D ditherTexture;

    [Range(0.0f, 1.0f)]
    public float ditherStrength = 0.22f;

    [Header("Raymarch Optimization")]
    public bool emptySpaceSkipping = true;

    [Range(1.0f, 8.0f)]
    public float emptyStepMultiplier = 2.5f;

    [Range(1, 16)]
    public int emptyStepTrigger = 3;

    [Header("Reconstruction")]
    public bool temporalAccumulation = true;

    [Range(0.0f, 0.98f)]
    public float temporalBlend = 0.86f;

    public bool depthAwareUpsample = true;

    [Range(0.001f, 0.25f)]
    public float depthUpsampleTolerance = 0.035f;

    [Range(0.0f, 1.0f)]
    public float edgeSharpening = 0.18f;

    [Header("Atmosphere Blend")]
    public Color horizonTint = new Color(0.58f, 0.68f, 0.82f, 1.0f);

    [Range(0.0f, 1.0f)]
    public float aerialPerspective = 0.35f;

    [Range(0.0f, 2.0f)]
    public float horizonFade = 0.8f;

    [Header("Cloud Shadows")]
    public bool cloudShadows = true;

    [Range(0.0f, 1.0f)]
    public float cloudShadowStrength = 0.28f;

    [Range(0.0f, 4.0f)]
    public float cloudShadowSoftness = 1.2f;

    [Header("Debug")]
    public CloudDebugView debugView = CloudDebugView.Final;

    [Header("Wind")]
    public Vector3 windDirection = new Vector3(1.0f, 0.0f, 0.25f);

    [Range(0.0f, 10.0f)]
    public float windSpeed = 0.08f;

    [Range(0.0f, 2.0f)]
    public float shapeWindSpeedMultiplier = 0.25f;

    [Range(0.0f, 5.0f)]
    public float detailWindSpeedMultiplier = 0.65f;

    [Header("Lighting")]
    public Light sunLight;

    public Color cloudColor = Color.white;
    public Color shadowColor = new Color(0.42f, 0.50f, 0.63f, 1.0f);

    [Range(0.0f, 4.0f)]
    public float lightAbsorption = 1.15f;

    [Range(1, 32)]
    public int lightSteps = 8;

    [Range(1.0f, 80.0f)]
    public float lightStepSize = 12.0f;

    [Range(0.0f, 2.0f)]
    public float ambientLight = 0.34f;

    [Range(0.0f, 4.0f)]
    public float sunLightDimmer = 1.65f;

    public Color scatteringTint = Color.white;

    [Header("Scattering")]
    [Range(-0.8f, 0.8f)]
    public float phaseForward = 0.58f;

    [Range(0.0f, 1.0f)]
    public float multiScattering = 0.58f;

    [Range(0.0f, 1.0f)]
    public float powderEffectIntensity = 0.42f;

    [Range(0.0f, 5.0f)]
    public float silverIntensity = 1.15f;

    [Range(1.0f, 32.0f)]
    public float silverSpread = 9.0f;

    public float LayerTop => layerBottom + layerThickness;

    private void OnEnable()
    {
        Active = this;
    }

    private void OnDisable()
    {
        if (Active == this)
            Active = null;
    }

    private void OnValidate()
    {
        EnsurePresetBank();

        layerBottom = Mathf.Max(0.0f, layerBottom);
        layerThickness = Mathf.Max(1.0f, layerThickness);
        maxCloudDistance = Mathf.Max(10.0f, maxCloudDistance);
        distanceFade = Mathf.Clamp(distanceFade, 1.0f, maxCloudDistance);

        shapeWorldSize = Mathf.Max(1.0f, shapeWorldSize);
        shapeHeightWorldSize = Mathf.Max(1.0f, shapeHeightWorldSize);
        detailWorldSize = Mathf.Max(1.0f, detailWorldSize);
        detailHeightWorldSize = Mathf.Max(1.0f, detailHeightWorldSize);
        weatherWorldSize = Mathf.Max(1.0f, weatherWorldSize);
        curveTextureResolution = Mathf.Clamp(curveTextureResolution, 16, 512);

        EnsureCurves();
        detailStrength = erosionFactor;

        if (cloudPreset != CloudPreset.Custom && cloudPreset != previousPreset)
        {
            ApplyPreset(cloudPreset);
            previousPreset = cloudPreset;
        }
    }

    [ContextMenu("Apply Current Preset")]
    public void ApplyCurrentPreset()
    {
        SaveCurrentPreset();
    }

    public void SaveCurrentPreset()
    {
        EnsurePresetBank();

        CloudPresetState presetState = GetPresetState(cloudPreset);
        if (presetState == null)
            return;

        CaptureState(presetState);
        previousPreset = cloudPreset;
    }

    [ContextMenu("Apply Portfolio Shot Look")]
    public void ApplyPortfolioShotLook()
    {
        cloudPreset = CloudPreset.Custom;
        previousPreset = CloudPreset.Custom;

        showLayerGizmo = false;
        layerBottom = 55.0f;
        layerThickness = 135.0f;
        maxCloudDistance = 1750.0f;
        distanceFade = 420.0f;

        shapeWorldSize = 390.0f;
        shapeHeightWorldSize = 165.0f;
        detailWorldSize = 70.0f;
        detailHeightWorldSize = 52.0f;
        weatherWorldSize = 2500.0f;

        densityMultiplier = 8.9f;
        densityThreshold = 0.17f;
        coverage = 0.88f;
        shapeFactor = 0.76f;
        bottomFade = 0.045f;
        topFade = 0.44f;
        topDensity = 0.24f;
        heightDensityPower = 0.82f;
        altitudeDistortion = 0.1f;

        erosionFactor = 0.27f;
        microErosion = true;
        microErosionFactor = 0.09f;
        erosionOcclusion = 0.16f;
        weatherInfluence = 0.32f;
        weatherMinimum = 0.38f;
        weatherContrast = 1.12f;

        stepSize = 3.0f;
        maxSteps = 320;
        ditherStrength = 0.18f;
        temporalAccumulation = true;
        temporalBlend = 0.78f;
        depthAwareUpsample = true;
        depthUpsampleTolerance = 0.03f;
        edgeSharpening = 0.14f;
        emptySpaceSkipping = true;
        emptyStepMultiplier = 2.2f;
        emptyStepTrigger = 3;

        horizonTint = new Color(0.58f, 0.68f, 0.82f, 1.0f);
        aerialPerspective = 0.32f;
        horizonFade = 0.78f;
        cloudShadows = true;
        cloudShadowStrength = 0.24f;
        cloudShadowSoftness = 1.25f;
        debugView = CloudDebugView.Final;

        windDirection = new Vector3(1.0f, 0.0f, 0.25f);
        windSpeed = 0.035f;
        shapeWindSpeedMultiplier = 0.18f;
        detailWindSpeedMultiplier = 0.35f;

        cloudColor = Color.white;
        shadowColor = new Color(0.42f, 0.50f, 0.63f, 1.0f);
        lightAbsorption = 1.12f;
        lightSteps = 8;
        lightStepSize = 13.0f;
        ambientLight = 0.36f;
        sunLightDimmer = 1.58f;
        scatteringTint = Color.white;

        phaseForward = 0.58f;
        multiScattering = 0.62f;
        powderEffectIntensity = 0.45f;
        silverIntensity = 1.05f;
        silverSpread = 8.0f;

        densityCurve = MakeCurve(0.0f, 0.0f, 0.1f, 1.0f, 0.7f, 0.76f, 1.0f, 0.06f);
        erosionCurve = MakeCurve(0.0f, 0.07f, 0.24f, 0.5f, 1.0f, 0.96f);
        ambientOcclusionCurve = MakeCurve(0.0f, 0.44f, 0.45f, 0.74f, 1.0f, 1.0f);
        detailStrength = erosionFactor;
    }

    [ContextMenu("Apply Performance Preview Look")]
    public void ApplyPerformancePreviewLook()
    {
        ApplyPortfolioShotLook();
        stepSize = 5.0f;
        maxSteps = 180;
        lightSteps = 5;
        lightStepSize = 18.0f;
        temporalBlend = 0.84f;
        ditherStrength = 0.25f;
        emptyStepMultiplier = 3.0f;
        cloudShadowStrength = 0.16f;
    }

    [ContextMenu("Reset Cloud Debug View")]
    public void ResetCloudDebugView()
    {
        debugView = CloudDebugView.Final;
        showLayerGizmo = false;
    }

    private void ApplyPreset(CloudPreset preset)
    {
        EnsurePresetBank();

        CloudPresetState presetState = GetPresetState(preset);
        if (presetState == null)
            return;

        ApplyState(presetState);
        previousPreset = preset;
    }

    [ContextMenu("Reset Built-In Preset Bank")]
    public void ResetBuiltInPresetBank()
    {
        InitializeBuiltInPresetBank(true);
        ApplyPreset(cloudPreset);
    }

    private void EnsurePresetBank()
    {
        if (sparsePreset == null)
            sparsePreset = new CloudPresetState();

        if (cloudyPreset == null)
            cloudyPreset = new CloudPresetState();

        if (overcastPreset == null)
            overcastPreset = new CloudPresetState();

        if (stormyPreset == null)
            stormyPreset = new CloudPresetState();

        if (!sparsePreset.initialized || !cloudyPreset.initialized || !overcastPreset.initialized || !stormyPreset.initialized)
            InitializeBuiltInPresetBank(false);
    }

    private void InitializeBuiltInPresetBank(bool force)
    {
        CloudPreset activePreset = cloudPreset;
        CloudPreset previousActivePreset = previousPreset;
        CloudPresetState currentState = new CloudPresetState();
        CaptureState(currentState);

        if (force || !sparsePreset.initialized)
        {
            ApplyBuiltInPreset(CloudPreset.Sparse);
            CaptureState(sparsePreset);
        }

        if (force || !cloudyPreset.initialized)
        {
            ApplyBuiltInPreset(CloudPreset.Cloudy);
            CaptureState(cloudyPreset);
        }

        if (force || !overcastPreset.initialized)
        {
            ApplyBuiltInPreset(CloudPreset.Overcast);
            CaptureState(overcastPreset);
        }

        if (force || !stormyPreset.initialized)
        {
            ApplyBuiltInPreset(CloudPreset.Stormy);
            CaptureState(stormyPreset);
        }

        ApplyState(currentState);
        cloudPreset = activePreset;
        previousPreset = previousActivePreset;
    }

    private CloudPresetState GetPresetState(CloudPreset preset)
    {
        switch (preset)
        {
            case CloudPreset.Sparse:
                return sparsePreset;
            case CloudPreset.Cloudy:
                return cloudyPreset;
            case CloudPreset.Overcast:
                return overcastPreset;
            case CloudPreset.Stormy:
                return stormyPreset;
            default:
                return null;
        }
    }

    private void CaptureState(CloudPresetState state)
    {
        state.initialized = true;
        state.showLayerGizmo = showLayerGizmo;
        state.layerBottom = layerBottom;
        state.layerThickness = layerThickness;
        state.maxCloudDistance = maxCloudDistance;
        state.distanceFade = distanceFade;

        state.shapeWorldSize = shapeWorldSize;
        state.shapeHeightWorldSize = shapeHeightWorldSize;
        state.detailWorldSize = detailWorldSize;
        state.detailHeightWorldSize = detailHeightWorldSize;
        state.weatherWorldSize = weatherWorldSize;

        state.densityMultiplier = densityMultiplier;
        state.densityThreshold = densityThreshold;
        state.coverage = coverage;
        state.shapeFactor = shapeFactor;
        state.shapeOffset = shapeOffset;

        state.curveTextureResolution = curveTextureResolution;
        state.densityCurve = CloneCurve(densityCurve);
        state.erosionCurve = CloneCurve(erosionCurve);
        state.ambientOcclusionCurve = CloneCurve(ambientOcclusionCurve);

        state.bottomFade = bottomFade;
        state.topFade = topFade;
        state.topDensity = topDensity;
        state.heightDensityPower = heightDensityPower;
        state.altitudeDistortion = altitudeDistortion;

        state.erosionFactor = erosionFactor;
        state.microErosion = microErosion;
        state.microErosionFactor = microErosionFactor;
        state.erosionOcclusion = erosionOcclusion;
        state.detailStrength = detailStrength;

        state.weatherMap = weatherMap;
        state.weatherOffset = weatherOffset;
        state.weatherInfluence = weatherInfluence;
        state.weatherMinimum = weatherMinimum;
        state.weatherContrast = weatherContrast;
        state.weatherWindSpeedMultiplier = weatherWindSpeedMultiplier;

        state.stepSize = stepSize;
        state.maxSteps = maxSteps;
        state.ditherTexture = ditherTexture;
        state.ditherStrength = ditherStrength;

        state.emptySpaceSkipping = emptySpaceSkipping;
        state.emptyStepMultiplier = emptyStepMultiplier;
        state.emptyStepTrigger = emptyStepTrigger;

        state.temporalAccumulation = temporalAccumulation;
        state.temporalBlend = temporalBlend;
        state.depthAwareUpsample = depthAwareUpsample;
        state.depthUpsampleTolerance = depthUpsampleTolerance;
        state.edgeSharpening = edgeSharpening;

        state.horizonTint = horizonTint;
        state.aerialPerspective = aerialPerspective;
        state.horizonFade = horizonFade;

        state.cloudShadows = cloudShadows;
        state.cloudShadowStrength = cloudShadowStrength;
        state.cloudShadowSoftness = cloudShadowSoftness;

        state.debugView = debugView;

        state.windDirection = windDirection;
        state.windSpeed = windSpeed;
        state.shapeWindSpeedMultiplier = shapeWindSpeedMultiplier;
        state.detailWindSpeedMultiplier = detailWindSpeedMultiplier;

        state.sunLight = sunLight;
        state.cloudColor = cloudColor;
        state.shadowColor = shadowColor;
        state.lightAbsorption = lightAbsorption;
        state.lightSteps = lightSteps;
        state.lightStepSize = lightStepSize;
        state.ambientLight = ambientLight;
        state.sunLightDimmer = sunLightDimmer;
        state.scatteringTint = scatteringTint;

        state.phaseForward = phaseForward;
        state.multiScattering = multiScattering;
        state.powderEffectIntensity = powderEffectIntensity;
        state.silverIntensity = silverIntensity;
        state.silverSpread = silverSpread;
    }

    private void ApplyState(CloudPresetState state)
    {
        if (state == null || !state.initialized)
            return;

        showLayerGizmo = state.showLayerGizmo;
        layerBottom = state.layerBottom;
        layerThickness = state.layerThickness;
        maxCloudDistance = state.maxCloudDistance;
        distanceFade = state.distanceFade;

        shapeWorldSize = state.shapeWorldSize;
        shapeHeightWorldSize = state.shapeHeightWorldSize;
        detailWorldSize = state.detailWorldSize;
        detailHeightWorldSize = state.detailHeightWorldSize;
        weatherWorldSize = state.weatherWorldSize;

        densityMultiplier = state.densityMultiplier;
        densityThreshold = state.densityThreshold;
        coverage = state.coverage;
        shapeFactor = state.shapeFactor;
        shapeOffset = state.shapeOffset;

        curveTextureResolution = state.curveTextureResolution;
        densityCurve = CloneCurve(state.densityCurve);
        erosionCurve = CloneCurve(state.erosionCurve);
        ambientOcclusionCurve = CloneCurve(state.ambientOcclusionCurve);

        bottomFade = state.bottomFade;
        topFade = state.topFade;
        topDensity = state.topDensity;
        heightDensityPower = state.heightDensityPower;
        altitudeDistortion = state.altitudeDistortion;

        erosionFactor = state.erosionFactor;
        microErosion = state.microErosion;
        microErosionFactor = state.microErosionFactor;
        erosionOcclusion = state.erosionOcclusion;
        detailStrength = state.detailStrength;

        weatherMap = state.weatherMap;
        weatherOffset = state.weatherOffset;
        weatherInfluence = state.weatherInfluence;
        weatherMinimum = state.weatherMinimum;
        weatherContrast = state.weatherContrast;
        weatherWindSpeedMultiplier = state.weatherWindSpeedMultiplier;

        stepSize = state.stepSize;
        maxSteps = state.maxSteps;
        ditherTexture = state.ditherTexture;
        ditherStrength = state.ditherStrength;

        emptySpaceSkipping = state.emptySpaceSkipping;
        emptyStepMultiplier = state.emptyStepMultiplier;
        emptyStepTrigger = state.emptyStepTrigger;

        temporalAccumulation = state.temporalAccumulation;
        temporalBlend = state.temporalBlend;
        depthAwareUpsample = state.depthAwareUpsample;
        depthUpsampleTolerance = state.depthUpsampleTolerance;
        edgeSharpening = state.edgeSharpening;

        horizonTint = state.horizonTint;
        aerialPerspective = state.aerialPerspective;
        horizonFade = state.horizonFade;

        cloudShadows = state.cloudShadows;
        cloudShadowStrength = state.cloudShadowStrength;
        cloudShadowSoftness = state.cloudShadowSoftness;

        debugView = state.debugView;

        windDirection = state.windDirection;
        windSpeed = state.windSpeed;
        shapeWindSpeedMultiplier = state.shapeWindSpeedMultiplier;
        detailWindSpeedMultiplier = state.detailWindSpeedMultiplier;

        sunLight = state.sunLight;
        cloudColor = state.cloudColor;
        shadowColor = state.shadowColor;
        lightAbsorption = state.lightAbsorption;
        lightSteps = state.lightSteps;
        lightStepSize = state.lightStepSize;
        ambientLight = state.ambientLight;
        sunLightDimmer = state.sunLightDimmer;
        scatteringTint = state.scatteringTint;

        phaseForward = state.phaseForward;
        multiScattering = state.multiScattering;
        powderEffectIntensity = state.powderEffectIntensity;
        silverIntensity = state.silverIntensity;
        silverSpread = state.silverSpread;

        detailStrength = erosionFactor;
        EnsureCurves();
    }

    private static AnimationCurve CloneCurve(AnimationCurve source)
    {
        if (source == null)
            return null;

        return new AnimationCurve(source.keys)
        {
            preWrapMode = source.preWrapMode,
            postWrapMode = source.postWrapMode
        };
    }

    private void ApplyBuiltInPreset(CloudPreset preset)
    {
        switch (preset)
        {
            case CloudPreset.Sparse:
                densityMultiplier = 5.8f;
                densityThreshold = 0.25f;
                coverage = 0.62f;
                shapeFactor = 0.9f;
                shapeWorldSize = 390.0f;
                shapeHeightWorldSize = 155.0f;
                detailWorldSize = 72.0f;
                detailHeightWorldSize = 48.0f;
                weatherWorldSize = 2300.0f;
                bottomFade = 0.08f;
                topFade = 0.3f;
                topDensity = 0.12f;
                heightDensityPower = 0.98f;
                altitudeDistortion = 0.08f;
                erosionFactor = 0.46f;
                microErosion = true;
                microErosionFactor = 0.16f;
                erosionOcclusion = 0.18f;
                weatherInfluence = 0.48f;
                weatherMinimum = 0.28f;
                ambientLight = 0.4f;
                sunLightDimmer = 1.55f;
                lightAbsorption = 0.95f;
                phaseForward = 0.55f;
                multiScattering = 0.6f;
                powderEffectIntensity = 0.35f;
                temporalBlend = 0.9f;
                cloudShadowStrength = 0.18f;
                aerialPerspective = 0.42f;
                horizonFade = 0.9f;
                emptyStepMultiplier = 3.0f;
                densityCurve = MakeCurve(0.0f, 0.0f, 0.18f, 0.85f, 0.55f, 0.62f, 1.0f, 0.0f);
                erosionCurve = MakeCurve(0.0f, 0.18f, 0.35f, 0.75f, 1.0f, 1.0f);
                ambientOcclusionCurve = MakeCurve(0.0f, 0.58f, 0.5f, 0.85f, 1.0f, 1.0f);
                break;

            case CloudPreset.Cloudy:
                densityMultiplier = 8.2f;
                densityThreshold = 0.18f;
                coverage = 0.86f;
                shapeFactor = 0.78f;
                shapeWorldSize = 390.0f;
                shapeHeightWorldSize = 150.0f;
                detailWorldSize = 70.0f;
                detailHeightWorldSize = 48.0f;
                weatherWorldSize = 2400.0f;
                bottomFade = 0.06f;
                topFade = 0.38f;
                topDensity = 0.24f;
                heightDensityPower = 0.84f;
                altitudeDistortion = 0.1f;
                erosionFactor = 0.3f;
                microErosion = true;
                microErosionFactor = 0.1f;
                erosionOcclusion = 0.16f;
                weatherInfluence = 0.38f;
                weatherMinimum = 0.38f;
                ambientLight = 0.36f;
                sunLightDimmer = 1.58f;
                lightAbsorption = 1.12f;
                phaseForward = 0.58f;
                multiScattering = 0.62f;
                powderEffectIntensity = 0.45f;
                temporalBlend = 0.8f;
                cloudShadowStrength = 0.24f;
                aerialPerspective = 0.32f;
                horizonFade = 0.8f;
                emptyStepMultiplier = 2.2f;
                shadowColor = new Color(0.42f, 0.50f, 0.63f, 1.0f);
                densityCurve = MakeCurve(0.0f, 0.0f, 0.12f, 1.0f, 0.72f, 0.72f, 1.0f, 0.05f);
                erosionCurve = MakeCurve(0.0f, 0.08f, 0.22f, 0.55f, 1.0f, 1.0f);
                ambientOcclusionCurve = MakeCurve(0.0f, 0.45f, 0.45f, 0.75f, 1.0f, 1.0f);
                break;

            case CloudPreset.Overcast:
                densityMultiplier = 7.8f;
                densityThreshold = 0.14f;
                coverage = 0.97f;
                shapeFactor = 0.58f;
                shapeWorldSize = 520.0f;
                shapeHeightWorldSize = 180.0f;
                detailWorldSize = 82.0f;
                detailHeightWorldSize = 58.0f;
                weatherWorldSize = 3200.0f;
                bottomFade = 0.04f;
                topFade = 0.56f;
                topDensity = 0.38f;
                heightDensityPower = 0.72f;
                altitudeDistortion = 0.04f;
                erosionFactor = 0.2f;
                microErosion = true;
                microErosionFactor = 0.06f;
                erosionOcclusion = 0.08f;
                weatherInfluence = 0.35f;
                weatherMinimum = 0.55f;
                ambientLight = 0.48f;
                sunLightDimmer = 1.25f;
                lightAbsorption = 1.05f;
                phaseForward = 0.46f;
                multiScattering = 0.7f;
                powderEffectIntensity = 0.5f;
                temporalBlend = 0.9f;
                cloudShadowStrength = 0.34f;
                aerialPerspective = 0.28f;
                horizonFade = 0.65f;
                emptyStepMultiplier = 2.0f;
                shadowColor = new Color(0.46f, 0.52f, 0.62f, 1.0f);
                densityCurve = MakeCurve(0.0f, 0.22f, 0.08f, 1.0f, 0.86f, 0.95f, 1.0f, 0.18f);
                erosionCurve = MakeCurve(0.0f, 0.04f, 0.45f, 0.28f, 1.0f, 0.62f);
                ambientOcclusionCurve = MakeCurve(0.0f, 0.32f, 0.55f, 0.62f, 1.0f, 0.9f);
                break;

            case CloudPreset.Stormy:
                densityMultiplier = 10.5f;
                densityThreshold = 0.18f;
                coverage = 0.92f;
                shapeFactor = 0.82f;
                shapeWorldSize = 340.0f;
                shapeHeightWorldSize = 190.0f;
                detailWorldSize = 62.0f;
                detailHeightWorldSize = 56.0f;
                weatherWorldSize = 2100.0f;
                bottomFade = 0.035f;
                topFade = 0.48f;
                topDensity = 0.16f;
                heightDensityPower = 0.82f;
                altitudeDistortion = 0.16f;
                erosionFactor = 0.34f;
                microErosion = true;
                microErosionFactor = 0.12f;
                erosionOcclusion = 0.3f;
                weatherInfluence = 0.48f;
                weatherMinimum = 0.35f;
                ambientLight = 0.22f;
                sunLightDimmer = 1.1f;
                lightAbsorption = 1.85f;
                phaseForward = 0.62f;
                multiScattering = 0.48f;
                powderEffectIntensity = 0.58f;
                temporalBlend = 0.82f;
                cloudShadowStrength = 0.48f;
                aerialPerspective = 0.22f;
                horizonFade = 0.55f;
                emptyStepMultiplier = 2.3f;
                shadowColor = new Color(0.20f, 0.25f, 0.35f, 1.0f);
                densityCurve = MakeCurve(0.0f, 0.35f, 0.06f, 1.0f, 0.82f, 0.95f, 1.0f, 0.08f);
                erosionCurve = MakeCurve(0.0f, 0.05f, 0.22f, 0.42f, 1.0f, 0.95f);
                ambientOcclusionCurve = MakeCurve(0.0f, 0.24f, 0.5f, 0.55f, 1.0f, 0.85f);
                break;
        }

        detailStrength = erosionFactor;
        EnsureCurves();
    }

    private void EnsureCurves()
    {
        if (densityCurve == null || densityCurve.length == 0)
            densityCurve = MakeCurve(0.0f, 0.0f, 0.12f, 1.0f, 0.72f, 0.72f, 1.0f, 0.05f);

        if (erosionCurve == null || erosionCurve.length == 0)
            erosionCurve = MakeCurve(0.0f, 0.08f, 0.22f, 0.55f, 1.0f, 1.0f);

        if (ambientOcclusionCurve == null || ambientOcclusionCurve.length == 0)
            ambientOcclusionCurve = MakeCurve(0.0f, 0.45f, 0.45f, 0.75f, 1.0f, 1.0f);
    }

    private static AnimationCurve MakeCurve(params float[] values)
    {
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i + 1 < values.Length; i += 2)
            curve.AddKey(new Keyframe(values[i], values[i + 1]));

        for (int i = 0; i < curve.length; i++)
            curve.SmoothTangents(i, 0.0f);

        return curve;
    }

    private void OnDrawGizmos()
    {
        if (!showLayerGizmo)
            return;

        float previewSize = Mathf.Min(maxCloudDistance * 0.35f, 450.0f);
        Vector3 center = new Vector3(transform.position.x, layerBottom + layerThickness * 0.5f, transform.position.z);

        Gizmos.color = new Color(0.35f, 0.75f, 1.0f, 0.9f);
        Gizmos.DrawWireCube(center, new Vector3(previewSize, layerThickness, previewSize));
    }
}
