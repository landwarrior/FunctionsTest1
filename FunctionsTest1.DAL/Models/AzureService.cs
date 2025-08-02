using System;
using System.Collections.Generic;

namespace FunctionsTest1.DAL.Models;

public partial class AzureService
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? DisplayName { get; set; }

    public string? ResourceType { get; set; }
}
