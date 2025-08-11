namespace FunctionsTest1.DAL.Models;

public partial class AzureService
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? DisplayName { get; set; }

    public string? ResourceType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreateUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdateUser { get; set; }
}
