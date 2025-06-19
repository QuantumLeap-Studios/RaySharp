using System;
using System.Diagnostics;

public class SettingsCommand : ICommand
{
    public string Name => "Settings";
    public string Description => "Configure RaySharp settings";

    public bool Matches(string query) =>
        query.StartsWith("settings", StringComparison.OrdinalIgnoreCase) ||
        query.StartsWith("config", StringComparison.OrdinalIgnoreCase) ||
        query.StartsWith("options", StringComparison.OrdinalIgnoreCase) ||
        query.Equals("settings", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query)
    {
        RaySharp.SettingsWindow settingsWindow = new RaySharp.SettingsWindow();
        settingsWindow.ShowDialog();
    }
    public void Awake()
    {
    }
}
