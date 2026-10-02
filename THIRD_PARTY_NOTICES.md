# References and Third-Party Content

## Reference Implementations

- [jiaozi158 / UnityVolumetricCloudsURP](https://github.com/jiaozi158/UnityVolumetricCloudsURP) was studied for cloud authoring, density/erosion controls, rendering architecture and visual comparison
- [jaagupku / volumetric-clouds](https://github.com/jaagupku/volumetric-clouds) was consulted during the earlier ray-marching study
- Unity URP shader libraries are used through the package manager rather than copied into this repository

This repository retains its own cloud component, editor tools and renderer-feature structure. Reference study does not imply equal algorithms, visual quality or performance. A reference MIT notice is included below for the jiaozi158 project; it does not grant a blanket licence for every file or external artwork in this repository.

## Excluded Presentation Assets

The local SCM project contains [Free Snow Mountain by ProAssets](https://assetstore.unity.com/packages/3d/environments/landscapes/free-snow-mountain-63002), including its textures, materials, prefab and example scene. The product page specifies the Standard Unity Asset Store EULA. Its [Appendix 1, sections 2.2.1 and 2.2.1.1](https://unity.com/legal/as-terms) permit embedded use in a qualifying original product but do not grant permission to publish the source assets as an unrestricted public download. The model and texture files are therefore excluded from the GitHub snapshot. The original user-provided demonstration recording still shows the mountain. This project does not claim authorship of that model. The public `SampleScene` removes its dependencies using Unity's scene and asset APIs.

Unity template tutorial assets and local user layouts are also excluded from the public snapshot. Unity packages retain their own licences and are restored using `Packages/manifest.json` and `packages-lock.json`.

## jiaozi158 Reference Licence

MIT License

Copyright (c) 2024 jiaozi158

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## 中文说明

开发中参考了 jiaozi158 与 jaagupku 的体积云项目，并通过 Unity Package Manager 使用 URP。本文保留参考出处，不把参考学习等同于算法、效果或性能完全一致。

本地 SCM 工程保留 ProAssets 的 Free Snow Mountain。该资源采用 Standard Unity Asset Store EULA；资源页标注免费，但并不授予公开再分发原始模型和贴图的权限。因此 GitHub 版本排除这些源资产，保留视频中的展示效果并注明来源。公开场景通过 Unity API 去除相应依赖，仍可独立运行体积云系统。

以上 MIT 文本为参考项目的许可声明，不是为全部工程文件或外部美术资源授予统一许可。
