using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.Admin;

public class RegisterUserViewModel
{
    [Required]
    [Display(Name = "Imię")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [Display(Name = "Numer telefonu")]
    public string? PhoneNumber { get; set; }
    
    [Required]
    [Display(Name = "Rola")]
    public string Role { get; set; } = string.Empty;
}