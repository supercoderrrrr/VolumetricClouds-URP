# Development Archive / 开发归档

[English](#english) | [简体中文](#简体中文)

## English

### Why the First Git Commit Includes the Project

This project began as a study of ray-marched volumetric clouds in Unity URP. Development iterations were tracked with **Unity Version Control rather than Git**. The working project was later imported into GitHub for portfolio presentation and continued maintenance.

The first Git commit is a snapshot of the implementation at migration time, not the beginning of development. Earlier revisions remain in Unity Version Control; changes after migration are recorded in this repository's Git history.

This document summarizes the implementation's development. It is a technical retrospective, not a raw changeset or client-log export. The stages below refer to the current code, not individually checkoutable historical Git versions.

### Before the GitHub Import

| Stage | Work | Current Entry Points |
| --- | --- | --- |
| GPU noise generation | Periodic Worley noise, value-noise FBM, Compute Shader voxel generation and offline Texture3D baking | [CloudNoiseGenerator.compute](Assets/Clouds/Shaders/CloudNoiseGenerator.compute), [CloudNoiseGeneratorWindow](Assets/Clouds/Editor/CloudNoiseGeneratorWindow.cs) |
| Authoring tools | Shape/Detail generation, RGBA slice preview, weather coverage maps and dither texture generation | [CloudNoisePreviewWindow](Assets/Clouds/Editor/CloudNoisePreviewWindow.cs), [CloudWeatherMapGeneratorWindow](Assets/Clouds/Editor/CloudWeatherMapGeneratorWindow.cs), [CloudDitherTextureGenerator](Assets/Clouds/Editor/CloudDitherTextureGenerator.cs) |
| URP cloud rendering | Develop the bounded cloud prototype into a horizontal world-space cloud layer, with depth reconstruction and Beer-Lambert integration | [CloudRayMarchRenderFeature](Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs), [CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| Cloud shape control | Weather-driven coverage, height curves, rotated noise domains, detail erosion and wind offsets | [CloudVolume](Assets/Clouds/Runtime/CloudVolume.cs), [CloudCurveTexture](Assets/Clouds/Runtime/CloudCurveTexture.cs) |
| Cloud lighting | Secondary light marching, Henyey-Greenstein phase, ambient occlusion controls, approximate multiple-scattering fill, powder and silver lining | [CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| Reconstruction and quality | Reduced-resolution rendering, temporal reprojection, neighborhood history clamp, depth-aware upsampling and configurable step budgets | [CloudRayMarchRenderFeature](Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs), [CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| Presets and showcase | Editable preset storage, grouped inspector controls, camera animation and the mountain demonstration scene | [CloudVolume](Assets/Clouds/Runtime/CloudVolume.cs), [CloudVolumeEditor](Assets/Clouds/Editor/CloudVolumeEditor.cs) |

### Updates on GitHub

| Update | Work | Commit |
| --- | --- | --- |
| Project import | Publish the Unity project, cloud tools, URP settings, generated textures, bilingual README and original recording | [e796640](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/e796640cc33ed79062e4eae60b9812b0d6af2063) |
| Homepage preview | Place the recording preview and full video together on the project homepage | [8382360](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/83823602fbaf12ad3fdc498779b731ba8ab2095b) |
| Feature clips and diagnostics | Add coverage, sun-direction and noise-slice clips; stop view sampling at the visible interval boundary; correct RaySteps and Lighting diagnostics | [93d52c5](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/93d52c537f585bee8c04ca6e741d7960123cbcdd) |

The diagnostic fixes were also saved in Unity Version Control. The original scene and saved preset bank were not replaced by the temporary capture settings.

Checks used Unity 2022.3.62f2 and Direct3D 11: script/shader import, Compute Shader noise generation, Texture3D GUID preservation, camera renders and diagnostic comparisons. The feature clips are automated parameter sweeps with fixed camera and wind. Their playback rate is not a runtime performance measurement.

### Next Steps

1. Add repeatable tests for density evaluation, cloud-layer intersections and step termination.
2. Store temporal history per camera and test fast movement and animated clouds.
3. Profile view marching, light marching and reconstruction across quality settings.
4. Evaluate conservative empty-space acceleration and validate more graphics backends.

The current implementation uses a horizontal layer and heuristic empty-space skipping, not an SDF or planetary atmosphere. Multiple scattering, aerial perspective and cloud shadows remain approximations. Usage and media are in [README.md](README.md); reference projects and asset licences are in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

---

## 简体中文

### 为什么首次 Git 提交包含完整项目

本项目从 Unity URP 光线步进体积云的学习逐步迭代，开发阶段使用 **Unity Version Control，而不是 Git** 保存版本。形成可运行的工程后，迁移到 GitHub，用于作品集展示与后续维护。

首次 Git 提交保存的是迁移时的项目快照，不代表项目在一次提交中从零完成。迁移前的版本保留在 Unity Version Control 中，迁移后的修改通过本仓库的 Git 历史继续记录。

本文件按技术阶段整理开发过程，属于技术归档，不是原始 changeset 或客户端日志的导出。下方代码入口指向当前实现，各阶段不对应可以独立检出的历史 Git 版本。

### 迁移前的实现演进

| 阶段 | 内容 | 当前代码入口 |
| --- | --- | --- |
| GPU 噪声生成 | 周期 Worley、Value Noise FBM、Compute Shader 体素计算和离线 Texture3D 烘焙 | [CloudNoiseGenerator.compute](Assets/Clouds/Shaders/CloudNoiseGenerator.compute)、[CloudNoiseGeneratorWindow](Assets/Clouds/Editor/CloudNoiseGeneratorWindow.cs) |
| 编辑器工具 | Shape／Detail 生成、RGBA 切片预览、天气覆盖图和抖动纹理生成 | [CloudNoisePreviewWindow](Assets/Clouds/Editor/CloudNoisePreviewWindow.cs)、[CloudWeatherMapGeneratorWindow](Assets/Clouds/Editor/CloudWeatherMapGeneratorWindow.cs)、[CloudDitherTextureGenerator](Assets/Clouds/Editor/CloudDitherTextureGenerator.cs) |
| URP 云渲染 | 从局部云盒原型发展为世界空间水平云层，加入深度重建与 Beer-Lambert 积分 | [CloudRayMarchRenderFeature](Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs)、[CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| 云形控制 | 天气覆盖率、高度曲线、旋转噪声采样域、细节侵蚀和风场偏移 | [CloudVolume](Assets/Clouds/Runtime/CloudVolume.cs)、[CloudCurveTexture](Assets/Clouds/Runtime/CloudCurveTexture.cs) |
| 云内光照 | 朝太阳的二次步进、Henyey-Greenstein 相位、环境遮蔽，以及近似多重散射、粉末效应和银边 | [CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| 重建与画质 | 低分辨率渲染、时间重投影、邻域历史钳制、深度感知上采样和可配置的步数预算 | [CloudRayMarchRenderFeature](Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs)、[CloudRayMarch.shader](Assets/Clouds/Shaders/CloudRayMarch.shader) |
| 预设与展示 | 可保存的预设、分组 Inspector、摄像机动画和雪山演示场景 | [CloudVolume](Assets/Clouds/Runtime/CloudVolume.cs)、[CloudVolumeEditor](Assets/Clouds/Editor/CloudVolumeEditor.cs) |

### 迁移后的开发记录

| 更新 | 内容 | 提交 |
| --- | --- | --- |
| 工程导入 | 发布 Unity 工程、云工具、URP 配置、生成纹理、双语 README 和原始视频 | [e796640](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/e796640cc33ed79062e4eae60b9812b0d6af2063) |
| 首页预览 | 将录制预览和完整视频入口放到项目首页 | [8382360](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/83823602fbaf12ad3fdc498779b731ba8ab2095b) |
| 分项演示与诊断 | 添加覆盖率、太阳方向、噪声切片演示；修复有效区间外继续采样的问题，完善 RaySteps 和 Lighting 调试 | [93d52c5](https://github.com/supercoderrrrr/VolumetricClouds-URP/commit/93d52c537f585bee8c04ca6e741d7960123cbcdd) |

诊断修复也已保存到 Unity Version Control。原始场景和已保存的预设没有被录制时使用的临时参数覆盖。

验证使用 Unity 2022.3.62f2／Direct3D 11，包含脚本与 Shader 导入、Compute Shader 噪声生成、Texture3D GUID 保留、相机渲染及诊断对照。分项视频通过固定相机和风场、自动扫描参数生成；播放帧率不代表运行时性能。

### 后续方向

1. 补充密度计算、云层求交和步进停止条件的可重复测试。
2. 为不同相机维护独立时间历史，测试快速运动和动态云。
3. 分别测量视线步进、光线步进及重建在不同画质下的开销。
4. 评估保守空区加速方案，验证更多图形后端。

当前使用水平云层和启发式空区跳步，未实现 SDF 或行星大气；多重散射、空气透视及云影属于近似模型。使用方法与展示素材见 [README.md](README.md)，参考项目与资源许可见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
