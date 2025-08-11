using Azure.Core;
using Azure.Identity;
using FunctionsTest1.Common;
using FunctionsTest1.Daos;
using FunctionsTest1.DAL.Models;
using FunctionsTest1.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionsTest1.Services
{

    public class ServiceListService(IAzureServiceDao _azureServiceDao, ILogger _logger)
    {

        public async Task SyncAzureServicesAsync(string tenantId, string clientId, string clientSecret)
        {
            var apiUrl = "https://management.azure.com/providers/Microsoft.Support/services?api-version=2024-04-01";
            _logger.Info($"Calling API: {apiUrl}");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var tokenRequestContext = new TokenRequestContext(["https://management.azure.com/.default"]);
            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            var apiRequest = new ApiRequestDto
            {
                Url = apiUrl,
                Method = HttpMethod.Get,
                BearerToken = accessToken.Token
            };
            var apiResponse = await ApiCommon.SendRequestAsync(apiRequest, _logger);
            if (!apiResponse.IsSuccess)
            {
                _logger.Error($"API call failed. Status: {apiResponse.StatusCode}");
                return;
            }

            try
            {
                // Newtonsoft.Json を使って DTO へデシリアライズ
                var serviceList = JsonConvert.DeserializeObject<AzureServiceListDto>(apiResponse.Content!);
                if (serviceList?.Value != null)
                {
                    var apiIds = new HashSet<string>();
                    foreach (var svc in serviceList.Value)
                    {
                        if (string.IsNullOrEmpty(svc.Id)) continue;
                        apiIds.Add(svc.Id);
                        var entity = new AzureService
                        {
                            Id = svc.Id,
                            Name = svc.Name ?? string.Empty,
                            Type = svc.Type ?? string.Empty,
                            DisplayName = svc.Properties?.DisplayName,
                            ResourceType = svc.Properties?.ResourceTypes != null ? string.Join(",", svc.Properties.ResourceTypes) : null
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
                _logger.Error($"DB登録処理で例外: {ex.Message}");
            }
        }
    }
}
