# Esoteric Ebb Cheat Menu / 奥秘消退 修改器

一个用于 **Esoteric Ebb（奥秘消退）** 的 BepInEx 6 IL2CPP 游戏内 GUI 修改器。

An in-game GUI cheat menu for **Esoteric Ebb**, built as a BepInEx 6 IL2CPP plugin.

> 非官方玩家作品，与游戏开发者无关。本项目不包含任何游戏资源。
>
> Unofficial fan-made mod. Not affiliated with the game's developer. No game assets are included.

## Features / 功能

- **Resources 资源**
  - Crowns 金币：增加、设置为指定数值
  - Level-up points 升级点：增加、设置、清零
  - EXP 经验：增加、直接升级
  - HP 生命：回满、增减、修改最大生命
- **Attributes 属性**
  - STR / DEX / CON / INT / WIS / CHA 六维加减
  - 一键全 10 / 15 / 20 / 25
  - AC 护甲等级、Proficiency 熟练加值
- **Skills 技能**
  - 18 项技能数值加减
  - 熟练开关
  - 全部 +1 / +5
  - 按属性与熟练重算
- **Misc 其他**
  - `CheatEnabled`、`GodMode`、`JesusMode`
  - `FreecamCheat`、`OutlineOffMode`

## Requirements / 运行要求

- Windows
- Steam 版 **Esoteric Ebb / 奥秘消退**
- 已安装 **BepInEx 6 IL2CPP (x64)**
- 游戏使用 Unity 6（IL2CPP）

测试环境：

- Esoteric Ebb Steam build（2026-10）
- BepInEx `6.0.0-be.755`
- Unity `6000.1.17f1`

## Installation / 安装

1. 确认游戏目录下已安装 BepInEx 6 IL2CPP。
2. 新建目录：

   ```
   <游戏目录>\BepInEx\plugins\EsotericEbbCheatMenu\
   ```

3. 将 [`EsotericEbbCheatMenu.dll`](EsotericEbbCheatMenu.dll) 复制进去。
4. 启动游戏，按 `F8` 打开或关闭菜单。

目录结构示例：

```
Esoteric Ebb/
  BepInEx/
    plugins/
      EsotericEbbCheatMenu/
        EsotericEbbCheatMenu.dll
```

## Usage / 使用

- `F8`：打开 / 关闭窗口
- 拖动窗口顶部：移动窗口
- 页签：资源 / 属性 / 技能 / 其他
- 修改的是当前游戏内存中的数值；请在游戏里正常保存一次，数值才会写入存档。

## Build from source / 从源码构建

构建需要：

- .NET SDK（含 Roslyn 编译器，脚本会自动查找）
- .NET 6 reference pack（`Microsoft.NETCore.App.Ref 6.0.x`）
- 已安装的 Esoteric Ebb 游戏本体（用于读取 BepInEx 生成的程序集）

在 PowerShell 中运行：

```powershell
./build.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\Esoteric Ebb"
```

编译结果输出到 `EsotericEbbCheatMenu.dll`。

如果希望编译后直接安装到游戏目录：

```powershell
./build.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\Esoteric Ebb" -Deploy
```

## Notes / 说明

- 游戏的 Unity 构建裁剪了部分 IMGUI 方法（例如 `GUILayout.Space`、`GUILayout.Toggle`、`GUILayout.BeginScrollView`）。本插件只使用该构建中实际存在的方法，并用 `try/finally` 保证 GUI Clip 平衡。
- 修改器不会在启动时自动修改任何数值；只有点击按钮时才会写入内存。
- 如果升级点数值过高，游戏会因存在待分配升级点而自动弹出升级界面。使用“资源 → 升级点 → 清零”即可停止。
- 本项目只发布插件代码与编译产物，不包含任何游戏文件或 BepInEx 本体。

## License / 许可证

[MIT](LICENSE)

## Disclaimer / 免责声明

This project is an unofficial mod for single-player use. Use at your own risk. The author is not responsible for save corruption, game updates breaking the mod, or any other damage.

本项目为非官方单机修改插件，使用风险由使用者自行承担。
