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
        public bool Enabled { get; set; } = true;

        private static readonly string Path = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DesktopToggle", "settings.json");

        public void Save()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, JsonSerializer.Serialize(this));
        }

        public static Settings Load()
        {
            try
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }
    }
}
