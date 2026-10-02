# Development History / 开发记录

Development was versioned in Unity Version Control before the GitHub publication. The first Git commit is an import of the release snapshot, not the first day of implementation. This file and changesets.json preserve the actual SCM ids, timestamps, comments and changed paths; they do not invent earlier Git commits.

本项目此前使用 Unity Version Control 保存版本。GitHub 的首次提交是发布版本的迁移快照，并非项目从零完成的时间。以下记录直接导出真实 SCM 变更集编号、时间、原始日志与文件清单，没有伪造历史 Git 提交。账户邮箱、云组织及服务器标识和本机绝对路径已移除。

Historical comments describe the implementation at the time and are not a current feature checklist. CS4 mentioned the custom inspector, but it was still Private in the workspace; the final check-in corrects that omission.

历史日志描述当时的实现，并不等同于当前技术功能列表。CS4 日志已提及自定义 Inspector，但文件当时仍是 Private；最终整理提交补齐了该文件。

## CS0 | 2026-08-05T21:18:45+08:00 | /main

(No check-in comment / 无提交日志)

Changed items: 0

<details><summary>Changed paths / 文件清单</summary>


</details>

## CS1 | 2026-08-07T11:57:40+08:00 | /main

实现 Unity URP Ray Marching 体积云渲染原型及配套噪声生成工具

本次提交完成了一套基于 URP 后处理管线的实时体积云渲染流程，包含 GPU 离线噪声生成、3D Texture 云密度采样、云盒 ray marching、基础体积光照、天气图控制、云形态参数控制及低分辨率渲染优化。

主要实现内容：

1. 实现 GPU 体积噪声生成流程
- 新增 Compute Shader 噪声生成器，用 GPU 并行计算 3D 噪声数据。
- 实现基于 Hash 的 tileable Worley Noise，支持多频率 F1 细胞噪声生成。
- 实现 Value Noise FBM，并与 Worley FBM 组合生成 Perlin-Worley 风格 Shape Noise。
- 使用 ComputeBuffer 接收 GPU 计算结果，并离线保存为 Unity Texture3D 资产。
- 生成 Shape Noise 与 Detail Noise 两类 3D Texture：
  - Shape Noise 用于控制云的大体积轮廓。
  - Detail Noise 用于云边缘侵蚀和细节破碎。

2. 实现编辑器工具链
- 新增 Cloud Noise Generator 编辑器窗口，用于生成 Shape / Detail 体积噪声。
- 新增 3D Noise Preview 工具，可按 Slice 和 RGBA 通道预览 Texture3D 噪声切片。
- 新增 Weather Map Generator，用 2D FBM 噪声生成天气图，控制水平云量分布。
- 新增 Dither Texture Generator，用于生成体积步进抖动纹理。

3. 实现 URP 体积云渲染管线
- 新增 ScriptableRendererFeature / ScriptableRenderPass，将体积云作为 URP 后处理 Pass 注入渲染流程。
- 使用 Camera Depth Texture 进行屏幕空间深度重建。
- 在 Shader 中通过 ComputeWorldSpacePosition 重建每个像素对应的世界空间射线方向。
- 实现 rayBoxDst 云盒快速求交，用 AABB 相交算法计算视线进入云盒的距离和盒内行进距离。
- 基于 ray marching 沿视线步进采样 3D Shape Noise / Detail Noise，累积云密度和透明度。

4. 实现云密度与形态控制
- 新增 CloudVolume 场景组件，用于可视化和控制云盒范围。
- 支持 Density Multiplier、Density Threshold、Coverage、Detail Strength 等密度参数。
- 实现云层高度渐变控制，包括 Bottom Fade 和 Top Fade，用于形成更自然的云底和云顶。
- 支持 Weather Map 控制局部云量，实现大范围云区、空洞和云带分布。
- 支持风向、主噪声流动、细节噪声流动和天气图漂移控制。

5. 实现基础体积光照
- 实现朝太阳方向的 Lightmarch，用多步采样估算云体内部遮蔽。
- 使用 Beer-Lambert 吸收模型，通过 exp(-density * absorption) 计算光线透过率。
- 支持 Shadow Color、Ambient Light、Light Absorption、Light Steps、Light Step Size 等光照参数。
- 实现 Henyey-Greenstein 相位函数和银边高光参数，用于模拟体积云前向散射效果。

6. 实现渲染优化与质量控制
- 实现低分辨率体积云渲染 + 全分辨率 Composite Pass。
- Pass 0 在低分辨率 RT 中输出云颜色和透明度。
- Pass 1 将低分辨率云结果上采样并合成回相机颜色缓冲。
- 支持 Render Scale 参数，在画质和性能之间切换。
- 加入 jitter / dither 采样起点偏移，用于减轻 ray marching 步进切片感。
- 提高 Max Steps 上限，使小 Step Size 下仍可完整穿过云盒，减少云体变薄问题。

当前项目已形成完整的体积云核心流程：
Compute Shader 离线噪声生成 -> Texture3D 云噪声资产 -> URP 后处理注入 -> 深度重建 -> 云盒求交 -> ray marching 密度累积 ->

Changed items: 51

<details><summary>Changed paths / 文件清单</summary>

- `Added` `/Assets`
- `Added` `/Assets/Clouds`
- `Added` `/Assets/Clouds/Editor`
- `Added` `/Assets/Clouds/Editor/CloudDitherTextureGenerator.cs`
- `Added` `/Assets/Clouds/Editor/CloudDitherTextureGenerator.cs.meta`
- `Added` `/Assets/Clouds/Editor/CloudNoiseGeneratorWindow.cs`
- `Added` `/Assets/Clouds/Editor/CloudNoiseGeneratorWindow.cs.meta`
- `Added` `/Assets/Clouds/Editor/CloudNoisePreviewWindow.cs`
- `Added` `/Assets/Clouds/Editor/CloudNoisePreviewWindow.cs.meta`
- `Added` `/Assets/Clouds/Editor/CloudWeatherMapGeneratorWindow.cs`
- `Added` `/Assets/Clouds/Editor/CloudWeatherMapGeneratorWindow.cs.meta`
- `Added` `/Assets/Clouds/Editor.meta`
- `Added` `/Assets/Clouds/GeneratedNoise`
- `Added` `/Assets/Clouds/GeneratedNoise/DetailWorley32.asset`
- `Added` `/Assets/Clouds/GeneratedNoise/DetailWorley32.asset.meta`
- `Added` `/Assets/Clouds/GeneratedNoise/ShapeWorley64.asset`
- `Added` `/Assets/Clouds/GeneratedNoise/ShapeWorley64.asset.meta`
- `Added` `/Assets/Clouds/GeneratedNoise.meta`
- `Added` `/Assets/Clouds/Materials`
- `Added` `/Assets/Clouds/Materials/CloudRayMarch_Mat.mat`
- `Added` `/Assets/Clouds/Materials/CloudRayMarch_Mat.mat.meta`
- `Added` `/Assets/Clouds/Materials.meta`
- `Added` `/Assets/Clouds/Runtime`
- `Added` `/Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs`
- `Added` `/Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs.meta`
- `Added` `/Assets/Clouds/Runtime/CloudVolume.cs`
- `Added` `/Assets/Clouds/Runtime/CloudVolume.cs.meta`
- `Added` `/Assets/Clouds/Runtime.meta`
- `Added` `/Assets/Clouds/Shaders`
- `Added` `/Assets/Clouds/Shaders/CloudNoiseGenerator.compute`
- `Added` `/Assets/Clouds/Shaders/CloudNoiseGenerator.compute.meta`
- `Added` `/Assets/Clouds/Shaders/CloudNoisePreview.shader`
- `Added` `/Assets/Clouds/Shaders/CloudNoisePreview.shader.meta`
- `Added` `/Assets/Clouds/Shaders/CloudRayMarch.shader`
- `Added` `/Assets/Clouds/Shaders/CloudRayMarch.shader.meta`
- `Added` `/Assets/Clouds/Shaders.meta`
- `Added` `/Assets/Clouds/Textures`
- `Added` `/Assets/Clouds/Textures/CloudDither.png`
- `Added` `/Assets/Clouds/Textures/CloudDither.png.meta`
- `Added` `/Assets/Clouds/Textures/WeatherMap.png`
- `Added` `/Assets/Clouds/Textures/WeatherMap.png.meta`
- `Added` `/Assets/Clouds/Textures.meta`
- `Added` `/Assets/Clouds.meta`
- `Added` `/Assets/Scenes`
- `Added` `/Assets/Scenes/SampleScene.unity`
- `Added` `/Assets/Scenes/SampleScene.unity.meta`
- `Added` `/Assets/Settings`
- `Added` `/Assets/Settings/URP-HighFidelity-Renderer.asset`
- `Added` `/Assets/Settings/URP-HighFidelity-Renderer.asset.meta`
- `Added` `/Assets/Settings/URP-HighFidelity.asset`
- `Added` `/Assets/Settings/URP-HighFidelity.asset.meta`

</details>

## CS2 | 2026-08-07T14:14:11+08:00 | /main

-参考其他项目做了一些优化

Changed items: 16

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Clouds/Materials/CloudRayMarch_Mat.mat`
- `Changed` `/Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs`
- `Changed` `/Assets/Clouds/Runtime/CloudVolume.cs`
- `Changed` `/Assets/Clouds/Shaders/CloudRayMarch.shader`
- `Added` `/Assets/Clouds/GeneratedNoise/DetailWorley64.asset`
- `Added` `/Assets/Clouds/GeneratedNoise/DetailWorley64.asset.meta`
- `Added` `/Assets/Clouds/GeneratedNoise/ShapeWorley128.asset`
- `Added` `/Assets/Clouds/GeneratedNoise/ShapeWorley128.asset.meta`
- `Added` `/Assets/Clouds/Runtime/CloudCurveTexture.cs`
- `Added` `/Assets/Clouds/Runtime/CloudCurveTexture.cs.meta`
- `Added` `/Assets/Clouds/Runtime/CloudVolumeFollowCamera.cs`
- `Added` `/Assets/Clouds/Runtime/CloudVolumeFollowCamera.cs.meta`
- `Deleted` `/Assets/Clouds/GeneratedNoise/DetailWorley32.asset`
- `Deleted` `/Assets/Clouds/GeneratedNoise/DetailWorley32.asset.meta`
- `Deleted` `/Assets/Clouds/GeneratedNoise/ShapeWorley64.asset`
- `Deleted` `/Assets/Clouds/GeneratedNoise/ShapeWorley64.asset.meta`

</details>

## CS3 | 2026-08-07T15:06:37+08:00 | /main

- 做了更多优化

Changed items: 5

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs`
- `Changed` `/Assets/Clouds/Runtime/CloudVolume.cs`
- `Changed` `/Assets/Clouds/Shaders/CloudRayMarch.shader`
- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Settings/URP-HighFidelity-Renderer.asset`

</details>

## CS4 | 2026-08-07T18:44:45+08:00 | /main

实现 URP Ray March 体积云渲染系统

- 搭建基于 Compute Shader 的 3D 云噪声离线生成流程。
- 生成并接入 Shape Worley 与 Detail Worley 3D Texture。
- 实现 URP ScriptableRendererFeature 体积云渲染管线。
- 实现世界空间云层求交与 Ray March 光线步进。
- 实现基于深度重建的屏幕后处理合成。
- 加入高度曲线控制云密度、侵蚀与环境遮蔽。
- 加入 Weather Map 控制大尺度云覆盖分布。
- 加入 Detail Noise 与 Micro Erosion 控制云边缘破碎细节。
- 加入 Light Marching、光吸收、多重散射、Powder Effect 与 Silver Lining 控制。
- 实现低分辨率体积云渲染与 Temporal Accumulation。
- 实现 Depth-Aware Upsampling 与边缘锐化重建。
- 加入近似云影效果，用于影响场景几何体明暗。
- 加入抗平铺噪声采样，减少重复云块图案。
- 优化 Scene/Game 相机切换时的 Temporal History 处理。
- 将原本固定云盒流程改为世界空间云层渲染。
- 添加 CloudVolume 自定义 Inspector 面板。
- 添加可编辑 Sparse / Cloudy / Overcast / Stormy 预设系统。
- 添加 Portfolio Look 与 Performance Preview 快捷参数。
- 添加 CloudOnly、Alpha、Lighting、Weather、Height、RaySteps 等调试视图。
- 补充体积云渲染流程说明文档。

Changed items: 4

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Clouds/Materials/CloudRayMarch_Mat.mat`
- `Changed` `/Assets/Clouds/Runtime/CloudVolume.cs`
- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Settings/URP-HighFidelity-Renderer.asset`

</details>

## CS5 | 2026-10-02T10:28:16+08:00 | /main

完成体积云作品集演示并补齐工程版本控制文件

- 完成雪山演示场景、摄像机动画及最终云层参数配置
- 整理 URP 高质量渲染配置与云材质的场景设置
- 将漏交的 CloudVolume 自定义 Inspector、流程文档及对应 .meta 纳入版本控制
- 补齐 Packages 清单和锁文件，以及 ProjectSettings，确保工程版本与依赖可还原
- 补齐场景目录、URP 配置、全局管线设置等必要资产的 .meta
- 修复重复生成 Texture3D 时删除旧资产造成 GUID 改变的问题，保留渲染资源引用
- 为噪声生成器添加输出路径检查及 ComputeBuffer 释放保障
- 将天气图生成器中的中文编码异常注释整理为简短英文注释
- 保留 SCM 原有开发记录，GitHub 使用迁移快照并附真实变更集日志说明

本次提交为工程完整性与作品集整理，不声称新增 SDF 加速或物理多重散射求解


Changed items: 124

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Clouds/Editor/CloudNoiseGeneratorWindow.cs`
- `Changed` `/Assets/Clouds/Editor/CloudWeatherMapGeneratorWindow.cs`
- `Changed` `/Assets/Clouds/Materials/CloudRayMarch_Mat.mat`
- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Changed` `/Assets/Settings/URP-HighFidelity.asset`
- `Added` `/Assets/Clouds/Editor/CloudVolumeEditor.cs`
- `Added` `/Assets/Clouds/Editor/CloudVolumeEditor.cs.meta`
- `Added` `/Assets/Clouds/VolumetricCloudsPipeline.md`
- `Added` `/Assets/Clouds/VolumetricCloudsPipeline.md.meta`
- `Added` `/Assets/Settings/SampleSceneProfile.asset`
- `Added` `/Assets/Settings/SampleSceneProfile.asset.meta`
- `Added` `/Assets/Settings/URP-Balanced-Renderer.asset`
- `Added` `/Assets/Settings/URP-Balanced-Renderer.asset.meta`
- `Added` `/Assets/Settings/URP-Balanced.asset`
- `Added` `/Assets/Settings/URP-Balanced.asset.meta`
- `Added` `/Assets/Settings/URP-Performant-Renderer.asset`
- `Added` `/Assets/Settings/URP-Performant-Renderer.asset.meta`
- `Added` `/Assets/Settings/URP-Performant.asset`
- `Added` `/Assets/Settings/URP-Performant.asset.meta`
- `Added` `/Assets/Snow Mountain`
- `Added` `/Assets/Snow Mountain.meta`
- `Added` `/Assets/Animation`
- `Added` `/Assets/Animation.meta`
- `Added` `/Assets/Readme.asset`
- `Added` `/Assets/Readme.asset.meta`
- `Added` `/Assets/Scenes.meta`
- `Added` `/Assets/Settings.meta`
- `Added` `/Assets/TutorialInfo`
- `Added` `/Assets/TutorialInfo.meta`
- `Added` `/Assets/UniversalRenderPipelineGlobalSettings.asset`
- `Added` `/Assets/UniversalRenderPipelineGlobalSettings.asset.meta`
- `Added` `/.gitignore`
- `Added` `/Documentation`
- `Added` `/ignore.conf`
- `Added` `/Packages`
- `Added` `/ProjectSettings`
- `Added` `/Assets/Snow Mountain/Materials`
- `Added` `/Assets/Snow Mountain/Materials/enviromentMap.mat`
- `Added` `/Assets/Snow Mountain/Materials/enviromentMap.mat.meta`
- `Added` `/Assets/Snow Mountain/Materials/mountain_Snow_000.mat`
- `Added` `/Assets/Snow Mountain/Materials/mountain_Snow_000.mat.meta`
- `Added` `/Assets/Snow Mountain/Materials/whitePlane.mat`
- `Added` `/Assets/Snow Mountain/Materials/whitePlane.mat.meta`
- `Added` `/Assets/Snow Mountain/Materials.meta`
- `Added` `/Assets/Snow Mountain/Prefab`
- `Added` `/Assets/Snow Mountain/Prefab/mountain_Snow_000.prefab`
- `Added` `/Assets/Snow Mountain/Prefab/mountain_Snow_000.prefab.meta`
- `Added` `/Assets/Snow Mountain/Prefab.meta`
- `Added` `/Assets/Snow Mountain/Scenes`
- `Added` `/Assets/Snow Mountain/Scenes/SnowMountains.unity`
- `Added` `/Assets/Snow Mountain/Scenes/SnowMountains.unity.meta`
- `Added` `/Assets/Snow Mountain/Scenes/SnowMountainsSettings.lighting`
- `Added` `/Assets/Snow Mountain/Scenes/SnowMountainsSettings.lighting.meta`
- `Added` `/Assets/Snow Mountain/Scenes.meta`
- `Added` `/Assets/Snow Mountain/Source`
- `Added` `/Assets/Snow Mountain/Source/cubemap`
- `Added` `/Assets/Snow Mountain/Source/cubemap/003_Sunrise_SkyboxVol2.hdr`
- `Added` `/Assets/Snow Mountain/Source/cubemap/003_Sunrise_SkyboxVol2.hdr.meta`
- `Added` `/Assets/Snow Mountain/Source/cubemap.meta`
- `Added` `/Assets/Snow Mountain/Source/fbx`
- `Added` `/Assets/Snow Mountain/Source/fbx/Materials`
- `Added` `/Assets/Snow Mountain/Source/fbx/Materials/defaultMat.mat`
- `Added` `/Assets/Snow Mountain/Source/fbx/Materials/defaultMat.mat.meta`
- `Added` `/Assets/Snow Mountain/Source/fbx/Materials.meta`
- `Added` `/Assets/Snow Mountain/Source/fbx/mountain_Snow_000.fbx`
- `Added` `/Assets/Snow Mountain/Source/fbx/mountain_Snow_000.fbx.meta`
- `Added` `/Assets/Snow Mountain/Source/fbx.meta`
- `Added` `/Assets/Snow Mountain/Source/tga`
- `Added` `/Assets/Snow Mountain/Source/tga/mountain_Snow_000_Aldedo.tga`
- `Added` `/Assets/Snow Mountain/Source/tga/mountain_Snow_000_Aldedo.tga.meta`
- `Added` `/Assets/Snow Mountain/Source/tga/mountain_Snow_000_Normal.tga`
- `Added` `/Assets/Snow Mountain/Source/tga/mountain_Snow_000_Normal.tga.meta`
- `Added` `/Assets/Snow Mountain/Source/tga.meta`
- `Added` `/Assets/Snow Mountain/Source.meta`
- `Added` `/Assets/Animation/CameraShowCase.anim`
- `Added` `/Assets/Animation/CameraShowCase.anim.meta`
- `Added` `/Assets/Animation/Main Camera.controller`
- `Added` `/Assets/Animation/Main Camera.controller.meta`
- `Added` `/Assets/TutorialInfo/Icons`
- `Added` `/Assets/TutorialInfo/Icons/URP.png`
- `Added` `/Assets/TutorialInfo/Icons/URP.png.meta`
- `Added` `/Assets/TutorialInfo/Icons.meta`
- `Added` `/Assets/TutorialInfo/Layout.wlt`
- `Added` `/Assets/TutorialInfo/Layout.wlt.meta`
- `Added` `/Assets/TutorialInfo/Scripts`
- `Added` `/Assets/TutorialInfo/Scripts/Editor`
- `Added` `/Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs`
- `Added` `/Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs.meta`
- `Added` `/Assets/TutorialInfo/Scripts/Editor.meta`
- `Added` `/Assets/TutorialInfo/Scripts/Readme.cs`
- `Added` `/Assets/TutorialInfo/Scripts/Readme.cs.meta`
- `Added` `/Assets/TutorialInfo/Scripts.meta`
- `Added` `/Documentation/scm`
- `Added` `/Documentation/scm/changesets.json`
- `Added` `/Documentation/scm/HISTORY.md`
- `Added` `/Documentation/SCM_FINAL_CHECKIN.txt`
- `Added` `/Packages/manifest.json`
- `Added` `/Packages/packages-lock.json`
- `Added` `/ProjectSettings/QualitySettings.asset`
- `Added` `/ProjectSettings/AudioManager.asset`
- `Added` `/ProjectSettings/BurstAotSettings_StandaloneWindows.json`
- `Added` `/ProjectSettings/ClusterInputManager.asset`
- `Added` `/ProjectSettings/CommonBurstAotSettings.json`
- `Added` `/ProjectSettings/DynamicsManager.asset`
- `Added` `/ProjectSettings/EditorBuildSettings.asset`
- `Added` `/ProjectSettings/EditorSettings.asset`
- `Added` `/ProjectSettings/GraphicsSettings.asset`
- `Added` `/ProjectSettings/InputManager.asset`
- `Added` `/ProjectSettings/MemorySettings.asset`
- `Added` `/ProjectSettings/NavMeshAreas.asset`
- `Added` `/ProjectSettings/PackageManagerSettings.asset`
- `Added` `/ProjectSettings/Physics2DSettings.asset`
- `Added` `/ProjectSettings/PresetManager.asset`
- `Added` `/ProjectSettings/ProjectSettings.asset`
- `Added` `/ProjectSettings/ProjectVersion.txt`
- `Added` `/ProjectSettings/SceneTemplateSettings.json`
- `Added` `/ProjectSettings/ShaderGraphSettings.asset`
- `Added` `/ProjectSettings/TagManager.asset`
- `Added` `/ProjectSettings/TimeManager.asset`
- `Added` `/ProjectSettings/UnityConnectSettings.asset`
- `Added` `/ProjectSettings/URPProjectSettings.asset`
- `Added` `/ProjectSettings/VersionControlSettings.asset`
- `Added` `/ProjectSettings/VFXManager.asset`
- `Added` `/ProjectSettings/XRSettings.asset`

</details>

## CS6 | 2026-10-02T10:49:52+08:00 | /main

清理作品集场景的太阳 Animator 丢失引用

- 最终工程还原检查发现 Directional Light 的 Animator 引用了不存在的控制器 GUID
- 本地未找到对应太阳动画资源，通过 Unity Editor API 清除丢失引用并禁用空 Animator
- 保留太阳 Transform 姿态与光照参数，不重新制作或声称已恢复缺失的太阳动画
- 确认摄像机现有 Main Camera.controller 与 CameraShowCase 动画剪辑引用正常并保持原样
- 发布副本同步清理引用，公开天空演示仍保持摄像机 Animator 禁用以方便参数调节
- 更新 GitHub 发布验证说明及真实 SCM 变更集导出


Changed items: 2

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Scenes/SampleScene.unity`
- `Added` `/Documentation/SCM_FINAL_REFERENCE_FIX.txt`

</details>

## CS7 | 2026-10-02T11:59:27+08:00 | /main

修复体积云诊断视图与有效采样区间，补充分项展示说明

1. 修复视线步进在启发式空区跳步后越过有效云层／场景深度区间仍继续采样的问题
2. RaySteps 改为按当前 Max Steps 预算归一化的采样热力图，区分无有效区间与采样占比
3. 诊断视图绕过大气染色和历史混合，切换调试模式时禁用旧历史混合
4. 诊断通道移到主要 URP 后处理之后，并在执行时重新取得交换后的相机颜色目标
5. Lighting 调试改为按不透明度贡献加权的光线透过率，避免单个透亮采样将整条射线显示为白色
6. 使用 Unity 2022.3.62f2／D3D11 在独立发布副本完成覆盖率 60 帧、太阳方向 60 帧、噪声切片 48 帧及调试回归；生成约 6 秒的 GIF／MP4 小段供公开 README 展示
7. 空密度对照确认跳步降低视线采样预算占比，大气与时间开关未改变诊断像素；尚未测量 GPU 耗时，不据此宣称加速倍数
8. 原场景、摄像机动画、雪山资产和保存的预设参数保持不变；临时录制脚本和原始渲染帧不纳入发布工程

公开 README 同时保留英文和中文版本，记录演示临时基准参数、逐帧数值及跳步关闭／开启的诊断对照


Changed items: 3

<details><summary>Changed paths / 文件清单</summary>

- `Changed` `/Assets/Clouds/Shaders/CloudRayMarch.shader`
- `Changed` `/Assets/Clouds/Runtime/CloudRayMarchRenderFeature.cs`
- `Added` `/Documentation/SCM_SHOWCASE_CHECKIN.txt`

</details>
