using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RaySharp.Models
{
    public class Settings
    {
        public bool ShowAnimation { get; set; } = true;
        public int MaxSearchResults { get; set; } = 15;
        public string SearchTheme { get; set; } = "Default";
        public bool AlwaysOnTop { get; set; } = false;
        public List<string> EnabledPlugins { get; set; } = new List<string>();
        public List<string> PluginPaths { get; set; } = new List<string>();
        public Dictionary<string, object> PluginSettings { get; set; } = new Dictionary<string, object>();
        public string HotkeyModifier { get; set; } = "Control";
        public string HotkeyKey { get; set; } = "Space";
    }
}
