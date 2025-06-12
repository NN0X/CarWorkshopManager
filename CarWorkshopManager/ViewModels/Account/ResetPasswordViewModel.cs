using System.ComponentModel.DataAnnotations;

namespace CarWorkshopManager.ViewModels.Account;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Nowe hasło")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Potwierdź Hasło")]
    [Compare("Password", ErrorMessage = "Hasła nie są jednakowe.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
