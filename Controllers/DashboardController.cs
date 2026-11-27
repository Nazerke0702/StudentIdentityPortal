using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentIdentityPortal.Constants;
using StudentIdentityPortal.Models;
using StudentIdentityPortal.ViewModels;

namespace StudentIdentityPortal.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Student)]
public class DashboardController(UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        var isAdmin = await users.IsInRoleAsync(user, RoleNames.Admin);
        var studentCount = await users.GetUsersInRoleAsync(RoleNames.Student);
        return View(new DashboardViewModel($"{user.FirstName} {user.LastName}", user.Email ?? string.Empty,
            isAdmin ? RoleNames.Admin : RoleNames.Student, user.CreatedAt, studentCount.Count));
    }
}
