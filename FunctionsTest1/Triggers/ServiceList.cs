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

            // content(JSON)をパースし、AzureServiceエンティティとしてDBに登録・更新
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("value", out var services))
                {
                    foreach (var svc in services.EnumerateArray())
                    {
                        var id = svc.GetProperty("id").GetString();
                        var name = svc.GetProperty("name").GetString();
                        var type = svc.GetProperty("type").GetString();
                        var displayName = svc.GetProperty("properties").TryGetProperty("displayName", out var dn) ? dn.GetString() : null;
                        var resourceTypes = svc.GetProperty("properties").TryGetProperty("resourceTypes", out var rt) && rt.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? string.Join(",", rt.EnumerateArray().Select(x => x.GetString()))
                            : null;

                        // 既存データの有無を確認
                        var entity = _dbContext.AzureServices.FirstOrDefault(e => e.Id == id);
                        if (entity == null)
                        {
                            entity = new DAL.Models.AzureService
                            {
                                Id = id ?? string.Empty,
                                Name = name ?? string.Empty,
                                Type = type ?? string.Empty,
                                DisplayName = displayName,
                                ResourceType = resourceTypes
                            };
                            _dbContext.AzureServices.Add(entity);
                        }
                        else
                        {
                            entity.Name = name ?? string.Empty;
                            entity.Type = type ?? string.Empty;
                            entity.DisplayName = displayName;
                            entity.ResourceType = resourceTypes;
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"DB登録処理で例外: {ex.Message}");
            }

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("API call completed. DB updated.");
            return res;
        }
    }
}
