using GymManagment.DAL.Models;
using GymMangment.BLL.ViewModels.AccountViewModels;
using GymMangment.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.Json;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    CurrentRole = roles.FirstOrDefault(),
                    AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
                });
            }

            return View(model);
        }

        // POST: Admin/AssignRole
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // Remove all existing roles first
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Assign new role
            if (!string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Role '{role}' assigned to {user.FullName} successfully!";
            }
            else
            {
                TempData["WarningMessage"] = $"All roles removed from {user.FullName}.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.DeleteAsync(user);
            TempData["WarningMessage"] = $"User {user.FullName} deleted successfully!";
            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/ManageAnnouncement
        [HttpGet]
        public IActionResult ManageAnnouncement()
        {
            var filePath = Path.Combine(_env.WebRootPath, "data", "announcement.json");
            var model = new AnnouncementViewModel();

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(filePath);
                    model = JsonSerializer.Deserialize<AnnouncementViewModel>(json) ?? new AnnouncementViewModel();
                }
                catch
                {
                    TempData["ErrorMessage"] = "Error reading the announcement file.";
                }
            }

            return View(model);
        }

        // POST: Admin/ManageAnnouncement
        [HttpPost]
        public IActionResult ManageAnnouncement(AnnouncementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var directoryPath = Path.Combine(_env.WebRootPath, "data");
            var filePath = Path.Combine(directoryPath, "announcement.json");

            try
            {
                Directory.CreateDirectory(directoryPath);
                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);
                TempData["SuccessMessage"] = "Announcement updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to save announcement: {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}