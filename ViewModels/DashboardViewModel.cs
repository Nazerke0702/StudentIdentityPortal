namespace StudentIdentityPortal.ViewModels;

public record DashboardViewModel(string DisplayName, string Email, string Role, DateTime CreatedAt, int StudentCount);
