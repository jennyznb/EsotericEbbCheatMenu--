# 发布到 GitHub / Publish to GitHub

本地 Git 仓库已经初始化并提交完成。

## 1. 新建空仓库

打开：

https://github.com/new?name=EsotericEbbCheatMenu&description=BepInEx+cheat+menu+for+Esoteric+Ebb

建议设置：

- Repository name：`EsotericEbbCheatMenu`
- 选择 `Public` 或 `Private`
- **不要**勾选 `Add a README file`
- **不要**选择 `.gitignore`
- **不要**选择 license

然后点击 **Create repository**。

## 2. 一键推送

在 PowerShell 中运行：

```powershell
cd C:\Users\JANE\Documents\Codex\2026-10-05\w\outputs\EsotericEbbCheatMenu
.\publish.ps1
```

脚本会提示你输入 GitHub 用户名，然后自动构造仓库地址：

```
https://github.com/<你的用户名>/EsotericEbbCheatMenu.git
```

脚本还会自动打开 GitHub 新建仓库页面。创建时请选择空仓库，**不要**勾选 README、.gitignore 或 license。

创建完成后回到 PowerShell 按回车，脚本会自动推送。第一次推送会弹出 Git Credential Manager，登录 GitHub 并授权即可。

如果仓库名不是 `EsotericEbbCheatMenu`，可以用 `-RepoName` 指定。

如果本地已经配置了 `origin` remote，脚本会直接使用它，不会重新询问。

推送完成后，仓库里会包含：

- `README.md`
- `LICENSE`
- `CHANGELOG.md`
- `src/CheatMenu.cs`
- `build.ps1`
- `dist/EsotericEbbCheatMenu.dll`
- `v1.2.0` 标签

## 如果 Git Credential Manager 没有弹窗

请在**普通 Windows PowerShell / Windows Terminal** 中运行（不要用 Codex 内置终端或沙箱终端）：

```powershell
git config --global credential.gitHubAuthModes device
cd C:\Users\JANE\Documents\Codex\2026-10-05\w\outputs\EsotericEbbCheatMenu
git push -u origin main
```

这时终端会直接显示一个设备代码，例如：

```
To complete authentication, open https://github.com/login/device and enter code XXXX-XXXX
```

用浏览器打开 `https://github.com/login/device`，输入终端里显示的代码并授权即可。

然后再推送标签：

```powershell
git push origin --tags
```

## 如果设备登录也不可用

可以改用 Personal Access Token（PAT）：

1. 在 GitHub 的 `Settings → Developer settings → Personal access tokens` 创建一个有 `repo` 权限的 token。
2. 在 PowerShell 中运行（把 `<TOKEN>` 换成你的 token）：

```powershell
git push https://<TOKEN>@github.com/jennyznb/EsotericEbbCheatMenu--.git main
git push https://<TOKEN>@github.com/jennyznb/EsotericEbbCheatMenu--.git --tags
```
