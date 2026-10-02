using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CloudVolume))]
[CanEditMultipleObjects]
public class CloudVolumeEditor : Editor
{
    private SerializedProperty cloudPreset;
    private SerializedProperty showLayerGizmo;
    private SerializedProperty layerBottom;
    private SerializedProperty layerThickness;
    private SerializedProperty maxCloudDistance;
    private SerializedProperty distanceFade;
    private SerializedProperty shapeWorldSize;
    private SerializedProperty shapeHeightWorldSize;
    private SerializedProperty detailWorldSize;
    private SerializedProperty detailHeightWorldSize;
    private SerializedProperty weatherWorldSize;
    private SerializedProperty densityMultiplier;
    private SerializedProperty densityThreshold;
    private SerializedProperty coverage;
    private SerializedProperty shapeFactor;
    private SerializedProperty shapeOffset;
    private SerializedProperty curveTextureResolution;
    private SerializedProperty densityCurve;
    private SerializedProperty erosionCurve;
    private SerializedProperty ambientOcclusionCurve;
    private SerializedProperty bottomFade;
    private SerializedProperty topFade;
    private SerializedProperty topDensity;
    private SerializedProperty heightDensityPower;
    private SerializedProperty altitudeDistortion;
    private SerializedProperty erosionFactor;
    private SerializedProperty microErosion;
    private SerializedProperty microErosionFactor;
    private SerializedProperty erosionOcclusion;
    private SerializedProperty weatherMap;
    private SerializedProperty weatherOffset;
    private SerializedProperty weatherInfluence;
    private SerializedProperty weatherMinimum;
    private SerializedProperty weatherContrast;
    private SerializedProperty weatherWindSpeedMultiplier;
    private SerializedProperty stepSize;
    private SerializedProperty maxSteps;
    private SerializedProperty ditherTexture;
    private SerializedProperty ditherStrength;
    private SerializedProperty emptySpaceSkipping;
    private SerializedProperty emptyStepMultiplier;
    private SerializedProperty emptyStepTrigger;
    private SerializedProperty temporalAccumulation;
    private SerializedProperty temporalBlend;
    private SerializedProperty depthAwareUpsample;
    private SerializedProperty depthUpsampleTolerance;
    private SerializedProperty edgeSharpening;
    private SerializedProperty horizonTint;
    private SerializedProperty aerialPerspective;
    private SerializedProperty horizonFade;
    private SerializedProperty cloudShadows;
    private SerializedProperty cloudShadowStrength;
    private SerializedProperty cloudShadowSoftness;
    private SerializedProperty debugView;
    private SerializedProperty windDirection;
    private SerializedProperty windSpeed;
    private SerializedProperty shapeWindSpeedMultiplier;
    private SerializedProperty detailWindSpeedMultiplier;
    private SerializedProperty sunLight;
    private SerializedProperty cloudColor;
    private SerializedProperty shadowColor;
    private SerializedProperty lightAbsorption;
    private SerializedProperty lightSteps;
    private SerializedProperty lightStepSize;
    private SerializedProperty ambientLight;
    private SerializedProperty sunLightDimmer;
    private SerializedProperty scatteringTint;
    private SerializedProperty phaseForward;
    private SerializedProperty multiScattering;
    private SerializedProperty powderEffectIntensity;
    private SerializedProperty silverIntensity;
    private SerializedProperty silverSpread;

    private bool showShape = true;
    private bool showWeather = true;
    private bool showLighting = true;
    private bool showQuality = true;
    private bool showCurves;
    private bool showDebug;

    private void OnEnable()
    {
        cloudPreset = Find("cloudPreset");
        showLayerGizmo = Find("showLayerGizmo");
        layerBottom = Find("layerBottom");
        layerThickness = Find("layerThickness");
        maxCloudDistance = Find("maxCloudDistance");
        distanceFade = Find("distanceFade");
        shapeWorldSize = Find("shapeWorldSize");
        shapeHeightWorldSize = Find("shapeHeightWorldSize");
        detailWorldSize = Find("detailWorldSize");
        detailHeightWorldSize = Find("detailHeightWorldSize");
        weatherWorldSize = Find("weatherWorldSize");
        densityMultiplier = Find("densityMultiplier");
        densityThreshold = Find("densityThreshold");
        coverage = Find("coverage");
        shapeFactor = Find("shapeFactor");
        shapeOffset = Find("shapeOffset");
        curveTextureResolution = Find("curveTextureResolution");
        densityCurve = Find("densityCurve");
        erosionCurve = Find("erosionCurve");
        ambientOcclusionCurve = Find("ambientOcclusionCurve");
        bottomFade = Find("bottomFade");
        topFade = Find("topFade");
        topDensity = Find("topDensity");
        heightDensityPower = Find("heightDensityPower");
        altitudeDistortion = Find("altitudeDistortion");
        erosionFactor = Find("erosionFactor");
        microErosion = Find("microErosion");
        microErosionFactor = Find("microErosionFactor");
        erosionOcclusion = Find("erosionOcclusion");
        weatherMap = Find("weatherMap");
        weatherOffset = Find("weatherOffset");
        weatherInfluence = Find("weatherInfluence");
        weatherMinimum = Find("weatherMinimum");
        weatherContrast = Find("weatherContrast");
        weatherWindSpeedMultiplier = Find("weatherWindSpeedMultiplier");
        stepSize = Find("stepSize");
        maxSteps = Find("maxSteps");
        ditherTexture = Find("ditherTexture");
        ditherStrength = Find("ditherStrength");
        emptySpaceSkipping = Find("emptySpaceSkipping");
        emptyStepMultiplier = Find("emptyStepMultiplier");
        emptyStepTrigger = Find("emptyStepTrigger");
        temporalAccumulation = Find("temporalAccumulation");
        temporalBlend = Find("temporalBlend");
        depthAwareUpsample = Find("depthAwareUpsample");
        depthUpsampleTolerance = Find("depthUpsampleTolerance");
        edgeSharpening = Find("edgeSharpening");
        horizonTint = Find("horizonTint");
        aerialPerspective = Find("aerialPerspective");
        horizonFade = Find("horizonFade");
        cloudShadows = Find("cloudShadows");
        cloudShadowStrength = Find("cloudShadowStrength");
        cloudShadowSoftness = Find("cloudShadowSoftness");
        debugView = Find("debugView");
        windDirection = Find("windDirection");
        windSpeed = Find("windSpeed");
        shapeWindSpeedMultiplier = Find("shapeWindSpeedMultiplier");
        detailWindSpeedMultiplier = Find("detailWindSpeedMultiplier");
        sunLight = Find("sunLight");
        cloudColor = Find("cloudColor");
        shadowColor = Find("shadowColor");
        lightAbsorption = Find("lightAbsorption");
        lightSteps = Find("lightSteps");
        lightStepSize = Find("lightStepSize");
        ambientLight = Find("ambientLight");
        sunLightDimmer = Find("sunLightDimmer");
        scatteringTint = Find("scatteringTint");
        phaseForward = Find("phaseForward");
        multiScattering = Find("multiScattering");
        powderEffectIntensity = Find("powderEffectIntensity");
        silverIntensity = Find("silverIntensity");
        silverSpread = Find("silverSpread");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(cloudPreset);
        EditorGUILayout.HelpBox("Switching Cloud Preset loads the saved values for that preset. Apply Preset saves the current inspector values into the selected preset.", MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply Preset"))
            {
                ApplyToTargets(volume => volume.ApplyCurrentPreset(), "Apply Cloud Preset");
                serializedObject.Update();
            }

            if (GUILayout.Button("Portfolio Look"))
            {
                ApplyToTargets(volume => volume.ApplyPortfolioShotLook(), "Apply Portfolio Cloud Look");
                serializedObject.Update();
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Performance Preview"))
            {
                ApplyToTargets(volume => volume.ApplyPerformancePreviewLook(), "Apply Performance Cloud Look");
                serializedObject.Update();
            }

            if (GUILayout.Button("Final Debug"))
            {
                ApplyToTargets(volume => volume.ResetCloudDebugView(), "Reset Cloud Debug View");
                serializedObject.Update();
            }
        }

        if (GUILayout.Button("Reset Built-In Presets"))
        {
            ApplyToTargets(volume => volume.ResetBuiltInPresetBank(), "Reset Built-In Cloud Presets");
            serializedObject.Update();
        }

        EditorGUILayout.Space(6.0f);

        DrawLayer();
        DrawShape();
        DrawWeather();
        DrawLighting();
        DrawQuality();
        DrawDebug();

        serializedObject.ApplyModifiedProperties();
    }

    private SerializedProperty Find(string propertyName)
    {
        return serializedObject.FindProperty(propertyName);
    }

    private void DrawLayer()
    {
        EditorGUILayout.LabelField("Cloud Layer", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showLayerGizmo);
        EditorGUILayout.PropertyField(layerBottom);
        EditorGUILayout.PropertyField(layerThickness);
        EditorGUILayout.PropertyField(maxCloudDistance);
        EditorGUILayout.PropertyField(distanceFade);

        float top = layerBottom.floatValue + layerThickness.floatValue;
        EditorGUILayout.HelpBox("Current layer range: " + layerBottom.floatValue.ToString("0.0") + "m - " + top.ToString("0.0") + "m", MessageType.None);
    }

    private void DrawShape()
    {
        showShape = EditorGUILayout.Foldout(showShape, "Shape And Erosion", true);
        if (!showShape)
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(shapeWorldSize);
        EditorGUILayout.PropertyField(shapeHeightWorldSize);
        EditorGUILayout.PropertyField(detailWorldSize);
        EditorGUILayout.PropertyField(detailHeightWorldSize);
        EditorGUILayout.PropertyField(densityMultiplier);
        EditorGUILayout.PropertyField(densityThreshold);
        EditorGUILayout.PropertyField(coverage);
        EditorGUILayout.PropertyField(shapeFactor);
        EditorGUILayout.PropertyField(shapeOffset);
        EditorGUILayout.PropertyField(bottomFade);
        EditorGUILayout.PropertyField(topFade);
        EditorGUILayout.PropertyField(topDensity);
        EditorGUILayout.PropertyField(heightDensityPower);
        EditorGUILayout.PropertyField(altitudeDistortion);
        EditorGUILayout.PropertyField(erosionFactor);
        EditorGUILayout.PropertyField(microErosion);
        EditorGUILayout.PropertyField(microErosionFactor);
        EditorGUILayout.PropertyField(erosionOcclusion);

        showCurves = EditorGUILayout.Foldout(showCurves, "Height Curves", true);
        if (showCurves)
        {
            EditorGUILayout.PropertyField(curveTextureResolution);
            EditorGUILayout.PropertyField(densityCurve);
            EditorGUILayout.PropertyField(erosionCurve);
            EditorGUILayout.PropertyField(ambientOcclusionCurve);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawWeather()
    {
        showWeather = EditorGUILayout.Foldout(showWeather, "Weather And Wind", true);
        if (!showWeather)
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(weatherMap);
        EditorGUILayout.PropertyField(weatherWorldSize);
        EditorGUILayout.PropertyField(weatherOffset);
        EditorGUILayout.PropertyField(weatherInfluence);
        EditorGUILayout.PropertyField(weatherMinimum);
        EditorGUILayout.PropertyField(weatherContrast);
        EditorGUILayout.PropertyField(weatherWindSpeedMultiplier);
        EditorGUILayout.PropertyField(windDirection);
        EditorGUILayout.PropertyField(windSpeed);
        EditorGUILayout.PropertyField(shapeWindSpeedMultiplier);
        EditorGUILayout.PropertyField(detailWindSpeedMultiplier);
        EditorGUI.indentLevel--;
    }

    private void DrawLighting()
    {
        showLighting = EditorGUILayout.Foldout(showLighting, "Lighting And Atmosphere", true);
        if (!showLighting)
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sunLight);
        EditorGUILayout.PropertyField(cloudColor);
        EditorGUILayout.PropertyField(shadowColor);
        EditorGUILayout.PropertyField(scatteringTint);
        EditorGUILayout.PropertyField(lightAbsorption);
        EditorGUILayout.PropertyField(lightSteps);
        EditorGUILayout.PropertyField(lightStepSize);
        EditorGUILayout.PropertyField(ambientLight);
        EditorGUILayout.PropertyField(sunLightDimmer);
        EditorGUILayout.PropertyField(phaseForward);
        EditorGUILayout.PropertyField(multiScattering);
        EditorGUILayout.PropertyField(powderEffectIntensity);
        EditorGUILayout.PropertyField(silverIntensity);
        EditorGUILayout.PropertyField(silverSpread);
        EditorGUILayout.PropertyField(horizonTint);
        EditorGUILayout.PropertyField(aerialPerspective);
        EditorGUILayout.PropertyField(horizonFade);
        EditorGUILayout.PropertyField(cloudShadows);
        EditorGUILayout.PropertyField(cloudShadowStrength);
        EditorGUILayout.PropertyField(cloudShadowSoftness);
        EditorGUI.indentLevel--;
    }

    private void DrawQuality()
    {
        showQuality = EditorGUILayout.Foldout(showQuality, "Raymarch And Reconstruction", true);
        if (!showQuality)
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(stepSize);
        EditorGUILayout.PropertyField(maxSteps);
        EditorGUILayout.PropertyField(ditherTexture);
        EditorGUILayout.PropertyField(ditherStrength);
        EditorGUILayout.PropertyField(emptySpaceSkipping);
        EditorGUILayout.PropertyField(emptyStepMultiplier);
        EditorGUILayout.PropertyField(emptyStepTrigger);
        EditorGUILayout.PropertyField(temporalAccumulation);
        EditorGUILayout.PropertyField(temporalBlend);
        EditorGUILayout.PropertyField(depthAwareUpsample);
        EditorGUILayout.PropertyField(depthUpsampleTolerance);
        EditorGUILayout.PropertyField(edgeSharpening);

        if (stepSize.floatValue > 5.0f)
            EditorGUILayout.HelpBox("Large step sizes can reveal raymarch layers near cloud edges. Use Performance Preview for editing, then Portfolio Look for final captures.", MessageType.Info);

        EditorGUI.indentLevel--;
    }

    private void DrawDebug()
    {
        showDebug = EditorGUILayout.Foldout(showDebug, "Debug", true);
        if (!showDebug)
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(debugView);
        EditorGUILayout.HelpBox("Use Alpha, Lighting, Weather, Height, and RaySteps to isolate one part of the cloud pipeline while tuning.", MessageType.None);
        EditorGUI.indentLevel--;
    }

    private void ApplyToTargets(System.Action<CloudVolume> action, string undoName)
    {
        foreach (UnityEngine.Object targetObject in targets)
        {
            CloudVolume volume = (CloudVolume)targetObject;
            Undo.RecordObject(volume, undoName);
            action(volume);
            EditorUtility.SetDirty(volume);
        }
    }
}
