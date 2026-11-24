using System.ComponentModel.DataAnnotations;

namespace StudentIdentityPortal.ViewModels;

public class ProfileViewModel
{
    [Required, StringLength(60, MinimumLength = 2)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(60, MinimumLength = 2)] public string LastName { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
