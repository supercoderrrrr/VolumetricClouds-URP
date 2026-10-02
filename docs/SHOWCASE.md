# Showcase Settings / 演示设置

## English

The clips use automated parameter sweeps in Unity 2022.3.62f2 / Direct3D 11. Camera and wind stay fixed, temporal accumulation is off, and capture settings are not saved into the scene or presets.

| Clip | Parameter | Playback |
| --- | --- | --- |
| Coverage | Coverage 0.35 to approximately 1.0 and back | 60 frames / 6 seconds |
| Sun direction | Sun elevation 15 to approximately 75 degrees and back | 60 frames / 6 seconds |
| Noise slices | Shape/Detail R channel, Slice 0.02 to approximately 0.98 and back | 48 frames / 6 seconds |

Cloud clips start with Portfolio Look and the included scene camera. Render Scale is 0.75; Step Size / Max Steps are 3 / 320; Light Steps is 8; Density Multiplier / Threshold are 10 / 0.2; Ambient Light is 0.6; sun intensity is 0.8.

The coverage clip uses Sun Light Dimmer / Silver Intensity 1.1 / 0.5. The sun clip uses Coverage 0.88, Sun Light Dimmer / Silver Intensity 1.4 / 0.65, and a fixed sun Y rotation of camera Y minus 180 degrees. Per-frame values are in [showcase-parameters.csv](media/showcase-parameters.csv).

To reproduce the controls, select CloudVolume, apply Portfolio Look, set the values above and move Coverage. For lighting, restore Coverage to 0.88 and rotate the Directional Light around X. For slices, open Tools > Volumetric Clouds > Noise Preview, select R and move Slice. Do not save these settings into a preset unless you intend to replace it.

Noise slices sample the existing Texture3D assets; they do not bake new noise each frame. Debug screenshots use the included scene's saved settings, not these capture overrides. Clip playback rate is not measured runtime FPS.

## 简体中文

三段演示通过 Unity 2022.3.62f2／Direct3D 11 自动扫描参数生成：覆盖率约 0.35 至 1.0、太阳仰角约 15 至 75 度，以及 Shape／Detail 的 R 通道切片约 0.02 至 0.98。每段约 6 秒，相机和风固定，关闭时间累积，没有覆盖原场景或预设。

云演示基于 Portfolio Look，使用上方的渲染、密度及光照设置。选择 CloudVolume 后可拖动 Coverage；观察太阳方向时将覆盖率恢复为 0.88 并旋转 Directional Light 的 X。噪声预览使用 Tools > Volumetric Clouds > Noise Preview，选择 R 并移动 Slice。逐帧数值保留在 CSV 中，播放帧率不代表运行时性能。
