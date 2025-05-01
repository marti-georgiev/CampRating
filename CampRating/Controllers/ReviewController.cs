using Microsoft.AspNetCore.Mvc;
using CampRating.Models;
using CampRating.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace CampRating.Controllers
{
    /// <summary>
    /// Контролер за управление на ревюта
    /// </summary>
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, ILogger<ReviewController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Показва списък с всички ревюта
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.CampPlace)
                .ToListAsync();
            return View(reviews);
        }

        /// <summary>
        /// Показва форма за създаване на ново ревю
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Create(int campPlaceId)
        {
            var campPlace = await _context.CampPlaces.FindAsync(campPlaceId);
            if (campPlace == null)
            {
                return NotFound();
            }

            ViewBag.CampPlace = campPlace;
            return View();
        }

        /// <summary>
        /// Създава ново ревю
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Rating,Comment,CampPlaceId")] Review review)
        {
            _logger.LogInformation($"Опит за създаване на ревю за място за къмпингуване {review.CampPlaceId}");
            
            // Премахване на UserId от валидацията, тъй като се задава в контролера
            ModelState.Remove("UserId");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Невалидни данни: " + string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                
                var campPlace = await _context.CampPlaces.FindAsync(review.CampPlaceId);
                if (campPlace == null)
                {
                    return NotFound();
                }
                ViewBag.CampPlace = campPlace;
                return View(review);
            }

            try
            {
                // Вземане на текущия потребител
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("Текущият потребител не е намерен");
                    return Unauthorized();
                }

                // Задаване на потребителския идентификатор
                review.UserId = user.Id;
                review.CreatedAt = DateTime.UtcNow;

                // Добавяне на ревюто
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Успешно създадено ревю {review.Id} за място за къмпингуване {review.CampPlaceId}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirect = Url.Action("Details", "CampPlace", new { id = review.CampPlaceId }) });
                }
                
                return RedirectToAction("Details", "CampPlace", new { id = review.CampPlaceId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Грешка при създаване на ревю: {ex.Message}");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Възникна грешка при запазване на ревюто. Моля, опитайте отново." });
                }
                
                ModelState.AddModelError("", "Възникна грешка при запазване на ревюто. Моля, опитайте отново.");
                var campPlace = await _context.CampPlaces.FindAsync(review.CampPlaceId);
                if (campPlace == null)
                {
                    return NotFound();
                }
                ViewBag.CampPlace = campPlace;
                return View(review);
            }
        }

        /// <summary>
        /// Показва форма за редактиране на ревю
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.CampPlace)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            // Проверка за права за редактиране
            if (review.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(review);
        }

        /// <summary>
        /// Запазва промените по ревю
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment,CampPlaceId,UserId")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReview = await _context.Reviews.FindAsync(id);
                    if (existingReview == null)
                    {
                        return NotFound();
                    }

                    // Проверка за права за редактиране
                    if (existingReview.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    // Актуализиране на данните
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.CreatedAt = DateTime.UtcNow;

                    _context.Update(existingReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "CampPlace", new { id = review.CampPlaceId });
            }
            return View(review);
        }

        /// <summary>
        /// Показва форма за изтриване на ревю
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.CampPlace)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            // Проверка за права за изтриване
            if (review.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(review);
        }

        /// <summary>
        /// Изтрива ревю
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Проверка за права за изтриване
            if (review.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "CampPlace", new { id = review.CampPlaceId });
        }

        /// <summary>
        /// Проверява дали ревю съществува
        /// </summary>
        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
} 