using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentIdentityPortal.Models;
using StudentIdentityPortal.ViewModels;

namespace StudentIdentityPortal.Controllers;

[Authorize]
public class ProfileController(UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(new ProfileViewModel { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email ?? string.Empty, CreatedAt = user.CreatedAt });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        if (!ModelState.IsValid)
        {
            model.Email = user.Email ?? string.Empty;
            model.CreatedAt = user.CreatedAt;
            return View(model);
        }
        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        else
            TempData["Notice"] = "Your profile has been updated.";
        model.Email = user.Email ?? string.Empty;
        model.CreatedAt = user.CreatedAt;
        return View(model);
    }
}
