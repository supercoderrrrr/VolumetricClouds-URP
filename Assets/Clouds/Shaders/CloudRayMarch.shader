Shader "Volumetric Clouds/Cloud Raymarch"
{
    Properties
    {
        _ShapeNoiseTex ("Shape Noise", 3D) = "" {}
        _DetailNoiseTex ("Detail Noise", 3D) = "" {}
        _DensityCurveTex ("Density Curve", 2D) = "white" {}
        _WeatherMap ("Weather Map", 2D) = "white" {}
        _DitherTexture ("Dither Texture", 2D) = "gray" {}
        _CloudLowResTexture ("Cloud Low Res", 2D) = "black" {}
        _CloudCurrentTexture ("Cloud Current", 2D) = "black" {}
        _CloudHistoryTexture ("Cloud History", 2D) = "black" {}

        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.42, 0.50, 0.63, 1)
        _ScatteringTint ("Scattering Tint", Color) = (1, 1, 1, 1)

        _LayerBottom ("Layer Bottom", Float) = 80
        _LayerThickness ("Layer Thickness", Float) = 70
        _MaxCloudDistance ("Max Cloud Distance", Float) = 1200
        _DistanceFade ("Distance Fade", Float) = 280

        _ShapeWorldSize ("Shape World Size", Float) = 180
        _ShapeHeightWorldSize ("Shape Height World Size", Float) = 75
        _DetailWorldSize ("Detail World Size", Float) = 42
        _DetailHeightWorldSize ("Detail Height World Size", Float) = 24
        _WeatherWorldSize ("Weather World Size", Float) = 900

        _DensityMultiplier ("Density Multiplier", Float) = 6.2
        _DensityThreshold ("Density Threshold", Float) = 0.24
        _Coverage ("Coverage", Float) = 0.78
        _ShapeFactor ("Shape Factor", Float) = 0.86
        _StepSize ("Step Size", Float) = 4
        _MaxSteps ("Max Steps", Int) = 220

        _HorizonTint ("Horizon Tint", Color) = (0.58, 0.68, 0.82, 1)
        _AerialPerspective ("Aerial Perspective", Float) = 0.35
        _HorizonFade ("Horizon Fade", Float) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE

        #pragma target 4.5

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #define CLOUD_PI 3.14159265
        #define CLOUD_EXTINCTION_SCALE 0.014
        #define CLOUD_LIGHT_EXTINCTION_SCALE 0.018

        TEXTURE3D(_ShapeNoiseTex);
        SAMPLER(sampler_ShapeNoiseTex);

        TEXTURE3D(_DetailNoiseTex);
        SAMPLER(sampler_DetailNoiseTex);

        TEXTURE2D(_DensityCurveTex);
        SAMPLER(sampler_DensityCurveTex);

        TEXTURE2D(_WeatherMap);
        SAMPLER(sampler_WeatherMap);

        TEXTURE2D(_DitherTexture);
        SAMPLER(sampler_DitherTexture);

        TEXTURE2D_X(_CloudLowResTexture);
        TEXTURE2D_X(_CloudCurrentTexture);
        TEXTURE2D_X(_CloudHistoryTexture);

        CBUFFER_START(UnityPerMaterial)

        float _LayerBottom;
        float _LayerThickness;
        float _MaxCloudDistance;
        float _DistanceFade;

        float _ShapeWorldSize;
        float _ShapeHeightWorldSize;
        float _DetailWorldSize;
        float _DetailHeightWorldSize;
        float _WeatherWorldSize;

        float4 _CloudColor;
        float4 _ShadowColor;
        float4 _LightDirection;
        float4 _SunColor;
        float4 _ScatteringTint;

        float _DensityMultiplier;
        float _DensityThreshold;
        float _Coverage;
        float _ShapeFactor;
        float4 _ShapeOffset;

        float _BottomFade;
        float _TopFade;
        float _TopDensity;
        float _HeightDensityPower;
        float _AltitudeDistortion;

        float _ErosionFactor;
        float _MicroErosion;
        float _MicroErosionFactor;
        float _ErosionOcclusion;

        float _HasWeatherMap;
        float4 _WeatherOffset;
        float _WeatherInfluence;
        float _WeatherMinimum;
        float _WeatherContrast;

        float _StepSize;
        int _MaxSteps;
        float _HasDitherTexture;
        float _DitherStrength;
        float _TemporalBlend;
        float _DepthAwareUpsample;
        float _DepthUpsampleTolerance;
        float _EdgeSharpening;
        float4 _LowResTexelSize;
        float4x4 _PreviousViewProjectionMatrix;
        float4x4 _CurrentInverseViewProjectionMatrix;
        float _EmptySpaceSkipping;
        float _EmptyStepMultiplier;
        int _EmptyStepTrigger;
        float4 _HorizonTint;
        float _AerialPerspective;
        float _HorizonFade;
        float _CloudShadows;
        float _CloudShadowStrength;
        float _CloudShadowSoftness;
        int _DebugView;

        float4 _WindOffset;
        float4 _DetailWindOffset;

        float _HasDetailNoise;
        float _LightAbsorption;
        int _LightSteps;
        float _LightStepSize;
        float _AmbientLight;
        float _SunLightDimmer;

        float _PhaseForward;
        float _MultiScattering;
        float _PowderEffectIntensity;
        float _SilverIntensity;
        float _SilverSpread;

        CBUFFER_END

        struct CloudDensityData
        {
            float density;
            float height01;
            float ambientOcclusion;
            float edge;
        };

        float CloudRemap(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float t = saturate((value - oldMin) / max(0.0001, oldMax - oldMin));
            return lerp(newMin, newMax, t);
        }

        float2 CloudRayLayerDst(float3 rayOrigin, float3 rayDir)
        {
            float layerTop = _LayerBottom + max(0.001, _LayerThickness);
            float maxDistance = max(0.001, _MaxCloudDistance);

            if (abs(rayDir.y) < 0.0001)
            {
                if (rayOrigin.y >= _LayerBottom && rayOrigin.y <= layerTop)
                    return float2(0.0, maxDistance);

                return float2(0.0, 0.0);
            }

            float t0 = (_LayerBottom - rayOrigin.y) / rayDir.y;
            float t1 = (layerTop - rayOrigin.y) / rayDir.y;

            float entry = min(t0, t1);
            float exit = max(t0, t1);

            if (exit <= 0.0)
                return float2(0.0, 0.0);

            float dstToLayer = max(0.0, entry);

            if (dstToLayer >= maxDistance)
                return float2(0.0, 0.0);

            float dstOutLayer = min(exit, maxDistance);
            float dstInsideLayer = max(0.0, dstOutLayer - dstToLayer);

            return float2(dstToLayer, dstInsideLayer);
        }

        float3 CloudWorldPositionFromDepth(float2 uv, float rawDepth)
        {
            #if UNITY_REVERSED_Z
                float depth = rawDepth;
            #else
                float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, rawDepth);
            #endif

            return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
        }

        float3 CloudWorldPositionFromDepthMatrix(float2 uv, float rawDepth)
        {
            float4 clipPos = float4(uv * 2.0 - 1.0, rawDepth, 1.0);
            #if UNITY_UV_STARTS_AT_TOP
                clipPos.y = -clipPos.y;
            #endif

            float4 worldPos = mul(_CurrentInverseViewProjectionMatrix, clipPos);
            return worldPos.xyz / max(0.00001, worldPos.w);
        }

        bool CloudHasSceneDepth(float rawDepth)
        {
            #if UNITY_REVERSED_Z
                return rawDepth > 0.0001;
            #else
                return rawDepth < 0.9999;
            #endif
        }

        float CloudHash12(float2 p)
        {
            float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.x + p3.y) * p3.z);
        }

        float CloudDither(float2 uv)
        {
            if (_DitherStrength <= 0.0001)
                return 0.5;

            if (_HasDitherTexture > 0.5)
            {
                float2 ditherUV = uv * _ScreenParams.xy / 64.0;
                return SAMPLE_TEXTURE2D(_DitherTexture, sampler_DitherTexture, ditherUV).r;
            }

            return CloudHash12(uv * _ScreenParams.xy);
        }

        float3 CloudSampleCurves(float height01)
        {
            return SAMPLE_TEXTURE2D(_DensityCurveTex, sampler_DensityCurveTex, float2(saturate(height01), 0.5)).rgb;
        }

        float CloudVerticalFade(float height01)
        {
            float bottomFade = max(0.001, _BottomFade);
            float topFade = max(0.001, _TopFade);

            float bottom = smoothstep(0.0, bottomFade, height01);
            float top = 1.0 - smoothstep(1.0 - topFade, 1.0, height01);

            float upperHeight = saturate((height01 - 0.45) / 0.55);
            float topDensity = lerp(1.0, _TopDensity, upperHeight);

            float profile = saturate(bottom * top * topDensity);
            return pow(profile, max(0.001, _HeightDensityPower));
        }

        float CloudDistanceFade(float rayDst)
        {
            float fadeStart = max(0.0, _MaxCloudDistance - max(1.0, _DistanceFade));
            return 1.0 - smoothstep(fadeStart, _MaxCloudDistance, rayDst);
        }

        float2 CloudRotate2D(float2 p, float angle)
        {
            float s = sin(angle);
            float c = cos(angle);
            return float2(c * p.x - s * p.y, s * p.x + c * p.y);
        }

        float CloudWeatherCoverage(float3 worldPos)
        {
            if (_HasWeatherMap < 0.5)
                return 1.0;

            float2 weatherPos = worldPos.xz + _WeatherOffset.xy;
            float2 weatherUVA = weatherPos / max(1.0, _WeatherWorldSize);
            float2 weatherUVB = CloudRotate2D(weatherPos + float2(381.7, -192.4), 0.67) / max(1.0, _WeatherWorldSize * 1.83);
            weatherUVB += float2(0.37, 0.61);

            float weatherA = SAMPLE_TEXTURE2D(_WeatherMap, sampler_WeatherMap, weatherUVA).r;
            float weatherB = SAMPLE_TEXTURE2D(_WeatherMap, sampler_WeatherMap, weatherUVB).r;
            float weather = lerp(weatherA, weatherB, 0.28);

            weather = saturate(weather * (1.0 - _WeatherMinimum) + _WeatherMinimum);
            weather = pow(weather, max(0.01, _WeatherContrast));

            return weather;
        }

        float3 CloudShapeUVW(float3 worldPos, float height01)
        {
            float3 shapeWorldPos = worldPos + _ShapeOffset.xyz + _WindOffset.xyz;
            float3 shapeUVW = float3(
                shapeWorldPos.x / max(1.0, _ShapeWorldSize),
                shapeWorldPos.y / max(1.0, _ShapeHeightWorldSize),
                shapeWorldPos.z / max(1.0, _ShapeWorldSize)
            );

            float2 altitudeDir = normalize(float2(0.73, 0.41) + normalize(_WindOffset.xz + 0.001));
            shapeUVW.xz += altitudeDir * (height01 - 0.5) * _AltitudeDistortion;

            return shapeUVW;
        }

        float3 CloudShapeUVWSecondary(float3 worldPos, float height01)
        {
            float3 secondaryWorldPos = worldPos * 1.37 + float3(173.1, 61.7, -241.3);
            float3 secondaryUVW = CloudShapeUVW(secondaryWorldPos, height01);
            secondaryUVW.xz = CloudRotate2D(secondaryUVW.xz, 0.73);
            secondaryUVW.y += 0.19;
            return secondaryUVW;
        }

        float3 CloudDetailUVW(float3 worldPos)
        {
            float3 detailWorldPos = worldPos + _DetailWindOffset.xyz + _ShapeOffset.xyz * 0.37;
            return float3(
                detailWorldPos.x / max(1.0, _DetailWorldSize),
                detailWorldPos.y / max(1.0, _DetailHeightWorldSize),
                detailWorldPos.z / max(1.0, _DetailWorldSize)
            );
        }

        CloudDensityData CloudSampleDensity(float3 worldPos)
        {
            CloudDensityData data;
            data.density = 0.0;
            data.height01 = 0.0;
            data.ambientOcclusion = 1.0;
            data.edge = 0.0;

            float height01 = (worldPos.y - _LayerBottom) / max(0.001, _LayerThickness);

            if (height01 <= 0.0 || height01 >= 1.0)
                return data;

            height01 = saturate(height01);
            float3 curves = CloudSampleCurves(height01);

            float densityCurve = curves.r;
            float erosionCurve = curves.g;
            float ambientCurve = curves.b;

            float heightFade = CloudVerticalFade(height01);
            float heightMask = saturate(densityCurve * heightFade);

            if (heightMask <= 0.0001)
                return data;

            float weatherCoverage = CloudWeatherCoverage(worldPos);
            float localCoverage = saturate(lerp(_Coverage, _Coverage * weatherCoverage, _WeatherInfluence));

            float4 shapeNoiseA = SAMPLE_TEXTURE3D(_ShapeNoiseTex, sampler_ShapeNoiseTex, CloudShapeUVW(worldPos, height01));
            float4 shapeNoiseB = SAMPLE_TEXTURE3D(_ShapeNoiseTex, sampler_ShapeNoiseTex, CloudShapeUVWSecondary(worldPos, height01));
            float4 shapeNoise = lerp(shapeNoiseA, shapeNoiseB, 0.32);

            float shapeWorley = saturate(
                shapeNoise.g * 0.625 +
                shapeNoise.b * 0.25 +
                shapeNoise.a * 0.125
            );

            float baseNoise = saturate(lerp(shapeNoise.r, shapeNoise.r * 0.78 + shapeWorley * 0.22, _ShapeFactor));
            float coverageThreshold = saturate(_DensityThreshold + (1.0 - localCoverage) * 0.45);

            float density = CloudRemap(baseNoise, coverageThreshold, 1.0, 0.0, 1.0);
            density *= heightMask * localCoverage;

            float edgeMask = 1.0 - saturate(density * 1.8);

            if (_HasDetailNoise > 0.5)
            {
                float4 detailNoise = SAMPLE_TEXTURE3D(_DetailNoiseTex, sampler_DetailNoiseTex, CloudDetailUVW(worldPos));

                float erosionAmount = (1.0 - detailNoise.r) * _ErosionFactor * erosionCurve;
                erosionAmount *= lerp(0.45, 1.0, edgeMask);
                density = CloudRemap(density, erosionAmount * 0.75, 1.0, 0.0, 1.0);

                if (_MicroErosion > 0.5)
                {
                    float3 microUVW = CloudDetailUVW(worldPos * 1.73 + _DetailWindOffset.xyz * 18.0);
                    float microNoise = SAMPLE_TEXTURE3D(_DetailNoiseTex, sampler_DetailNoiseTex, microUVW).a;
                    float microAmount = (1.0 - microNoise) * _MicroErosionFactor * edgeMask;
                    density = CloudRemap(density, microAmount * 0.65, 1.0, 0.0, 1.0);
                }
            }

            data.density = saturate(density) * _DensityMultiplier;
            data.height01 = height01;
            data.ambientOcclusion = lerp(1.0, ambientCurve, saturate(_ErosionOcclusion));
            data.edge = edgeMask;

            return data;
        }

        float CloudHenyeyGreenstein(float cosTheta, float g)
        {
            float g2 = g * g;
            float denom = max(0.0001, 1.0 + g2 - 2.0 * g * cosTheta);
            return (1.0 - g2) / (4.0 * CLOUD_PI * pow(denom, 1.5));
        }

        float CloudLightmarch(float3 position, float3 lightDir)
        {
            int lightSteps = clamp(_LightSteps, 1, 32);

            float2 hit = CloudRayLayerDst(position + lightDir * 0.1, lightDir);
            float dstLimit = min(hit.y, _LightStepSize * lightSteps);

            if (dstLimit <= 0.0001)
                return 1.0;

            float stepSize = dstLimit / lightSteps;
            float rayDst = stepSize * 0.5;
            float densityAlongLight = 0.0;

            [loop]
            for (int i = 0; i < 32; i++)
            {
                if (i >= lightSteps)
                    break;

                float3 samplePos = position + lightDir * rayDst;
                densityAlongLight += CloudSampleDensity(samplePos).density * stepSize;
                rayDst += stepSize;
            }

            return exp(-densityAlongLight * _LightAbsorption * CLOUD_LIGHT_EXTINCTION_SCALE);
        }

        float4 CloudRaymarch(float3 rayOrigin, float3 rayDir, float dstToLayer, float dstInsideLayer, float sceneDst, float2 uv)
        {
            float dstLimit = min(dstInsideLayer, sceneDst - dstToLayer);

            if (dstLimit <= 0.0001)
                return float4(0.0, 0.0, 0.0, 0.0);

            float targetStepSize = max(0.01, _StepSize);
            int maxSteps = clamp(_MaxSteps, 1, 384);
            int stepCount = clamp((int)ceil(dstLimit / targetStepSize), 1, maxSteps);
            float actualStepSize = dstLimit / stepCount;

            float dither = CloudDither(uv);
            float jitter = 0.5 + (dither - 0.5) * _DitherStrength;
            float rayDst = dstToLayer + actualStepSize * jitter;
            float rayEnd = dstToLayer + dstLimit;

            float3 lightDir = normalize(_LightDirection.xyz);
            float cosTheta = dot(rayDir, lightDir);

            float forwardPhase = CloudHenyeyGreenstein(cosTheta, _PhaseForward) * 3.0;
            float backPhase = CloudHenyeyGreenstein(cosTheta, -0.35) * 0.75;
            float phase = max(0.18, forwardPhase + backPhase);

            float transmittance = 1.0;
            float3 cloudRGB = 0.0;
            float debugLight = 0.0;
            float debugHeight = 0.0;
            int usedSteps = 0;
            int emptySamples = 0;

            [loop]
            for (int i = 0; i < 384; i++)
            {
                if (i >= stepCount || rayDst >= rayEnd)
                    break;

                float3 samplePos = rayOrigin + rayDir * rayDst;
                CloudDensityData densityData = CloudSampleDensity(samplePos);
                densityData.density *= CloudDistanceFade(rayDst);
                usedSteps++;

                if (densityData.density > 0.001)
                {
                    emptySamples = 0;
                    float lightTransmittance = CloudLightmarch(samplePos, lightDir);
                    float multiScatterLight = lerp(lightTransmittance, sqrt(saturate(lightTransmittance)), _MultiScattering);
                    debugHeight = densityData.height01;

                    float powder = saturate(1.0 - exp(-densityData.density * 0.32));
                    powder = lerp(1.0, powder, _PowderEffectIntensity);

                    float silver = pow(saturate(cosTheta * 0.5 + 0.5), _SilverSpread);
                    silver *= _SilverIntensity * lightTransmittance;

                    float ambientHeight = lerp(0.62, 1.0, densityData.height01);
                    float3 ambient = _ShadowColor.rgb * _AmbientLight * ambientHeight * densityData.ambientOcclusion;

                    float3 directLight = _SunColor.rgb * _SunLightDimmer * multiScatterLight * phase * powder;
                    directLight += _SunColor.rgb * silver;

                    float3 sampleLighting = ambient + directLight;
                    float3 sampleColor = _CloudColor.rgb * _ScatteringTint.rgb * sampleLighting;

                    float opticalDepth = densityData.density * actualStepSize * CLOUD_EXTINCTION_SCALE;
                    float sampleAlpha = 1.0 - exp(-opticalDepth);

                    debugLight += transmittance * sampleAlpha * lightTransmittance;
                    cloudRGB += transmittance * sampleAlpha * sampleColor;
                    transmittance *= exp(-opticalDepth);

                    if (transmittance < 0.01)
                        break;
                }
                else
                {
                    emptySamples++;
                }

                float stepMultiplier = 1.0;
                if (_EmptySpaceSkipping > 0.5 && emptySamples >= _EmptyStepTrigger)
                    stepMultiplier = _EmptyStepMultiplier;

                rayDst += actualStepSize * stepMultiplier;
            }

            float cloudAlpha = saturate(1.0 - transmittance);

            if (_DebugView == 3)
                return float4((debugLight / max(0.0001, cloudAlpha)).xxx, cloudAlpha);

            if (_DebugView == 5)
                return float4(debugHeight.xxx, cloudAlpha);

            if (_DebugView == 6)
            {
                float fraction = saturate((float)usedSteps / maxSteps);
                float3 low = float3(0.05, 0.12, 0.45);
                float3 medium = float3(0.05, 0.85, 0.9);
                float3 high = float3(1.0, 0.85, 0.05);
                float3 limit = float3(0.95, 0.05, 0.02);
                float3 heat = fraction < 0.333333
                    ? lerp(low, medium, fraction * 3.0)
                    : fraction < 0.666667
                        ? lerp(medium, high, fraction * 3.0 - 1.0)
                        : lerp(high, limit, fraction * 3.0 - 2.0);
                return float4(heat, 1.0);
            }

            return float4(cloudRGB, cloudAlpha);
        }

        half4 FragRaymarch(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;

            float rawDepth = SampleSceneDepth(uv);
            float3 sceneWorldPos = CloudWorldPositionFromDepth(uv, rawDepth);

            float3 rayOrigin = _WorldSpaceCameraPos.xyz;
            float3 rayDir = normalize(sceneWorldPos - rayOrigin);
            float sceneDst = length(sceneWorldPos - rayOrigin);

            float2 layerHit = CloudRayLayerDst(rayOrigin, rayDir);

            if (layerHit.y <= 0.0001)
                return half4(0.0, 0.0, 0.0, 0.0);

            float4 cloud = CloudRaymarch(rayOrigin, rayDir, layerHit.x, layerHit.y, sceneDst, uv);
            return half4(cloud.rgb, cloud.a);
        }

        half4 FragComposite(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;

            half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
            float rawDepth = SampleSceneDepth(uv);
            float3 sceneWorldPos = CloudWorldPositionFromDepth(uv, rawDepth);
            float3 viewDir = normalize(sceneWorldPos - _WorldSpaceCameraPos.xyz);
            float sceneDst = length(sceneWorldPos - _WorldSpaceCameraPos.xyz);

            half4 cloud = 0.0;

            if (_DepthAwareUpsample > 0.5)
            {
                float centerDepth = SampleSceneDepth(uv);
                float tolerance = max(0.00001, _DepthUpsampleTolerance);

                float2 offsets[5] =
                {
                    float2(0.0, 0.0),
                    float2(1.0, 0.0),
                    float2(-1.0, 0.0),
                    float2(0.0, 1.0),
                    float2(0.0, -1.0)
                };

                float totalWeight = 0.0;

                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    float2 tapUV = uv + offsets[i] * _LowResTexelSize.xy;
                    float tapDepth = SampleSceneDepth(tapUV);
                    float depthWeight = exp(-abs(tapDepth - centerDepth) / tolerance);
                    float spatialWeight = i == 0 ? 1.0 : 0.55;
                    float weight = depthWeight * spatialWeight;

                    cloud += SAMPLE_TEXTURE2D_X(_CloudLowResTexture, sampler_LinearClamp, tapUV) * weight;
                    totalWeight += weight;
                }

                cloud /= max(0.0001, totalWeight);
            }
            else
            {
                cloud = SAMPLE_TEXTURE2D_X(_CloudLowResTexture, sampler_LinearClamp, uv);
            }

            if (_DebugView == 2)
                return half4(saturate(cloud.a).xxx, 1.0);

            if (_DebugView == 4)
            {
                float weatherCoverage = CloudWeatherCoverage(sceneWorldPos);
                return half4(weatherCoverage.xxx, 1.0);
            }

            if (_DebugView >= 3 && _DebugView <= 6)
                return half4(cloud.rgb, 1.0);

            cloud.a = saturate(cloud.a);
            cloud.a = lerp(cloud.a, smoothstep(0.0, 1.0, cloud.a), _EdgeSharpening);

            if (_CloudShadows > 0.5 && CloudHasSceneDepth(rawDepth))
            {
                float shadowTransmittance = CloudLightmarch(sceneWorldPos + normalize(_LightDirection.xyz) * 0.25, normalize(_LightDirection.xyz));
                shadowTransmittance = pow(saturate(shadowTransmittance), max(0.01, _CloudShadowSoftness));
                sceneColor.rgb *= lerp(1.0, shadowTransmittance, _CloudShadowStrength);
            }

            float horizonAmount = pow(1.0 - saturate(abs(viewDir.y)), max(0.01, _HorizonFade));
            float distanceAmount = saturate(sceneDst / max(1.0, _MaxCloudDistance));
            float aerialAmount = saturate(horizonAmount * distanceAmount * _AerialPerspective);
            cloud.rgb = lerp(cloud.rgb, _HorizonTint.rgb * max(0.15, cloud.a), aerialAmount);

            if (_DebugView == 1)
                return half4(cloud.rgb, 1.0);

            half3 result = sceneColor.rgb * (1.0 - cloud.a) + cloud.rgb;
            return half4(result, sceneColor.a);
        }

        half4 FragTemporal(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;

            half4 current = SAMPLE_TEXTURE2D_X(_CloudCurrentTexture, sampler_LinearClamp, uv);
            if (_DebugView >= 2)
                return current;

            float rawDepth = SampleSceneDepth(uv);
            float3 worldPos = CloudWorldPositionFromDepthMatrix(uv, rawDepth);
            float4 previousClip = mul(_PreviousViewProjectionMatrix, float4(worldPos, 1.0));
            float2 previousUV = previousClip.xy / max(0.00001, previousClip.w);

            #if UNITY_UV_STARTS_AT_TOP
                previousUV.y = -previousUV.y;
            #endif

            previousUV = previousUV * 0.5 + 0.5;

            float validHistory = step(0.0, previousUV.x) * step(previousUV.x, 1.0) * step(0.0, previousUV.y) * step(previousUV.y, 1.0);
            half4 history = SAMPLE_TEXTURE2D_X(_CloudHistoryTexture, sampler_LinearClamp, previousUV);

            half4 minCloud = current;
            half4 maxCloud = current;

            float2 offsets[5] =
            {
                float2(0.0, 0.0),
                float2(1.0, 0.0),
                float2(-1.0, 0.0),
                float2(0.0, 1.0),
                float2(0.0, -1.0)
            };

            [unroll]
            for (int i = 1; i < 5; i++)
            {
                half4 tap = SAMPLE_TEXTURE2D_X(_CloudCurrentTexture, sampler_LinearClamp, uv + offsets[i] * _LowResTexelSize.xy);
                minCloud = min(minCloud, tap);
                maxCloud = max(maxCloud, tap);
            }

            history = clamp(history, minCloud, maxCloud);

            half blend = saturate(_TemporalBlend * validHistory);
            half4 result = lerp(current, history, blend);
            result.a = saturate(result.a);
            return result;
        }

        ENDHLSL

        Pass
        {
            Name "Cloud Raymarch"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragRaymarch
            ENDHLSL
        }

        Pass
        {
            Name "Cloud Temporal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragTemporal
            ENDHLSL
        }

        Pass
        {
            Name "Cloud Composite"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite
            ENDHLSL
        }
    }
}
