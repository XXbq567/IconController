# Desktop Icon Toggle for Windows 11

轻量级托盘工具，一键快捷键显示/隐藏桌面图标。

- 下载即用：单文件 EXE，无需安装  
- 快捷键可自定义，响应 < 100 ms  
- 支持开机自启、托盘图标显隐  
- 低饱和度灰 + 淡蓝主题  

## 使用
1. 在 [Releases](https://github.com/你的用户名/DesktopToggle/releases) 下载 `DesktopToggle.exe`  
2. 运行后托盘图标 → 双击打开设置  
3. 修改快捷键、自启或显隐托盘图标 → 保存即可

## 快捷键格式
`Ctrl+Alt+H`、`Ctrl+Shift+F12` … 大小写不敏感。

## 编译
本地：`dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`  
GitHub：推送 `v1.0.0` 标签即自动发布。
