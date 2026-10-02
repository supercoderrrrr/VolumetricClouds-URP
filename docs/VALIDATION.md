# Release Validation / 发布验证

Validation was performed on the isolated public export using Unity 2022.3.62f2 and Direct3D 11. The original SCM scene and downloaded mountain assets were preserved locally.

## Completed Checks

- Unity imported the project and compiled the runtime/editor scripts
- The real Compute Shader generated a 32 x 32 x 32 Texture3D test asset, then regenerated it with a different seed
- The voxel array contained 32,768 entries and a non-uniform shape channel
- Texture regeneration retained the original asset GUID
- The bundled ShapeWorley128 and DetailWorley64 textures were loaded and all four channel slices rendered
- URP rendered Final, Alpha, Lighting and RaySteps camera captures at 1280 x 720
- Shader error checks completed without cloud-shader errors
- Tool images were captured from Unity's own GUI render buffers and checked visually
- The downloaded mountain assets were removed from the public scene and asset tree using Unity APIs
- Private files required to restore the original project were checked into SCM as CS5
- A stale Directional Light Animator controller reference was identified by the GUID audit, cleared through Unity's editor API in both the source and public scene, and recorded as CS6; the missing sun animation is not reconstructed, while the existing camera animation remains intact
- CS0 through CS6 were exported with original timestamps, comments and changed paths, omitting account email and cloud workspace identifiers

The final static audit is recorded in [RELEASE_AUDIT.json](RELEASE_AUDIT.json). The batch render scope is also recorded in [UNITY_VALIDATION.txt](UNITY_VALIDATION.txt). Temporary capture/test scripts are not part of the shipped project.

## Scope and Remaining Work

These checks establish script import, compute execution, asset identity and successful still renders. They do not establish GPU timing, FPS, temporal quality under camera motion or cross-platform compatibility. There is no automated PlayMode suite for this renderer. The supplied original recording demonstrates an earlier interactive run, not a regression test of every release change.

Scene-depth-based temporal reprojection and shared camera history can produce motion artifacts. The empty-region step heuristic can miss fine density features. Height-curve texture updates allocate/upload every material update. Cloud shadows and multiple-scattering fill are approximations. These limitations are intentionally documented rather than labelled as SDF acceleration or physically complete atmospheric rendering.

Static credential-pattern scans reduce accidental exposure but cannot prove that every possible secret is absent. Authentication configuration, local editor caches, SCM workspace databases and raw export logs are excluded entirely.

## 中文

验证使用独立的公开发布副本、Unity 2022.3.62f2 和 Direct3D 11，原始 SCM 雪山场景没有被替换。

已完成脚本导入编译、实际 Compute Shader 连续生成测试、32,768 体素与非纯色检查、GUID 保留检查、随工程附带的 3D 纹理通道切片输出、四种调试视图渲染、Shader 错误检查及工具截图人工核对。必要的 Private 工程文件已补入 SCM 的 CS5；最终引用审查发现 Directional Light 的 Animator 控制器缺失，通过 Unity API 同步清除其丢失引用并禁用空 Animator，记录为 CS6。缺失的太阳动画没有重新制作，现有摄像机动画保持原样。公开历史导出包含 CS0 至 CS6，并移除账户邮箱与云工作区标识。

公开场景通过 Unity API 移除雪山源资产依赖。截图使用 Unity 自身的 GUI 渲染缓冲区生成，临时截图与测试脚本不纳入发布工程。最终文件审查结果见 RELEASE_AUDIT.json。

尚未测量 FPS 和 GPU 耗时，也未执行完整 PlayMode 回归、快速相机运动下的时间重建测试或跨平台验证。原始视频展示之前的交互运行效果，不作为最终改动的完整回归测试。启发式空区跳步、共享历史、曲线纹理更新以及近似散射与云影仍存在文档中说明的实现限制。
