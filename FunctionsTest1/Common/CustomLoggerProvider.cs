using Microsoft.Extensions.Logging;

namespace FunctionsTest1.Common
{
    // このクラスは現在使用していません

    public class CustomLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
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
            // これだと Application Insights には Information レベルとして記録されてしまう
            Console.WriteLine(log);
            // ILogger 経由で Application Insights に送ったログはフォーマットできないクソ仕様
            // しかも Warning と Error はなぜかダブっている
            // 以下みたいな感じで確認できた
            // 2025-06-24T12:49:33Z   [Warning]   This is a warning message.
            // 2025-06-24T12:49:33Z   [Error]   This is an error message.
            // 2025-06-24T12:49:33Z   [Information]   Executing 'Functions.HttpExample' (Reason='This function was programmatically called via the host APIs.', Id=cb4fe2a0-5b06-42f3-9368-821ec695f849)
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | Request starting HTTP/1.1 GET http://localhost:50021/api/httpexample - - - (Microsoft.AspNetCore.Hosting.Diagnostics)
            // 2025-06-24T12:49:33Z   [Information]   C# HTTP trigger function processed a request.
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | Executing endpoint 'HttpExample' (Microsoft.AspNetCore.Routing.EndpointMiddleware)
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | C# HTTP trigger function processed a request. (My.Functions.HttpExample)
            // 2025-06-24T12:49:33Z   [Warning]   This is a warning message.
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Warning    ] | This is a warning message. (My.Functions.HttpExample)
            // 2025-06-24T12:49:33Z   [Error]   This is an error message.
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Error      ] | This is an error message. (My.Functions.HttpExample)
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | Executing OkObjectResult, writing value of type 'System.String'. (Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor)
            // 2025-06-24T12:49:33Z   [Information]   Executing OkObjectResult, writing value of type 'System.String'.
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | Executed endpoint 'HttpExample' (Microsoft.AspNetCore.Routing.EndpointMiddleware)
            // 2025-06-24T12:49:33Z   [Information]   2025-06-24 12:49:32 [Information] | Request finished HTTP/1.1 GET https://functionstest1.azurewebsites.net/api/httpexample - 200 - text/plain;+charset=utf-8 24.6867ms (Microsoft.AspNetCore.Hosting.Diagnostics)
            // 2025-06-24T12:49:33Z   [Information]   Executed 'Functions.HttpExample' (Succeeded, Id=cb4fe2a0-5b06-42f3-9368-821ec695f849, Duration=29ms)

            // ただし、AWS の CloudWatch Logs みたいにいつでも確認する方法が分からない
            // ブラウザで FunctionsTest1 のログストリームを開きながら Functions の URL にアクセスすると確認できる
        }
    }
}
