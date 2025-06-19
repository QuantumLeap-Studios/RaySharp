using System;
using System.Diagnostics;

public class OpenCalculatorCommand : ICommand
{
    public string Name => "Calculator";
    public string Description => "Open Calculator";

    public bool Matches(string query) =>
        query.Equals("calc", StringComparison.OrdinalIgnoreCase) ||
        query.Equals("calculator", StringComparison.OrdinalIgnoreCase);

    public void Execute(string query) =>
        Process.Start("calc.exe");
    public void Awake()
    {
    }
}
