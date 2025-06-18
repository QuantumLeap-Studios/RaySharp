using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class FileSearchCommand : ICommand
{
    public string Name => "Find File";
    public string Description => "Search for files on your computer (f <filename>)";

    public bool Matches(string query) =>
        query.StartsWith("f ", StringComparison.OrdinalIgnoreCase) ||
        query.StartsWith("file ", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query)
    {
        // WIP: No operation performed
    }
}
