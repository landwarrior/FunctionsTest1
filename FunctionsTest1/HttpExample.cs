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

    [Function("HttpExample")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogDebug("This is a debug message.");
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        _logger.LogWarning("This is a warning message.");
        _logger.LogError("This is an error message.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
