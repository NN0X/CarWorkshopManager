using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.Customer;

public class CreateCustomerViewModel
{
    [Required] 
    [MaxLength(200)] 
    [Display(Name = "Imię")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [MaxLength(20)]
    [Display(Name = "Numer telefonu")]
    public string PhoneNumber { get; set; } = string.Empty;
}