using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampRating.Models;
using CampRating.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CampRating.Controllers
{
    /// <summary>
    /// Контролер за управление на места за къмпингуване
    /// </summary>
    public class CampPlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CampPlaceController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Показва списък с всички места за къмпингуване
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var campPlaces = await _context.CampPlaces
                .Include(c => c.User)
                .ToListAsync();
            return View(campPlaces);
        }

        /// <summary>
        /// Показва детайли за конкретно място за къмпингуване
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campPlace = await _context.CampPlaces
                .Include(c => c.User)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (campPlace == null)
            {
                return NotFound();
            }

            var viewModel = new CampPlaceDetailsViewModel
            {
                CampPlace = campPlace,
                NewReview = new Review { CampPlaceId = campPlace.Id }
            };

            return View(viewModel);
        }

        /// <summary>
        /// Показва форма за създаване на ново място за къмпингуване
        /// </summary>
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Създава ново място за къмпингуване
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Latitude,Longitude,Photo")] CampPlace campPlace, IFormFile photo)
        {
            if (ModelState.IsValid)
            {
                // Обработка на качената снимка
                if (photo != null && photo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "campplaces");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }

                    campPlace.Photo = "/images/campplaces/" + uniqueFileName;
                }

                campPlace.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                campPlace.DateCreated = DateTime.Now;
                _context.Add(campPlace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(campPlace);
        }

        /// <summary>
        /// Показва форма за редактиране на място за къмпингуване
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campPlace = await _context.CampPlaces.FindAsync(id);
            if (campPlace == null)
            {
                return NotFound();
            }

            // Проверка за права за редактиране
            if (campPlace.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(campPlace);
        }

        /// <summary>
        /// Запазва промените по място за къмпингуване
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Latitude,Longitude,Photo")] CampPlace campPlace, IFormFile photo)
        {
            if (id != campPlace.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCampPlace = await _context.CampPlaces.FindAsync(id);
                    if (existingCampPlace == null)
                    {
                        return NotFound();
                    }

                    // Проверка за права за редактиране
                    if (existingCampPlace.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
                    {
                        return Forbid();
                    }

                    // Обработка на нова снимка
                    if (photo != null && photo.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "campplaces");
                        Directory.CreateDirectory(uploadsFolder);

                        // Изтриване на старата снимка
                        if (!string.IsNullOrEmpty(existingCampPlace.Photo))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCampPlace.Photo.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(fileStream);
                        }

                        existingCampPlace.Photo = "/images/campplaces/" + uniqueFileName;
                    }

                    // Актуализиране на данните
                    existingCampPlace.Name = campPlace.Name;
                    existingCampPlace.Description = campPlace.Description;
                    existingCampPlace.Latitude = campPlace.Latitude;
                    existingCampPlace.Longitude = campPlace.Longitude;
                    existingCampPlace.DateModified = DateTime.Now;

                    _context.Update(existingCampPlace);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CampPlaceExists(campPlace.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(campPlace);
        }

        /// <summary>
        /// Показва форма за изтриване на място за къмпингуване
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campPlace = await _context.CampPlaces
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (campPlace == null)
            {
                return NotFound();
            }

            // Проверка за права за изтриване
            if (campPlace.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(campPlace);
        }

        /// <summary>
        /// Изтрива място за къмпингуване
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var campPlace = await _context.CampPlaces.FindAsync(id);
            if (campPlace == null)
            {
                return NotFound();
            }

            // Проверка за права за изтриване
            if (campPlace.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Изтриване на снимката
            if (!string.IsNullOrEmpty(campPlace.Photo))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, campPlace.Photo.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.CampPlaces.Remove(campPlace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверява дали място за къмпингуване съществува
        /// </summary>
        private bool CampPlaceExists(int id)
        {
            return _context.CampPlaces.Any(e => e.Id == id);
        }
    }
} 