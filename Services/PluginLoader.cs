using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RaySharp.Services
{
    public static class PluginLoader
    {
        public static List<ICommand> LoadPlugins(List<string> pluginPaths)
        {
            List<ICommand> loadedCommands = new List<ICommand>();

            foreach (string pluginPath in pluginPaths)
            {
                if (!File.Exists(pluginPath))
                    continue;

                try
                {
                    Assembly assembly = Assembly.LoadFrom(pluginPath);
                    
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            try
                            {
                                if (Activator.CreateInstance(type) is ICommand command)
                                {
                                    loadedCommands.Add(command);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return loadedCommands;
        }
    }
}
