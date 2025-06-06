using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.Models.Domain;

public class Customer
{
    public int Id { get; set; }
    
    [Required] 
    [MaxLength(200)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required] 
    [MaxLength(200)]
    public string LastName { get; set; } = string.Empty;
    
    [Required] 
    [Phone] 
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required] 
    [EmailAddress] 
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}