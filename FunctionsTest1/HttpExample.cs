using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace My.Functions;

public class HttpExample
{
    private readonly ILogger<HttpExample> _logger;

    public HttpExample(ILogger<HttpExample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// ここに処理を書けばいいのかもしれない。
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("HttpExample")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        // デバッグレベルのログはどこにも出力されない（出力させる方法が分からない）
        _logger.LogDebug("This is a debug message.");
        // req は  Microsoft.AspNetCore.Http.DefaultHttpRequest って出力される
        _logger.LogInformation("C# HTTP trigger function processed a request. req: {req}", req);
        // req.QueryString は ?hoge=fuga みたいに出力される
        _logger.LogInformation("query string: {req.QueryString}", req.QueryString);
        // req.Method は GET や POST みたいに出力される
        _logger.LogInformation("method: {req.Method}", req.Method);
        // req.Path は /api/httpexample みたいに出力される
        _logger.LogInformation("path: {req.Path}", req.Path);
        // req.Body は Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestStream って出力される
        _logger.LogInformation("body: {req.Body}", req.Body);
        _logger.LogWarning("This is a warning message.");
        _logger.LogError("This is an error message.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
