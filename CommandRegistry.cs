using RaySharp.Models;
using RaySharp.Services;
using System.Collections.Generic;
using System.Linq;

public static class CommandRegistry
{
    private static readonly List<ICommand> Commands = new();
    private static readonly List<ICommand> PluginCommands = new();

    public static void Register(ICommand command) => Commands.Add(command);

    public static void RegisterPlugin(ICommand command) => PluginCommands.Add(command);

    public static void ClearPlugins() => PluginCommands.Clear();

    public static IEnumerable<ICommand> Search(string query) =>
        Commands.Where(c => c.Matches(query))
        .Concat(PluginCommands.Where(c => c.Matches(query)));

    public static void InitDefaults()
    {
        Commands.Clear();
        Register(new OpenNotepadCommand());
        Register(new GoogleSearchCommand());
        Register(new OpenCalculatorCommand());
        Register(new PlaySongCommand());
        Register(new SettingsCommand());

        LoadEnabledPlugins();
    }

    private static void LoadEnabledPlugins()
    {
        var settings = SettingsManager.LoadSettings();
        var enabledPluginPaths = settings.PluginPaths.Where(p => settings.EnabledPlugins.Contains(p)).ToList();
        var pluginCommands = PluginLoader.LoadPlugins(enabledPluginPaths);

        ClearPlugins();
        foreach (var command in pluginCommands)
        {
            RegisterPlugin(command);
        }
    }
}