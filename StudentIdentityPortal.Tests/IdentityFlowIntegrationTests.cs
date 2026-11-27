using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StudentIdentityPortal.Constants;
using StudentIdentityPortal.Models;
using Xunit;

namespace StudentIdentityPortal.Tests;

public class IdentityFlowIntegrationTests(PortalFactory factory) : IClassFixture<PortalFactory>
{
    private const string StudentPassword = "Campus!Student2026";
    private static readonly string AdminEmail = $"test-admin-{Guid.NewGuid():N}@example.test";
    private static readonly string AdminPassword = $"Aa1!{Guid.NewGuid():N}";

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    [Fact]
    public async Task Anonymous_users_can_open_public_pages()
    {
        using var client = CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/Register")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/Login")).StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_is_redirected_from_protected_pages_to_login()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login", LocationPath(response));
    }

    [Fact]
    public async Task Valid_registration_creates_user_and_signs_in()
    {
        using var client = CreateClient();
        var response = await RegisterAsync(client, Email());
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Dashboard", LocationPath(response));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Dashboard")).StatusCode);
    }

    [Fact]
    public async Task Invalid_registration_does_not_create_a_user()
    {
        using var client = CreateClient();
        var token = await GetTokenAsync(client, "/Account/Register");
        var response = await client.PostAsync("/Account/Register", Form(token,
            ("FirstName", ""), ("LastName", ""), ("Email", "not-an-email"),
            ("Password", "weak"), ("ConfirmPassword", "different")));
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field-error", html);
        Assert.Contains("The FirstName field is required.", html);
    }

    [Fact]
    public async Task Registration_assigns_only_the_student_role()
    {
        using var client = CreateClient();
        var email = Email();
        await RegisterAsync(client, email);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(new[] { RoleNames.Student }, (await users.GetRolesAsync(user!)).OrderBy(x => x));
    }

    [Fact]
    public async Task Forged_admin_role_field_is_ignored_during_registration()
    {
        using var client = CreateClient();
        var email = Email();
        var token = await GetTokenAsync(client, "/Account/Register");
        var response = await client.PostAsync("/Account/Register", Form(token,
            ("FirstName", "Forged"), ("LastName", "Student"), ("Email", email),
            ("Password", StudentPassword), ("ConfirmPassword", StudentPassword), ("Role", RoleNames.Admin)));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.False(await users.IsInRoleAsync(user!, RoleNames.Admin));
        Assert.True(await users.IsInRoleAsync(user!, RoleNames.Student));
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_without_creating_another_user()
    {
        using var client = CreateClient();
        var email = Email();
        await RegisterAsync(client, email);
        await client.PostAsync("/Account/Logout", Form(await GetTokenAsync(client, "/Dashboard")));
        var token = await GetTokenAsync(client, "/Account/Register");
        var response = await client.PostAsync("/Account/Register", Form(token,
            ("FirstName", "Duplicate"), ("LastName", "Account"), ("Email", email),
            ("Password", StudentPassword), ("ConfirmPassword", StudentPassword)));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Single(await users.GetUsersInRoleAsync(RoleNames.Student), x => x.Email == email);
    }

    [Fact]
    public async Task Invalid_password_and_unknown_user_cannot_sign_in()
    {
        using var client = CreateClient();
        var email = Email();
        await RegisterAsync(client, email);
        await client.PostAsync("/Account/Logout", Form(await GetTokenAsync(client, "/Dashboard")));
        foreach (var (loginEmail, password) in new[] { (email, "Wrong!Password2026"), (Email(), StudentPassword) })
        {
            var token = await GetTokenAsync(client, "/Account/Login");
            var response = await client.PostAsync("/Account/Login", Form(token,
                ("Email", loginEmail), ("Password", password), ("RememberMe", "false")));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("The email or password was not recognized.", await response.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task Student_can_sign_in_and_open_student_pages()
    {
        using var client = CreateClient();
        var email = Email();
        await RegisterAsync(client, email);
        await client.PostAsync("/Account/Logout", Form(await GetTokenAsync(client, "/Dashboard")));
        var token = await GetTokenAsync(client, "/Account/Login");
        var response = await client.PostAsync("/Account/Login", Form(token,
            ("Email", email), ("Password", StudentPassword), ("RememberMe", "false")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Students")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Profile")).StatusCode);
    }

    [Fact]
    public async Task Student_is_denied_admin_dashboard_by_direct_url()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Email());
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", LocationPath(response));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
    }

    [Fact]
    public async Task Student_cannot_submit_admin_action_by_direct_post()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Email());
        var token = await GetTokenAsync(client, "/Dashboard");
        var response = await client.PostAsync("/Admin/DeleteStudent", Form(token, ("id", "not-a-student")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", LocationPath(response));
    }

    [Fact]
    public async Task Admin_can_open_admin_dashboard()
    {
        using var client = CreateClient();
        await SignInAsAdminAsync(client);
        var response = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("PORTAL MANAGEMENT", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_can_use_antiforgery_protected_student_removal()
    {
        using var client = CreateClient();
        var studentEmail = Email();
        await RegisterAsync(client, studentEmail);
        await client.PostAsync("/Account/Logout", Form(await GetTokenAsync(client, "/Dashboard")));
        await SignInAsAdminAsync(client);
        var adminPage = await client.GetAsync("/Admin");
        var html = await adminPage.Content.ReadAsStringAsync();
        var token = ExtractToken(html);
        var userId = await FindUserIdAsync(studentEmail);
        var response = await client.PostAsync("/Admin/DeleteStudent", Form(token, ("id", userId)));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin", response.Headers.Location?.OriginalString);
        Assert.Null(await FindUserAsync(studentEmail));
    }

    [Fact]
    public async Task Logout_get_does_not_sign_the_user_out()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Email());
        var response = await client.GetAsync("/Account/Logout");
        Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Dashboard")).StatusCode);
    }

    [Fact]
    public async Task Logout_post_with_antiforgery_signs_out_and_protected_page_requires_login()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Email());
        var token = await GetTokenAsync(client, "/Dashboard");
        var response = await client.PostAsync("/Account/Logout", Form(token));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", LocationPath(response));
        var protectedResponse = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.StartsWith("/Account/Login", LocationPath(protectedResponse));
    }

    [Fact]
    public async Task Logout_post_without_antiforgery_token_is_rejected_without_signing_out()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Email());
        var response = await client.PostAsync("/Account/Logout", Form(null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Dashboard")).StatusCode);
    }

    [Fact]
    public async Task Login_rejects_external_return_url()
    {
        using var client = CreateClient();
        var email = Email();
        await RegisterAsync(client, email);
        await client.PostAsync("/Account/Logout", Form(await GetTokenAsync(client, "/Dashboard")));
        var token = await GetTokenAsync(client, "/Account/Login");
        var response = await client.PostAsync("/Account/Login", Form(token,
            ("Email", email), ("Password", StudentPassword), ("RememberMe", "false"), ("ReturnUrl", "https://evil.example")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Dashboard", LocationPath(response));
        Assert.False((response.Headers.Location?.OriginalString ?? string.Empty).StartsWith("https://evil.example", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Registration_requires_antiforgery_token()
    {
        using var client = CreateClient();
        var response = await client.PostAsync("/Account/Register", Form(null,
            ("FirstName", "No"), ("LastName", "Token"), ("Email", Email()),
            ("Password", StudentPassword), ("ConfirmPassword", StudentPassword)));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SignInAsAdminAsync(HttpClient client)
    {
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(AdminEmail);
            if (user is null)
            {
                user = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true, FirstName = "Test", LastName = "Admin" };
                var result = await users.CreateAsync(user, AdminPassword);
                Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
                var assigned = await users.AddToRoleAsync(user, RoleNames.Admin);
                Assert.True(assigned.Succeeded, string.Join(", ", assigned.Errors.Select(e => e.Description)));
            }
        }
        var token = await GetTokenAsync(client, "/Account/Login");
        var response = await client.PostAsync("/Account/Login", Form(token,
            ("Email", AdminEmail), ("Password", AdminPassword), ("RememberMe", "false")));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Testing_environment_does_not_seed_configured_admin_account()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync("must-not-seed@example.test"));
    }

    private async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email)
    {
        var token = await GetTokenAsync(client, "/Account/Register");
        return await client.PostAsync("/Account/Register", Form(token,
            ("FirstName", "Nazerke"), ("LastName", "Evidence"), ("Email", email),
            ("Password", StudentPassword), ("ConfirmPassword", StudentPassword)));
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return ExtractToken(await response.Content.ReadAsStringAsync());
    }

    private static string ExtractToken(string html)
    {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
        Assert.True(match.Success, "The response did not contain an antiforgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent Form(string? token, params (string Key, string Value)[] fields)
    {
        var values = fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
        if (token is not null) values.Add(new("__RequestVerificationToken", token));
        return new FormUrlEncodedContent(values);
    }

    private static string Email() => $"student-{Guid.NewGuid():N}@example.test";

    private static string? LocationPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        return location is null ? null : location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
    }

    private async Task<string> FindUserIdAsync(string email) => (await FindUserAsync(email))!.Id;

    private async Task<ApplicationUser?> FindUserAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
    }
}
