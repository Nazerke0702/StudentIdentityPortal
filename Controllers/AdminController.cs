using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentIdentityPortal.Constants;
using StudentIdentityPortal.Models;

namespace StudentIdentityPortal.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController(UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var students = await users.GetUsersInRoleAsync(RoleNames.Student);
        return View(students.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(string id)
    {
        var student = await users.FindByIdAsync(id);
        if (student is null || !await users.IsInRoleAsync(student, RoleNames.Student)) return NotFound();
        var result = await users.DeleteAsync(student);
        if (!result.Succeeded)
            TempData["Error"] = string.Join(" ", result.Errors.Select(x => x.Description));
        else
            TempData["Notice"] = $"{student.FirstName} {student.LastName} was removed.";
        return RedirectToAction(nameof(Index));
    }
}
