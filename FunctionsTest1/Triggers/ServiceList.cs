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
        private readonly Services.ServiceListService _serviceListService;

        public ServiceList(ILoggerFactory loggerFactory, Daos.IAzureServiceDao azureServiceDao)
        {
            _logger = loggerFactory.CreateLogger<ServiceList>();
            _serviceListService = new Services.ServiceListService(azureServiceDao, _logger);
        }

        [Function("ServiceList")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            // 環境変数や設定から認証情報を取得
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                var errorRes = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorRes.WriteStringAsync("Azure認証情報が設定されていません。");
                return errorRes;
            }

            await _serviceListService.SyncAzureServicesAsync(tenantId, clientId, clientSecret);

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("API call completed. DB updated.");
            return res;
        }
    }
}
