using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using FunctionsTest1.Common;
using FunctionsTest1.Daos;
using FunctionsTest1.DAL.Models;
using FunctionsTest1.Dtos;
using Microsoft.Extensions.Logging;

namespace FunctionsTest1.Services
{
    public class ServiceListService
    {
        private readonly IAzureServiceDao _azureServiceDao;
        private readonly ILogger _logger;

        public ServiceListService(IAzureServiceDao azureServiceDao, ILogger logger)
        {
            _azureServiceDao = azureServiceDao;
            _logger = logger;
        }

        public async Task SyncAzureServicesAsync(string tenantId, string clientId, string clientSecret)
        {
            var apiUrl = "https://management.azure.com/providers/Microsoft.Support/services?api-version=2024-04-01";
            _logger.LogInformation($"Calling API: {apiUrl}");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            var apiRequest = new ApiRequestDto
            {
                Url = apiUrl,
                Method = System.Net.Http.HttpMethod.Get,
                BearerToken = accessToken.Token
            };
            var apiResponse = await ApiCommon.SendRequestAsync(apiRequest);
            if (!apiResponse.IsSuccess)
            {
                _logger.LogError($"API call failed. Status: {apiResponse.StatusCode}");
                return;
            }

            try
            {
                var json = JsonDocument.Parse(apiResponse.Content);
                if (json.RootElement.TryGetProperty("value", out var services))
                {
                    var apiIds = new HashSet<string>();
                    foreach (var svc in services.EnumerateArray())
                    {
                        var id = svc.GetProperty("id").GetString();
                        if (string.IsNullOrEmpty(id)) continue;
                        apiIds.Add(id);
                        var name = svc.GetProperty("name").GetString();
                        var type = svc.GetProperty("type").GetString();
                        var displayName = svc.GetProperty("properties").TryGetProperty("displayName", out var dn) ? dn.GetString() : null;
                        var resourceTypes = svc.GetProperty("properties").TryGetProperty("resourceTypes", out var rt) && rt.ValueKind == JsonValueKind.Array
                            ? string.Join(",", rt.EnumerateArray().Select(x => x.GetString()))
                            : null;
                        var entity = new AzureService
                        {
                            Id = id,
                            Name = name ?? string.Empty,
                            Type = type ?? string.Empty,
                            DisplayName = displayName,
                            ResourceType = resourceTypes
                        };
                        await _azureServiceDao.AddOrUpdateAsync(entity);
                    }
                    var dbIds = (await _azureServiceDao.GetAllAsync()).Select(e => e.Id).ToList();
                    var removeIds = dbIds.Except(apiIds).ToList();
                    if (removeIds.Count > 0)
                    {
                        await _azureServiceDao.RemoveByIdsAsync(removeIds);
                    }
                    await _azureServiceDao.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DB登録処理で例外: {ex.Message}");
            }
        }
    }
}
