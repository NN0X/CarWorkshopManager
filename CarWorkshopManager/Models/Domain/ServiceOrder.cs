using System.ComponentModel.DataAnnotations;
using CarWorkshopManager.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarWorkshopManager.Models.Domain;

public class ServiceOrder
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int StatusId { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedById { get; set; } = string.Empty;

    [Required]
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    [MaxLength(200)] 
    public string CustomerNameSnapshot { get; set; } = string.Empty;

    [MaxLength(10)]
    public string RegistrationNumberSnapshot { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal TotalNet { get; set; }

    [Precision(18, 2)]
    public decimal TotalVat { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public OrderStatus Status { get; set; } = null!;
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<ServiceTask> Tasks { get; set; } = new List<ServiceTask>();
    public ICollection<OrderComment> Comments { get; set; } = new List<OrderComment>();
}
