using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// DbContextのDI登録（接続文字列は環境変数やlocal.settings.jsonから取得）
builder.Services.AddDbContext<FunctionsTest1.DAL.Contexts.TestDbContext>(options =>
{
    // local.settings.jsonのValuesまたは環境変数から接続文字列を取得
    var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("接続文字列(DefaultConnection)が設定されていません。");
    }
    options.UseSqlServer(connectionString);
});

// 標準的なコンソールログ機能の設定
// builder.Services.AddLogging(loggingBuilder =>
// {
//     loggingBuilder.AddConsole(options =>
//     {
//         // 新しいAPIを使用
//         options.FormatterName = "SimpleConsole";
//     });
//      // フォーマッターの設定を追加
//     loggingBuilder.AddSimpleConsole(options =>
//     {
//         options.IncludeScopes = false;
//         options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
//     });
// });
// カスタムロガーのプロバイダーを追加する場合は、以下のように追加するが使わなくなったのでコメントアウト
// builder.Services.AddLogging(loggingBuilder =>
// {
//     loggingBuilder.AddProvider(new CustomLoggerProvider());
// });

builder.Build().Run();
