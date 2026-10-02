# Validation / 验证

## English

Checked with Unity 2022.3.62f2 and Direct3D 11:

- Runtime/editor scripts and cloud shaders imported without compilation errors
- Compute Shader baking produced a non-uniform 32 x 32 x 32 Texture3D; regenerating it preserved the asset GUID
- ShapeWorley128 and DetailWorley64 loaded and rendered through the slice-preview shader
- The included scene rendered Final, Alpha, Lighting and RaySteps outputs
- Empty-density renders verified reduced view-ray sampling with skipping enabled and unchanged diagnostic colors when atmosphere/history settings changed
- Coverage, sun-direction and noise-slice sequences were rendered with the actual project shaders

These checks do not cover runtime GPU timing, fast-motion temporal quality, a full PlayMode regression suite, XR or other graphics backends. Current rendering limitations are described in [README.md](../README.md), and development notes are in [DEVELOPMENT.md](../DEVELOPMENT.md).

## 简体中文

已在 Unity 2022.3.62f2／Direct3D 11 下检查脚本与 Shader 编译、非纯色 32³ 噪声生成、重复生成时的 GUID 保留、Shape／Detail 切片预览、四种相机调试输出、空区跳步与调试颜色隔离，并生成三段实际参数演示。

尚未覆盖 GPU 耗时、快速运动下的时间重建、完整 PlayMode 回归、XR 或其他图形后端。当前实现边界见 [README.md](../README.md)，开发记录见 [DEVELOPMENT.md](../DEVELOPMENT.md)。
