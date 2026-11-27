using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentIdentityPortal.Constants;
using StudentIdentityPortal.Models;
using StudentIdentityPortal.ViewModels;

namespace StudentIdentityPortal.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class StudentsController(UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var students = await users.GetUsersInRoleAsync(RoleNames.Student);
        return View(students.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new StudentViewModel(x.Id, x.FirstName, x.LastName, x.Email ?? string.Empty, x.CreatedAt)));
    }

    public async Task<IActionResult> Details(string id)
    {
        var student = await users.FindByIdAsync(id);
        if (student is null || !await users.IsInRoleAsync(student, RoleNames.Student)) return NotFound();
        return View(new StudentViewModel(student.Id, student.FirstName, student.LastName, student.Email ?? string.Empty, student.CreatedAt));
    }
}
