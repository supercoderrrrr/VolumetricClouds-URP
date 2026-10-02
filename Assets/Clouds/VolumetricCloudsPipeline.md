# Volumetric Clouds Pipeline

This project renders a URP volumetric cloud layer with low-resolution ray marching, temporal reconstruction, weather-driven shape control, and light scattering.

## Runtime Flow

1. `CloudRaymarchRenderFeature` injects a render pass before post processing.
2. Pass 0 ray marches a world-space cloud layer into a low-resolution cloud texture.
3. Pass 1 temporally accumulates the cloud texture and clamps history with current-neighborhood min/max.
4. Pass 2 depth-aware upsamples the cloud texture, applies terrain cloud shadows, blends aerial perspective, and composites clouds over the camera color.

## Authoring Flow

1. Generate `ShapeWorley128.asset` and `DetailWorley64.asset` with the cloud noise generator.
2. Generate or assign `WeatherMap.png` to control large-scale coverage.
3. Assign the noise textures and material in the URP renderer feature.
4. Add one `CloudVolume` to the scene.
5. Use the custom `CloudVolume` inspector buttons:
   - `Portfolio Look`: balanced final capture settings.
   - `Performance Preview`: faster iteration settings.
   - `Final Debug`: restores normal final rendering after using debug views.

## Main Controls

- `Shape World Size`: larger values reduce visible repetition and create larger cloud masses.
- `Weather World Size`: larger values create broader weather cells.
- `Density Multiplier`: increases optical thickness.
- `Density Threshold`: lowers or raises how much base noise becomes cloud.
- `Coverage`: controls large-scale cloud amount.
- `Step Size`: lower values reduce raymarch layers but cost more performance.
- `Temporal Blend`: higher values smooth noise but can ghost if camera movement is aggressive.
- `Debug View`: isolate alpha, lighting, weather, height, or ray step usage.

## Implementation Notes

- The shader uses world-space cloud layer intersection instead of a visible cloud box.
- Shape and weather sampling use secondary rotated domains to reduce obvious texture tiling.
- Scene view disables temporal blending, and camera switches invalidate temporal history to avoid hard split-screen artifacts.
- Cloud shadows are approximate screen-space receiver darkening from the same lightmarch function.
