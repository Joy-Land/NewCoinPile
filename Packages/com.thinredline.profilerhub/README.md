## 功能

- 将开发中使用的各种性能工具，集成到一起，方便使用。

## Issue链接

https://git.youle.game/TC/TAG/tag-documents/-/issues/274

## 依赖

- Sirenix Odin Inspector（鉴于目前项目中基本都安装了此插件，所以暂不建立unity package级别的依赖）
- Evolution.EvoCore（鉴于目前项目中基本都安装了此插件，所以暂不建立unity package级别的依赖）

## 规范参考

https://git.youle.game/TC/TAG/tag-documents/-/issues/25
https://git.youle.game/TC/TAG/tag-documents/-/issues/34
https://git.youle.game/gbf/client/Client/-/issues/489

## Package structure

```none
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── Third Party Notices.md
  ├── Editor
  │	  ├── (tool)SceneViewFovControl/*.*
  │   ├── EvoProfilerHubEditorCfg.cs
  │   └── Evolution.Profilerhub.Editor.asmdef
  ├── Runtime
  │	  ├── (tool)OverdrawMonitor/*.*
  │   ├── EvoProfilerHubCfg.cs
  │   └── Evolution.Profilerhub.asmdef
  ├── Tests
  │   ├── Editor
  │   │   ├── Evolution.Profilerhub.Editor.Tests.asmdef
  │   │   └── EditorExampleTest.cs
  │   └── Runtime
  │        ├── Evolution.Profilerhub.Tests.asmdef
  │        └── RuntimeExampleTest.cs
  ├── Samples
  │   └── Example
  │       └── SampleExample.cs
  └── Documentation
       ├── profilerhub.md
       └── Images
```