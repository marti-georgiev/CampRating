using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampRating.Models;
using Microsoft.AspNetCore.Authorization;

namespace CampRating.Controllers
{
    /// <summary>
    /// Контролер за управление на потребители (достъпен само за администратори)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Показва списък с всички потребители
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                })
                .ToListAsync();

            return View(users);
        }

        /// <summary>
        /// Показва форма за редактиране на потребител
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentRoles = userRoles.ToList(),
                AllRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Запазва промените по потребител
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Актуализиране на данните за потребителя
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Актуализиране на ролите
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    
                    if (model.SelectedRoles != null)
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Ако сме стигнали дотук, нещо е се провалило - показваме формата отново
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            model.AllRoles = allRoles;
            return View(model);
        }

        /// <summary>
        /// Изтрива потребител
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Проверка дали не се опитваме да изтрием последния администратор
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
                if (adminCount <= 1)
                {
                    ModelState.AddModelError("", "Не може да изтриете последния администратор.");
                    return RedirectToAction(nameof(Index));
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Възникна грешка при изтриване на потребителя.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 