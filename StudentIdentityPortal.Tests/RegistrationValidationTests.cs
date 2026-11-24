using System.ComponentModel.DataAnnotations;
using StudentIdentityPortal.ViewModels;
using Xunit;

namespace StudentIdentityPortal.Tests;

public class RegistrationValidationTests
{
    [Fact]
    public void Valid_registration_data_passes_validation()
    {
        var model = new RegisterViewModel
        {
            FirstName = "Nazerke",
            LastName = "Student",
            Email = "nazerke@example.edu",
            Password = "Campus!2026",
            ConfirmPassword = "Campus!2026"
        };

        Assert.Empty(Validate(model));
    }

    [Theory]
    [InlineData("campus!2026")]
    [InlineData("CAMPUS!2026")]
    [InlineData("CampusOnly!")]
    [InlineData("Campus2026")]
    [InlineData("Camp!2")]
    public void Weak_password_is_rejected(string password)
    {
        var model = new RegisterViewModel
        {
            FirstName = "Nazerke",
            LastName = "Student",
            Email = "nazerke@example.edu",
            Password = password,
            ConfirmPassword = password
        };

        Assert.NotEmpty(Validate(model));
    }

    [Fact]
    public void Confirmation_must_match_password()
    {
        var model = new RegisterViewModel
        {
            FirstName = "Nazerke",
            LastName = "Student",
            Email = "nazerke@example.edu",
            Password = "Campus!2026",
            ConfirmPassword = "Different!2026"
        };

        Assert.Contains(Validate(model), error => error.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
