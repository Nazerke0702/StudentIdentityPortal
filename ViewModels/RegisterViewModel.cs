using System.ComponentModel.DataAnnotations;

namespace StudentIdentityPortal.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(60, MinimumLength = 2)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(60, MinimumLength = 2)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, DataType(DataType.EmailAddress)] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "Use upper and lower case letters, a number, and a symbol.")]
    public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
}
