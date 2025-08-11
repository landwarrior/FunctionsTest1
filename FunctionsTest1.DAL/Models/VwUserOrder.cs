namespace FunctionsTest1.DAL.Models;

public partial class VwUserOrder
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }
}
