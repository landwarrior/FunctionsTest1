using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Identity;
using FunctionsTest1.Common;
using FunctionsTest1.DAL.Contexts;

namespace FunctionsTest1.Triggers
{
    public class ServiceList
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly TestDbContext _dbContext;

        public ServiceList(ILoggerFactory loggerFactory, TestDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<ServiceList>();
            _dbContext = dbContext;
        }

        [Function("ServiceList")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var apiUrl = "https://management.azure.com/providers/Microsoft.Support/services?api-version=2024-04-01";
            _logger.Info($"Calling API: {apiUrl}");

            // 環境変数や設定から認証情報を取得
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // Azure Management API 用のスコープ
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // APIレスポンス(JSON)をパースし、DBのAzureServiceテーブルと同期する
            try
            {
                // レスポンスJSONをパース
                var json = System.Text.Json.JsonDocument.Parse(content);
                // "value"配列にサービス情報が格納されている
                if (json.RootElement.TryGetProperty("value", out var services))
                {
                    // APIレスポンスに含まれるid一覧を保持
                    var apiIds = new HashSet<string>();
                    // 各サービス情報をDBへ登録・更新
                    foreach (var svc in services.EnumerateArray())
                    {
                        // サービスのid, name, type, displayName, resourceTypesを取得
                        var id = svc.GetProperty("id").GetString();
                        // idがnullまたは空文字の場合は以降の処理をスキップ
                        if (string.IsNullOrEmpty(id))
                        {
                            continue;
                        }
                        apiIds.Add(id);
                        var name = svc.GetProperty("name").GetString();
                        var type = svc.GetProperty("type").GetString();
                        var displayName = svc.GetProperty("properties").TryGetProperty("displayName", out var dn) ? dn.GetString() : null;
                        var resourceTypes = svc.GetProperty("properties").TryGetProperty("resourceTypes", out var rt) && rt.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? string.Join(",", rt.EnumerateArray().Select(x => x.GetString()))
                            : null;

                        // DBに既存データがあるか確認
                        var entity = _dbContext.AzureServices.FirstOrDefault(e => e.Id == id);
                        if (entity == null)
                        {
                            // 新規の場合は追加
                            entity = new DAL.Models.AzureService
                            {
                                Id = id,
                                Name = name ?? string.Empty,
                                Type = type ?? string.Empty,
                                DisplayName = displayName,
                                ResourceType = resourceTypes
                            };
                            _dbContext.AzureServices.Add(entity);
                        }
                        else
                        {
                            // 既存の場合は内容を更新
                            entity.Name = name ?? string.Empty;
                            entity.Type = type ?? string.Empty;
                            entity.DisplayName = displayName;
                            entity.ResourceType = resourceTypes;
                        }
                    }

                    // DBに存在するidのうち、APIレスポンスに含まれないものを抽出
                    var dbIds = _dbContext.AzureServices.Select(e => e.Id).ToList(); // DB内の全id一覧
                    var removeIds = dbIds.Except(apiIds).ToList(); // 削除対象id一覧
                    if (removeIds.Count > 0)
                    {
                        // 削除対象idに該当するエンティティを取得し、まとめて削除
                        var removeEntities = _dbContext.AzureServices.Where(e => removeIds.Contains(e.Id)).ToList();
                        _dbContext.AzureServices.RemoveRange(removeEntities);
                    }

                    // 追加・更新・削除をDBへ反映
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // 例外発生時はエラーログ出力
                _logger.Error($"DB登録処理で例外: {ex.Message}");
            }

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("API call completed. DB updated.");
            return res;
        }
    }
}
