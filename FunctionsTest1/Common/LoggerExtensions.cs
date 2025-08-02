using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FunctionsTest1.Common
{

    /// <summary>
    /// ログ出力の拡張メソッドを提供するクラスです。
    /// ログ出力の際に、呼び出し元のファイル名・メソッド名・行番号を含めて出力することができます。
    /// このクラスをどこかで using しておくと、ILogger のメソッドを呼び出す際に、呼び出し元のファイル名・メソッド名・行番号を含めてログ出力することができるらしい。
    /// 明示的に使用するように書いてないけど、不思議～
    /// なお、 namespace が同じなので using も書いてない。
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 呼び出し元のファイル名・メソッド名・行番号を含めてログ出力します。
        /// </summary>
        /// <param name="logger">ILogger インスタンス</param>
        /// <param name="logLevel">ログレベル</param>
        /// <param name="message">ログメッセージ</param>
        /// <param name="category">カテゴリ（任意）</param>
        /// <param name="exception">例外（任意）</param>
        /// <param name="filePath">呼び出し元ファイルパス（自動取得）</param>
        /// <param name="memberName">呼び出し元メソッド名（自動取得）</param>
        /// <param name="lineNumber">呼び出し元行番号（自動取得）</param>
        private static void LogWithCallerInfo(
            this ILogger logger,
            LogLevel logLevel,
            string message,
            string? category = null,
            Exception? exception = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var fileName = Path.GetFileName(filePath);
            var cat = !string.IsNullOrEmpty(category) ? $"({category}) " : "";
            // ログレベルを11文字で左詰めにパディング
            string logLevelStr = $"{logLevel,-11}";
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"{now} [{logLevelStr}] | {cat}{message} [in {fileName}::{memberName}:{lineNumber}]";
            logger.Log(logLevel, exception, logMessage);
        }

        /// <summary>
        /// Debugレベルのログを呼び出し元情報付きで出力します。
        /// </summary>
        /// <param name="logger">ILogger インスタンス</param>
        /// <param name="message">ログメッセージ</param>
        /// <param name="category">カテゴリ（任意）</param>
        /// <param name="filePath">呼び出し元ファイルパス（自動取得）</param>
        /// <param name="memberName">呼び出し元メソッド名（自動取得）</param>
        /// <param name="lineNumber">呼び出し元行番号（自動取得）</param>
        public static void Debug(this ILogger logger, string message, string? category = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
            => logger.LogWithCallerInfo(LogLevel.Debug, message, category, null, filePath, memberName, lineNumber);

        /// <summary>
        /// Infoレベルのログを呼び出し元情報付きで出力します。
        /// </summary>
        /// <param name="logger">ILogger インスタンス</param>
        /// <param name="message">ログメッセージ</param>
        /// <param name="category">カテゴリ（任意）</param>
        /// <param name="filePath">呼び出し元ファイルパス（自動取得）</param>
        /// <param name="memberName">呼び出し元メソッド名（自動取得）</param>
        /// <param name="lineNumber">呼び出し元行番号（自動取得）</param>
        public static void Info(this ILogger logger, string message, string? category = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
            => logger.LogWithCallerInfo(LogLevel.Information, message, category, null, filePath, memberName, lineNumber);

        /// <summary>
        /// Warnレベルのログを呼び出し元情報付きで出力します。
        /// </summary>
        /// <param name="logger">ILogger インスタンス</param>
        /// <param name="message">ログメッセージ</param>
        /// <param name="category">カテゴリ（任意）</param>
        /// <param name="filePath">呼び出し元ファイルパス（自動取得）</param>
        /// <param name="memberName">呼び出し元メソッド名（自動取得）</param>
        /// <param name="lineNumber">呼び出し元行番号（自動取得）</param>
        public static void Warn(this ILogger logger, string message, string? category = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
            => logger.LogWithCallerInfo(LogLevel.Warning, message, category, null, filePath, memberName, lineNumber);

        /// <summary>
        /// Errorレベルのログを呼び出し元情報付きで出力します。
        /// </summary>
        /// <param name="logger">ILogger インスタンス</param>
        /// <param name="message">ログメッセージ</param>
        /// <param name="exception">例外（任意）</param>
        /// <param name="category">カテゴリ（任意）</param>
        /// <param name="filePath">呼び出し元ファイルパス（自動取得）</param>
        /// <param name="memberName">呼び出し元メソッド名（自動取得）</param>
        /// <param name="lineNumber">呼び出し元行番号（自動取得）</param>
        public static void Error(this ILogger logger, string message, Exception? exception = null, string? category = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
            => logger.LogWithCallerInfo(LogLevel.Error, message, category, exception, filePath, memberName, lineNumber);
    }
}
