

namespace homework3.Services;

public enum LogLevels
{
    Info,
    Warning,
    Error
}

public interface ICustomLogger
{
    void LogAction(string message, LogLevels level = LogLevels.Info);
}
