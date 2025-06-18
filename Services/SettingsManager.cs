using RaySharp.Models;
using System;
using System.IO;
using System.Text.Json;

namespace RaySharp.Services
{
    public static class SettingsManager
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RaySharp");
        
        private static readonly string SettingsFilePath = Path.Combine(AppDataPath, "settings.json");
        private static Settings _cachedSettings;

        public static Settings LoadSettings()
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                    return _cachedSettings;
                }
                catch (Exception)
                {
                    _cachedSettings = new Settings();
                    return _cachedSettings;
                }
            }

            _cachedSettings = new Settings();
            SaveSettings(_cachedSettings);       
            return _cachedSettings;
        }

        public static void SaveSettings(Settings settings)
        {
            try
            {
                _cachedSettings = settings;
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
            }
        }
        
        public static string GetPluginDirectory()
        {
            string pluginDir = Path.Combine(AppDataPath, "Plugins");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
                
            return pluginDir;
        }
    }
}
