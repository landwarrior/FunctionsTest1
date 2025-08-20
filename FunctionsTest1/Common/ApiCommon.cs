using System.Net.Http.Headers;
using FunctionsTest1.Dtos;
using Microsoft.Extensions.Logging;

namespace FunctionsTest1.Common
{
    public class ApiCommon
    {
        private const int MaxRetries = 3;
        private const int DelayMilliseconds = 1000;

        /// <summary>
        /// APIリクエストを送信し、レスポンスを取得します。
        /// </summary>
        /// <param name="requestDto"></param>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        /// <returns></returns>
        public static async Task<ApiResponseDto> SendRequestAsync(ApiRequestDto requestDto, ILogger logger, IHttpClientFactory httpClientFactory)
        {
            using var httpClient = httpClientFactory.CreateClient();
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
                    var response = await httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();
                    return new ApiResponseDto
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content,
                        IsSuccess = response.IsSuccessStatusCode
                    };
                }
                catch (HttpRequestException ex)
                {
                    logger.Warn($"API アクセスに失敗しました。例外: {ex.Message}");
                    await Task.Delay(DelayMilliseconds);
                }
                catch (Exception ex)
                {
                    logger.Error($"予期しないエラーが発生しました。例外: {ex.Message}");
                    await Task.Delay(DelayMilliseconds);
                }
            }
            return new ApiResponseDto { StatusCode = 0, Content = null, IsSuccess = false };
        }
    }
}
