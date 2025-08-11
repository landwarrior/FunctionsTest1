using Newtonsoft.Json;
using System.Collections.Generic;

namespace FunctionsTest1.Dtos
{
    public class AzureServiceDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("properties")]
        public AzureServicePropertiesDto Properties { get; set; } = new();
    }

    public class AzureServicePropertiesDto
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonProperty("resourceTypes")]
        public List<string> ResourceTypes { get; set; } = [];
    }

    public class AzureServiceListDto
    {
        [JsonProperty("value")]
        public List<AzureServiceDto> Value { get; set; } = [];
    }
}
