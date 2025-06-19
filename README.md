![RaySharp Logo](https://raw.githubusercontent.com/QuantumLeap-Studios/RaySharp/master/Media/RaysharpBig.png)

RaySharp is a powerful launcher application for Windows that provides quick access to applications, files, and custom commands through a sleek and customizable interface. Similar to other launchers like Alfred for Mac or Spotlight, RaySharp aims to boost your productivity by providing a fast way to search and execute commands.

## Features

- **Quick Access**: Launch applications, search files, and execute commands with just a few keystrokes
- **Built-in Commands**: Search the web, open applications, perform calculations, and more
- **File Search**: Quickly find files across your system
- **Themes**: Choose between Default, Light, and Dark themes
- **Plugins**: Extend functionality with custom plugins
- **Math Calculations**: Perform quick calculations directly in the search bar

### Requirements

- Windows 10 or later
- .NET 8.0 Runtime

### Configuration

RaySharp stores its settings in `%LocalAppData%\RaySharp\settings.json`. The application manages this file, but advanced users can modify it directly when the application is closed.

## Usage

1. Press `Ctrl+Space` (default hotkey) to open RaySharp
2. Type your query or command
3. Use arrow keys to navigate through results
4. Press Enter to execute the selected command

### Built-in Commands

- `g <query>` or `google <query>`: Search Google
- `open notepad`: Open Notepad
- `calc` or `calculator`: Open Calculator
- `f <filename>` or `file <filename>`: Search for files
- `settings`: Open RaySharp settings
- `<math expression>`: Calculate mathematical expressions

## Creating Custom Plugins

RaySharp supports plugins that implement the `ICommand` interface. Here's how to create your own:

### 1. Create a new Class Library Project


```
dotnet new classlib -n YourPluginName

```

### 2. Add a reference to RaySharp's interfaces

Either reference the main RaySharp assembly or create the required interface:


```cs
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    bool Matches(string query);
    void Execute(string query);
}

```

### 3. Implement the ICommand interface


```cs
using System;

namespace YourPluginNamespace
{
    public class YourCustomCommand : ICommand
    {
        public string Name => "Your Command Name";
        public string Description => "Description of what your command does";

        public bool Matches(string query)
        {
            // Define when your command should be shown in results
            // For example, if command should be triggered by "yourcommand":
            return query.StartsWith("yourcommand", StringComparison.OrdinalIgnoreCase);
        }

        public void Execute(string query)
        {
            // Implement what happens when your command is executed
            // For example:
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.example.com",
                UseShellExecute = true
            });
        }
    }
}

```

### 4. Build your plugin

Build your project to create a DLL file.

### 5. Add your plugin to RaySharp

1. Open RaySharp
2. Type "settings" and press Enter
3. Click "Import Plugin" button
4. Browse to and select your plugin DLL
5. Ensure the plugin is enabled by checking the checkbox next to it
6. Click "Save"

Your plugin should now be available in RaySharp!

## Advanced Plugin Development

### Command Icons

RaySharp automatically assigns icons to common commands. You can take advantage of this by using specific command names:

- "google" or "g": 🌐
- "notepad": 📝
- "calculator" or "calc": 🧮
- "file" or "f": 📄
- "settings": ⚙️
- etc.

### Plugin Settings

You can store and retrieve plugin-specific settings using the PluginSettings dictionary in the Settings class:


```cs
// Get settings from RaySharp.Models.Settings class
var settings = SettingsManager.LoadSettings();

// Store plugin settings
settings.PluginSettings["YourPluginName.SomeSetting"] = "value";
SettingsManager.SaveSettings(settings);

// Retrieve plugin settings
var value = settings.PluginSettings.TryGetValue("YourPluginName.SomeSetting", out var settingValue) 
    ? settingValue.ToString() 
    : "default";

```

## Troubleshooting

### Plugin Not Loading

- Ensure your plugin DLL implements the ICommand interface correctly
- Verify that the plugin is enabled in Settings
- Check that the plugin file exists in the location specified in settings
- Make sure your plugin is built with a compatible .NET version

### Settings Not Saving

- Ensure RaySharp has write permissions to `%LocalAppData%\RaySharp`
- Try running RaySharp as administrator if problems persist

## Contributing

Contributions are welcome! Feel free to fork the repository, make changes, and submit pull requests.

## License

[MIT License](LICENSE)
