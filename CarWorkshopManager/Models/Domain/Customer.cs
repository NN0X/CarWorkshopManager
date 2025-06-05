using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.Models.Domain;

public class Customer
{
    public int Id { get; set; }
    
    [Required] 
    [MaxLength(200)]
    public string FirstName { get; set; } = String.Empty;
    
    [Required] 
    [MaxLength(200)]
    public string LastName { get; set; } = String.Empty;
    
    [Required] 
    [Phone] 
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = String.Empty;
    
    [Required] 
    [EmailAddress] 
    [MaxLength(200)]
    public string Email { get; set; } = String.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}