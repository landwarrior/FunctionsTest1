using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// builder.Services.AddLogging(loggingBuilder =>
// {
//     loggingBuilder.AddConsole(options =>
//     {
//         options.IncludeScopes = false;
//         options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
//     });
// });
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddProvider(new CustomLoggerProvider());
});

builder.Build().Run();
