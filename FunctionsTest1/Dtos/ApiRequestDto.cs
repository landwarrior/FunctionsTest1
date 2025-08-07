using System.Net.Http;

namespace FunctionsTest1.Dtos
{
    public class ApiRequestDto
    {
        public string? Url { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public string? BearerToken { get; set; }
        public HttpContent? Content { get; set; }
    }
}
