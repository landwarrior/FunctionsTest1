using Microsoft.Extensions.Logging;
using System;

public class CustomLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName);
    }

    public void Dispose() { }
}

public class CustomLogger : ILogger
{
    private readonly string _categoryName;

    public CustomLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        // ログレベルを11文字で左詰めにパディング
        string logLevelStr = $"{logLevel,-11}";
        var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevelStr}] | {formatter(state, exception)} ({_categoryName})";
        if (exception != null)
        {
            log += $"\nException: {exception}";
        }
        Console.WriteLine(log);
    }
}
