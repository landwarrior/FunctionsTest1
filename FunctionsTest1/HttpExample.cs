using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using My.Functions.Models;

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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        // デバッグレベルのログはどこにも出力されない（出力させる方法が分からない）
        _logger.LogDebug("This is a debug message.");
        // req は  Microsoft.AspNetCore.Http.DefaultHttpRequest って出力される
        _logger.LogInformation("C# HTTP trigger function processed a request. req: {req}", req);
        // req.QueryString は ?hoge=fuga みたいに出力される
        _logger.LogInformation("query string: {QueryString}", req.QueryString);
        // req.Method は GET や POST みたいに出力される
        _logger.LogInformation("method: {Method}", req.Method);
        // req.Path は /api/httpexample みたいに出力される
        _logger.LogInformation("path: {Path}", req.Path);
        // req.Body は Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestStream って出力される
        _logger.LogInformation("body: {req.Body}", req.Body);
        _logger.LogWarning("This is a warning message.");
        _logger.LogError("This is an error message.");

         // クエリパラメータで実行するアクションを指定
        var action = req.Query["action"].ToString().ToLower();
        List<Content> result = new();
        _logger.LogInformation("Executing action: {Action}", action);
        switch (action)
        {
            case "aitnewall":
                result = await Actions.AitNewAllAsync();
                break;
            case "aitranking":
                result = await Actions.AitRankingAsync();
                break;
            case "itmedianews":
                result = await Actions.ItmediaNewsAsync();
                break;
            case "qiita":
                result = await Actions.QiitaAsync();
                break;
            default:
                return new BadRequestObjectResult("Invalid action. Use: aitnewall, aitranking, itmedianews, qiita");
        }

        return new OkObjectResult(result);
    }
}
