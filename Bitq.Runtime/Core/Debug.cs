namespace Bitq.Core;

public static class Debug
{
    public static Action<string, LogType> OnLog = delegate { };
    
    public static void Log(object message, LogType logType = LogType.Log)
    {
        var msg = $"{logType}: {message}";
        Console.WriteLine(msg);
        OnLog.Invoke(msg, logType);
    }

    public static void LogRaw(object message)
    {
        Console.WriteLine(message);
        OnLog.Invoke(message.ToString(), LogType.Log);
    }
}

public enum LogType
{
    Log,
    Warning,
    Error
}