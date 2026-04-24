using homework3.Services;

public class ConsoleLogger : ICustomLogger
{
    public void LogAction(string message, LogLevels level = LogLevels.Info)
    {
        Console.ForegroundColor = level switch
        {
            LogLevels.Info => ConsoleColor.Cyan,
            LogLevels.Warning => ConsoleColor.Yellow,
            LogLevels.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        Console.Write($"[{level.ToString().ToUpper()}] {DateTime.Now:HH:mm:ss} ");

        Console.ResetColor();
        Console.WriteLine($"- {message}");
    }
}
