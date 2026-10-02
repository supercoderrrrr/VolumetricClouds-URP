using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudRaymarchRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Resources")]
        public Material cloudMaterial;
        public Texture3D shapeNoise;
        public Texture3D detailNoise;

        [Header("Performance")]
        [Range(0.25f, 1.0f)]
        public float renderScale = 0.75f;

        [Header("Fallback")]
        public Texture2D fallbackDitherTexture;
    }

    public Settings settings = new Settings();

    private CloudRaymarchPass cloudPass;

    public override void Create()
    {
        cloudPass = new CloudRaymarchPass(settings);
        cloudPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (cloudPass == null)
            Create();

        if (settings.cloudMaterial == null)
            return;

        cloudPass.Setup(renderer.cameraColorTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (cloudPass == null)
            Create();

        if (settings.cloudMaterial == null || settings.shapeNoise == null)
            return;

        cloudPass.renderPassEvent = settings.renderPassEvent;
        renderer.EnqueuePass(cloudPass);
    }

    protected override void Dispose(bool disposing)
    {
        cloudPass?.Dispose();
    }

    private class CloudRaymarchPass : ScriptableRenderPass
    {
        private readonly Settings settings;
        private readonly ProfilingSampler passProfilingSampler = new ProfilingSampler("Cloud Raymarch");
        private readonly CloudCurveTexture cloudCurveTexture = new CloudCurveTexture();

        private RTHandle source;
        private RTHandle cloudLowResTexture;
        private RTHandle cloudTemporalTexture;
        private RTHandle cloudHistoryTexture;
        private RTHandle tempColorTexture;

        private bool historyValid;
        private int historyWidth;
        private int historyHeight;
        private Matrix4x4 previousViewProjectionMatrix = Matrix4x4.identity;
        private Vector3 previousCameraPosition;
        private Quaternion previousCameraRotation;
        private int previousCameraInstanceId = -1;
        private CameraType previousCameraType = CameraType.Preview;

        private static readonly int ShapeNoiseTexId = Shader.PropertyToID("_ShapeNoiseTex");
        private static readonly int DetailNoiseTexId = Shader.PropertyToID("_DetailNoiseTex");
        private static readonly int HasDetailNoiseId = Shader.PropertyToID("_HasDetailNoise");
        private static readonly int DensityCurveTexId = Shader.PropertyToID("_DensityCurveTex");
        private static readonly int CloudLowResTextureId = Shader.PropertyToID("_CloudLowResTexture");
        private static readonly int CloudCurrentTextureId = Shader.PropertyToID("_CloudCurrentTexture");
        private static readonly int CloudHistoryTextureId = Shader.PropertyToID("_CloudHistoryTexture");

        private static readonly int LayerBottomId = Shader.PropertyToID("_LayerBottom");
        private static readonly int LayerThicknessId = Shader.PropertyToID("_LayerThickness");
        private static readonly int MaxCloudDistanceId = Shader.PropertyToID("_MaxCloudDistance");
        private static readonly int DistanceFadeId = Shader.PropertyToID("_DistanceFade");

        private static readonly int ShapeWorldSizeId = Shader.PropertyToID("_ShapeWorldSize");
        private static readonly int ShapeHeightWorldSizeId = Shader.PropertyToID("_ShapeHeightWorldSize");
        private static readonly int DetailWorldSizeId = Shader.PropertyToID("_DetailWorldSize");
        private static readonly int DetailHeightWorldSizeId = Shader.PropertyToID("_DetailHeightWorldSize");
        private static readonly int WeatherWorldSizeId = Shader.PropertyToID("_WeatherWorldSize");

        private static readonly int CloudColorId = Shader.PropertyToID("_CloudColor");
        private static readonly int ShadowColorId = Shader.PropertyToID("_ShadowColor");
        private static readonly int LightDirectionId = Shader.PropertyToID("_LightDirection");
        private static readonly int SunColorId = Shader.PropertyToID("_SunColor");
        private static readonly int ScatteringTintId = Shader.PropertyToID("_ScatteringTint");

        private static readonly int DensityMultiplierId = Shader.PropertyToID("_DensityMultiplier");
        private static readonly int DensityThresholdId = Shader.PropertyToID("_DensityThreshold");
        private static readonly int CoverageId = Shader.PropertyToID("_Coverage");
        private static readonly int ShapeFactorId = Shader.PropertyToID("_ShapeFactor");
        private static readonly int ShapeOffsetId = Shader.PropertyToID("_ShapeOffset");

        private static readonly int BottomFadeId = Shader.PropertyToID("_BottomFade");
        private static readonly int TopFadeId = Shader.PropertyToID("_TopFade");
        private static readonly int TopDensityId = Shader.PropertyToID("_TopDensity");
        private static readonly int HeightDensityPowerId = Shader.PropertyToID("_HeightDensityPower");
        private static readonly int AltitudeDistortionId = Shader.PropertyToID("_AltitudeDistortion");

        private static readonly int ErosionFactorId = Shader.PropertyToID("_ErosionFactor");
        private static readonly int MicroErosionId = Shader.PropertyToID("_MicroErosion");
        private static readonly int MicroErosionFactorId = Shader.PropertyToID("_MicroErosionFactor");
        private static readonly int ErosionOcclusionId = Shader.PropertyToID("_ErosionOcclusion");

        private static readonly int WeatherMapId = Shader.PropertyToID("_WeatherMap");
        private static readonly int HasWeatherMapId = Shader.PropertyToID("_HasWeatherMap");
        private static readonly int WeatherOffsetId = Shader.PropertyToID("_WeatherOffset");
        private static readonly int WeatherInfluenceId = Shader.PropertyToID("_WeatherInfluence");
        private static readonly int WeatherMinimumId = Shader.PropertyToID("_WeatherMinimum");
        private static readonly int WeatherContrastId = Shader.PropertyToID("_WeatherContrast");

        private static readonly int StepSizeId = Shader.PropertyToID("_StepSize");
        private static readonly int MaxStepsId = Shader.PropertyToID("_MaxSteps");
        private static readonly int DitherTextureId = Shader.PropertyToID("_DitherTexture");
        private static readonly int HasDitherTextureId = Shader.PropertyToID("_HasDitherTexture");
        private static readonly int DitherStrengthId = Shader.PropertyToID("_DitherStrength");
        private static readonly int TemporalBlendId = Shader.PropertyToID("_TemporalBlend");
        private static readonly int DepthAwareUpsampleId = Shader.PropertyToID("_DepthAwareUpsample");
        private static readonly int DepthUpsampleToleranceId = Shader.PropertyToID("_DepthUpsampleTolerance");
        private static readonly int EdgeSharpeningId = Shader.PropertyToID("_EdgeSharpening");
        private static readonly int LowResTexelSizeId = Shader.PropertyToID("_LowResTexelSize");
        private static readonly int PreviousViewProjectionMatrixId = Shader.PropertyToID("_PreviousViewProjectionMatrix");
        private static readonly int CurrentInverseViewProjectionMatrixId = Shader.PropertyToID("_CurrentInverseViewProjectionMatrix");
        private static readonly int EmptySpaceSkippingId = Shader.PropertyToID("_EmptySpaceSkipping");
        private static readonly int EmptyStepMultiplierId = Shader.PropertyToID("_EmptyStepMultiplier");
        private static readonly int EmptyStepTriggerId = Shader.PropertyToID("_EmptyStepTrigger");
        private static readonly int HorizonTintId = Shader.PropertyToID("_HorizonTint");
        private static readonly int AerialPerspectiveId = Shader.PropertyToID("_AerialPerspective");
        private static readonly int HorizonFadeId = Shader.PropertyToID("_HorizonFade");
        private static readonly int CloudShadowsId = Shader.PropertyToID("_CloudShadows");
        private static readonly int CloudShadowStrengthId = Shader.PropertyToID("_CloudShadowStrength");
        private static readonly int CloudShadowSoftnessId = Shader.PropertyToID("_CloudShadowSoftness");
        private static readonly int DebugViewId = Shader.PropertyToID("_DebugView");

        private static readonly int WindOffsetId = Shader.PropertyToID("_WindOffset");
        private static readonly int DetailWindOffsetId = Shader.PropertyToID("_DetailWindOffset");

        private static readonly int LightAbsorptionId = Shader.PropertyToID("_LightAbsorption");
        private static readonly int LightStepsId = Shader.PropertyToID("_LightSteps");
        private static readonly int LightStepSizeId = Shader.PropertyToID("_LightStepSize");
        private static readonly int AmbientLightId = Shader.PropertyToID("_AmbientLight");
        private static readonly int SunLightDimmerId = Shader.PropertyToID("_SunLightDimmer");

        private static readonly int PhaseForwardId = Shader.PropertyToID("_PhaseForward");
        private static readonly int MultiScatteringId = Shader.PropertyToID("_MultiScattering");
        private static readonly int PowderEffectIntensityId = Shader.PropertyToID("_PowderEffectIntensity");
        private static readonly int SilverIntensityId = Shader.PropertyToID("_SilverIntensity");
        private static readonly int SilverSpreadId = Shader.PropertyToID("_SilverSpread");

        public CloudRaymarchPass(Settings settings)
        {
            this.settings = settings;
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public void Setup(RTHandle source)
        {
            this.source = source;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor fullDesc = renderingData.cameraData.cameraTargetDescriptor;
            fullDesc.depthBufferBits = 0;
            fullDesc.msaaSamples = 1;
            fullDesc.colorFormat = RenderTextureFormat.ARGBHalf;

            RenderingUtils.ReAllocateIfNeeded(
                ref tempColorTexture,
                fullDesc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_CloudTempColorTexture"
            );

            RenderTextureDescriptor lowDesc = fullDesc;
            float scale = Mathf.Clamp(settings.renderScale, 0.25f, 1.0f);
            lowDesc.width = Mathf.Max(1, Mathf.RoundToInt(fullDesc.width * scale));
            lowDesc.height = Mathf.Max(1, Mathf.RoundToInt(fullDesc.height * scale));

            RenderingUtils.ReAllocateIfNeeded(
                ref cloudLowResTexture,
                lowDesc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_CloudLowResTexture"
            );

            RenderingUtils.ReAllocateIfNeeded(
                ref cloudTemporalTexture,
                lowDesc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_CloudTemporalTexture"
            );

            RenderingUtils.ReAllocateIfNeeded(
                ref cloudHistoryTexture,
                lowDesc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_CloudHistoryTexture"
            );

            if (historyWidth != lowDesc.width || historyHeight != lowDesc.height)
            {
                historyValid = false;
                historyWidth = lowDesc.width;
                historyHeight = lowDesc.height;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.cloudMaterial == null || settings.shapeNoise == null || source == null)
                return;

            if (renderingData.cameraData.cameraType == CameraType.Preview)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("Cloud Raymarch");

            using (new ProfilingScope(cmd, passProfilingSampler))
            {
                ApplyMaterialProperties(renderingData.cameraData.camera);

                Blitter.BlitCameraTexture(cmd, source, cloudLowResTexture, settings.cloudMaterial, 0);
                settings.cloudMaterial.SetTexture(CloudCurrentTextureId, cloudLowResTexture);
                settings.cloudMaterial.SetTexture(CloudHistoryTextureId, cloudHistoryTexture);

                Blitter.BlitCameraTexture(cmd, cloudLowResTexture, cloudTemporalTexture, settings.cloudMaterial, 1);
                settings.cloudMaterial.SetTexture(CloudLowResTextureId, cloudTemporalTexture);

                Blitter.BlitCameraTexture(cmd, source, tempColorTexture);
                Blitter.BlitCameraTexture(cmd, tempColorTexture, source, settings.cloudMaterial, 2);

                Blitter.BlitCameraTexture(cmd, cloudTemporalTexture, cloudHistoryTexture);
                historyValid = true;
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void ApplyMaterialProperties(Camera camera)
        {
            Material material = settings.cloudMaterial;
            CloudVolume volume = CloudVolume.Active;

            float layerBottom = 80.0f;
            float layerThickness = 70.0f;
            float maxCloudDistance = 1200.0f;
            float distanceFade = 280.0f;

            float shapeWorldSize = 180.0f;
            float shapeHeightWorldSize = 75.0f;
            float detailWorldSize = 42.0f;
            float detailHeightWorldSize = 24.0f;
            float weatherWorldSize = 900.0f;

            float densityMultiplier = 6.2f;
            float densityThreshold = 0.24f;
            float coverage = 0.78f;
            float shapeFactor = 0.86f;
            Vector3 shapeOffset = Vector3.zero;

            int curveResolution = 128;
            AnimationCurve densityCurve = null;
            AnimationCurve erosionCurve = null;
            AnimationCurve ambientOcclusionCurve = null;

            float bottomFade = 0.08f;
            float topFade = 0.32f;
            float topDensity = 0.18f;
            float heightDensityPower = 0.9f;
            float altitudeDistortion = 0.08f;

            float erosionFactor = 0.38f;
            bool microErosion = true;
            float microErosionFactor = 0.14f;
            float erosionOcclusion = 0.18f;

            Texture2D weatherMap = null;
            Vector2 weatherOffset = Vector2.zero;
            float weatherInfluence = 0.55f;
            float weatherMinimum = 0.22f;
            float weatherContrast = 1.15f;
            float weatherWindSpeedMultiplier = 0.015f;

            float stepSize = 4.0f;
            int maxSteps = 220;
            Texture2D ditherTexture = settings.fallbackDitherTexture;
            float ditherStrength = 0.22f;
            bool temporalAccumulation = true;
            float temporalBlend = 0.86f;
            bool depthAwareUpsample = true;
            float depthUpsampleTolerance = 0.035f;
            float edgeSharpening = 0.18f;
            bool emptySpaceSkipping = true;
            float emptyStepMultiplier = 2.5f;
            int emptyStepTrigger = 3;
            Color horizonTint = new Color(0.58f, 0.68f, 0.82f, 1.0f);
            float aerialPerspective = 0.35f;
            float horizonFade = 0.8f;
            bool cloudShadows = true;
            float cloudShadowStrength = 0.28f;
            float cloudShadowSoftness = 1.2f;
            int debugView = 0;

            Vector3 windDirection = new Vector3(1.0f, 0.0f, 0.25f);
            float windSpeed = 0.08f;
            float shapeWindSpeedMultiplier = 0.25f;
            float detailWindSpeedMultiplier = 0.65f;

            Light sunLight = RenderSettings.sun;
            Color cloudColor = Color.white;
            Color shadowColor = new Color(0.42f, 0.50f, 0.63f, 1.0f);
            float lightAbsorption = 1.15f;
            int lightSteps = 8;
            float lightStepSize = 12.0f;
            float ambientLight = 0.34f;
            float sunLightDimmer = 1.65f;
            Color scatteringTint = Color.white;

            float phaseForward = 0.58f;
            float multiScattering = 0.58f;
            float powderEffectIntensity = 0.42f;
            float silverIntensity = 1.15f;
            float silverSpread = 9.0f;

            if (volume != null)
            {
                layerBottom = volume.layerBottom;
                layerThickness = volume.layerThickness;
                maxCloudDistance = volume.maxCloudDistance;
                distanceFade = volume.distanceFade;

                shapeWorldSize = volume.shapeWorldSize;
                shapeHeightWorldSize = volume.shapeHeightWorldSize;
                detailWorldSize = volume.detailWorldSize;
                detailHeightWorldSize = volume.detailHeightWorldSize;
                weatherWorldSize = volume.weatherWorldSize;

                densityMultiplier = volume.densityMultiplier;
                densityThreshold = volume.densityThreshold;
                coverage = volume.coverage;
                shapeFactor = volume.shapeFactor;
                shapeOffset = volume.shapeOffset;

                curveResolution = volume.curveTextureResolution;
                densityCurve = volume.densityCurve;
                erosionCurve = volume.erosionCurve;
                ambientOcclusionCurve = volume.ambientOcclusionCurve;

                bottomFade = volume.bottomFade;
                topFade = volume.topFade;
                topDensity = volume.topDensity;
                heightDensityPower = volume.heightDensityPower;
                altitudeDistortion = volume.altitudeDistortion;

                erosionFactor = volume.erosionFactor;
                microErosion = volume.microErosion;
                microErosionFactor = volume.microErosionFactor;
                erosionOcclusion = volume.erosionOcclusion;

                weatherMap = volume.weatherMap;
                weatherOffset = volume.weatherOffset;
                weatherInfluence = volume.weatherInfluence;
                weatherMinimum = volume.weatherMinimum;
                weatherContrast = volume.weatherContrast;
                weatherWindSpeedMultiplier = volume.weatherWindSpeedMultiplier;

                stepSize = volume.stepSize;
                maxSteps = volume.maxSteps;
                ditherTexture = volume.ditherTexture != null ? volume.ditherTexture : settings.fallbackDitherTexture;
                ditherStrength = volume.ditherStrength;
                temporalAccumulation = volume.temporalAccumulation;
                temporalBlend = volume.temporalBlend;
                depthAwareUpsample = volume.depthAwareUpsample;
                depthUpsampleTolerance = volume.depthUpsampleTolerance;
                edgeSharpening = volume.edgeSharpening;
                emptySpaceSkipping = volume.emptySpaceSkipping;
                emptyStepMultiplier = volume.emptyStepMultiplier;
                emptyStepTrigger = volume.emptyStepTrigger;
                horizonTint = volume.horizonTint;
                aerialPerspective = volume.aerialPerspective;
                horizonFade = volume.horizonFade;
                cloudShadows = volume.cloudShadows;
                cloudShadowStrength = volume.cloudShadowStrength;
                cloudShadowSoftness = volume.cloudShadowSoftness;
                debugView = (int)volume.debugView;

                windDirection = volume.windDirection;
                windSpeed = volume.windSpeed;
                shapeWindSpeedMultiplier = volume.shapeWindSpeedMultiplier;
                detailWindSpeedMultiplier = volume.detailWindSpeedMultiplier;

                sunLight = volume.sunLight != null ? volume.sunLight : RenderSettings.sun;
                cloudColor = volume.cloudColor;
                shadowColor = volume.shadowColor;
                lightAbsorption = volume.lightAbsorption;
                lightSteps = volume.lightSteps;
                lightStepSize = volume.lightStepSize;
                ambientLight = volume.ambientLight;
                sunLightDimmer = volume.sunLightDimmer;
                scatteringTint = volume.scatteringTint;

                phaseForward = volume.phaseForward;
                multiScattering = volume.multiScattering;
                powderEffectIntensity = volume.powderEffectIntensity;
                silverIntensity = volume.silverIntensity;
                silverSpread = volume.silverSpread;
            }

            cloudCurveTexture.Update(densityCurve, erosionCurve, ambientOcclusionCurve, curveResolution);

            Vector3 lightDirection = new Vector3(0.3f, 0.7f, 0.2f).normalized;
            Color sunColor = Color.white;

            if (sunLight != null)
            {
                lightDirection = -sunLight.transform.forward.normalized;
                sunColor = sunLight.color * sunLight.intensity;
            }

            Vector3 windDir = windDirection.sqrMagnitude > 0.0001f ? windDirection.normalized : Vector3.zero;
            float time = Application.isPlaying ? Time.time : 0.0f;

            Vector3 shapeWindOffset = windDir * windSpeed * shapeWindSpeedMultiplier * time;
            Vector3 detailWindOffset = windDir * windSpeed * detailWindSpeedMultiplier * time;
            Vector2 weatherWindOffset = new Vector2(windDir.x, windDir.z) * windSpeed * weatherWindSpeedMultiplier * time;

            float actualTemporalBlend = temporalAccumulation && historyValid ? temporalBlend : 0.0f;

            if (camera != null && camera.cameraType == CameraType.SceneView)
                actualTemporalBlend = 0.0f;

            if (camera != null && historyValid)
            {
                int cameraInstanceId = camera.GetInstanceID();
                bool switchedCamera = cameraInstanceId != previousCameraInstanceId || camera.cameraType != previousCameraType;
                float moveDistance = Vector3.Distance(camera.transform.position, previousCameraPosition);
                float rotateAngle = Quaternion.Angle(camera.transform.rotation, previousCameraRotation);

                if (switchedCamera || moveDistance > 12.0f || rotateAngle > 18.0f)
                    actualTemporalBlend = 0.0f;
            }

            Matrix4x4 currentViewProjectionMatrix = Matrix4x4.identity;
            Matrix4x4 currentInverseViewProjectionMatrix = Matrix4x4.identity;

            if (camera != null)
            {
                Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
                Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                currentViewProjectionMatrix = projectionMatrix * viewMatrix;
                currentInverseViewProjectionMatrix = currentViewProjectionMatrix.inverse;

                previousCameraPosition = camera.transform.position;
                previousCameraRotation = camera.transform.rotation;
                previousCameraInstanceId = camera.GetInstanceID();
                previousCameraType = camera.cameraType;
            }

            material.SetTexture(ShapeNoiseTexId, settings.shapeNoise);
            material.SetTexture(DetailNoiseTexId, settings.detailNoise);
            material.SetFloat(HasDetailNoiseId, settings.detailNoise != null ? 1.0f : 0.0f);
            material.SetTexture(DensityCurveTexId, cloudCurveTexture.Texture);
            material.SetTexture(CloudLowResTextureId, cloudTemporalTexture);
            material.SetTexture(CloudCurrentTextureId, cloudLowResTexture);
            material.SetTexture(CloudHistoryTextureId, cloudHistoryTexture);

            material.SetFloat(LayerBottomId, layerBottom);
            material.SetFloat(LayerThicknessId, layerThickness);
            material.SetFloat(MaxCloudDistanceId, maxCloudDistance);
            material.SetFloat(DistanceFadeId, distanceFade);

            material.SetFloat(ShapeWorldSizeId, shapeWorldSize);
            material.SetFloat(ShapeHeightWorldSizeId, shapeHeightWorldSize);
            material.SetFloat(DetailWorldSizeId, detailWorldSize);
            material.SetFloat(DetailHeightWorldSizeId, detailHeightWorldSize);
            material.SetFloat(WeatherWorldSizeId, weatherWorldSize);

            material.SetColor(CloudColorId, cloudColor);
            material.SetColor(ShadowColorId, shadowColor);
            material.SetVector(LightDirectionId, new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
            material.SetColor(SunColorId, sunColor);
            material.SetColor(ScatteringTintId, scatteringTint);

            material.SetFloat(DensityMultiplierId, densityMultiplier);
            material.SetFloat(DensityThresholdId, densityThreshold);
            material.SetFloat(CoverageId, coverage);
            material.SetFloat(ShapeFactorId, shapeFactor);
            material.SetVector(ShapeOffsetId, new Vector4(shapeOffset.x, shapeOffset.y, shapeOffset.z, 0.0f));

            material.SetFloat(BottomFadeId, bottomFade);
            material.SetFloat(TopFadeId, topFade);
            material.SetFloat(TopDensityId, topDensity);
            material.SetFloat(HeightDensityPowerId, heightDensityPower);
            material.SetFloat(AltitudeDistortionId, altitudeDistortion);

            material.SetFloat(ErosionFactorId, erosionFactor);
            material.SetFloat(MicroErosionId, microErosion ? 1.0f : 0.0f);
            material.SetFloat(MicroErosionFactorId, microErosionFactor);
            material.SetFloat(ErosionOcclusionId, erosionOcclusion);

            material.SetTexture(WeatherMapId, weatherMap);
            material.SetFloat(HasWeatherMapId, weatherMap != null ? 1.0f : 0.0f);
            Vector2 finalWeatherOffset = weatherOffset + weatherWindOffset;
            material.SetVector(WeatherOffsetId, new Vector4(finalWeatherOffset.x, finalWeatherOffset.y, 0.0f, 0.0f));
            material.SetFloat(WeatherInfluenceId, weatherInfluence);
            material.SetFloat(WeatherMinimumId, weatherMinimum);
            material.SetFloat(WeatherContrastId, weatherContrast);

            material.SetFloat(StepSizeId, stepSize);
            material.SetInt(MaxStepsId, maxSteps);

            material.SetTexture(DitherTextureId, ditherTexture);
            material.SetFloat(HasDitherTextureId, ditherTexture != null ? 1.0f : 0.0f);
            material.SetFloat(DitherStrengthId, ditherStrength);
            material.SetFloat(TemporalBlendId, actualTemporalBlend);
            material.SetFloat(DepthAwareUpsampleId, depthAwareUpsample ? 1.0f : 0.0f);
            material.SetFloat(DepthUpsampleToleranceId, depthUpsampleTolerance);
            material.SetFloat(EdgeSharpeningId, edgeSharpening);
            material.SetMatrix(PreviousViewProjectionMatrixId, previousViewProjectionMatrix);
            material.SetMatrix(CurrentInverseViewProjectionMatrixId, currentInverseViewProjectionMatrix);
            material.SetFloat(EmptySpaceSkippingId, emptySpaceSkipping ? 1.0f : 0.0f);
            material.SetFloat(EmptyStepMultiplierId, emptyStepMultiplier);
            material.SetInt(EmptyStepTriggerId, emptyStepTrigger);
            material.SetColor(HorizonTintId, horizonTint);
            material.SetFloat(AerialPerspectiveId, aerialPerspective);
            material.SetFloat(HorizonFadeId, horizonFade);
            material.SetFloat(CloudShadowsId, cloudShadows ? 1.0f : 0.0f);
            material.SetFloat(CloudShadowStrengthId, cloudShadowStrength);
            material.SetFloat(CloudShadowSoftnessId, cloudShadowSoftness);
            material.SetInt(DebugViewId, debugView);

            Vector4 texelSize = cloudLowResTexture != null
                ? new Vector4(1.0f / Mathf.Max(1, historyWidth), 1.0f / Mathf.Max(1, historyHeight), historyWidth, historyHeight)
                : new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            material.SetVector(LowResTexelSizeId, texelSize);

            material.SetVector(WindOffsetId, new Vector4(shapeWindOffset.x, shapeWindOffset.y, shapeWindOffset.z, 0.0f));
            material.SetVector(DetailWindOffsetId, new Vector4(detailWindOffset.x, detailWindOffset.y, detailWindOffset.z, 0.0f));

            material.SetFloat(LightAbsorptionId, lightAbsorption);
            material.SetInt(LightStepsId, lightSteps);
            material.SetFloat(LightStepSizeId, lightStepSize);
            material.SetFloat(AmbientLightId, ambientLight);
            material.SetFloat(SunLightDimmerId, sunLightDimmer);

            material.SetFloat(PhaseForwardId, phaseForward);
            material.SetFloat(MultiScatteringId, multiScattering);
            material.SetFloat(PowderEffectIntensityId, powderEffectIntensity);
            material.SetFloat(SilverIntensityId, silverIntensity);
            material.SetFloat(SilverSpreadId, silverSpread);

            previousViewProjectionMatrix = currentViewProjectionMatrix;
        }

        public void Dispose()
        {
            cloudLowResTexture?.Release();
            cloudTemporalTexture?.Release();
            cloudHistoryTexture?.Release();
            tempColorTexture?.Release();
            cloudCurveTexture.Release();
        }
    }
}
