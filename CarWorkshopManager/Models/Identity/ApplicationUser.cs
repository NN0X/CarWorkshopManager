using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CarWorkshopManager.Models.Identity;

public class ApplicationUser : IdentityUser
{   
    [Required]
    [MaxLength(200)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string LastName { get; set; } = string.Empty;
} 