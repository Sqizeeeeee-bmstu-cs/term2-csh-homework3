



using homework3.Services;

public class ConsoleLogger : ICustomLogger
{
    public void LogAction(string message, LogLevels level)
    {
        Console.BackgroundColor = level switch
        {
            LogLevels.Info => ConsoleColor.Blue,
            LogLevels.Warning => ConsoleColor.Yellow,
            LogLevels.Error => ConsoleColor.Red,
            _ => ConsoleColor.Black
        };

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{level.ToString().ToUpper()}] {DateTime.Now:HH:mm:ss} - {message}");

        Console.ResetColor();
    }
}
