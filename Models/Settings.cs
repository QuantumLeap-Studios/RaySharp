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
        public bool AlwaysOnTop { get; set; } = true;
        public List<string> EnabledPlugins { get; set; } = new List<string>();
        public List<string> PluginPaths { get; set; } = new List<string>();
        public Dictionary<string, object> PluginSettings { get; set; } = new Dictionary<string, object>();

        [JsonIgnore]
        public List<string> SearchPaths { get; set; } = new List<string>();
    }
}
