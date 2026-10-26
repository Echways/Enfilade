using System.ComponentModel.DataAnnotations;

namespace Enfilade.Tests.Hosts;

public sealed class OrderDraft
{
    [Required]
    public string Customer { get; set; } = "";

    public decimal Total { get; set; }
}
