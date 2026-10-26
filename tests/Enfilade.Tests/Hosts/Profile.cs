using System.ComponentModel.DataAnnotations;

namespace Enfilade.Tests.Hosts;

public sealed class Profile
{
    [Required]
    [Display(Name = "Клиент", Description = "Как в договоре")]
    public string Customer { get; set; } = "";

    public string Note { get; set; } = "";

    public OrderStatus Status { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public DateOnly Day { get; set; }
}
