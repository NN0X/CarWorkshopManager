using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.Models.Domain;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [MaxLength(17)]
    public string Vin { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    public int VehicleBrandId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    public int ProductionYear { get; set; }

    [Range(0, 5000000)]
    public int Mileage { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
    public VehicleBrand VehicleBrand { get; set; } = null!;
}
