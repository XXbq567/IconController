using System;
using System.IO;
using System.Text.Json;

namespace DesktopToggle
{
    public class Settings
    {
        public string Hotkey { get; set; } = "Ctrl+Alt+H";
        public bool AutoStart { get; set; } = false;
        public bool ShowTrayIcon { get; set; } = true;
        public bool Enabled { get; set; } = false;   // 首次默认关闭
        public bool HideIcons { get; set; } = false; // 首次默认不隐藏
        public bool FirstRun { get; set; } = true;   // 用来弹设置窗

        private static readonly string DirPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DesktopToggle");

        private static readonly string FilePath = Path.Combine(DirPath, "settings.json");

        public void Save()
        {
            Directory.CreateDirectory(DirPath);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }

        public static Settings Load()
        {
            try
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }
    }
}
