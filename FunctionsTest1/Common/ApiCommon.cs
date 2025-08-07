using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Headers;
using System.Text.Json;
using FunctionsTest1.Dtos;

namespace FunctionsTest1.Common
{
    public class ApiCommon
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const int MaxRetries = 3;
        private const int DelayMilliseconds = 1000;

        public static async Task<ApiResponseDto> SendRequestAsync(ApiRequestDto requestDto)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(requestDto.Method, requestDto.Url);
                    if (!string.IsNullOrEmpty(requestDto.BearerToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestDto.BearerToken);
                    }
                    if (requestDto.Content != null)
                    {
                        request.Content = requestDto.Content;
                    }
                    var response = await _httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    return new ApiResponseDto
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content,
                        IsSuccess = response.IsSuccessStatusCode
                    };
                }
                catch (HttpRequestException ex) when (attempt < MaxRetries)
                {
                    await Task.Delay(DelayMilliseconds);
                }
                catch (TaskCanceledException) when (attempt < MaxRetries)
                {
                    await Task.Delay(DelayMilliseconds);
                }
            }
            return new ApiResponseDto { StatusCode = 0, Content = null, IsSuccess = false };
        }
    }
}
