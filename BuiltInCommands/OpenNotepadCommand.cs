using System.Diagnostics;

public class OpenNotepadCommand : ICommand
{
    public string Name => "Notepad";
    public string Description => "Open Notepad";

    public bool Matches(string query) =>
        query.Equals("notepad", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query) =>
        Process.Start("notepad.exe");

    public void Awake()
    {
    }
}
