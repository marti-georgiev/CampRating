using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampRating.Models;
using CampRating.Data;

namespace CampRating.Controllers
{
    /// <summary>
    /// Контролер за началната страница и общи функционалности
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Показва началната страница с места за къмпингуване
        /// </summary>
        public async Task<IActionResult> Index(string searchString)
        {
            var campPlaces = from c in _context.CampPlaces
                            select c;

            // Филтриране по име, ако е зададено търсене
            if (!string.IsNullOrEmpty(searchString))
            {
                campPlaces = campPlaces.Where(c => c.Name.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            var model = new HomeViewModel
            {
                CampPlaces = await campPlaces
                    .Include(c => c.Reviews)
                    .OrderByDescending(c => c.DateCreated)
                    .ToListAsync()
            };

            // За администраторите показваме статистика
            if (User.IsInRole("Admin"))
            {
                model.TotalUsers = await _userManager.Users.CountAsync();
                model.TotalCampPlaces = await _context.CampPlaces.CountAsync();
                model.TotalReviews = await _context.Reviews.CountAsync();
            }

            return View(model);
        }

        /// <summary>
        /// Показва страницата за поверителност
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Показва страницата за отказан достъп
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Показва страницата за грешка
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 