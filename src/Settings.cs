using System.IO;
using System.Text.Json;

namespace IconController
{
    public class Settings
    {
        public string Hotkey { get; set; } = "Ctrl+Alt+H";
        public bool AutoStart { get; set; }   = false;
        public bool ShowTrayIcon { get; set; }= true;
        public bool Enabled { get; set; }     = false;
        public bool HideIcons { get; set; }   = false;
        public bool FirstRun { get; set; }    = true;

        private static readonly string Dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IconController");
        private static readonly string FilePath = Path.Combine(Dir, "settings.json");

        public void Save()
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }

        public static Settings Load()
        {
            try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); }
            catch { return new(); }
        }
    }
}
